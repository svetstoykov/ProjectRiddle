import { useRef, useState, type ReactElement } from "react";

import { InfoDialog } from "../../../shared/components/InfoDialog";
import type { RiddleRangeKind } from "../models/riddleRange";
import styles from "./CluePresentation.module.css";

export interface ClueRange {
    readonly kind: RiddleRangeKind;
    readonly start: number;
    readonly end: number;
}

export interface CluePresentationProps {
    readonly clue: string;
    readonly answerPattern: string;
    readonly ranges: readonly ClueRange[];
    readonly activeKinds: ReadonlySet<RiddleRangeKind>;
}

const kindOrder: readonly RiddleRangeKind[] = ["definition", "indicator", "fodder"];

const kindLabels: Record<RiddleRangeKind, string> = {
    definition: "Дефиниция",
    indicator: "Индикатор",
    fodder: "Материал",
};

/* What each part of a cryptic clue does, in the words a first-time solver needs rather than the jargon. */
const kindExplanations: Record<RiddleRangeKind, string> = {
    definition:
        "Частта, която значи същото като отговора, както в обикновена кръстословица. Стои в началото или в края на загадката — никога по средата.",
    indicator:
        "Думите, които подсказват какво да направиш с материала: да го разбъркаш, обърнеш, скъсиш или да извадиш скрити в него букви.",
    fodder: "Буквите и думите, върху които действа индикаторът. От тях, след указаната обработка, се получава отговорът.",
};

const kindClassNames: Record<RiddleRangeKind, string> = {
    definition: styles.definition ?? "definition",
    indicator: styles.indicator ?? "indicator",
    fodder: styles.fodder ?? "fodder",
};

interface ClueSegment {
    readonly start: number;
    readonly end: number;
    readonly text: string;
    readonly kinds: readonly RiddleRangeKind[];
}

function clampOffset(value: number, length: number): number {
    return Math.max(0, Math.min(length, value));
}

function isValidRange(range: ClueRange, length: number): boolean {
    return (
        Number.isInteger(range.start) &&
        Number.isInteger(range.end) &&
        range.end > range.start &&
        range.start < length &&
        range.end > 0
    );
}

function coveringKinds(
    start: number,
    end: number,
    ranges: readonly ClueRange[],
    activeKinds: ReadonlySet<RiddleRangeKind>,
): RiddleRangeKind[] {
    const kinds = new Set<RiddleRangeKind>();

    for (const range of ranges) {
        if (activeKinds.has(range.kind) && range.start <= start && range.end >= end) {
            kinds.add(range.kind);
        }
    }

    return kindOrder.filter((kind) => kinds.has(kind));
}

function splitClue(
    clue: string,
    ranges: readonly ClueRange[],
    activeKinds: ReadonlySet<RiddleRangeKind>,
): ClueSegment[] {
    const length = clue.length;
    const validRanges = ranges.filter((range) => isValidRange(range, length));
    const boundaries = new Set<number>([0, length]);

    for (const range of validRanges) {
        boundaries.add(clampOffset(range.start, length));
        boundaries.add(clampOffset(range.end, length));
    }

    const ordered = [...boundaries].sort((left, right) => left - right);
    const segments: ClueSegment[] = [];

    for (let index = 0; index < ordered.length - 1; index += 1) {
        const start = ordered[index];
        const end = ordered[index + 1];

        if (start === undefined || end === undefined || end <= start) {
            continue;
        }

        segments.push({
            start,
            end,
            text: clue.slice(start, end),
            kinds: coveringKinds(start, end, validRanges, activeKinds),
        });
    }

    return segments;
}

export function CluePresentation({ clue, answerPattern, ranges, activeKinds }: CluePresentationProps): ReactElement {
    const segments = splitClue(clue, ranges, activeKinds);
    const [explainedKind, setExplainedKind] = useState<RiddleRangeKind | undefined>(undefined);
    // The trigger is whichever legend entry was pressed, so the dialog returns focus to the term the reader asked about.
    const triggerRef = useRef<HTMLButtonElement | null>(null);

    return (
        <div className={styles.presentation}>
            <div className={styles.clue}>
                <p className={styles.clueText} lang="bg">
                    {segments.map((segment) =>
                        segment.kinds.length === 0 ? (
                            <span key={`${segment.start}:${segment.end}`} className={styles.segment}>
                                {segment.text}
                            </span>
                        ) : (
                            <mark
                                key={`${segment.start}:${segment.end}`}
                                className={[styles.segment, ...segment.kinds.map((kind) => kindClassNames[kind])].join(
                                    " ",
                                )}
                                data-kinds={segment.kinds.join(" ")}
                            >
                                {segment.text}
                            </mark>
                        ),
                    )}
                    {/* Cryptic clues carry their letter count as an enumeration at the end of the clue itself. */}
                    {answerPattern === "" ? null : (
                        <>
                            {" "}
                            <span className={styles.enumeration}>({answerPattern})</span>
                        </>
                    )}
                </p>
            </div>
            <ul className={styles.legend}>
                {kindOrder.map((kind) => {
                    const isActive = activeKinds.has(kind);

                    return (
                        <li key={kind} className={isActive ? undefined : styles.legendIdle}>
                            <button
                                type="button"
                                className={styles.legendItem}
                                aria-haspopup="dialog"
                                aria-label={`${kindLabels[kind]} — какво означава?`}
                                onClick={(event) => {
                                    triggerRef.current = event.currentTarget;
                                    setExplainedKind(kind);
                                }}
                            >
                                <span className={`${styles.swatch} ${kindClassNames[kind]}`} aria-hidden="true" />
                                {kindLabels[kind]}
                                <span className={styles.legendInfo} aria-hidden="true">
                                    ?
                                </span>
                            </button>
                        </li>
                    );
                })}
            </ul>
            <InfoDialog
                open={explainedKind !== undefined}
                title={explainedKind === undefined ? "" : kindLabels[explainedKind]}
                onClose={() => {
                    setExplainedKind(undefined);
                }}
                returnFocusRef={triggerRef}
            >
                <p>{explainedKind === undefined ? null : kindExplanations[explainedKind]}</p>
            </InfoDialog>
        </div>
    );
}
