import type { RiddleRangeKind } from "../../riddles/models/riddleRange";
import type { Riddle, RiddleContentRequest } from "./adminRiddle";

export interface EditableRange {
    readonly key: string;
    readonly kind: RiddleRangeKind;
    readonly start: number;
    readonly end: number;
}

export interface RiddleDraft {
    readonly clue: string;
    readonly answer: string;
    readonly explanation: string;
    readonly ranges: readonly EditableRange[];
}

export interface RiddleContentErrors {
    readonly clue: readonly string[];
    readonly answer: readonly string[];
    readonly explanation: readonly string[];
    readonly ranges: readonly string[];
    readonly summary: readonly string[];
}

export function emptyRiddleContentErrors(): RiddleContentErrors {
    return {
        clue: [],
        answer: [],
        explanation: [],
        ranges: [],
        summary: [],
    };
}

export function emptyRiddleDraft(): RiddleDraft {
    return {
        clue: "",
        answer: "",
        explanation: "",
        ranges: [],
    };
}

export function draftFromRiddle(riddle: Riddle): RiddleDraft {
    return {
        clue: riddle.clue,
        answer: riddle.answer,
        explanation: riddle.explanation,
        ranges: riddle.ranges.map((range) => ({
            key: range.id,
            kind: range.kind,
            start: range.start,
            end: range.end,
        })),
    };
}

export function requestFromDraft(draft: RiddleDraft): RiddleContentRequest {
    return {
        clue: draft.clue,
        answer: draft.answer,
        explanation: draft.explanation,
        ranges: draft.ranges.map((range) => ({
            kind: range.kind,
            start: range.start,
            end: range.end,
        })),
    };
}

export function draftsEqual(left: RiddleDraft, right: RiddleDraft): boolean {
    if (
        left.clue !== right.clue ||
        left.answer !== right.answer ||
        left.explanation !== right.explanation ||
        left.ranges.length !== right.ranges.length
    ) {
        return false;
    }

    return left.ranges.every((range, index) => {
        const other = right.ranges[index];
        return (
            other !== undefined && range.kind === other.kind && range.start === other.start && range.end === other.end
        );
    });
}

export function sortEditableRanges(ranges: readonly EditableRange[]): EditableRange[] {
    return [...ranges].sort((left, right) => {
        if (left.start !== right.start) {
            return left.start - right.start;
        }

        if (left.end !== right.end) {
            return left.end - right.end;
        }

        if (left.kind !== right.kind) {
            return left.kind.localeCompare(right.kind);
        }

        return left.key.localeCompare(right.key);
    });
}
