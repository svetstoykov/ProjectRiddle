import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import { CardGridSkeleton } from "../../../shared/components/ContentSkeletons";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { sessionQueryOptions } from "../../auth/api/sessionQuery";
import { accountRiddleProgressQueryOptions, riddleArchiveInfiniteQueryOptions } from "../api/riddleQueries";
import { ArchiveCard } from "../components/ArchiveCard";
import { addLocalDays, formatMonth, monthKey } from "../models/localDate";
import type { PublicRiddleDiscoveryItem } from "../models/publicRiddle";
import styles from "./ArchivePage.module.css";

interface MonthGroup {
    readonly key: string;
    readonly label: string;
    readonly items: readonly PublicRiddleDiscoveryItem[];
}

/**
 * The server bounds an account-progress read to 366 inclusive days, so the span is clamped to the most recent year of
 * loaded pages rather than growing without limit.
 */
const maxProgressSpanDays = 365;

function groupByMonth(items: readonly PublicRiddleDiscoveryItem[]): readonly MonthGroup[] {
    const groups: MonthGroup[] = [];

    for (const item of items) {
        const key = monthKey(item.publicationDate);
        const current = groups.at(-1);

        if (current !== undefined && current.key === key) {
            groups[groups.length - 1] = { ...current, items: [...current.items, item] };
            continue;
        }

        groups.push({ key, label: formatMonth(item.publicationDate), items: [item] });
    }

    return groups;
}

export function ArchivePage(): ReactElement {
    const sessionQuery = useQuery(sessionQueryOptions);
    const archiveQuery = useInfiniteQuery(riddleArchiveInfiniteQueryOptions());
    const isAuthenticated = (sessionQuery.data ?? null) !== null;

    const items = archiveQuery.data?.pages.flatMap((page) => page.items) ?? [];
    const newest = items.at(0)?.publicationDate;
    const oldest = items.at(-1)?.publicationDate;
    const toDate = newest ?? "";
    const clampedFrom = newest === undefined ? "" : addLocalDays(newest, -maxProgressSpanDays);
    const fromDate = oldest === undefined || oldest < clampedFrom ? clampedFrom : oldest;

    const progressQuery = useQuery(
        accountRiddleProgressQueryOptions(fromDate, toDate, isAuthenticated && items.length > 0),
    );
    const outcomeByRiddleId = new Map(
        (progressQuery.data?.items ?? []).map((snapshot) => [snapshot.riddleId, snapshot.status]),
    );

    if (archiveQuery.isPending) {
        return (
            <>
                <DocumentTitle title="Архив" />
                <p className="visuallyHidden" role="status">
                    Зареждаме архива…
                </p>
                <CardGridSkeleton />
            </>
        );
    }

    if (archiveQuery.isError) {
        return (
            <>
                <DocumentTitle title="Архив" />
                <PageStatus
                    tone="error"
                    eyebrow="Архив"
                    title="Архивът временно не е достъпен"
                    message="Заявката не можа да бъде изпълнена."
                    action={{
                        label: "Опитай отново",
                        onClick: () => {
                            void archiveQuery.refetch();
                        },
                    }}
                />
            </>
        );
    }

    return (
        <section className={styles.page} aria-labelledby="archive-title">
            <DocumentTitle title="Архив" />
            <p className="eyebrow">Архив</p>
            <h1 id="archive-title">Предишни загадки</h1>
            {items.length === 0 ? (
                <p role="status">Все още няма архивни загадки.</p>
            ) : (
                groupByMonth(items).map((group) => (
                    <section key={group.key} className={styles.month} aria-label={group.label}>
                        <h2 className={styles.monthTitle}>{group.label}</h2>
                        <ul className={styles.cards}>
                            {group.items.map((item) => (
                                <li key={item.id}>
                                    <ArchiveCard
                                        item={item}
                                        outcome={outcomeByRiddleId.get(item.id)}
                                        isAuthenticated={isAuthenticated}
                                    />
                                </li>
                            ))}
                        </ul>
                    </section>
                ))
            )}
            {archiveQuery.hasNextPage ? (
                <button
                    type="button"
                    className={styles.loadMore}
                    aria-busy={archiveQuery.isFetchingNextPage}
                    disabled={archiveQuery.isFetchingNextPage}
                    onClick={() => {
                        void archiveQuery.fetchNextPage();
                    }}
                >
                    {archiveQuery.isFetchingNextPage ? "Зареждаме…" : "Покажи още"}
                </button>
            ) : null}
        </section>
    );
}
