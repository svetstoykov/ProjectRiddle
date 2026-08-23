import type { CSSProperties, ReactElement } from "react";

import styles from "./AnswerTiles.module.css";

export interface AnswerTileView {
    readonly position: number;
    readonly character: string;
    readonly isLocked: boolean;
}

export interface AnswerWordView {
    readonly wordIndex: number;
    readonly tiles: readonly AnswerTileView[];
}

export interface AnswerTilesProps {
    readonly words: readonly AnswerWordView[];
    readonly activePosition: number | undefined;
    readonly tone: "playing" | "solved" | "revealed";
    /** The attempt number that was just refused, while its flash lasts; `undefined` at every other moment. */
    readonly rejectedAttempt: number | undefined;
    readonly onSelectPosition: (position: number) => void;
}

const toneClassNames: Record<Exclude<AnswerTilesProps["tone"], "playing">, string> = {
    solved: styles.solved ?? "solved",
    revealed: styles.revealed ?? "revealed",
};

function tileLabel(wordIndex: number, indexInWord: number, character: string): string {
    const place = `Буква ${indexInWord + 1} от дума ${wordIndex + 1}`;
    return character === "" ? place : `${place}: ${character}`;
}

export function AnswerTiles({
    words,
    activePosition,
    tone,
    rejectedAttempt,
    onSelectPosition,
}: AnswerTilesProps): ReactElement {
    const longestWord = Math.max(...words.map((word) => word.tiles.length));
    const toneClass = tone === "playing" ? undefined : toneClassNames[tone];
    // Two refusals in a row would otherwise keep the same class and leave the shake mid-flight, because a CSS
    // animation only restarts when its name changes. Alternating names by attempt number always restarts it.
    const rejectedClass =
        rejectedAttempt === undefined
            ? undefined
            : rejectedAttempt % 2 === 0
              ? styles.rejectedEven
              : styles.rejectedOdd;
    const activeWord = words.find((word) => word.tiles.some((tile) => tile.position === activePosition));
    const activeIndexInWord =
        activeWord === undefined ? undefined : activeWord.tiles.findIndex((tile) => tile.position === activePosition);
    const activeCharacter =
        activeWord === undefined || activeIndexInWord === undefined
            ? ""
            : (activeWord.tiles[activeIndexInWord]?.character ?? "");

    return (
        <>
            <div
                className={[styles.board, toneClass, rejectedClass].filter(Boolean).join(" ")}
                style={{ "--longest-word": longestWord } as CSSProperties}
                role="group"
                aria-label="Отговор"
            >
                {words.map((word) => (
                    <div key={word.wordIndex} className={styles.word}>
                        {word.tiles.map((tile, indexInWord) => (
                            <button
                                key={tile.position}
                                type="button"
                                className={[styles.tile, tile.isLocked ? styles.locked : undefined].join(" ")}
                                style={{ "--tile-index": tile.position } as CSSProperties}
                                aria-current={tile.position === activePosition ? "true" : undefined}
                                aria-label={tileLabel(word.wordIndex, indexInWord, tile.character)}
                                tabIndex={tone === "playing" && tile.position === activePosition ? 0 : -1}
                                disabled={tile.isLocked || tone !== "playing"}
                                onClick={() => {
                                    onSelectPosition(tile.position);
                                }}
                            >
                                {tile.character}
                            </button>
                        ))}
                    </div>
                ))}
            </div>
            {activeWord !== undefined && activeIndexInWord !== undefined ? (
                <p className="visuallyHidden" aria-live="polite">
                    {tileLabel(activeWord.wordIndex, activeIndexInWord, activeCharacter)}
                </p>
            ) : null}
        </>
    );
}
