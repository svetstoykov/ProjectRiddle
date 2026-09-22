import type { ReactElement } from "react";

import styles from "./IntroductoryGuide.module.css";

/**
 * One sentence above the first practice clue: that this is practice, and the method every cryptic shares. The next
 * step for this clue sits under the hint button instead, where the solver is already looking, so the board keeps its
 * room on a phone.
 */
export function IntroductoryGuide(): ReactElement {
    return (
        <p className={styles.guide}>
            <strong className={styles.label}>Упражнение:</strong> една част от уликата описва отговора, другата — как да
            го сглобиш.
        </p>
    );
}
