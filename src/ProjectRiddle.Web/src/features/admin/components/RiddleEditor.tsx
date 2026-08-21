import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef, useState, type ReactElement } from "react";
import { useBeforeUnload, useBlocker, useNavigate } from "react-router-dom";

import { reconcileSessionAfterAuthorizationFailure } from "../../auth/api/sessionQuery";
import { isApplicationError } from "../../../shared/api/errors";
import { ConfirmationDialog } from "../../../shared/components/ConfirmationDialog";
import { PageStatus } from "../../../shared/components/PageStatus";
import { adminRiddleDetailQueryOptions, adminRiddleKeys } from "../api/adminRiddleQueries";
import { adminRiddlesApi } from "../api/adminRiddlesApi";
import { riddleMessageForCode, unknownRiddleFailure } from "../messages/adminMessages";
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
    const stayRef = useRef<HTMLButtonElement>(null);
    const summaryRef = useRef<HTMLDivElement>(null);
    const [draft, setDraft] = useState<RiddleDraft>(emptyRiddleDraft);
    const [baseline, setBaseline] = useState<RiddleDraft>(emptyRiddleDraft);
    const [initializedId, setInitializedId] = useState<string | undefined>(undefined);
    const [errors, setErrors] = useState<RiddleContentErrors>(emptyRiddleContentErrors);
    const [shouldFocusSummary, setShouldFocusSummary] = useState(false);
    const [missing, setMissing] = useState(false);
    const [saveNotice, setSaveNotice] = useState<string | null>(null);

    const detailQuery = useQuery({
        ...adminRiddleDetailQueryOptions(riddleId ?? "new"),
        enabled: !isNew,
    });

    const isDirty = !draftsEqual(draft, baseline);
    const blocker = useBlocker(isDirty);
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
            const next = draftFromRiddle(created);
            setDraft(next);
            setBaseline(next);
            setErrors(emptyRiddleContentErrors());
            setInitializedId(created.id);
            if (blocker.state === "blocked") {
                blocker.reset();
            }
            void navigate(`/admin/riddles/${created.id}`, { replace: true });
        },
        onError: (error: unknown) => {
            if (reconcileSessionAfterAuthorizationFailure(queryClient, error)) {
                return;
            }

            setErrors(mapRiddleContentErrors(error));
            setShouldFocusSummary(true);
        },
    });

    const updateMutation = useMutation({
        mutationFn: (input: RiddleDraft) => adminRiddlesApi.update(riddleId ?? "", requestFromDraft(input)),
        onSuccess: async (updated) => {
            queryClient.setQueryData(adminRiddleKeys.detail(updated.id), updated);
            await queryClient.invalidateQueries({ queryKey: adminRiddleKeys.lists() });
            const next = draftFromRiddle(updated);
            setDraft(next);
            setBaseline(next);
            setErrors(emptyRiddleContentErrors());
            setSaveNotice("Промените са запазени.");
            if (blocker.state === "blocked") {
                blocker.reset();
            }
        },
        onError: (error: unknown) => {
            if (isApplicationError(error) && (error.status === 404 || error.code === "riddles.notFound")) {
                setMissing(true);
                return;
            }

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

        setSaveNotice(null);
        if (isNew) {
            createMutation.mutate(requestFromDraft(draft));
            return;
        }

        updateMutation.mutate(draft);
    }

    const saving = createMutation.isPending || updateMutation.isPending;

    if (!isNew && detailQuery.isPending && initializedId !== riddleId) {
        return (
            <PageStatus
                eyebrow="Администрация"
                title="Зареждаме загадката…"
                message="Изчакваме текущото съдържание за редакция."
            />
        );
    }

    if (!isNew && authorizationFailure && initializedId !== riddleId) {
        return (
            <PageStatus
                tone="error"
                eyebrow="Администрация"
                title="Няма достъп до загадката"
                message="Сесията или правата трябва да бъдат проверени отново."
            />
        );
    }

    if (!isNew && notFound) {
        return (
            <PageStatus
                tone="error"
                eyebrow="Администрация"
                title="Загадката не е намерена"
                message={riddleMessageForCode("riddles.notFound") ?? "Загадката не е намерена."}
            />
        );
    }

    if (!isNew && detailQuery.isError && !authorizationFailure && !notFound && initializedId !== riddleId) {
        const message = isApplicationError(detailQuery.error)
            ? unknownRiddleFailure(detailQuery.error.traceId)
            : unknownRiddleFailure(undefined);

        return (
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
        );
    }

    return (
        <section className={styles.editor} aria-labelledby="riddle-editor-title">
            <div className={styles.header}>
                <p className="eyebrow">Администрация</p>
                <h1 id="riddle-editor-title">{isNew ? "Нова загадка" : "Редакция на загадка"}</h1>
            </div>
            {errors.summary.length > 0 ? (
                <div
                    ref={summaryRef}
                    className={styles.summary}
                    tabIndex={-1}
                    role="alert"
                    aria-labelledby="riddle-content-error-heading"
                >
                    <h2 id="riddle-content-error-heading">Има проблем със съдържанието</h2>
                    <ul>
                        {errors.summary.map((message) => (
                            <li key={message}>{message}</li>
                        ))}
                    </ul>
                </div>
            ) : null}
            {saveNotice !== null ? (
                <p className={styles.notice} role="status">
                    {saveNotice}
                </p>
            ) : null}
            <div className={styles.layout}>
                <div className={styles.authoring}>
                    <RiddleContentForm draft={draft} errors={errors} disabled={saving} onChange={setDraft} />
                    <button ref={stayRef} type="button" aria-busy={saving} disabled={saving} onClick={handleSave}>
                        {saving ? "Запазване…" : "Запази"}
                    </button>
                </div>
                <RiddlePreview clue={draft.clue} answerPattern={draft.answerPattern} ranges={draft.ranges} />
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
                returnFocusRef={stayRef}
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
