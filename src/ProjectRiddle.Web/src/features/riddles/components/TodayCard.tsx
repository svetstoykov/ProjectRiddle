import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import { formatFullDate } from "../models/localDate";
import type { PublicRiddlePlay } from "../models/publicRiddle";
import type { RiddleRangeKind } from "../models/riddleRange";
import { CluePresentation } from "./CluePresentation";
import styles from "./TodayCard.module.css";

export interface TodayCardProps {
    readonly today: PublicRiddlePlay | undefined;
    readonly isUnavailable: boolean;
}

const noHints: ReadonlySet<RiddleRangeKind> = new Set();

export function TodayCard({ today, isUnavailable }: TodayCardProps): ReactElement {
    if (isUnavailable || today === undefined) {
        return (
            <section className={styles.card} aria-labelledby="today-title">
                <p className="eyebrow">Днешната криптика</p>
                <h1 id="today-title" className={styles.heading}>
                    Днес няма криптика.
                </h1>
                <p className={styles.lead}>Виж архива, докато чакаш.</p>
                <Link className={`button ${styles.play}`} to="/archive">
                    Към архива
                </Link>
            </section>
        );
    }

    return (
        <section className={styles.card} aria-labelledby="today-title">
            <p className="eyebrow">Днешната криптика</p>
            <h1 id="today-title" className={styles.heading}>
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
