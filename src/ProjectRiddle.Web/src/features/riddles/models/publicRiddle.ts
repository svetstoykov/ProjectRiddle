import type { RiddleRangeKind } from "./riddleRange";

export interface PublicRiddleRange {
    readonly kind: RiddleRangeKind;
    readonly start: number;
    readonly end: number;
}

export interface PublicRiddlePlay {
    readonly id: string;
    readonly publicationDate: string;
    readonly clue: string;
    readonly answerPattern: string;
    readonly ranges: readonly PublicRiddleRange[];
}

export interface PublicRiddleDiscoveryItem {
    readonly id: string;
    readonly publicationDate: string;
    readonly clueExcerpt: string;
    readonly answerPattern: string;
}

export interface PublicRiddleWeek {
    readonly weekStart: string;
    readonly weekEnd: string;
    readonly today: string;
    readonly items: readonly PublicRiddleDiscoveryItem[];
}

export interface PublicRiddleList {
    readonly page: number;
    readonly pageSize: number;
    readonly totalCount: number;
    readonly items: readonly PublicRiddleDiscoveryItem[];
}
