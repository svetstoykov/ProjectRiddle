import type { PublicRiddleRange } from "../../riddles/models/publicRiddle";
import type { RiddleRangeKind } from "../../riddles/models/riddleRange";

export interface ClueParts {
    readonly definition: string | undefined;
    readonly indicator: string | undefined;
    readonly fodder: string | undefined;
    /** Everything outside the definition: the half of the clue that says how to build the answer. */
    readonly wordplay: string | undefined;
}

function textOf(clue: string, ranges: readonly PublicRiddleRange[], kind: RiddleRangeKind): string | undefined {
    const range = ranges.find((item) => item.kind === kind);
    const text = range === undefined ? "" : clue.slice(range.start, range.end).trim();
    return text.length === 0 ? undefined : text;
}

/** Reads the marked parts of a clue back out of its own text, so guidance quotes exactly what the solver sees. */
export function clueParts(clue: string, ranges: readonly PublicRiddleRange[]): ClueParts {
    const definitionRange = ranges.find((item) => item.kind === "definition");
    const wordplay =
        definitionRange === undefined
            ? undefined
            : `${clue.slice(0, definitionRange.start)} ${clue.slice(definitionRange.end)}`.replace(/\s+/gu, " ").trim();

    return {
        definition: textOf(clue, ranges, "definition"),
        indicator: textOf(clue, ranges, "indicator"),
        fodder: textOf(clue, ranges, "fodder"),
        wordplay: wordplay === undefined || wordplay.length === 0 ? undefined : wordplay,
    };
}
