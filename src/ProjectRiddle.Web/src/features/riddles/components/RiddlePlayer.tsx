import { useState, type KeyboardEvent, type ReactElement } from "react";

import { buildAnswerWords, buildSubmission, letterCountOf, parseAnswerPattern } from "../models/answerTiles";
import type { PublicRiddlePlay } from "../models/publicRiddle";
import type { RiddlePlayState } from "../models/riddleProgress";
import type { RiddleRangeKind } from "../models/riddleRange";
import { AnswerTiles, type AnswerWordView } from "./AnswerTiles";
import { BulgarianKeyboard } from "./BulgarianKeyboard";
import { CluePresentation } from "./CluePresentation";
import { HintControls } from "./HintControls";
import { RevealControl } from "./RevealControl";
import { RiddleOutcome } from "./RiddleOutcome";
import styles from "./RiddlePlayer.module.css";

export interface RiddlePlayerProps {
    readonly play: PublicRiddlePlay;
    readonly playState: RiddlePlayState;
    readonly pendingHint: RiddleRangeKind | undefined;
    readonly isSubmitting: boolean;
    readonly isRevealing: boolean;
    readonly onSubmitAnswer: (answer: string) => void;
    readonly onUseHint: (kind: RiddleRangeKind) => void;
    readonly onRevealLetter: () => void;
}

const cyrillicLetter = /^\p{Script=Cyrillic}$/u;

export function RiddlePlayer({
    play,
    playState,
    pendingHint,
    isSubmitting,
    isRevealing,
    onSubmitAnswer,
    onUseHint,
    onRevealLetter,
}: RiddlePlayerProps): ReactElement {
    const wordLengths = parseAnswerPattern(play.answerPattern);
    const letterCount = letterCountOf(wordLengths);
    const [entries, setEntries] = useState<readonly string[]>(() => Array.from({ length: letterCount }, () => ""));
    const [selectedPosition, setSelectedPosition] = useState<number | undefined>(undefined);
    const [hiddenHints, setHiddenHints] = useState<ReadonlySet<RiddleRangeKind>>(() => new Set());

    if (wordLengths.length === 0) {
        return (
            <p className={styles.unavailable} role="alert">
                Тази загадка не може да бъде показана. Опитай отново по-късно.
            </p>
        );
    }

    const revealedByPosition = new Map(playState.revealedLetters.map((letter) => [letter.position, letter.character]));
    const isLocked = (position: number): boolean => revealedByPosition.has(position);
    const characterAt = (position: number): string => revealedByPosition.get(position) ?? entries[position] ?? "";
    const isTerminal = playState.progress.status !== "inProgress";

    const nextEditablePosition = (from: number): number | undefined => {
        for (let position = Math.max(0, from); position < letterCount; position += 1) {
            if (!isLocked(position)) {
                return position;
            }
        }

        return undefined;
    };

    const previousEditablePosition = (from: number): number | undefined => {
        for (let position = Math.min(from, letterCount - 1); position >= 0; position -= 1) {
            if (!isLocked(position)) {
                return position;
            }
        }

        return undefined;
    };

    // The cursor is clamped during rendering rather than synchronized with an effect, so a server-chosen reveal that
    // locks the selected tile moves the cursor without an extra render pass.
    const activePosition =
        selectedPosition !== undefined && !isLocked(selectedPosition) ? selectedPosition : nextEditablePosition(0);

    const filledEditableCount = Array.from({ length: letterCount }, (_unused, position) => position).filter(
        (position) => !isLocked(position) && characterAt(position) !== "",
    ).length;
    const editableCount = letterCount - revealedByPosition.size;
    const canSubmit = !isTerminal && editableCount > 0 && filledEditableCount === editableCount;

    const words: readonly AnswerWordView[] = buildAnswerWords(wordLengths).map((word) => ({
        wordIndex: word.wordIndex,
        tiles: word.positions.map((position) => ({
            position,
            character: characterAt(position),
            isLocked: isLocked(position),
        })),
    }));

    const visibleHints = new Set(playState.progress.usedHints.filter((kind) => !hiddenHints.has(kind)));

    function writeLetter(letter: string): void {
        if (isTerminal || activePosition === undefined) {
            return;
        }

        const target = activePosition;
        setEntries((current) => current.map((value, position) => (position === target ? letter : value)));
        setSelectedPosition(nextEditablePosition(target + 1) ?? target);
    }

    function eraseLetter(): void {
        if (isTerminal || activePosition === undefined) {
            return;
        }

        const target =
            characterAt(activePosition) !== "" ? activePosition : previousEditablePosition(activePosition - 1);

        if (target === undefined) {
            return;
        }

        setEntries((current) => current.map((value, position) => (position === target ? "" : value)));
        setSelectedPosition(target);
    }

    function submitAnswer(): void {
        if (!canSubmit) {
            return;
        }

        const characters = Array.from({ length: letterCount }, (_unused, position) => characterAt(position));
        onSubmitAnswer(buildSubmission(wordLengths, characters));
    }

    function toggleHint(kind: RiddleRangeKind): void {
        setHiddenHints((current) => {
            const next = new Set(current);

            if (next.has(kind)) {
                next.delete(kind);
            } else {
                next.add(kind);
            }

            return next;
        });
    }

    function handleKeyDown(event: KeyboardEvent<HTMLElement>): void {
        if (isTerminal || event.ctrlKey || event.metaKey || event.altKey) {
            return;
        }

        const isOnScreenKey = event.target instanceof HTMLButtonElement && event.target.dataset.playerKey === "true";

        if (event.key === "Backspace") {
            event.preventDefault();
            eraseLetter();
            return;
        }

        if (event.key === "Enter" && !isOnScreenKey) {
            event.preventDefault();
            submitAnswer();
            return;
        }

        if (event.key.length === 1 && cyrillicLetter.test(event.key)) {
            event.preventDefault();
            writeLetter(event.key.toLocaleUpperCase("bg"));
        }
    }

    return (
        <section className={styles.player} aria-label="Загадка" onKeyDown={handleKeyDown}>
            <CluePresentation
                clue={play.clue}
                answerPattern={play.answerPattern}
                ranges={play.ranges}
                activeKinds={visibleHints}
            />
            <RiddleOutcome
                status={playState.progress.status}
                lastAnswerIncorrect={playState.isCorrect === false}
                answerAttemptCount={playState.progress.answerAttemptCount}
                explanation={playState.explanation}
            />
            <AnswerTiles
                words={words}
                activePosition={activePosition}
                tone={
                    playState.progress.status === "solved"
                        ? "solved"
                        : playState.progress.status === "fullyRevealed"
                          ? "revealed"
                          : "playing"
                }
                onSelectPosition={setSelectedPosition}
            />
            <div className={styles.assists}>
                <HintControls
                    usedHints={playState.progress.usedHints}
                    visibleHints={visibleHints}
                    pendingHint={pendingHint}
                    disabled={isTerminal}
                    onUnlock={onUseHint}
                    onToggle={toggleHint}
                />
                <RevealControl
                    revealedCount={playState.progress.revealedPositions.length}
                    letterCount={letterCount}
                    disabled={playState.progress.status === "solved"}
                    isRevealing={isRevealing}
                    onReveal={onRevealLetter}
                />
            </div>
            <BulgarianKeyboard
                onLetter={writeLetter}
                onBackspace={eraseLetter}
                onSubmit={submitAnswer}
                canSubmit={canSubmit}
                isSubmitting={isSubmitting}
                disabled={isTerminal}
            />
        </section>
    );
}
