import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";

import { anonymousProgressRequestForRiddle } from "../storage/anonymousRiddleProgress";
import { riddlesApi } from "./riddlesApi";

export const archivePageSize = 31;

export const riddleKeys = {
    all: () => ["riddles"] as const,
    week: () => ["riddles", "week"] as const,
    today: () => ["riddles", "today"] as const,
    play: (riddleId: string) => ["riddles", "play", riddleId] as const,
    playState: (riddleId: string) => ["riddles", "playState", riddleId] as const,
    archive: () => ["riddles", "archive"] as const,
    allProgress: () => ["riddles", "progress"] as const,
    progress: (fromDate: string, toDate: string) => ["riddles", "progress", fromDate, toDate] as const,
};

export function riddleWeekQueryOptions() {
    return queryOptions({
        queryKey: riddleKeys.week(),
        queryFn: riddlesApi.getWeek,
    });
}

export function todayRiddleQueryOptions() {
    return queryOptions({
        queryKey: riddleKeys.today(),
        queryFn: riddlesApi.getToday,
        retry: false,
    });
}

export function riddlePlayQueryOptions(riddleId: string) {
    return queryOptions({
        queryKey: riddleKeys.play(riddleId),
        queryFn: () => riddlesApi.getPlay(riddleId),
        retry: false,
    });
}

/**
 * Resume is modelled as a query even though it is a POST, because it is a read of permitted display state that needs
 * a request body to carry the anonymous snapshot. Commands write their result into this key with `setQueryData`.
 */
export function riddlePlayStateQueryOptions(riddleId: string, isAuthenticated: boolean) {
    return queryOptions({
        queryKey: riddleKeys.playState(riddleId),
        queryFn: () =>
            riddlesApi.resume(riddleId, isAuthenticated ? undefined : anonymousProgressRequestForRiddle(riddleId)),
        retry: false,
        staleTime: Number.POSITIVE_INFINITY,
    });
}

export function riddleArchiveInfiniteQueryOptions() {
    return infiniteQueryOptions({
        queryKey: riddleKeys.archive(),
        queryFn: ({ pageParam }) => riddlesApi.listArchive(pageParam, archivePageSize),
        initialPageParam: 1,
        getNextPageParam: (lastPage) =>
            lastPage.page * lastPage.pageSize < lastPage.totalCount ? lastPage.page + 1 : undefined,
    });
}

export function accountRiddleProgressQueryOptions(fromDate: string, toDate: string, enabled: boolean) {
    return queryOptions({
        queryKey: riddleKeys.progress(fromDate, toDate),
        queryFn: () => riddlesApi.listProgress(fromDate, toDate),
        enabled,
        retry: false,
    });
}
