import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";

import { isApplicationError } from "../../../shared/api/errors";
import { notifications } from "../../../shared/notifications/notifications";
import { sessionQueryOptions } from "../../auth/api/sessionQuery";
import type { PublicRiddlePlay } from "../models/publicRiddle";
import type { RiddlePlayState } from "../models/riddleProgress";
import type { RiddleRangeKind } from "../models/riddleRange";
import {
    anonymousProgressRequestForRiddle,
    evictAnonymousProgressForOtherRiddle,
    writeAnonymousRiddleProgress,
} from "../storage/anonymousRiddleProgress";
import {
    riddleKeys,
    riddlePlayQueryOptions,
    riddlePlayStateQueryOptions,
    todayRiddleQueryOptions,
} from "./riddleQueries";
import { riddlesApi } from "./riddlesApi";

export type RiddlePlayStatus =
    "pending" | "ready" | "todayUnavailable" | "notFound" | "authenticationRequired" | "failed";

export interface RiddlePlaySession {
    readonly status: RiddlePlayStatus;
    readonly play: PublicRiddlePlay | undefined;
    readonly playState: RiddlePlayState | undefined;
    readonly traceId: string | undefined;
    readonly pendingHint: RiddleRangeKind | undefined;
    readonly isSubmitting: boolean;
    readonly isRevealing: boolean;
    readonly submitAnswer: (answer: string) => void;
    readonly useHint: (kind: RiddleRangeKind) => void;
    readonly revealLetter: () => void;
    readonly retry: () => void;
}

function errorCode(error: unknown): string | undefined {
    return isApplicationError(error) ? error.code : undefined;
}

function requiresAccount(error: unknown): boolean {
    return isApplicationError(error) && error.status === 401;
}

export function useRiddlePlaySession(riddleId: string | undefined): RiddlePlaySession {
    const queryClient = useQueryClient();
    const sessionQuery = useQuery(sessionQueryOptions);
    const isAuthenticated = (sessionQuery.data ?? null) !== null;
    const [hasRolledOver, setHasRolledOver] = useState(false);

    const todayQuery = useQuery({ ...todayRiddleQueryOptions(), enabled: riddleId === undefined });
    const playQuery = useQuery({ ...riddlePlayQueryOptions(riddleId ?? ""), enabled: riddleId !== undefined });

    const play = riddleId === undefined ? todayQuery.data : playQuery.data;
    const projectionError = riddleId === undefined ? todayQuery.error : playQuery.error;
    const activeRiddleId = play?.id;

    const playStateQuery = useQuery({
        ...riddlePlayStateQueryOptions(activeRiddleId ?? "", isAuthenticated),
        enabled: activeRiddleId !== undefined,
    });

    // Discard a record that describes a different riddle before the new board accepts input. The read path already
    // refuses to post a mismatched snapshot, so this only clears bytes that are no longer anyone's history.
    useEffect(() => {
        if (activeRiddleId === undefined || isAuthenticated) {
            return;
        }

        evictAnonymousProgressForOtherRiddle(activeRiddleId);
    }, [activeRiddleId, isAuthenticated]);

    function applyPlayState(state: RiddlePlayState): void {
        if (activeRiddleId === undefined) {
            return;
        }

        queryClient.setQueryData(riddleKeys.playState(activeRiddleId), state);

        if (!isAuthenticated) {
            writeAnonymousRiddleProgress(state.progress);
        }

        if (state.progress.status !== "inProgress") {
            void queryClient.invalidateQueries({ queryKey: riddleKeys.week() });
            void queryClient.invalidateQueries({ queryKey: riddleKeys.allProgress() });
        }
    }

    function handleCommandError(error: unknown): void {
        if (requiresAccount(error)) {
            setHasRolledOver(true);
            return;
        }

        notifications.error("Нещо се обърка от наша страна. Пробвай пак.");
    }

    const answerMutation = useMutation({
        mutationFn: (variables: { readonly riddleId: string; readonly answer: string }) =>
            riddlesApi.submitAnswer(
                variables.riddleId,
                variables.answer,
                isAuthenticated ? undefined : anonymousProgressRequestForRiddle(variables.riddleId),
            ),
        onSuccess: applyPlayState,
        onError: handleCommandError,
    });

    const hintMutation = useMutation({
        mutationFn: (variables: { readonly riddleId: string; readonly kind: RiddleRangeKind }) =>
            riddlesApi.useHint(
                variables.riddleId,
                variables.kind,
                isAuthenticated ? undefined : anonymousProgressRequestForRiddle(variables.riddleId),
            ),
        onSuccess: applyPlayState,
        onError: handleCommandError,
    });

    const revealMutation = useMutation({
        mutationFn: (variables: { readonly riddleId: string }) =>
            riddlesApi.revealLetter(
                variables.riddleId,
                isAuthenticated ? undefined : anonymousProgressRequestForRiddle(variables.riddleId),
            ),
        onSuccess: applyPlayState,
        onError: handleCommandError,
    });

    const failure = projectionError ?? playStateQuery.error;

    function resolveStatus(): RiddlePlayStatus {
        if (hasRolledOver || requiresAccount(projectionError) || requiresAccount(playStateQuery.error)) {
            return "authenticationRequired";
        }

        if (errorCode(projectionError) === "riddles.today.unavailable") {
            return "todayUnavailable";
        }

        if (errorCode(failure) === "riddles.notFound") {
            return "notFound";
        }

        if (failure !== null && failure !== undefined) {
            return "failed";
        }

        return play !== undefined && playStateQuery.data !== undefined ? "ready" : "pending";
    }

    return {
        status: resolveStatus(),
        play,
        playState: playStateQuery.data,
        traceId: isApplicationError(failure) ? failure.traceId : undefined,
        pendingHint: hintMutation.isPending ? hintMutation.variables?.kind : undefined,
        isSubmitting: answerMutation.isPending,
        isRevealing: revealMutation.isPending,
        submitAnswer: (answer: string) => {
            if (activeRiddleId !== undefined) {
                answerMutation.mutate({ riddleId: activeRiddleId, answer });
            }
        },
        useHint: (kind: RiddleRangeKind) => {
            if (activeRiddleId !== undefined) {
                hintMutation.mutate({ riddleId: activeRiddleId, kind });
            }
        },
        revealLetter: () => {
            if (activeRiddleId !== undefined) {
                revealMutation.mutate({ riddleId: activeRiddleId });
            }
        },
        retry: () => {
            void (riddleId === undefined ? todayQuery.refetch() : playQuery.refetch());
            void playStateQuery.refetch();
        },
    };
}
