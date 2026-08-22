import type { ReactElement } from "react";

import styles from "./RevealControl.module.css";

export interface RevealControlProps {
    readonly revealedCount: number;
    readonly letterCount: number;
    readonly disabled: boolean;
    readonly isRevealing: boolean;
    readonly onReveal: () => void;
}

export function RevealControl({
    revealedCount,
    letterCount,
    disabled,
    isRevealing,
    onReveal,
}: RevealControlProps): ReactElement {
    return (
        <div className={styles.control}>
            <button
                type="button"
                className={styles.reveal}
                aria-busy={isRevealing}
                disabled={disabled || isRevealing || revealedCount >= letterCount}
                onClick={onReveal}
            >
                {isRevealing ? "Разкриваме…" : "Разкрий буква"}
            </button>
            <p className={styles.counter}>
                Разкрити букви: {revealedCount} от {letterCount}
            </p>
        </div>
    );
}
