import type { ReactElement } from "react";

import type { RiddleProgressStatus } from "../models/riddleProgress";
import styles from "./RiddleOutcome.module.css";

export interface RiddleOutcomeProps {
    readonly status: RiddleProgressStatus;
    readonly lastAnswerIncorrect: boolean;
    readonly answerAttemptCount: number;
    readonly explanation: string | undefined;
}

export function RiddleOutcome({
    status,
    lastAnswerIncorrect,
    answerAttemptCount,
    explanation,
}: RiddleOutcomeProps): ReactElement | null {
    if (status === "solved") {
        return (
            <section className={[styles.outcome, styles.solved].join(" ")} role="status">
                <h2 className={styles.title}>Позна!</h2>
                <p className={styles.message}>Отговорът е верен след {answerAttemptCount} опита.</p>
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

    if (lastAnswerIncorrect) {
        return (
            <p className={styles.incorrect} role="status">
                Отговорът не е верен. Поправи буквите и опитай отново.
            </p>
        );
    }

    return null;
}
