import { useQueries, useQuery } from "@tanstack/react-query";

import { accountCourseProgressQueryOptions, courseLessonQueryOptions } from "./courseQueries";
import type { Course, CourseCatalog, CourseLessonSummary } from "../models/courseCatalog";
import type { ResolvedCourseProgress, ResolvedLessonProgress } from "../models/courseProgress";
import { lessonsNeededForCourse, resolveAvailability } from "../models/courseProgress";
import { readAnonymousCourseProgress } from "../storage/anonymousCourseProgress";

function emptyResolvedProgress(): ResolvedCourseProgress {
    return {
        lessons: new Map(),
        isPending: false,
        error: undefined,
        retry: () => undefined,
    };
}

function completedIdsForLesson(
    lesson: CourseLessonSummary,
    completedExerciseIds: ReadonlySet<string>,
    detailExercises: readonly { readonly id: string }[] | undefined,
): ReadonlySet<string> {
    if (lesson.progress !== undefined) {
        return new Set(lesson.progress.completedExerciseIds);
    }

    if (detailExercises === undefined) {
        return new Set();
    }

    return new Set(
        detailExercises.filter((exercise) => completedExerciseIds.has(exercise.id)).map((exercise) => exercise.id),
    );
}

function buildResolvedLessons(
    lessons: readonly CourseLessonSummary[],
    completedExerciseIds: ReadonlySet<string>,
    detailByLessonId: ReadonlyMap<string, readonly { readonly id: string }[]>,
    accountLessons: ReadonlyMap<string, { readonly completedExerciseCount: number; readonly isComplete: boolean }>,
): ReadonlyMap<string, ResolvedLessonProgress> {
    const completionByLesson = new Map<string, ResolvedLessonProgress>();

    for (const lesson of lessons) {
        const completedIds = completedIdsForLesson(lesson, completedExerciseIds, detailByLessonId.get(lesson.id));
        const accountLesson = accountLessons.get(lesson.key);
        const completedExerciseCount =
            lesson.progress?.completedExerciseCount ?? accountLesson?.completedExerciseCount ?? completedIds.size;
        const isComplete =
            lesson.progress?.completedExerciseCount !== undefined
                ? completedExerciseCount === lesson.exerciseCount && lesson.exerciseCount > 0
                : (accountLesson?.isComplete ??
                  (completedExerciseCount === lesson.exerciseCount && lesson.exerciseCount > 0));

        completionByLesson.set(lesson.key, {
            completedExerciseCount,
            completedExerciseIds: completedIds,
            isComplete,
            isAvailable: false,
        });
    }

    return new Map(
        lessons.map((lesson) => {
            const progress = completionByLesson.get(lesson.key)!;
            return [lesson.key, { ...progress, isAvailable: resolveAvailability(lesson, completionByLesson) }];
        }),
    );
}

export function useResolvedCourseProgress(
    catalog: CourseCatalog | undefined,
    course: Course | undefined,
    isAuthenticated: boolean,
): ResolvedCourseProgress {
    const accountQuery = useQuery(accountCourseProgressQueryOptions(isAuthenticated));
    const neededLessons = catalog === undefined || course === undefined ? [] : lessonsNeededForCourse(catalog, course);
    const anonymousQueries = useQueries({
        queries: neededLessons.map((lesson) => ({
            ...courseLessonQueryOptions(lesson.id),
            enabled: !isAuthenticated,
        })),
    });

    if (catalog === undefined || course === undefined) {
        return emptyResolvedProgress();
    }

    const accountLessons = new Map((accountQuery.data?.lessons ?? []).map((lesson) => [lesson.lessonKey, lesson]));
    const anonymousProgress = readAnonymousCourseProgress();
    const completedExerciseIds = new Set(
        isAuthenticated
            ? (accountQuery.data?.completedExerciseIds ?? [])
            : anonymousProgress.completedExercises.map((completion) => completion.exerciseId),
    );
    const detailByLessonId = new Map(
        anonymousQueries.flatMap((query, index) => {
            const detail = query.data;
            const lesson = neededLessons[index];
            return detail === undefined || lesson === undefined ? [] : [[lesson.id, detail.exercises] as const];
        }),
    );
    const lessons = buildResolvedLessons(neededLessons, completedExerciseIds, detailByLessonId, accountLessons);
    const failedQueries = anonymousQueries.filter((query) => query.isError);
    const isPending = isAuthenticated ? accountQuery.isPending : anonymousQueries.some((query) => query.isPending);
    const error = isAuthenticated ? accountQuery.error : failedQueries.at(0)?.error;
    const retry = (): void => {
        if (isAuthenticated) {
            if (accountQuery.isError) {
                void accountQuery.refetch();
            }
            return;
        }

        void Promise.all(failedQueries.map((query) => query.refetch()));
    };

    return { lessons, isPending, error, retry };
}
