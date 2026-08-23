import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import { accountRiddleProgressQueryOptions } from "../api/riddleQueries";
import { addLocalDays, compareLocalDates, formatDayNumber, formatFullDate, formatWeekday } from "../models/localDate";
import type { PublicRiddleWeek } from "../models/publicRiddle";
import type { RiddleProgressStatus } from "../models/riddleProgress";
import styles from "./WeekStrip.module.css";

export interface WeekStripProps {
    readonly week: PublicRiddleWeek;
    readonly isAuthenticated: boolean;
}

type DayMarker = "locked" | "solved" | "started" | "none";

const outcomeLabels: Record<RiddleProgressStatus, string> = {
    inProgress: "започната",
    fullyRevealed: "разкрита",
    solved: "решена",
};

/**
 * The marker distinguishes days by shape as well as colour: a lock for account-only days, a tick for solved ones and
 * a dot for days already started.
 */
function DayMark({ marker }: { readonly marker: DayMarker }): ReactElement | null {
    if (marker === "locked") {
        return (
            <svg className={styles.mark} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                <rect x="5" y="10.5" width="14" height="10" rx="2" />
                <path d="M8.5 10.5V7.75a3.5 3.5 0 0 1 7 0v2.75" />
            </svg>
        );
    }

    if (marker === "solved") {
        return (
            <svg
                className={`${styles.mark} ${styles.solvedMark}`}
                viewBox="0 0 24 24"
                aria-hidden="true"
                focusable="false"
            >
                <path d="m5 12.5 4.5 4.5L19 7.5" />
            </svg>
        );
    }

    if (marker === "started") {
        return <span className={styles.dot} aria-hidden="true" />;
    }

    return null;
}

export function WeekStrip({ week, isAuthenticated }: WeekStripProps): ReactElement {
    const progressQuery = useQuery(accountRiddleProgressQueryOptions(week.weekStart, week.weekEnd, isAuthenticated));

    const days = Array.from({ length: 7 }, (_unused, offset) => addLocalDays(week.weekStart, offset));
    const itemsByDate = new Map(week.items.map((item) => [item.publicationDate, item]));
    const outcomeByRiddleId = new Map(
        (progressQuery.data?.items ?? []).map((snapshot) => [snapshot.riddleId, snapshot.status]),
    );

    function renderDay(day: string): ReactElement {
        const item = itemsByDate.get(day);
        const isFuture = compareLocalDates(day, week.today) > 0;
        const isToday = day === week.today;

        const head = (
            <>
                <span className={styles.weekday}>{formatWeekday(day)}</span>
                <span className={styles.dayNumber}>{formatDayNumber(day)}</span>
            </>
        );

        if (isFuture) {
            return (
                <div className={[styles.day, styles.future].join(" ")}>
                    {head}
                    <span className="visuallyHidden">{`${formatFullDate(day)}: предстои`}</span>
                </div>
            );
        }

        if (item === undefined) {
            return (
                <div className={[styles.day, styles.gap].join(" ")}>
                    {head}
                    <span className="visuallyHidden">{`${formatFullDate(day)}: няма загадка`}</span>
                </div>
            );
        }

        // Every day other than today is account-only, so a signed-out visitor goes straight to the access notice
        // instead of a request whose outcome is already known.
        const requiresAccount = !isToday && !isAuthenticated;
        const outcome = outcomeByRiddleId.get(item.id);
        const marker: DayMarker = requiresAccount
            ? "locked"
            : outcome === "solved"
              ? "solved"
              : outcome === undefined
                ? "none"
                : "started";
        const state = requiresAccount
            ? "изисква профил"
            : isToday
              ? "днешната загадка"
              : outcome === undefined
                ? "неиграна"
                : outcomeLabels[outcome];

        return (
            <Link
                className={[
                    styles.day,
                    styles.available,
                    isToday ? styles.today : undefined,
                    requiresAccount ? styles.locked : undefined,
                ].join(" ")}
                to={isToday ? "/riddles/today" : `/riddles/${item.id}`}
                state={requiresAccount ? { requiresAccount: true } : undefined}
                aria-label={`${formatFullDate(day)}: ${state}`}
            >
                {head}
                <DayMark marker={marker} />
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
            <p className={styles.footnote}>
                {isAuthenticated
                    ? "Всеки ден от седмицата остава достъпен в профила ти."
                    : "Днешната загадка е свободна. По-ранните дни се играят с профил."}
            </p>
        </section>
    );
}
