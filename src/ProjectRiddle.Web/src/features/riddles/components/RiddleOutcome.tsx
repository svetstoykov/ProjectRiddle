import type { ReactElement } from "react";

import type { RiddleProgressStatus } from "../models/riddleProgress";
import styles from "./RiddleOutcome.module.css";

export interface RiddleOutcomeProps {
    readonly status: RiddleProgressStatus;
    readonly answerAttemptCount: number;
    readonly explanation: string | undefined;
}

function attemptPhrase(count: number): string {
    return count === 1 ? "след 1 опит" : `след ${count} опита`;
}

export function RiddleOutcome({ status, answerAttemptCount, explanation }: RiddleOutcomeProps): ReactElement | null {
    if (status === "solved") {
        return (
            <section className={styles.solved} role="status">
                <h2 className={styles.title}>Позна!</h2>
                <p className={styles.message}>Отговорът е верен {attemptPhrase(answerAttemptCount)}.</p>
                {explanation !== undefined ? <p className={styles.explanation}>{explanation}</p> : null}
            </section>
        );
    }

    if (status === "fullyRevealed") {
        return (
            <section className={[styles.outcome, styles.revealed].join(" ")} role="status">
                <h2 className={styles.title}>Загадката е разкрита</h2>
                <p className={styles.message}>Всички букви са разкрити, без загадката да бъде решена.</p>
                {explanation !== undefined ? <p className={styles.explanation}>{explanation}</p> : null}
            </section>
        );
    }

    return null;
}
