import type { RiddleRangeKind } from "../../riddles/models/riddleRange";

export type RiddlePublicationState = "draft" | "scheduled" | "published" | "unpublished";

export interface RiddleRange {
    readonly id: string;
    readonly kind: RiddleRangeKind;
    readonly start: number;
    readonly end: number;
}

export interface Riddle {
    readonly id: string;
    readonly clue: string;
    readonly answer: string;
    readonly answerPattern: string;
    readonly explanation: string;
    readonly publicationState: RiddlePublicationState;
    readonly sofiaPublicationDate: string | null;
    readonly ranges: readonly RiddleRange[];
    readonly createdAtUtc: string;
    readonly updatedAtUtc: string;
}

export interface RiddleContentRequest {
    readonly clue: string;
    readonly answer: string;
    readonly answerPattern: string;
    readonly explanation: string;
    readonly ranges: readonly Pick<RiddleRange, "kind" | "start" | "end">[];
}

export interface RiddleListResponse {
    readonly riddles: readonly Riddle[];
}

export interface ScheduleRiddleRequest {
    readonly publicationDate: string;
}

export interface PublishRiddleRequest {
    readonly publicationDate?: string;
}
