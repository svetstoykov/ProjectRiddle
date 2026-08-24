import type { ReactElement } from "react";

import styles from "./CourseProgressPill.module.css";

export interface CourseProgressPillProps {
    readonly completed: number;
    readonly total: number;
}

export function CourseProgressPill({ completed, total }: CourseProgressPillProps): ReactElement {
    const bounded = total === 0 ? 0 : Math.max(0, Math.min(completed, total));
    const percentage = total === 0 ? 0 : (bounded / total) * 100;

    return (
        <div className={styles.progress} aria-label={`${bounded} от ${total} завършени`}>
            <div className={styles.track} aria-hidden="true">
                <span className={styles.completed} style={{ width: `${percentage}%` }} />
            </div>
            <span className={styles.count}>
                {bounded}/{total}
            </span>
        </div>
    );
}
