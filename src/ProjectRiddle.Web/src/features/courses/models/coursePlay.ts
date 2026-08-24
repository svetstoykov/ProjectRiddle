import type { RevealedLetter, RiddleProgressStatus } from "../../riddles/models/riddleProgress";
import type { RiddleRangeKind } from "../../riddles/models/riddleRange";

export interface CourseProgressSnapshot {
    readonly exerciseId: string;
    readonly status: RiddleProgressStatus;
    readonly answerAttemptCount: number;
    readonly usedHints: readonly RiddleRangeKind[];
    readonly revealedPositions: readonly number[];
    readonly letterRevealCount: number;
}

export interface CoursePlayState {
    readonly progress: CourseProgressSnapshot;
    readonly revealedLetters: readonly RevealedLetter[];
    readonly answer?: string;
    readonly explanation?: string;
    readonly teachingNote?: string;
    readonly isCorrect?: boolean;
}

export interface AnonymousCourseExerciseProgressRequest {
    readonly schemaVersion: 1;
    readonly exerciseId: string;
    readonly status: RiddleProgressStatus;
    readonly answerAttemptCount: number;
    readonly usedHints: readonly RiddleRangeKind[];
    readonly revealedPositions: readonly number[];
}
