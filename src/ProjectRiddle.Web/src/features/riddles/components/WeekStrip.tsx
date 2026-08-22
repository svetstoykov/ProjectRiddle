import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import { accountRiddleProgressQueryOptions } from "../api/riddleQueries";
import { addLocalDays, compareLocalDates, formatDayNumber, formatFullDate, formatWeekday } from "../models/localDate";
import type { PublicRiddleDiscoveryItem, PublicRiddleWeek } from "../models/publicRiddle";
import type { RiddleProgressStatus } from "../models/riddleProgress";
import styles from "./WeekStrip.module.css";

export interface WeekStripProps {
    readonly week: PublicRiddleWeek;
    readonly isAuthenticated: boolean;
}

const outcomeLabels: Record<RiddleProgressStatus, string> = {
    inProgress: "започната",
    fullyRevealed: "разкрита",
    solved: "решена",
};

export function WeekStrip({ week, isAuthenticated }: WeekStripProps): ReactElement {
    const progressQuery = useQuery(accountRiddleProgressQueryOptions(week.weekStart, week.weekEnd, isAuthenticated));

    const days = Array.from({ length: 7 }, (_unused, offset) => addLocalDays(week.weekStart, offset));
    const itemsByDate = new Map(week.items.map((item) => [item.publicationDate, item]));
    const outcomeByRiddleId = new Map(
        (progressQuery.data?.items ?? []).map((snapshot) => [snapshot.riddleId, snapshot.status]),
    );

    function renderDay(day: string): ReactElement {
        const item: PublicRiddleDiscoveryItem | undefined = itemsByDate.get(day);
        const isFuture = compareLocalDates(day, week.today) > 0;
        const isToday = day === week.today;
        const outcome = item === undefined ? undefined : outcomeByRiddleId.get(item.id);

        const head = (
            <>
                <span className={styles.weekday}>{formatWeekday(day)}</span>
                <span className={styles.dayNumber}>{formatDayNumber(day)}</span>
            </>
        );

        if (isFuture) {
            return (
                <div className={[styles.day, styles.future].join(" ")} aria-disabled="true">
                    {head}
                    <span className={styles.note}>предстои</span>
                </div>
            );
        }

        if (item === undefined) {
            return (
                <div className={[styles.day, styles.gap].join(" ")}>
                    {head}
                    <span className={styles.note}>няма загадка</span>
                </div>
            );
        }

        const label = `${formatFullDate(day)}: ${item.clueExcerpt}. Брой букви ${item.answerPattern}${
            outcome === undefined ? "" : `. Състояние: ${outcomeLabels[outcome]}`
        }`;

        return (
            <Link
                className={[styles.day, styles.available, isToday ? styles.today : undefined].join(" ")}
                to={isToday ? "/riddles/today" : `/riddles/${item.id}`}
                state={!isToday && !isAuthenticated ? { requiresAccount: true } : undefined}
                aria-label={label}
            >
                {head}
                <span className={styles.excerpt}>{item.clueExcerpt}</span>
                <span className={styles.pattern}>{item.answerPattern}</span>
                {outcome === undefined ? null : <span className={styles.outcome}>{outcomeLabels[outcome]}</span>}
            </Link>
        );
    }

    return (
        <section className={styles.strip} aria-labelledby="week-title">
            <h2 id="week-title" className={styles.title}>
                Тази седмица
            </h2>
            <ol className={styles.days}>
                {days.map((day) => (
                    <li key={day} className={styles.dayItem}>
                        {renderDay(day)}
                    </li>
                ))}
            </ol>
        </section>
    );
}
