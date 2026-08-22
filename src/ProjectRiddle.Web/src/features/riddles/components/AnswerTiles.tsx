import type { ReactElement } from "react";

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
    readonly onSelectPosition: (position: number) => void;
}

const toneClassNames: Record<AnswerTilesProps["tone"], string> = {
    playing: styles.playing ?? "playing",
    solved: styles.solved ?? "solved",
    revealed: styles.revealed ?? "revealed",
};

function tileLabel(wordIndex: number, indexInWord: number, character: string): string {
    const place = `Буква ${indexInWord + 1} от дума ${wordIndex + 1}`;
    return character === "" ? place : `${place}: ${character}`;
}

export function AnswerTiles({ words, activePosition, tone, onSelectPosition }: AnswerTilesProps): ReactElement {
    return (
        <div className={[styles.board, toneClassNames[tone]].join(" ")} role="group" aria-label="Отговор">
            {words.map((word) => (
                <div key={word.wordIndex} className={styles.word}>
                    {word.tiles.map((tile, indexInWord) => (
                        <button
                            key={tile.position}
                            type="button"
                            className={[styles.tile, tile.isLocked ? styles.locked : undefined].join(" ")}
                            aria-current={tile.position === activePosition ? "true" : undefined}
                            aria-label={tileLabel(word.wordIndex, indexInWord, tile.character)}
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
    );
}
