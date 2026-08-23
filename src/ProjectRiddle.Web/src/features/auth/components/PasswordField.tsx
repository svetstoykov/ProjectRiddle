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
    const toggleLabel = visible ? "Скрий паролата" : "Покажи паролата";

    return (
        <div className={styles.field}>
            <label htmlFor={id}>{label}</label>
            <div className={styles.passwordRow}>
                <input
                    id={id}
                    type={visible ? "text" : "password"}
                    className={styles.passwordInput}
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
                    aria-label={toggleLabel}
                    aria-pressed={visible}
                    aria-controls={id}
                    title={toggleLabel}
                    disabled={disabled}
                    onClick={() => {
                        setVisible((current) => !current);
                    }}
                >
                    <svg className={styles.icon} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                        <path d="M1.8 12S5.9 5.4 12 5.4 22.2 12 22.2 12 18.1 18.6 12 18.6 1.8 12 1.8 12Z" />
                        <circle cx="12" cy="12" r="3.2" />
                        {visible ? <path d="M4 4 20 20" /> : null}
                    </svg>
                </button>
            </div>
            <FieldError id={errorId} messages={errors} />
        </div>
    );
}
