import { currentProgressSchemaVersion, progressStorage } from "../../../shared/storage/progressStorage";
import { letterCountOf, parseAnswerPattern } from "../../riddles/models/answerTiles";
import type { CourseLessonExercise } from "../models/courseCatalog";
import type { AnonymousCourseExerciseProgressRequest } from "../models/coursePlay";

export type CourseCompletionStatus = "solved" | "fullyRevealed";

export interface AnonymousCourseCompletion {
    readonly exerciseId: string;
    readonly status: CourseCompletionStatus;
}

export interface AnonymousCourseProgress {
    readonly completedExercises: readonly AnonymousCourseCompletion[];
    readonly primerDismissed: boolean;
    readonly dismissedLessonIntroKeys: readonly string[];
}

const emptyCourseProgress = (): AnonymousCourseProgress => ({
    completedExercises: [],
    primerDismissed: false,
    dismissedLessonIntroKeys: [],
});

const isRecord = (value: unknown): value is Record<string, unknown> => typeof value === "object" && value !== null;

const isCompletion = (value: unknown): value is AnonymousCourseCompletion =>
    isRecord(value) &&
    typeof value.exerciseId === "string" &&
    value.exerciseId.length > 0 &&
    (value.status === "solved" || value.status === "fullyRevealed");

export function readAnonymousCourseProgress(): AnonymousCourseProgress {
    const value: unknown = progressStorage.read().courses;

    if (
        !isRecord(value) ||
        !Array.isArray(value.completedExercises) ||
        !value.completedExercises.every(isCompletion) ||
        typeof value.primerDismissed !== "boolean" ||
        !Array.isArray(value.dismissedLessonIntroKeys) ||
        !value.dismissedLessonIntroKeys.every((key) => typeof key === "string")
    ) {
        return emptyCourseProgress();
    }

    return {
        completedExercises: value.completedExercises,
        primerDismissed: value.primerDismissed,
        dismissedLessonIntroKeys: value.dismissedLessonIntroKeys,
    };
}

function writeCourses(courses: AnonymousCourseProgress): void {
    progressStorage.write({ ...progressStorage.read(), courses });
}

export function recordCourseCompletion(completion: AnonymousCourseCompletion): void {
    const current = readAnonymousCourseProgress();
    const existing = current.completedExercises.find((item) => item.exerciseId === completion.exerciseId);
    const nextCompletion =
        existing?.status === "solved" && completion.status === "fullyRevealed" ? existing : completion;

    writeCourses({
        ...current,
        completedExercises: [
            ...current.completedExercises.filter((item) => item.exerciseId !== completion.exerciseId),
            nextCompletion,
        ],
    });
}

export function dismissCoursePrimer(): void {
    writeCourses({ ...readAnonymousCourseProgress(), primerDismissed: true });
}

export function dismissLessonIntro(lessonKey: string): void {
    const current = readAnonymousCourseProgress();
    writeCourses({
        ...current,
        dismissedLessonIntroKeys: [...new Set([...current.dismissedLessonIntroKeys, lessonKey])],
    });
}

export function terminalProgressRequest(
    exercise: CourseLessonExercise,
    status: CourseCompletionStatus,
): AnonymousCourseExerciseProgressRequest {
    const letterCount = letterCountOf(parseAnswerPattern(exercise.answerPattern));

    return {
        schemaVersion: currentProgressSchemaVersion,
        exerciseId: exercise.id,
        status,
        answerAttemptCount: 0,
        usedHints: [],
        revealedPositions:
            status === "fullyRevealed" ? Array.from({ length: letterCount }, (_value, index) => index) : [],
    };
}
