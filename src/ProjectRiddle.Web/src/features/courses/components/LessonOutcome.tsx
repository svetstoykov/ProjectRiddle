import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import styles from "./LessonOutcome.module.css";

export interface LessonOutcomeProps {
    readonly completed: number;
    readonly total: number;
    readonly message: string;
    readonly teachingNote: string | undefined;
    readonly actionLabel: string;
    readonly actionTo: string;
}

export function LessonOutcome({
    completed,
    total,
    message,
    teachingNote,
    actionLabel,
    actionTo,
}: LessonOutcomeProps): ReactElement {
    return (
        <section className={styles.outcome} aria-label="Напредък в урока">
            <p className={styles.progress}>
                {completed} от {total} задачи завършени
            </p>
            <p>{message}</p>
            {teachingNote === undefined ? null : <p className={styles.note}>{teachingNote}</p>}
            <Link className="button" to={actionTo}>
                {actionLabel}
            </Link>
        </section>
    );
}
