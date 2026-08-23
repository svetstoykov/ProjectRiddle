import { useState, type CSSProperties, type ReactElement } from "react";

import styles from "./SolvedConfetti.module.css";

const pieceCount = 30;

const pieceColors: readonly string[] = [
    "var(--color-electric-yellow)",
    "var(--color-correct-green)",
    "var(--color-primary-strong)",
    "var(--color-tertiary)",
    "var(--color-primary)",
];

interface ConfettiPiece {
    readonly id: number;
    readonly style: CSSProperties;
}

function buildPieces(): readonly ConfettiPiece[] {
    return Array.from({ length: pieceCount }, (_unused, index) => ({
        id: index,
        style: {
            "--piece-color": pieceColors[index % pieceColors.length],
            "--piece-left": `${(index / pieceCount) * 100 + Math.random() * 3}%`,
            "--piece-drift": `${Math.round(Math.random() * 160 - 80)}px`,
            "--piece-spin": `${Math.round(Math.random() * 720 - 360)}deg`,
            "--piece-delay": `${Math.round(Math.random() * 400)}ms`,
            "--piece-duration": `${2200 + Math.round(Math.random() * 1400)}ms`,
            "--piece-width": `${6 + Math.round(Math.random() * 6)}px`,
        } as CSSProperties,
    }));
}

/**
 * A one-shot celebration for a solved riddle. The pieces are laid out once when the burst mounts, so a re-render of
 * the board never reshuffles a fall that is already under way. The whole burst is decorative: it carries no meaning
 * that the result panel does not already state in words, and it is dropped entirely under reduced motion.
 */
export function SolvedConfetti(): ReactElement {
    const [pieces] = useState(buildPieces);

    return (
        <div className={styles.confetti} aria-hidden="true">
            {pieces.map((piece) => (
                <span key={piece.id} className={styles.piece} style={piece.style} />
            ))}
        </div>
    );
}
