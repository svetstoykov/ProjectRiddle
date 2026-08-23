import { useState, type ReactElement } from "react";

import type { RiddleProgressStatus } from "../models/riddleProgress";
import styles from "./OutcomeCarousel.module.css";

export interface OutcomeCarouselProps {
    readonly status: RiddleProgressStatus;
    readonly answerAttemptCount: number;
    readonly explanation: string | undefined;
}

interface OutcomeCard {
    readonly id: string;
    readonly title: string;
    readonly body?: string;
}

function solvedSummaryTitle(attemptCount: number): string {
    return attemptCount <= 1 ? "Правилен отговор!" : `Правилен отговор! ${attemptCount} опита.`;
}

function buildCards(props: OutcomeCarouselProps): readonly OutcomeCard[] {
    const summary: OutcomeCard =
        props.status === "solved"
            ? {
                  id: "summary",
                  title: solvedSummaryTitle(props.answerAttemptCount),
              }
            : {
                  id: "summary",
                  title: "Загадката е разкрита",
                  body: "Всички букви са разкрити, без загадката да бъде решена.",
              };

    if (props.explanation === undefined) {
        return [summary];
    }

    return [summary, { id: "explanation", title: "Обяснение", body: props.explanation }];
}

/**
 * The finished riddle reads as a small deck in the space the assists and the check button leave behind: the result
 * first, then how the clue worked. More cards can follow the same shape without changing the surrounding board.
 */
export function OutcomeCarousel(props: OutcomeCarouselProps): ReactElement | null {
    const cards = buildCards(props);
    const [index, setIndex] = useState(0);
    // A finished riddle can gain its explanation on a later read, so the position is clamped instead of synchronized.
    const position = Math.min(index, cards.length - 1);
    const card = cards[position];

    if (props.status === "inProgress" || card === undefined) {
        return null;
    }

    return (
        <section
            className={styles.carousel}
            aria-roledescription="въртележка"
            aria-label="Резултат от загадката"
            data-solved={props.status === "solved" ? "" : undefined}
        >
            <div className={styles.viewport} role="status">
                <article
                    className={styles.card}
                    aria-roledescription="карта"
                    aria-label={`${position + 1} от ${cards.length}`}
                >
                    <h2 className={styles.title}>{card.title}</h2>
                    {card.body === undefined ? null : <p className={styles.body}>{card.body}</p>}
                </article>
            </div>
            {cards.length > 1 ? (
                <div className={styles.controls}>
                    <button
                        type="button"
                        className={styles.step}
                        aria-label="Предишна карта"
                        disabled={position === 0}
                        onClick={() => {
                            setIndex(position - 1);
                        }}
                    >
                        ‹
                    </button>
                    <ul className={styles.dots}>
                        {cards.map((item, dotIndex) => (
                            <li key={item.id}>
                                <button
                                    type="button"
                                    className={styles.dot}
                                    aria-label={item.title}
                                    aria-current={dotIndex === position ? "true" : undefined}
                                    onClick={() => {
                                        setIndex(dotIndex);
                                    }}
                                />
                            </li>
                        ))}
                    </ul>
                    <button
                        type="button"
                        className={styles.step}
                        aria-label="Следваща карта"
                        disabled={position === cards.length - 1}
                        onClick={() => {
                            setIndex(position + 1);
                        }}
                    >
                        ›
                    </button>
                </div>
            ) : null}
        </section>
    );
}
