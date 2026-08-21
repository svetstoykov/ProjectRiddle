import { useEffect, useRef, type ReactElement, type RefObject } from "react";

import styles from "./ConfirmationDialog.module.css";

export interface ConfirmationDialogProps {
    readonly open: boolean;
    readonly title: string;
    readonly description: string;
    readonly confirmLabel: string;
    readonly cancelLabel?: string;
    readonly busy: boolean;
    readonly danger: boolean;
    readonly onConfirm: () => void;
    readonly onCancel: () => void;
    readonly returnFocusRef: RefObject<HTMLElement | null>;
}

export function ConfirmationDialog({
    open,
    title,
    description,
    confirmLabel,
    cancelLabel = "Отказ",
    busy,
    danger,
    onConfirm,
    onCancel,
    returnFocusRef,
}: ConfirmationDialogProps): ReactElement {
    const dialogRef = useRef<HTMLDialogElement>(null);
    const titleId = "confirmation-dialog-title";
    const descriptionId = "confirmation-dialog-description";

    useEffect(() => {
        const dialog = dialogRef.current;

        if (dialog === null) {
            return;
        }

        if (open) {
            if (!dialog.open) {
                dialog.showModal();
            }

            return;
        }

        if (dialog.open) {
            dialog.close();
        }
    }, [open]);

    useEffect(() => {
        const dialog = dialogRef.current;

        if (dialog === null) {
            return;
        }

        const handleClose = (): void => {
            returnFocusRef.current?.focus();
        };

        dialog.addEventListener("close", handleClose);
        return () => {
            dialog.removeEventListener("close", handleClose);
        };
    }, [returnFocusRef]);

    return (
        <dialog
            ref={dialogRef}
            className={styles.dialog}
            aria-labelledby={titleId}
            aria-describedby={descriptionId}
            onCancel={(event) => {
                event.preventDefault();

                if (!busy) {
                    onCancel();
                }
            }}
        >
            <h2 id={titleId}>{title}</h2>
            <p id={descriptionId}>{description}</p>
            <div className={styles.actions}>
                <button type="button" className="buttonSecondary" autoFocus onClick={onCancel}>
                    {cancelLabel}
                </button>
                <button
                    type="button"
                    className={danger ? styles.danger : undefined}
                    aria-busy={busy}
                    disabled={busy}
                    onClick={() => {
                        if (!busy) {
                            onConfirm();
                        }
                    }}
                >
                    {busy ? "Изпълнява се…" : confirmLabel}
                </button>
            </div>
        </dialog>
    );
}
