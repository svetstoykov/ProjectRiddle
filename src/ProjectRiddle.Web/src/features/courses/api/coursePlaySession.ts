import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";

import { notifications } from "../../../shared/notifications/notifications";
import { currentProgressSchemaVersion } from "../../../shared/storage/progressStorage";
import type { RiddlePlayerView } from "../../riddles/models/publicRiddle";
import type { RiddlePlayerState } from "../../riddles/models/riddleProgress";
import type { RiddleRangeKind } from "../../riddles/models/riddleRange";
import type { CourseLessonExercise } from "../models/courseCatalog";
import type { AnonymousCourseExerciseProgressRequest, CoursePlayState } from "../models/coursePlay";
import { courseKeys, coursePlayStateQueryOptions } from "./courseQueries";
import { coursesApi } from "./coursesApi";
import {
    recordCourseCompletion,
    readAnonymousCourseProgress,
    terminalProgressRequest,
} from "../storage/anonymousCourseProgress";

export interface CoursePlaySession {
    readonly playState: CoursePlayState | undefined;
    readonly playerView: RiddlePlayerView | undefined;
    readonly playerState: RiddlePlayerState | undefined;
    readonly teachingNote: string | undefined;
    readonly error: unknown;
    readonly isPending: boolean;
    readonly pendingHint: RiddleRangeKind | undefined;
    readonly isSubmitting: boolean;
    readonly isRevealing: boolean;
    readonly submitAnswer: (answer: string) => void;
    readonly useHint: (kind: RiddleRangeKind) => void;
    readonly revealLetter: () => void;
    readonly retry: () => void;
}

function commandProgress(
    state: CoursePlayState | undefined,
    isAuthenticated: boolean,
): AnonymousCourseExerciseProgressRequest | undefined {
    if (isAuthenticated || state === undefined) {
        return undefined;
    }

    return {
        schemaVersion: currentProgressSchemaVersion,
        exerciseId: state.progress.exerciseId,
        status: state.progress.status,
        answerAttemptCount: state.progress.answerAttemptCount,
        usedHints: state.progress.usedHints,
        revealedPositions: state.progress.revealedPositions,
    };
}

export function useCoursePlaySession(
    exercise: CourseLessonExercise | undefined,
    isAuthenticated: boolean,
): CoursePlaySession {
    const queryClient = useQueryClient();
    const storedCompletion =
        exercise === undefined
            ? undefined
            : readAnonymousCourseProgress().completedExercises.find((item) => item.exerciseId === exercise.id);
    const resumeProgress =
        !isAuthenticated && exercise !== undefined && storedCompletion !== undefined
            ? terminalProgressRequest(exercise, storedCompletion.status)
            : undefined;
    const playStateQuery = useQuery({
        ...coursePlayStateQueryOptions(exercise?.id ?? "", resumeProgress),
        enabled: exercise !== undefined,
    });
    const previousExerciseIdRef = useRef<string | undefined>(undefined);

    useEffect(() => {
        const previousExerciseId = previousExerciseIdRef.current;

        if (previousExerciseId !== undefined && previousExerciseId !== exercise?.id) {
            queryClient.removeQueries({ queryKey: courseKeys.playState(previousExerciseId) });
        }

        previousExerciseIdRef.current = exercise?.id;
    }, [exercise?.id, queryClient]);

    function applyPlayState(state: CoursePlayState): void {
        if (exercise === undefined) {
            return;
        }

        queryClient.setQueryData(courseKeys.playState(exercise.id), state);

        if (!isAuthenticated && state.progress.status !== "inProgress") {
            recordCourseCompletion({ exerciseId: exercise.id, status: state.progress.status });
        }

        if (state.progress.status !== "inProgress") {
            void queryClient.invalidateQueries({ queryKey: courseKeys.catalog() });
            if (isAuthenticated) {
                void queryClient.invalidateQueries({ queryKey: courseKeys.progress() });
            }
        }
    }

    function handleCommandError(): void {
        notifications.error("Действието не може да бъде изпълнено. Опитай отново.");
    }

    const answerMutation = useMutation({
        mutationFn: (variables: {
            readonly exerciseId: string;
            readonly answer: string;
            readonly progress?: AnonymousCourseExerciseProgressRequest;
        }) => coursesApi.submitAnswer(variables.exerciseId, variables.answer, variables.progress),
        onSuccess: applyPlayState,
        onError: handleCommandError,
    });

    const hintMutation = useMutation({
        mutationFn: (variables: {
            readonly exerciseId: string;
            readonly kind: RiddleRangeKind;
            readonly progress?: AnonymousCourseExerciseProgressRequest;
        }) => coursesApi.useHint(variables.exerciseId, variables.kind, variables.progress),
        onSuccess: applyPlayState,
        onError: handleCommandError,
    });

    const revealMutation = useMutation({
        mutationFn: (variables: {
            readonly exerciseId: string;
            readonly progress?: AnonymousCourseExerciseProgressRequest;
        }) => coursesApi.revealLetter(variables.exerciseId, variables.progress),
        onSuccess: applyPlayState,
        onError: handleCommandError,
    });

    const playState = playStateQuery.data;
    const playerView: RiddlePlayerView | undefined =
        exercise === undefined
            ? undefined
            : {
                  id: exercise.id,
                  clue: exercise.clue,
                  answerPattern: exercise.answerPattern,
                  ranges: exercise.ranges,
              };
    const playerState: RiddlePlayerState | undefined =
        playState === undefined
            ? undefined
            : {
                  progress: playState.progress,
                  revealedLetters: playState.revealedLetters,
                  answer: playState.answer,
                  explanation: playState.explanation,
                  isCorrect: playState.isCorrect,
              };

    return {
        playState,
        playerView,
        playerState,
        teachingNote: playState?.teachingNote,
        error: exercise === undefined ? undefined : playStateQuery.error,
        isPending: exercise !== undefined && playStateQuery.isPending,
        pendingHint: hintMutation.isPending ? hintMutation.variables?.kind : undefined,
        isSubmitting: answerMutation.isPending,
        isRevealing: revealMutation.isPending,
        submitAnswer: (answer: string) => {
            if (exercise !== undefined) {
                answerMutation.mutate({
                    exerciseId: exercise.id,
                    answer,
                    progress: commandProgress(playStateQuery.data, isAuthenticated),
                });
            }
        },
        useHint: (kind: RiddleRangeKind) => {
            if (exercise !== undefined) {
                hintMutation.mutate({
                    exerciseId: exercise.id,
                    kind,
                    progress: commandProgress(playStateQuery.data, isAuthenticated),
                });
            }
        },
        revealLetter: () => {
            if (exercise !== undefined) {
                revealMutation.mutate({
                    exerciseId: exercise.id,
                    progress: commandProgress(playStateQuery.data, isAuthenticated),
                });
            }
        },
        retry: () => {
            if (exercise !== undefined) {
                void playStateQuery.refetch();
            }
        },
    };
}
