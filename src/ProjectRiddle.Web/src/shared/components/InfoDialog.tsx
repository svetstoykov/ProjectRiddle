import { useEffect, useId, useRef, type ReactElement, type ReactNode, type RefObject } from "react";

import styles from "./InfoDialog.module.css";

export interface InfoDialogProps {
    readonly open: boolean;
    readonly title: string;
    readonly children: ReactNode;
    readonly onClose: () => void;
    readonly returnFocusRef: RefObject<HTMLElement | null>;
}

/**
 * The application's explanatory modal: a centred panel over a blurred backdrop. It carries what the reader needs to
 * read and at most a way onward, so dismissal leaves whatever they were looking at exactly where it was behind it.
 */
export function InfoDialog({ open, title, children, onClose, returnFocusRef }: InfoDialogProps): ReactElement {
    const dialogRef = useRef<HTMLDialogElement>(null);
    const titleId = useId();

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
            onCancel={(event) => {
                event.preventDefault();
                onClose();
            }}
            onClick={(event) => {
                // The panel fills the dialog box itself, so a click that lands on the dialog came from the backdrop.
                if (event.target === dialogRef.current) {
                    onClose();
                }
            }}
        >
            <div className={styles.panel}>
                <div className={styles.header}>
                    <h2 id={titleId} className={styles.title}>
                        {title}
                    </h2>
                    <button type="button" className={styles.close} aria-label="Затвори" onClick={onClose}>
                        ×
                    </button>
                </div>
                <div className={styles.body}>{children}</div>
            </div>
        </dialog>
    );
}
