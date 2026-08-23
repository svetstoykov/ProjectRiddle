import { useEffect, useRef, useState, type ReactElement } from "react";

import {
    answerCharacters,
    buildAnswerWords,
    buildSubmission,
    letterCountOf,
    parseAnswerPattern,
} from "../models/answerTiles";
import type { PublicRiddlePlay } from "../models/publicRiddle";
import type { RiddlePlayState } from "../models/riddleProgress";
import type { RiddleRangeKind } from "../models/riddleRange";
import { AnswerTiles, type AnswerWordView } from "./AnswerTiles";
import { BulgarianKeyboard } from "./BulgarianKeyboard";
import { CluePresentation } from "./CluePresentation";
import { HintDialog } from "./HintDialog";
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
const hintKindCount = 3;
const incorrectAnswerMessage = "Отговорът не е верен. Поправи буквите и опитай отново.";

function isActivatableTarget(target: EventTarget | null): boolean {
    return target instanceof HTMLButtonElement || target instanceof HTMLAnchorElement;
}

function solutionCharacters(
    answer: string | undefined,
    isTerminal: boolean,
    letterCount: number,
): readonly string[] | undefined {
    if (!isTerminal || answer === undefined) {
        return undefined;
    }

    const characters = answerCharacters(answer);
    return characters.length === letterCount ? characters : undefined;
}

function isTextEntryTarget(target: EventTarget | null): boolean {
    return (
        target instanceof HTMLInputElement ||
        target instanceof HTMLTextAreaElement ||
        target instanceof HTMLSelectElement ||
        (target instanceof HTMLElement && target.isContentEditable)
    );
}

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
    const [isHintDialogOpen, setIsHintDialogOpen] = useState(false);
    const [hasEditedSinceSubmission, setHasEditedSinceSubmission] = useState(false);
    const hintTriggerRef = useRef<HTMLButtonElement>(null);

    const revealedByPosition = new Map(playState.revealedLetters.map((letter) => [letter.position, letter.character]));
    const isLocked = (position: number): boolean => revealedByPosition.has(position);
    const isTerminal = playState.progress.status !== "inProgress";
    // A finished riddle arrives with its answer, so a board resumed after solving shows the solution rather than the
    // empty tiles of a fresh visit. A length that disagrees with the pattern is ignored instead of filled in part.
    const solution = solutionCharacters(playState.answer, isTerminal, letterCount);
    const characterAt = (position: number): string =>
        solution?.[position] ?? revealedByPosition.get(position) ?? entries[position] ?? "";

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
    const showIncorrect = !isTerminal && playState.isCorrect === false && !hasEditedSinceSubmission;
    // Rejection and the fill reminder are mutually exclusive, so one line under the board carries both.
    const statusMessage = showIncorrect
        ? incorrectAnswerMessage
        : !isTerminal && !canSubmit
          ? "Попълни всички букви"
          : "";

    function writeLetter(letter: string): void {
        if (isTerminal || activePosition === undefined) {
            return;
        }

        const target = activePosition;
        setHasEditedSinceSubmission(true);
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

        setHasEditedSinceSubmission(true);
        setEntries((current) => current.map((value, position) => (position === target ? "" : value)));
        setSelectedPosition(target);
    }

    function submitAnswer(): void {
        if (!canSubmit) {
            return;
        }

        const characters = Array.from({ length: letterCount }, (_unused, position) => characterAt(position));
        setHasEditedSinceSubmission(false);
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

    // The board has no text input to hold focus, so physical typing is read from the document. The listener is
    // re-registered after every render because each handler must see the current cursor and board state.
    useEffect(() => {
        function handleKeyDown(event: KeyboardEvent): void {
            if (isTerminal || isHintDialogOpen || event.ctrlKey || event.metaKey || event.altKey) {
                return;
            }

            if (isTextEntryTarget(event.target)) {
                return;
            }

            if (event.key === "Backspace") {
                event.preventDefault();
                eraseLetter();
                return;
            }

            // A focused control owns its own activation keys, so Enter submits only when the board itself holds focus.
            if (event.key === "Enter" && !isActivatableTarget(event.target)) {
                event.preventDefault();
                submitAnswer();
                return;
            }

            if (event.key.length === 1 && cyrillicLetter.test(event.key)) {
                event.preventDefault();
                writeLetter(event.key.toLocaleUpperCase("bg"));
            }
        }

        document.addEventListener("keydown", handleKeyDown);
        return () => {
            document.removeEventListener("keydown", handleKeyDown);
        };
    });

    if (wordLengths.length === 0) {
        return (
            <p className={styles.unavailable} role="alert">
                Тази загадка не може да бъде показана. Опитай отново по-късно.
            </p>
        );
    }

    const words: readonly AnswerWordView[] = buildAnswerWords(wordLengths).map((word) => ({
        wordIndex: word.wordIndex,
        tiles: word.positions.map((position) => ({
            position,
            character: characterAt(position),
            isLocked: isLocked(position),
        })),
    }));

    const visibleHints = new Set(playState.progress.usedHints.filter((kind) => !hiddenHints.has(kind)));
    const usedHintCount = playState.progress.usedHints.length;

    return (
        <section className={styles.player} aria-label="Загадка">
            <div className={styles.clueSlot}>
                <CluePresentation
                    clue={play.clue}
                    answerPattern={play.answerPattern}
                    ranges={play.ranges}
                    activeKinds={visibleHints}
                />
            </div>
            <div className={styles.boardSlot}>
                <AnswerTiles
                    words={words}
                    activePosition={isTerminal ? undefined : activePosition}
                    tone={
                        playState.progress.status === "solved"
                            ? "solved"
                            : playState.progress.status === "fullyRevealed"
                              ? "revealed"
                              : "playing"
                    }
                    rejected={showIncorrect}
                    rejectToken={playState.progress.answerAttemptCount}
                    onSelectPosition={setSelectedPosition}
                />
                <p className={styles.status}>
                    {/*
                     * A hidden copy of the longest message holds the line open, so a rejected answer reports itself in
                     * place instead of growing the column and pushing the keyboard down the screen.
                     */}
                    <span className={styles.statusSizer} aria-hidden="true">
                        {incorrectAnswerMessage}
                    </span>
                    <span
                        className={[styles.statusText, showIncorrect ? styles.statusIncorrect : undefined].join(" ")}
                        role="status"
                    >
                        {statusMessage}
                    </span>
                </p>
            </div>
            <div className={styles.outcomeSlot}>
                <RiddleOutcome
                    status={playState.progress.status}
                    answerAttemptCount={playState.progress.answerAttemptCount}
                    explanation={playState.explanation}
                />
            </div>
            <div className={styles.actions}>
                <button
                    ref={hintTriggerRef}
                    type="button"
                    className={`buttonSecondary ${styles.hintTrigger}`}
                    aria-haspopup="dialog"
                    aria-expanded={isHintDialogOpen}
                    onClick={() => {
                        setIsHintDialogOpen(true);
                    }}
                >
                    Подсказки ·{" "}
                    <span className={styles.hintCount}>
                        {usedHintCount}/{hintKindCount}
                    </span>
                </button>
                <button
                    type="button"
                    className={styles.submit}
                    aria-busy={isSubmitting}
                    disabled={isTerminal || !canSubmit || isSubmitting}
                    onClick={submitAnswer}
                >
                    {isSubmitting ? "Проверяваме…" : "Провери"}
                </button>
            </div>
            <HintDialog
                open={isHintDialogOpen}
                usedHints={playState.progress.usedHints}
                visibleHints={visibleHints}
                pendingHint={pendingHint}
                revealedCount={playState.progress.revealedPositions.length}
                letterCount={letterCount}
                isRevealing={isRevealing}
                disabled={isTerminal}
                onUnlock={onUseHint}
                onToggle={toggleHint}
                onReveal={onRevealLetter}
                onClose={() => {
                    setIsHintDialogOpen(false);
                }}
                returnFocusRef={hintTriggerRef}
            />
            <div className={styles.keyboardSlot}>
                <BulgarianKeyboard onLetter={writeLetter} onBackspace={eraseLetter} disabled={isTerminal} />
            </div>
        </section>
    );
}
