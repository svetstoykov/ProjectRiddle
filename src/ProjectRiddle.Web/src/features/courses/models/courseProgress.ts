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
    readonly error: unknown;
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

export function resolveAvailability(
    lesson: CourseLessonSummary,
    progressByLesson: ReadonlyMap<string, ResolvedLessonProgress>,
): boolean {
    return lesson.prerequisiteLessonKeys.every((key) => progressByLesson.get(key)?.isComplete === true);
}
