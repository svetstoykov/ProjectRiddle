import type { ReactElement } from "react";

export interface FieldErrorProps {
    readonly id: string;
    readonly messages: readonly string[];
}

export function FieldError({ id, messages }: FieldErrorProps): ReactElement | null {
    return messages.length === 0 ? null : (
        <p id={id} className="fieldError" role="status">
            {messages.join(" ")}
        </p>
    );
}
