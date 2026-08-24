import type { ReactElement } from "react";

import styles from "./PrimerFigure.module.css";

export interface PrimerFigureProps {
    readonly figureKey: string | undefined;
}

export function PrimerFigure({ figureKey }: PrimerFigureProps): ReactElement | null {
    if (figureKey !== "clue-anatomy") {
        return null;
    }

    return (
        <figure className={styles.figure} aria-label="Дефиниция, материал и индикатор в примерна улика">
            <span className={styles.indicator}>Индикатор</span>
            <span className={styles.fodder}>Материал</span>
            <span className={styles.definition}>Дефиниция</span>
        </figure>
    );
}
