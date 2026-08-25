import { useId, useRef, useState, type ReactElement } from "react";

import { ClueTermText } from "../../../shared/components/ClueTermText";
import { ConfirmationDialog } from "../../../shared/components/ConfirmationDialog";
import { FieldError } from "../../../shared/components/FieldError";
import { rangeKindLabels } from "../messages/adminMessages";
import type { EditableRange } from "../models/riddleDraft";
import { sortEditableRanges } from "../models/riddleDraft";
import type { RiddleRangeKind } from "../../riddles/models/riddleRange";
import styles from "./RangeLabeller.module.css";

export interface RangeLabellerProps {
    readonly clue: string;
    readonly ranges: readonly EditableRange[];
    readonly errors: readonly string[];
    readonly disabled: boolean;
    readonly onChange: (clue: string, ranges: readonly EditableRange[]) => void;
}

interface TextSelection {
    readonly start: number;
    readonly end: number;
}

const kindOrder: readonly RiddleRangeKind[] = ["definition", "indicator", "fodder"];

const kindSwatchClassNames: Record<RiddleRangeKind, string> = {
    definition: styles.definition ?? "definition",
    indicator: styles.indicator ?? "indicator",
    fodder: styles.fodder ?? "fodder",
};

export function RangeLabeller({ clue, ranges, errors, disabled, onChange }: RangeLabellerProps): ReactElement {
    const clueId = useId();
    const errorId = useId();
    const textareaRef = useRef<HTMLTextAreaElement>(null);
    const [pendingSelection, setPendingSelection] = useState<TextSelection | null>(null);
    const [pendingClue, setPendingClue] = useState<string | null>(null);
    const [inlineMessages, setInlineMessages] = useState<readonly string[]>([]);

    function captureSelection(): void {
        const textarea = textareaRef.current;

        if (textarea === null || textarea.selectionStart === textarea.selectionEnd) {
            return;
        }

        setPendingSelection({
            start: textarea.selectionStart,
            end: textarea.selectionEnd,
        });
    }

    function applyKind(kind: RiddleRangeKind): void {
        const trimmed = clue.trim();

        if (trimmed !== clue) {
            onChange(trimmed, ranges);
            setPendingSelection(null);
            setInlineMessages(["Началните и крайните интервали бяха премахнати. Избери откъса отново."]);
            return;
        }

        if (pendingSelection === null || pendingSelection.start === pendingSelection.end) {
            setInlineMessages(["Избери откъс от уликата, преди да добавиш етикет."]);
            return;
        }

        const duplicate = ranges.some(
            (range) =>
                range.kind === kind && range.start === pendingSelection.start && range.end === pendingSelection.end,
        );

        if (duplicate) {
            setInlineMessages(["Този откъс вече има същия етикет."]);
            return;
        }

        const nextRange: EditableRange = {
            key: crypto.randomUUID(),
            kind,
            start: pendingSelection.start,
            end: pendingSelection.end,
        };
        setInlineMessages([]);
        onChange(clue, sortEditableRanges([...ranges, nextRange]));
    }

    function handleClueChange(nextClue: string): void {
        if (ranges.length === 0) {
            setPendingSelection(null);
            setInlineMessages([]);
            onChange(nextClue, ranges);
            return;
        }

        setPendingClue(nextClue);
    }

    const dialogOpen = pendingClue !== null;
    const combinedErrors = [...inlineMessages, ...errors];

    return (
        <div className={styles.labeller}>
            <div className={styles.field}>
                <label htmlFor={clueId}>Улика</label>
                <textarea
                    ref={textareaRef}
                    id={clueId}
                    value={clue}
                    disabled={disabled}
                    aria-invalid={combinedErrors.length > 0}
                    aria-describedby={combinedErrors.length > 0 ? errorId : undefined}
                    onSelect={captureSelection}
                    onChange={(event) => {
                        handleClueChange(event.target.value);
                    }}
                />
                <FieldError id={errorId} messages={combinedErrors} />
            </div>
            <div className={styles.actions}>
                {kindOrder.map((kind) => (
                    <button
                        key={kind}
                        type="button"
                        className={styles.kind}
                        disabled={disabled}
                        onClick={() => {
                            applyKind(kind);
                        }}
                    >
                        <span className={`${styles.swatch} ${kindSwatchClassNames[kind]}`} aria-hidden="true" />
                        {rangeKindLabels[kind]}
                    </button>
                ))}
            </div>
            {ranges.length > 0 ? (
                <ul className={styles.ranges}>
                    {ranges.map((range) => {
                        const selectedText = clue.slice(range.start, range.end);
                        const kindLabel = rangeKindLabels[range.kind];
                        return (
                            <li key={range.key} className={styles.range}>
                                <p>
                                    <strong>
                                        <ClueTermText text={kindLabel} />
                                    </strong>{" "}
                                    „{selectedText}“ [{range.start}, {range.end})
                                </p>
                                <button
                                    type="button"
                                    className={styles.remove}
                                    disabled={disabled}
                                    aria-label={`Премахни ${kindLabel}: ${selectedText}`}
                                    onClick={() => {
                                        onChange(
                                            clue,
                                            ranges.filter((item) => item.key !== range.key),
                                        );
                                    }}
                                >
                                    Премахни
                                </button>
                            </li>
                        );
                    })}
                </ul>
            ) : (
                <p className={styles.empty}>Няма добавени откъси.</p>
            )}
            <ConfirmationDialog
                open={dialogOpen}
                title="Промяна на уликата"
                description="Промяната на уликата ще премахне всички етикети по откъси."
                confirmLabel="Продължи и изчисти етикетите"
                busy={false}
                danger={false}
                returnFocusRef={textareaRef}
                onCancel={() => {
                    setPendingClue(null);
                }}
                onConfirm={() => {
                    if (pendingClue !== null) {
                        onChange(pendingClue, []);
                    }

                    setPendingClue(null);
                    setPendingSelection(null);
                    setInlineMessages([]);
                }}
            />
        </div>
    );
}
