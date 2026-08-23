import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef, useState, type ReactElement } from "react";
import { useBeforeUnload, useBlocker, useNavigate, type BlockerFunction } from "react-router-dom";

import { reconcileSessionAfterAuthorizationFailure } from "../../auth/api/sessionQuery";
import { ConfirmationDialog } from "../../../shared/components/ConfirmationDialog";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { ErrorSummary } from "../../../shared/components/ErrorSummary";
import { adminRiddleKeys } from "../api/adminRiddleQueries";
import { adminRiddlesApi } from "../api/adminRiddlesApi";
import {
    draftsEqual,
    emptyRiddleContentErrors,
    emptyRiddleDraft,
    requestFromDraft,
    type RiddleContentErrors,
    type RiddleDraft,
} from "../models/riddleDraft";
import { hasContentErrors, mapRiddleContentErrors, requiredContentErrors } from "./riddleContentErrors";
import { RiddleContentForm } from "./RiddleContentForm";
import { RiddlePreview } from "./RiddlePreview";
import styles from "./NewRiddleForm.module.css";

/**
 * Authors a riddle. Content is written once: a saved riddle is never edited, so this form exists only for creation
 * and the saved riddle opens in its read-only detail view.
 */
export function NewRiddleForm(): ReactElement {
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const titleRef = useRef<HTMLHeadingElement>(null);
    const summaryRef = useRef<HTMLDivElement>(null);
    const createdRiddlePathRef = useRef<string | null>(null);
    const [draft, setDraft] = useState<RiddleDraft>(emptyRiddleDraft);
    const [errors, setErrors] = useState<RiddleContentErrors>(emptyRiddleContentErrors);
    const [shouldFocusSummary, setShouldFocusSummary] = useState(false);

    const isDirty = !draftsEqual(draft, emptyRiddleDraft());
    const shouldBlockNavigation = useCallback<BlockerFunction>(
        ({ nextLocation }) => isDirty && nextLocation.pathname !== createdRiddlePathRef.current,
        [isDirty],
    );
    const blocker = useBlocker(shouldBlockNavigation);

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

    const createMutation = useMutation({
        mutationFn: adminRiddlesApi.create,
        onSuccess: async (created) => {
            queryClient.setQueryData(adminRiddleKeys.detail(created.id), created);
            await queryClient.invalidateQueries({ queryKey: adminRiddleKeys.lists() });
            const createdRiddlePath = `/admin/riddles/${created.id}`;
            createdRiddlePathRef.current = createdRiddlePath;
            setErrors(emptyRiddleContentErrors());
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

    return (
        <section className={styles.form} aria-labelledby="new-riddle-title">
            <DocumentTitle title="Нова загадка" />
            <div className={styles.header}>
                <p className="eyebrow">Администрация</p>
                <h1 ref={titleRef} id="new-riddle-title" tabIndex={-1}>
                    Нова загадка
                </h1>
                <p className={styles.immutableNotice}>
                    Съдържанието е окончателно след запис. Загадките не се редактират — при нужда изтрийте черновата и
                    създайте нова.
                </p>
            </div>
            <ErrorSummary
                messages={errors.summary}
                heading="Има проблем със съдържанието"
                headingId="riddle-content-error-heading"
                summaryRef={summaryRef}
            />
            <div className={styles.layout}>
                <div className={styles.authoring}>
                    <RiddleContentForm draft={draft} errors={errors} disabled={saving} onChange={setDraft} />
                    <button
                        type="button"
                        className={styles.save}
                        aria-busy={saving}
                        disabled={saving}
                        onClick={handleSave}
                    >
                        {saving ? "Запазване…" : "Запази"}
                    </button>
                </div>
                <RiddlePreview clue={draft.clue} answer={draft.answer} ranges={draft.ranges} />
            </div>
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
