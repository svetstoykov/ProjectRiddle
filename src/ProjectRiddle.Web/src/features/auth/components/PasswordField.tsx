import { useId, useState, type ReactElement } from "react";

import { FieldError } from "../../../shared/components/FieldError";
import styles from "./AccountForm.module.css";

export interface PasswordFieldProps {
    readonly id: string;
    readonly label: string;
    readonly value: string;
    readonly disabled: boolean;
    readonly errors: readonly string[];
    readonly autoComplete?: string;
    readonly onChange: (value: string) => void;
}

export function PasswordField({
    id,
    label,
    value,
    disabled,
    errors,
    autoComplete = "current-password",
    onChange,
}: PasswordFieldProps): ReactElement {
    const [visible, setVisible] = useState(false);
    const errorId = useId();
    const hasErrors = errors.length > 0;

    return (
        <div className={styles.field}>
            <label htmlFor={id}>{label}</label>
            <div className={styles.passwordRow}>
                <input
                    id={id}
                    type={visible ? "text" : "password"}
                    autoComplete={autoComplete}
                    value={value}
                    disabled={disabled}
                    aria-invalid={hasErrors}
                    aria-describedby={hasErrors ? errorId : undefined}
                    onChange={(event) => {
                        onChange(event.target.value);
                    }}
                />
                <button
                    type="button"
                    className={styles.visibility}
                    aria-pressed={visible}
                    aria-controls={id}
                    disabled={disabled}
                    onClick={() => {
                        setVisible((current) => !current);
                    }}
                >
                    {visible ? "Скрий паролата" : "Покажи паролата"}
                </button>
            </div>
            <FieldError id={errorId} messages={errors} />
        </div>
    );
}
