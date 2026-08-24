import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useId, useRef, useState, type ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import { reconcileSessionAfterAuthorizationFailure } from "../../auth/api/sessionQuery";
import { isApplicationError } from "../../../shared/api/errors";
import { ConfirmationDialog } from "../../../shared/components/ConfirmationDialog";
import { ErrorSummary } from "../../../shared/components/ErrorSummary";
import { FieldError } from "../../../shared/components/FieldError";
import { adminRiddleKeys } from "../api/adminRiddleQueries";
import { adminRiddlesApi } from "../api/adminRiddlesApi";
import { publicationStateLabels, riddleMessageForCode, unknownRiddleFailure } from "../messages/adminMessages";
import type { Riddle } from "../models/adminRiddle";
import { todayInSofia } from "../models/publicationDate";
import styles from "./PublicationPanel.module.css";

export interface PublicationPanelProps {
    readonly riddle: Riddle;
    readonly onMissing: () => void;
}

const commandsByState = {
    draft: ["schedule", "publish", "delete"],
    scheduled: ["publish", "unpublish"],
    published: ["unpublish"],
    unpublished: ["schedule", "publish", "delete"],
} as const;

type PublicationCommand = (typeof commandsByState)[keyof typeof commandsByState][number];

function isMissingRiddle(error: unknown): boolean {
    return isApplicationError(error) && (error.status === 404 || error.code === "riddles.notFound");
}

export function PublicationPanel({ riddle, onMissing }: PublicationPanelProps): ReactElement {
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const dateId = useId();
    const dateErrorId = useId();
    const unpublishRef = useRef<HTMLButtonElement>(null);
    const deleteRef = useRef<HTMLButtonElement>(null);
    // An unscheduled riddle opens on today in Sofia so the common case needs no typing. Nothing is sent until a
    // publication command is pressed.
    const [publicationDate, setPublicationDate] = useState(riddle.sofiaPublicationDate ?? todayInSofia());
    const [dateErrors, setDateErrors] = useState<readonly string[]>([]);
    const [summary, setSummary] = useState<readonly string[]>([]);
    const [notice, setNotice] = useState<string | null>(null);
    const [confirm, setConfirm] = useState<"unpublish" | "delete" | null>(null);

    useEffect(() => {
        setPublicationDate(riddle.sofiaPublicationDate ?? todayInSofia());
    }, [riddle.id, riddle.sofiaPublicationDate]);

    const commands = commandsByState[riddle.publicationState];

    function clearMessages(): void {
        setDateErrors([]);
        setSummary([]);
        setNotice(null);
    }

    function handleFailure(error: unknown): void {
        if (isMissingRiddle(error)) {
            onMissing();
            return;
        }

        if (reconcileSessionAfterAuthorizationFailure(queryClient, error)) {
            return;
        }

        if (!isApplicationError(error)) {
            setSummary([unknownRiddleFailure(undefined)]);
            return;
        }

        const mapped = riddleMessageForCode(error.code) ?? unknownRiddleFailure(error.traceId);
        if (error.code === "riddles.publicationDate.invalid" || error.code === "riddles.publicationDate.conflict") {
            setDateErrors([mapped]);
            setSummary([mapped]);
            return;
        }

        setSummary([mapped]);
    }

    async function persistSuccess(updated: Riddle, message: string): Promise<void> {
        queryClient.setQueryData(adminRiddleKeys.detail(updated.id), updated);
        await queryClient.invalidateQueries({ queryKey: adminRiddleKeys.lists() });
        clearMessages();
        setNotice(message);
    }

    const scheduleMutation = useMutation({
        mutationFn: () => adminRiddlesApi.schedule(riddle.id, publicationDate),
        onSuccess: (updated) => {
            void persistSuccess(
                updated,
                `Криптиката е насрочена за ${updated.sofiaPublicationDate ?? publicationDate}.`,
            );
        },
        onError: handleFailure,
    });

    const publishMutation = useMutation({
        mutationFn: () => {
            const omitDate =
                riddle.publicationState === "scheduled" &&
                (publicationDate.length === 0 || publicationDate === (riddle.sofiaPublicationDate ?? ""));
            return adminRiddlesApi.publish(riddle.id, omitDate ? undefined : publicationDate);
        },
        onSuccess: (updated) => {
            void persistSuccess(updated, "Криптиката е публикувана.");
        },
        onError: handleFailure,
    });

    const unpublishMutation = useMutation({
        mutationFn: () => adminRiddlesApi.unpublish(riddle.id),
        onSuccess: (updated) => {
            setConfirm(null);
            void persistSuccess(updated, "Криптиката е свалена и датата е свободна.");
        },
        onError: handleFailure,
    });

    const deleteMutation = useMutation({
        mutationFn: () => adminRiddlesApi.delete(riddle.id),
        onSuccess: async () => {
            setConfirm(null);
            queryClient.removeQueries({ queryKey: adminRiddleKeys.detail(riddle.id) });
            await queryClient.invalidateQueries({ queryKey: adminRiddleKeys.lists() });
            void navigate("/admin/riddles", { replace: true });
        },
        onError: handleFailure,
    });

    const busy =
        scheduleMutation.isPending ||
        publishMutation.isPending ||
        unpublishMutation.isPending ||
        deleteMutation.isPending;
    const needsDate = riddle.publicationState === "draft" || riddle.publicationState === "unpublished";

    function hasCommand(command: PublicationCommand): boolean {
        return (commands as readonly PublicationCommand[]).includes(command);
    }

    function requireDate(): boolean {
        if (publicationDate.length === 0) {
            setDateErrors(["Въведи дата на публикуване (София)."]);
            setSummary(["Въведи дата на публикуване (София)."]);
            return false;
        }

        return true;
    }

    return (
        <section className={styles.panel} aria-labelledby="publication-title">
            <h2 id="publication-title">Публикуване</h2>
            <p>
                Състояние: <span className={styles.chip}>{publicationStateLabels[riddle.publicationState]}</span>
            </p>
            <div className={styles.field}>
                <label htmlFor={dateId}>Дата на публикуване (София)</label>
                <input
                    id={dateId}
                    type="date"
                    value={publicationDate}
                    disabled={busy}
                    aria-invalid={dateErrors.length > 0}
                    aria-describedby={dateErrors.length > 0 ? dateErrorId : undefined}
                    onChange={(event) => {
                        setPublicationDate(event.target.value);
                    }}
                />
                <FieldError id={dateErrorId} messages={dateErrors} />
            </div>
            <ErrorSummary messages={summary} heading="Провери публикуването:" />
            {notice !== null ? (
                <p className={styles.notice} role="status">
                    {notice}
                </p>
            ) : null}
            <div className={styles.actions}>
                {hasCommand("schedule") ? (
                    <button
                        type="button"
                        className={hasCommand("publish") ? "buttonSecondary" : undefined}
                        disabled={busy}
                        onClick={() => {
                            if (requireDate()) {
                                scheduleMutation.mutate();
                            }
                        }}
                    >
                        Насрочи
                    </button>
                ) : null}
                {hasCommand("publish") ? (
                    <button
                        type="button"
                        disabled={busy}
                        onClick={() => {
                            if (needsDate && !requireDate()) {
                                return;
                            }

                            publishMutation.mutate();
                        }}
                    >
                        Публикувай
                    </button>
                ) : null}
                {hasCommand("unpublish") ? (
                    <button
                        ref={unpublishRef}
                        type="button"
                        className="buttonSecondary"
                        disabled={busy}
                        onClick={() => {
                            setConfirm("unpublish");
                        }}
                    >
                        Свали
                    </button>
                ) : null}
                {hasCommand("delete") ? (
                    <button
                        ref={deleteRef}
                        type="button"
                        className={styles.danger}
                        disabled={busy}
                        onClick={() => {
                            setConfirm("delete");
                        }}
                    >
                        Изтрий
                    </button>
                ) : null}
            </div>
            <ConfirmationDialog
                open={confirm !== null}
                title={confirm === "delete" ? "Изтриване на криптика" : "Сваляне от календара"}
                description={
                    confirm === "delete"
                        ? "Криптиката ще изчезне завинаги. Това не се връща."
                        : "Календарният ден ще се освободи и може да бъде използван за друга криптика."
                }
                confirmLabel={confirm === "delete" ? "Изтрий окончателно" : "Свали"}
                cancelLabel="Отказ"
                busy={unpublishMutation.isPending || deleteMutation.isPending}
                danger={confirm === "delete"}
                returnFocusRef={confirm === "delete" ? deleteRef : unpublishRef}
                onCancel={() => {
                    if (!unpublishMutation.isPending && !deleteMutation.isPending) {
                        setConfirm(null);
                    }
                }}
                onConfirm={() => {
                    if (confirm === "delete") {
                        deleteMutation.mutate();
                        return;
                    }

                    unpublishMutation.mutate();
                }}
            />
        </section>
    );
}
