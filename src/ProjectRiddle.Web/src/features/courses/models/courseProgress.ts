import type { Course, CourseCatalog, CourseLessonSummary } from "./courseCatalog";

export interface ResolvedLessonProgress {
    readonly completedExerciseCount: number;
    readonly completedExerciseIds: ReadonlySet<string>;
    readonly isComplete: boolean;
    readonly isAvailable: boolean;
}

export interface ResolvedCourseProgress {
    readonly lessons: ReadonlyMap<string, ResolvedLessonProgress>;
    readonly isPending: boolean;
    readonly error: unknown | undefined;
    readonly retry: () => void;
}

export function lessonsNeededForCourse(catalog: CourseCatalog, course: Course): readonly CourseLessonSummary[] {
    const byKey = new Map(catalog.courses.flatMap((item) => item.lessons).map((lesson) => [lesson.key, lesson]));
    const selected = new Map(course.lessons.map((lesson) => [lesson.key, lesson]));
    const queue = course.lessons.flatMap((lesson) => lesson.prerequisiteLessonKeys);

    for (const key of queue) {
        const lesson = byKey.get(key);

        if (lesson !== undefined && !selected.has(key)) {
            selected.set(key, lesson);
            queue.push(...lesson.prerequisiteLessonKeys);
        }
    }

    return [...selected.values()];
}

export interface LessonRecommendation {
    readonly lesson: CourseLessonSummary;
    /** `start` before anything in the course is done, `continue` once the solver has made progress in it. */
    readonly cue: "start" | "continue";
}

/**
 * The most useful practice to open next: a lesson already begun, otherwise the first open one in course order. A
 * completed lesson is never recommended, and a course with nothing left open has no recommendation.
 */
export function recommendLesson(
    course: Course,
    progressByLesson: ReadonlyMap<string, ResolvedLessonProgress>,
): LessonRecommendation | undefined {
    const ordered = [...course.lessons].sort((left, right) => left.ordinal - right.ordinal);
    const completedCount = (lesson: CourseLessonSummary): number =>
        progressByLesson.get(lesson.key)?.completedExerciseCount ?? 0;
    const open = ordered.filter((lesson) => {
        const progress = progressByLesson.get(lesson.key);
        return progress?.isAvailable === true && !progress.isComplete;
    });
    const lesson = open.find((item) => completedCount(item) > 0) ?? open[0];

    if (lesson === undefined) {
        return undefined;
    }

    return { lesson, cue: ordered.some((item) => completedCount(item) > 0) ? "continue" : "start" };
}

export function resolveAvailability(
    lesson: CourseLessonSummary,
    progressByLesson: ReadonlyMap<string, ResolvedLessonProgress>,
): boolean {
    return lesson.prerequisiteLessonKeys.every((key) => progressByLesson.get(key)?.isComplete === true);
}
