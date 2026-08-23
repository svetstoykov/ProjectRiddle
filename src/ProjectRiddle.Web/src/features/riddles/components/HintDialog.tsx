import { useEffect, useRef, type ReactElement, type RefObject } from "react";

import type { RiddleRangeKind } from "../models/riddleRange";
import styles from "./HintDialog.module.css";

export interface HintDialogProps {
    readonly open: boolean;
    readonly usedHints: readonly RiddleRangeKind[];
    readonly visibleHints: ReadonlySet<RiddleRangeKind>;
    readonly pendingHint: RiddleRangeKind | undefined;
    readonly revealedCount: number;
    readonly letterCount: number;
    readonly isRevealing: boolean;
    readonly disabled: boolean;
    readonly onUnlock: (kind: RiddleRangeKind) => void;
    readonly onToggle: (kind: RiddleRangeKind) => void;
    readonly onReveal: () => void;
    readonly onClose: () => void;
    readonly returnFocusRef: RefObject<HTMLElement | null>;
}

const hintOrder: readonly RiddleRangeKind[] = ["definition", "indicator", "fodder"];

const unlockLabels: Record<RiddleRangeKind, string> = {
    definition: "Покажи дефиницията",
    indicator: "Покажи индикатора",
    fodder: "Покажи материала",
};

const usedLabels: Record<RiddleRangeKind, string> = {
    definition: "Дефиниция",
    indicator: "Индикатор",
    fodder: "Материал",
};

const swatchClassNames: Record<RiddleRangeKind, string> = {
    definition: styles.definition ?? "definition",
    indicator: styles.indicator ?? "indicator",
    fodder: styles.fodder ?? "fodder",
};

/**
 * Presents every assist in one sheet that rises from the bottom edge over a clear backdrop, so the clue and the answer
 * tiles stay legible while it is open. Commands that change server state close the sheet, because their result is only
 * visible on the board.
 */
export function HintDialog({
    open,
    usedHints,
    visibleHints,
    pendingHint,
    revealedCount,
    letterCount,
    isRevealing,
    disabled,
    onUnlock,
    onToggle,
    onReveal,
    onClose,
    returnFocusRef,
}: HintDialogProps): ReactElement {
    const dialogRef = useRef<HTMLDialogElement>(null);
    const titleId = "hint-dialog-title";
    const revealExhausted = revealedCount >= letterCount;

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
                // The sheet fills the dialog box itself, so a click that lands on the dialog came from the backdrop.
                if (event.target === dialogRef.current) {
                    onClose();
                }
            }}
        >
            <div className={styles.sheet}>
                <div className={styles.header}>
                    <h2 id={titleId} className={styles.title}>
                        Подсказки
                    </h2>
                    <button type="button" className={styles.close} aria-label="Затвори подсказките" onClick={onClose}>
                        ×
                    </button>
                </div>
                <ul className={styles.hints}>
                    {hintOrder.map((kind) => {
                        const isUsed = usedHints.includes(kind);
                        const isPending = pendingHint === kind;
                        const isVisible = visibleHints.has(kind);

                        return (
                            <li key={kind}>
                                {isUsed ? (
                                    <button
                                        type="button"
                                        className={`${styles.row} ${styles.used}`}
                                        aria-pressed={isVisible}
                                        onClick={() => {
                                            onToggle(kind);
                                        }}
                                    >
                                        <span
                                            className={`${styles.swatch} ${swatchClassNames[kind]}`}
                                            aria-hidden="true"
                                        />
                                        <span className={styles.rowLabel}>{usedLabels[kind]}</span>
                                        <span className={styles.rowState}>{isVisible ? "показана" : "скрита"}</span>
                                    </button>
                                ) : (
                                    <button
                                        type="button"
                                        className={`${styles.row} ${styles.unlock}`}
                                        aria-busy={isPending}
                                        disabled={disabled || isPending}
                                        onClick={() => {
                                            onUnlock(kind);
                                            onClose();
                                        }}
                                    >
                                        <span
                                            className={`${styles.swatch} ${swatchClassNames[kind]}`}
                                            aria-hidden="true"
                                        />
                                        <span className={styles.rowLabel}>
                                            {isPending ? "Отключваме…" : unlockLabels[kind]}
                                        </span>
                                    </button>
                                )}
                            </li>
                        );
                    })}
                </ul>
                <hr className={styles.divider} />
                <button
                    type="button"
                    className={`${styles.row} ${styles.unlock}`}
                    aria-busy={isRevealing}
                    disabled={disabled || isRevealing || revealExhausted}
                    onClick={() => {
                        onReveal();
                        onClose();
                    }}
                >
                    <span className={styles.rowLabel}>{isRevealing ? "Разкриваме…" : "Разкрий буква"}</span>
                    <span className={styles.rowState}>
                        {revealedCount} от {letterCount}
                    </span>
                </button>
            </div>
        </dialog>
    );
}
