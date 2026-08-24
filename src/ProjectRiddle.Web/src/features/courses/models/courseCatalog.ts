import type { PublicRiddleRange } from "../../riddles/models/publicRiddle";

export type CourseLessonKind = "technique" | "mix" | "finalMix";

export interface CourseLessonProgress {
    readonly completedExerciseCount: number;
    readonly isAvailable: boolean;
    readonly completedExerciseIds: readonly string[];
}

export interface CourseLessonSummary {
    readonly id: string;
    readonly key: string;
    readonly ordinal: number;
    readonly title: string;
    readonly kind: CourseLessonKind;
    readonly exerciseCount: number;
    readonly prerequisiteLessonKeys: readonly string[];
    readonly progress?: CourseLessonProgress;
}

export interface Course {
    readonly id: string;
    readonly key: string;
    readonly ordinal: number;
    readonly title: string;
    readonly intro: string;
    readonly lessons: readonly CourseLessonSummary[];
}

export interface CourseCatalog {
    readonly courses: readonly Course[];
}

export interface CourseLessonExercise {
    readonly id: string;
    readonly ordinal: number;
    readonly setup?: string;
    readonly clue: string;
    readonly answerPattern: string;
    readonly ranges: readonly PublicRiddleRange[];
    readonly isComplete?: boolean;
}

export interface CourseLessonDetail {
    readonly id: string;
    readonly key: string;
    readonly ordinal: number;
    readonly title: string;
    readonly kind: CourseLessonKind;
    readonly intro?: string;
    readonly prerequisiteLessonKeys: readonly string[];
    readonly exercises: readonly CourseLessonExercise[];
}

export interface CoursePrimerPage {
    readonly ordinal: number;
    readonly title: string;
    readonly body: string;
    readonly figure?: string;
}

export interface CoursePrimer {
    readonly pages: readonly CoursePrimerPage[];
}

export interface AccountLessonCompletion {
    readonly lessonId: string;
    readonly lessonKey: string;
    readonly completedExerciseCount: number;
    readonly exerciseCount: number;
    readonly isComplete: boolean;
}

export interface AccountCourseProgress {
    readonly completedExerciseIds: readonly string[];
    readonly lessons: readonly AccountLessonCompletion[];
}
