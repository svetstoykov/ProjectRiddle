import { useId, type ReactElement } from "react";

import { FieldError } from "../../../shared/components/FieldError";
import type { RiddleContentErrors, RiddleDraft } from "../models/riddleDraft";
import { RangeLabeller } from "./RangeLabeller";
import styles from "./RiddleContentForm.module.css";

export interface RiddleContentFormProps {
    readonly draft: RiddleDraft;
    readonly errors: RiddleContentErrors;
    readonly disabled: boolean;
    readonly onChange: (draft: RiddleDraft) => void;
}

export function RiddleContentForm({ draft, errors, disabled, onChange }: RiddleContentFormProps): ReactElement {
    const answerId = useId();
    const patternId = useId();
    const explanationId = useId();
    const answerErrorId = useId();
    const patternErrorId = useId();
    const explanationErrorId = useId();

    return (
        <div className={styles.form}>
            <RangeLabeller
                clue={draft.clue}
                ranges={draft.ranges}
                errors={[...errors.clue, ...errors.ranges]}
                disabled={disabled}
                onChange={(clue, ranges) => {
                    onChange({ ...draft, clue, ranges });
                }}
            />
            <div className={styles.field}>
                <label htmlFor={answerId}>Отговор</label>
                <input
                    id={answerId}
                    type="text"
                    value={draft.answer}
                    disabled={disabled}
                    aria-invalid={errors.answer.length > 0}
                    aria-describedby={errors.answer.length > 0 ? answerErrorId : undefined}
                    onChange={(event) => {
                        onChange({ ...draft, answer: event.target.value });
                    }}
                />
                <FieldError id={answerErrorId} messages={errors.answer} />
            </div>
            <div className={styles.field}>
                <label htmlFor={patternId}>Брой букви в отговора</label>
                <input
                    id={patternId}
                    type="text"
                    value={draft.answerPattern}
                    disabled={disabled}
                    aria-invalid={errors.answerPattern.length > 0}
                    aria-describedby={errors.answerPattern.length > 0 ? patternErrorId : undefined}
                    onChange={(event) => {
                        onChange({ ...draft, answerPattern: event.target.value });
                    }}
                />
                <FieldError id={patternErrorId} messages={errors.answerPattern} />
            </div>
            <div className={styles.field}>
                <label htmlFor={explanationId}>Обяснение</label>
                <textarea
                    id={explanationId}
                    value={draft.explanation}
                    disabled={disabled}
                    aria-invalid={errors.explanation.length > 0}
                    aria-describedby={errors.explanation.length > 0 ? explanationErrorId : undefined}
                    onChange={(event) => {
                        onChange({ ...draft, explanation: event.target.value });
                    }}
                />
                <FieldError id={explanationErrorId} messages={errors.explanation} />
            </div>
        </div>
    );
}
