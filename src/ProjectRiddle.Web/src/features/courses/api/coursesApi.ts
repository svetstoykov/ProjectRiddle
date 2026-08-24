import { request } from "../../../shared/api/client";
import type { RiddleRangeKind } from "../../riddles/models/riddleRange";
import type { AccountCourseProgress, CourseCatalog, CourseLessonDetail, CoursePrimer } from "../models/courseCatalog";
import type { AnonymousCourseExerciseProgressRequest, CoursePlayState } from "../models/coursePlay";

interface CourseCommandBody {
    readonly progress?: AnonymousCourseExerciseProgressRequest;
}

interface CourseAnswerBody extends CourseCommandBody {
    readonly answer: string;
}

interface CourseHintBody extends CourseCommandBody {
    readonly kind: RiddleRangeKind;
}

export const coursesApi = {
    getCatalog: (): Promise<CourseCatalog> => request<CourseCatalog>({ method: "GET", url: "/api/courses" }),
    getPrimer: (): Promise<CoursePrimer> => request<CoursePrimer>({ method: "GET", url: "/api/courses/primer" }),
    getLesson: (lessonId: string): Promise<CourseLessonDetail> =>
        request<CourseLessonDetail>({ method: "GET", url: `/api/courses/lessons/${lessonId}` }),
    getProgress: (): Promise<AccountCourseProgress> =>
        request<AccountCourseProgress>({ method: "GET", url: "/api/courses/progress" }),
    resume: (exerciseId: string, progress?: AnonymousCourseExerciseProgressRequest): Promise<CoursePlayState> =>
        request<CoursePlayState>({
            method: "POST",
            url: `/api/courses/exercises/${exerciseId}/resume`,
            data: { progress },
        }),
    submitAnswer: (
        exerciseId: string,
        answer: string,
        progress?: AnonymousCourseExerciseProgressRequest,
    ): Promise<CoursePlayState> =>
        request<CoursePlayState>({
            method: "POST",
            url: `/api/courses/exercises/${exerciseId}/answer`,
            data: { answer, progress } satisfies CourseAnswerBody,
        }),
    useHint: (
        exerciseId: string,
        kind: RiddleRangeKind,
        progress?: AnonymousCourseExerciseProgressRequest,
    ): Promise<CoursePlayState> =>
        request<CoursePlayState>({
            method: "POST",
            url: `/api/courses/exercises/${exerciseId}/hint`,
            data: { kind, progress } satisfies CourseHintBody,
        }),
    revealLetter: (exerciseId: string, progress?: AnonymousCourseExerciseProgressRequest): Promise<CoursePlayState> =>
        request<CoursePlayState>({
            method: "POST",
            url: `/api/courses/exercises/${exerciseId}/reveal`,
            data: { progress } satisfies CourseCommandBody,
        }),
};
