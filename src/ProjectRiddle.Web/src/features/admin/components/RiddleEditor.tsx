import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef, useState, type ReactElement } from "react";
import { useBeforeUnload, useBlocker, useNavigate, type BlockerFunction } from "react-router-dom";

import { reconcileSessionAfterAuthorizationFailure } from "../../auth/api/sessionQuery";
import { isApplicationError } from "../../../shared/api/errors";
import { ConfirmationDialog } from "../../../shared/components/ConfirmationDialog";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { ErrorSummary } from "../../../shared/components/ErrorSummary";
import { PageStatus } from "../../../shared/components/PageStatus";
import { adminRiddleDetailQueryOptions, adminRiddleKeys } from "../api/adminRiddleQueries";
import { adminRiddlesApi } from "../api/adminRiddlesApi";
import { riddleMessageForCode, unknownRiddleFailure } from "../messages/adminMessages";
import type { Riddle } from "../models/adminRiddle";
import {
    draftFromRiddle,
    draftsEqual,
    emptyRiddleContentErrors,
    emptyRiddleDraft,
    requestFromDraft,
    type RiddleContentErrors,
    type RiddleDraft,
} from "../models/riddleDraft";
import { hasContentErrors, mapRiddleContentErrors, requiredContentErrors } from "./riddleContentErrors";
import { PublicationPanel } from "./PublicationPanel";
import { RiddleContentForm } from "./RiddleContentForm";
import { RiddlePreview } from "./RiddlePreview";
import styles from "./RiddleEditor.module.css";

export interface RiddleEditorProps {
    readonly riddleId?: string;
}

export function RiddleEditor({ riddleId }: RiddleEditorProps): ReactElement {
    const isNew = riddleId === undefined;
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const initialRiddleRef = useRef<Riddle | undefined>(
        riddleId === undefined ? undefined : queryClient.getQueryData<Riddle>(adminRiddleKeys.detail(riddleId)),
    );
    const titleRef = useRef<HTMLHeadingElement>(null);
    const summaryRef = useRef<HTMLDivElement>(null);
    const createdRiddlePathRef = useRef<string | null>(null);
    const [draft, setDraft] = useState<RiddleDraft>(() =>
        initialRiddleRef.current === undefined ? emptyRiddleDraft() : draftFromRiddle(initialRiddleRef.current),
    );
    const [baseline, setBaseline] = useState<RiddleDraft>(() =>
        initialRiddleRef.current === undefined ? emptyRiddleDraft() : draftFromRiddle(initialRiddleRef.current),
    );
    const [initializedId, setInitializedId] = useState<string | undefined>(() =>
        initialRiddleRef.current === undefined ? undefined : riddleId,
    );
    const [errors, setErrors] = useState<RiddleContentErrors>(emptyRiddleContentErrors);
    const [shouldFocusSummary, setShouldFocusSummary] = useState(false);
    const [missing, setMissing] = useState(false);

    const detailQuery = useQuery({
        ...adminRiddleDetailQueryOptions(riddleId ?? "new"),
        enabled: !isNew,
    });

    const isDirty = isNew && !draftsEqual(draft, baseline);
    const shouldBlockNavigation = useCallback<BlockerFunction>(
        ({ nextLocation }) => isDirty && !(isNew && nextLocation.pathname === createdRiddlePathRef.current),
        [isDirty, isNew],
    );
    const blocker = useBlocker(shouldBlockNavigation);
    const authorizationFailure =
        isApplicationError(detailQuery.error) && (detailQuery.error.status === 401 || detailQuery.error.status === 403);
    const notFound =
        missing ||
        (isApplicationError(detailQuery.error) &&
            (detailQuery.error.status === 404 || detailQuery.error.code === "riddles.notFound"));

    useBeforeUnload(
        useCallback(
            (event: BeforeUnloadEvent) => {
                if (isDirty) {
                    event.preventDefault();
                }
            },
            [isDirty],
        ),
    );

    useEffect(() => {
        if (shouldFocusSummary) {
            summaryRef.current?.focus();
            setShouldFocusSummary(false);
        }
    }, [shouldFocusSummary]);

    useEffect(() => {
        if (detailQuery.error !== null && authorizationFailure) {
            reconcileSessionAfterAuthorizationFailure(queryClient, detailQuery.error);
        }
    }, [authorizationFailure, detailQuery.error, queryClient]);

    useEffect(() => {
        const riddle = detailQuery.data;

        if (riddle === undefined || riddleId === undefined) {
            return;
        }

        const next = draftFromRiddle(riddle);

        if (initializedId !== riddleId) {
            setDraft(next);
            setBaseline(next);
            setInitializedId(riddleId);
            setMissing(false);
            return;
        }

        if (draftsEqual(draft, baseline) && !draftsEqual(draft, next)) {
            setDraft(next);
            setBaseline(next);
        }
    }, [baseline, detailQuery.data, draft, initializedId, riddleId]);

    const createMutation = useMutation({
        mutationFn: adminRiddlesApi.create,
        onSuccess: async (created) => {
            queryClient.setQueryData(adminRiddleKeys.detail(created.id), created);
            await queryClient.invalidateQueries({ queryKey: adminRiddleKeys.lists() });
            const createdRiddlePath = `/admin/riddles/${created.id}`;
            const next = draftFromRiddle(created);
            createdRiddlePathRef.current = createdRiddlePath;
            setDraft(next);
            setBaseline(next);
            setErrors(emptyRiddleContentErrors());
            setInitializedId(created.id);
            if (blocker.state === "blocked") {
                blocker.reset();
            }
            void navigate(createdRiddlePath, { replace: true });
        },
        onError: (error: unknown) => {
            if (reconcileSessionAfterAuthorizationFailure(queryClient, error)) {
                return;
            }

            setErrors(mapRiddleContentErrors(error));
            setShouldFocusSummary(true);
        },
    });

    function handleSave(): void {
        const required = requiredContentErrors(draft);
        if (hasContentErrors(required)) {
            setErrors(required);
            setShouldFocusSummary(true);
            return;
        }

        createMutation.mutate(requestFromDraft(draft));
    }

    const saving = createMutation.isPending;

    if (!isNew && detailQuery.isPending && initializedId !== riddleId) {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    eyebrow="Администрация"
                    title="Зареждаме загадката…"
                    message="Изчакваме текущото съдържание."
                />
            </>
        );
    }

    if (!isNew && authorizationFailure && initializedId !== riddleId) {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    tone="error"
                    eyebrow="Администрация"
                    title="Няма достъп до загадката"
                    message="Сесията или правата трябва да бъдат проверени отново."
                />
            </>
        );
    }

    if (!isNew && notFound) {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    tone="error"
                    eyebrow="Администрация"
                    title="Загадката не е намерена"
                    message={riddleMessageForCode("riddles.notFound") ?? "Загадката не е намерена."}
                />
            </>
        );
    }

    if (!isNew && detailQuery.isError && !authorizationFailure && !notFound && initializedId !== riddleId) {
        const message = isApplicationError(detailQuery.error)
            ? unknownRiddleFailure(detailQuery.error.traceId)
            : unknownRiddleFailure(undefined);

        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    tone="error"
                    eyebrow="Администрация"
                    title="Загадката временно не е достъпна"
                    message={message}
                    action={{
                        label: "Опитай отново",
                        onClick: () => {
                            void detailQuery.refetch();
                        },
                    }}
                />
            </>
        );
    }

    return (
        <section className={styles.editor} aria-labelledby="riddle-editor-title">
            <DocumentTitle title={isNew ? "Нова загадка" : "Загадка"} />
            <div className={styles.header}>
                <p className="eyebrow">Администрация</p>
                <h1 ref={titleRef} id="riddle-editor-title" tabIndex={-1}>
                    {isNew ? "Нова загадка" : "Загадка"}
                </h1>
            </div>
            <ErrorSummary
                messages={errors.summary}
                heading="Има проблем със съдържанието"
                headingId="riddle-content-error-heading"
                summaryRef={summaryRef}
            />
            {!isNew ? (
                <p className={styles.immutableNotice} role="note">
                    Съдържанието е окончателно след създаване. За корекция оттеглете загадката и създайте нова.
                </p>
            ) : null}
            <div className={styles.layout}>
                <div className={styles.authoring}>
                    <RiddleContentForm draft={draft} errors={errors} disabled={saving || !isNew} onChange={setDraft} />
                    {isNew ? (
                        <button
                            type="button"
                            className={styles.save}
                            aria-busy={saving}
                            disabled={saving}
                            onClick={handleSave}
                        >
                            {saving ? "Запазване…" : "Запази"}
                        </button>
                    ) : null}
                </div>
                <RiddlePreview clue={draft.clue} answer={draft.answer} ranges={draft.ranges} />
            </div>
            {detailQuery.data !== undefined && riddleId === detailQuery.data.id ? (
                <PublicationPanel
                    riddle={detailQuery.data}
                    contentDirty={isDirty}
                    onChanged={(updated) => {
                        if (!isDirty) {
                            const next = draftFromRiddle(updated);
                            setDraft(next);
                            setBaseline(next);
                        }
                    }}
                    onDeleted={() => {
                        setBaseline(draft);
                    }}
                    onMissing={() => {
                        setMissing(true);
                    }}
                />
            ) : null}
            <ConfirmationDialog
                open={blocker.state === "blocked"}
                title="Незапазени промени"
                description="Има незапазени промени по съдържанието. Ако напуснете, те ще бъдат загубени."
                confirmLabel="Напусни без запис"
                cancelLabel="Остани"
                busy={false}
                danger
                returnFocusRef={titleRef}
                onCancel={() => {
                    if (blocker.state === "blocked") {
                        blocker.reset();
                    }
                }}
                onConfirm={() => {
                    if (blocker.state === "blocked") {
                        blocker.proceed();
                    }
                }}
            />
        </section>
    );
}
