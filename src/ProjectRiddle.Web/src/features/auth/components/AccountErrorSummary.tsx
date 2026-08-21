import type { ReactElement, RefObject } from "react";

import { mapAccountErrors } from "./accountErrors";
import styles from "./AccountForm.module.css";

export interface AccountErrorSummaryProps {
    readonly error: unknown;
    readonly fieldNames: readonly string[];
    readonly summaryRef: RefObject<HTMLDivElement | null>;
}

export function AccountErrorSummary({ error, fieldNames, summaryRef }: AccountErrorSummaryProps): ReactElement | null {
    const mapped = mapAccountErrors(error, fieldNames);

    if (mapped.summary.length === 0) {
        return null;
    }

    return (
        <div
            ref={summaryRef}
            className={styles.summary}
            tabIndex={-1}
            role="alert"
            aria-labelledby="account-error-heading"
        >
            <h2 id="account-error-heading">Има проблем със заявката</h2>
            <ul>
                {mapped.summary.map((message) => (
                    <li key={message}>{message}</li>
                ))}
            </ul>
        </div>
    );
}
