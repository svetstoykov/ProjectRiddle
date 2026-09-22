import type { ReactElement, ReactNode } from "react";

import { ClueTermText } from "../../../shared/components/ClueTermText";
import { outcomeHeadline, revealedOutcomeLead, type RiddleOutcomeStatus } from "../../riddles/models/outcomeMessages";
import styles from "./IntroductoryOutcome.module.css";

export interface IntroductoryOutcomeProps {
    readonly status: RiddleOutcomeStatus;
    readonly fodder: string | undefined;
    readonly answer: string | undefined;
    /** How the two halves of the clue meet in the answer. It stands in for the shorter explanation when present. */
    readonly teachingNote: string | undefined;
    readonly explanation: string | undefined;
    readonly footer: ReactNode;
}

const upper = (text: string): string => text.toLocaleUpperCase("bg");

/**
 * The first practice ends on the whole explanation at once instead of a deck to page through: the letters moving into
 * the answer, then how the other half of the clue describes it, then the way onward.
 */
export function IntroductoryOutcome({
    status,
    fodder,
    answer,
    teachingNote,
    explanation,
    footer,
}: IntroductoryOutcomeProps): ReactElement {
    const note = teachingNote ?? explanation;

    return (
        <section
            className={styles.outcome}
            aria-labelledby="introductory-outcome-title"
            data-solved={status === "solved" ? "" : undefined}
        >
            <div className={styles.card} role="status">
                <h2 id="introductory-outcome-title" className={styles.title}>
                    {outcomeHeadline(status)}
                </h2>
                {status === "fullyRevealed" ? <p>{revealedOutcomeLead}</p> : null}
                {fodder === undefined || answer === undefined ? null : (
                    <p className={styles.transform}>
                        <span className={styles.word}>{upper(fodder)}</span>
                        <span className={styles.arrow} aria-hidden="true">
                            →
                        </span>
                        <span className="visuallyHidden">, пренаредено, дава </span>
                        <span className={styles.word}>{upper(answer)}</span>
                    </p>
                )}
                {note === undefined ? null : (
                    <p className={styles.note}>
                        <ClueTermText text={note} />
                    </p>
                )}
            </div>
            <div className={styles.footer}>{footer}</div>
        </section>
    );
}
