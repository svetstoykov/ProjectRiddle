import type { ReactElement, RefObject } from "react";
import { Link } from "react-router-dom";

import { formatFullDate } from "../models/localDate";
import type { PublicRiddlePlay } from "../models/publicRiddle";
import type { RiddleRangeKind } from "../models/riddleRange";
import type { TodayAlternatives } from "../models/todayAlternatives";
import { CluePresentation } from "./CluePresentation";
import styles from "./TodayCard.module.css";

export interface TodayCardProps {
    readonly today: PublicRiddlePlay | undefined;
    readonly isUnavailable: boolean;
    readonly alternatives: TodayAlternatives;
    /** Where focus lands when the home invitation closes over this card. */
    readonly headingRef?: RefObject<HTMLHeadingElement | null>;
}

const noHints: ReadonlySet<RiddleRangeKind> = new Set();

export function TodayCard({ today, isUnavailable, alternatives, headingRef }: TodayCardProps): ReactElement {
    if (isUnavailable || today === undefined) {
        return (
            <section className={styles.card} aria-labelledby="today-title">
                <p className="eyebrow">Днешната криптика</p>
                <h1 id="today-title" ref={headingRef} tabIndex={-1} className={styles.heading}>
                    Днес няма криптика.
                </h1>
                <p className={styles.lead}>{alternatives.message}</p>
                <div className={styles.actions}>
                    <Link className="button" to={alternatives.primary.to}>
                        {alternatives.primary.label}
                    </Link>
                    <Link className="button buttonSecondary" to={alternatives.secondary.to}>
                        {alternatives.secondary.label}
                    </Link>
                </div>
            </section>
        );
    }

    return (
        <section className={styles.card} aria-labelledby="today-title">
            <p className="eyebrow">Днешната криптика</p>
            <h1 id="today-title" ref={headingRef} tabIndex={-1} className={styles.heading}>
                {formatFullDate(today.publicationDate)}
            </h1>
            <CluePresentation
                clue={today.clue}
                answerPattern={today.answerPattern}
                ranges={today.ranges}
                activeKinds={noHints}
            />
            <Link className={`button ${styles.play}`} to="/riddles/today">
                Играй
            </Link>
        </section>
    );
}
