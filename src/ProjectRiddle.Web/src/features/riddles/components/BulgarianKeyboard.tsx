import type { ReactElement } from "react";

import styles from "./BulgarianKeyboard.module.css";

export interface BulgarianKeyboardProps {
    readonly onLetter: (letter: string) => void;
    readonly onBackspace: () => void;
    readonly onSubmit: () => void;
    readonly canSubmit: boolean;
    readonly isSubmitting: boolean;
    readonly disabled: boolean;
}

const letterRows: readonly (readonly string[])[] = [
    ["А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "Й"],
    ["К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У"],
    ["Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ь", "Ю", "Я"],
];

export function BulgarianKeyboard({
    onLetter,
    onBackspace,
    onSubmit,
    canSubmit,
    isSubmitting,
    disabled,
}: BulgarianKeyboardProps): ReactElement {
    return (
        <div className={styles.keyboard} role="group" aria-label="Кирилска клавиатура">
            {letterRows.map((row) => (
                <div key={row.join("")} className={styles.row}>
                    {row.map((letter) => (
                        <button
                            key={letter}
                            type="button"
                            data-player-key="true"
                            className={styles.key}
                            disabled={disabled}
                            onClick={() => {
                                onLetter(letter);
                            }}
                        >
                            {letter}
                        </button>
                    ))}
                </div>
            ))}
            <div className={styles.actions}>
                <button
                    type="button"
                    data-player-key="true"
                    className={styles.backspace}
                    aria-label="Изтрий последната буква"
                    disabled={disabled}
                    onClick={onBackspace}
                >
                    ⌫
                </button>
                <button
                    type="button"
                    data-player-key="true"
                    className={styles.check}
                    aria-busy={isSubmitting}
                    disabled={disabled || !canSubmit || isSubmitting}
                    onClick={onSubmit}
                >
                    {isSubmitting ? "Проверяваме…" : "ПРОВЕРИ"}
                </button>
            </div>
        </div>
    );
}
