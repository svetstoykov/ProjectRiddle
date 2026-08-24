import type { RiddleRangeKind } from "./riddleRange";

export type RiddleProgressStatus = "inProgress" | "fullyRevealed" | "solved";

export interface RiddlePlayerProgress {
    readonly status: RiddleProgressStatus;
    readonly answerAttemptCount: number;
    readonly usedHints: readonly RiddleRangeKind[];
    readonly revealedPositions: readonly number[];
    readonly letterRevealCount: number;
}

export interface RevealedLetter {
    readonly position: number;
    readonly character: string;
}

/**
 * The server omits `answer`, `explanation`, and `isCorrect` when they are null. Their absence is never a failure:
 * `answer` and `explanation` are released only in a terminal state, and `isCorrect` only on an answer result.
 */
export interface RiddlePlayerState {
    readonly progress: RiddlePlayerProgress;
    readonly revealedLetters: readonly RevealedLetter[];
    readonly answer?: string;
    readonly explanation?: string;
    readonly isCorrect?: boolean;
}

export interface RiddleProgressSnapshot extends RiddlePlayerProgress {
    readonly riddleId: string;
    readonly publicationDate: string;
}

export interface RiddlePlayState extends RiddlePlayerState {
    readonly progress: RiddleProgressSnapshot;
}

export interface AccountRiddleProgressList {
    readonly items: readonly RiddleProgressSnapshot[];
}

/**
 * The riddles segment of the browser progress document. It never contains answer characters, a complete answer,
 * an explanation, credentials, or session material; answer-sensitive display is rehydrated through resume.
 */
export interface AnonymousRiddleProgress {
    readonly riddleId: string;
    readonly publicationDate: string;
    readonly status: RiddleProgressStatus;
    readonly answerAttemptCount: number;
    readonly usedHints: readonly RiddleRangeKind[];
    readonly revealedPositions: readonly number[];
}

export interface AnonymousRiddleProgressRequest extends AnonymousRiddleProgress {
    readonly schemaVersion: number;
}
