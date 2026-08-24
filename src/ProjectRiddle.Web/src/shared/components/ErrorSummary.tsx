import { useId, type ReactElement, type RefObject } from "react";

import styles from "./ErrorSummary.module.css";

export interface ErrorSummaryProps {
    readonly messages: readonly string[];
    readonly heading?: string;
    readonly headingId?: string;
    readonly summaryRef?: RefObject<HTMLDivElement | null>;
}

export function ErrorSummary({
    messages,
    heading = "Провери следното:",
    headingId,
    summaryRef,
}: ErrorSummaryProps): ReactElement | null {
    const generatedId = useId();
    const labelledBy = headingId ?? generatedId;

    if (messages.length === 0) {
        return null;
    }

    return (
        <div ref={summaryRef} className={styles.summary} tabIndex={-1} role="alert" aria-labelledby={labelledBy}>
            <h2 id={labelledBy}>{heading}</h2>
            <ul>
                {messages.map((message) => (
                    <li key={message}>{message}</li>
                ))}
            </ul>
        </div>
    );
}
