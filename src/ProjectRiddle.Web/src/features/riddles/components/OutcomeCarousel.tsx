import { useState, type ReactElement, type ReactNode } from "react";

import { ClueTermText } from "../../../shared/components/ClueTermText";
import type { RiddleProgressStatus } from "../models/riddleProgress";
import styles from "./OutcomeCarousel.module.css";

export interface OutcomeCarouselCard {
    readonly id: string;
    readonly title: string;
    readonly body?: string;
}

export interface OutcomeCarouselProps {
    readonly status: RiddleProgressStatus;
    readonly answerAttemptCount: number;
    readonly explanation: string | undefined;
    readonly summaryBody?: string;
    readonly extraCards?: readonly OutcomeCarouselCard[];
    readonly footer?: ReactNode;
}

function solvedSummaryTitle(attemptCount: number): string {
    return attemptCount <= 1 ? "Позна я от раз!" : `Позна я. ${attemptCount} опита.`;
}

function buildCards(props: OutcomeCarouselProps): readonly OutcomeCarouselCard[] {
    const summary: OutcomeCarouselCard =
        props.status === "solved"
            ? {
                  id: "summary",
                  title: solvedSummaryTitle(props.answerAttemptCount),
                  body: props.summaryBody,
              }
            : {
                  id: "summary",
                  title: "Криптиката е разкрита",
                  body: "Разкри всички букви, без да я решиш.",
              };

    const cards: OutcomeCarouselCard[] = [summary];

    if (props.explanation !== undefined) {
        cards.push({ id: "explanation", title: "Обяснение", body: props.explanation });
    }

    if (props.extraCards !== undefined) {
        cards.push(...props.extraCards);
    }

    return cards;
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
            aria-label="Резултат от криптиката"
            data-solved={props.status === "solved" ? "" : undefined}
        >
            <div className={styles.viewport} role="status">
                <article
                    className={styles.card}
                    aria-roledescription="карта"
                    aria-label={`${position + 1} от ${cards.length}`}
                >
                    <h2 className={styles.title}>
                        <ClueTermText text={card.title} />
                    </h2>
                    {card.body === undefined ? null : (
                        <p className={styles.body}>
                            <ClueTermText text={card.body} />
                        </p>
                    )}
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
            {props.footer === undefined ? null : <div className={styles.footer}>{props.footer}</div>}
        </section>
    );
}
