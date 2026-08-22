import type { ReactElement } from "react";
import { Link } from "react-router-dom";

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
                <p className="eyebrow">Днешната загадка</p>
                <h1 id="today-title">Днес няма публикувана загадка</h1>
                <p className={styles.lead}>Разгледай архива, докато чакаш следващата.</p>
                <Link className="button" to="/archive">
                    Към архива
                </Link>
            </section>
        );
    }

    return (
        <section className={styles.card} aria-labelledby="today-title">
            <p className="eyebrow">Днешната загадка</p>
            <h1 id="today-title">Загадката за днес</h1>
            <CluePresentation
                clue={today.clue}
                answerPattern={today.answerPattern}
                ranges={today.ranges}
                activeKinds={noHints}
            />
            <Link className="button" to="/riddles/today">
                Играй
            </Link>
        </section>
    );
}
