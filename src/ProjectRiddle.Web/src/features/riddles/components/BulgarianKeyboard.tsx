import type { ReactElement } from "react";

import styles from "./BulgarianKeyboard.module.css";

export interface BulgarianKeyboardProps {
    readonly onLetter: (letter: string) => void;
    readonly onBackspace: () => void;
    readonly disabled: boolean;
}

// The alphabet runs in order across three rows. The last row is one letter shorter because erase closes it, which
// keeps all three rows near the same key width.
const letterRows: readonly (readonly string[])[] = [
    ["А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "Й", "К"],
    ["Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф"],
    ["Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ь", "Ю", "Я"],
];

/**
 * Types letters and erases them. Answer checking lives with the board's own actions, so the keyboard stays a text
 * entry surface and nothing on it commits an answer.
 */
export function BulgarianKeyboard({ onLetter, onBackspace, disabled }: BulgarianKeyboardProps): ReactElement {
    const lastRowIndex = letterRows.length - 1;

    return (
        <div className={styles.keyboard} role="group" aria-label="Кирилска клавиатура">
            {letterRows.map((row, rowIndex) => (
                <div key={row.join("")} className={styles.row}>
                    {row.map((letter) => (
                        <button
                            key={letter}
                            type="button"
                            className={styles.key}
                            disabled={disabled}
                            onClick={() => {
                                onLetter(letter);
                            }}
                        >
                            {letter}
                        </button>
                    ))}
                    {rowIndex === lastRowIndex ? (
                        <button
                            type="button"
                            className={`${styles.key} ${styles.backspace}`}
                            aria-label="Изтрий последната буква"
                            disabled={disabled}
                            onClick={onBackspace}
                        >
                            ⌫
                        </button>
                    ) : null}
                </div>
            ))}
        </div>
    );
}
