import { queryOptions } from "@tanstack/react-query";

import type { AnonymousCourseExerciseProgressRequest } from "../models/coursePlay";
import { coursesApi } from "./coursesApi";

export const courseKeys = {
    all: () => ["courses"] as const,
    catalog: () => ["courses", "catalog"] as const,
    primer: () => ["courses", "primer"] as const,
    lesson: (lessonId: string) => ["courses", "lesson", lessonId] as const,
    playState: (exerciseId: string) => ["courses", "playState", exerciseId] as const,
    progress: () => ["courses", "progress"] as const,
};

export const courseCatalogQueryOptions = () =>
    queryOptions({ queryKey: courseKeys.catalog(), queryFn: coursesApi.getCatalog, retry: false });

export const coursePrimerQueryOptions = () =>
    queryOptions({ queryKey: courseKeys.primer(), queryFn: coursesApi.getPrimer, retry: false });

export const courseLessonQueryOptions = (lessonId: string) =>
    queryOptions({
        queryKey: courseKeys.lesson(lessonId),
        queryFn: () => coursesApi.getLesson(lessonId),
        retry: false,
    });

export const accountCourseProgressQueryOptions = (enabled: boolean) =>
    queryOptions({ queryKey: courseKeys.progress(), queryFn: coursesApi.getProgress, enabled, retry: false });

export const coursePlayStateQueryOptions = (exerciseId: string, progress?: AnonymousCourseExerciseProgressRequest) =>
    queryOptions({
        queryKey: courseKeys.playState(exerciseId),
        queryFn: () => coursesApi.resume(exerciseId, progress),
        retry: false,
        staleTime: Number.POSITIVE_INFINITY,
    });
