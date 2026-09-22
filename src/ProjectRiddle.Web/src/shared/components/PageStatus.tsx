import { useId, type ReactElement } from "react";

import { ClueTermText } from "./ClueTermText";
import styles from "./PageStatus.module.css";

export interface PageStatusAction {
    readonly label: string;
    readonly onClick: () => void;
}

export interface PageStatusProps {
    readonly eyebrow?: string;
    readonly title: string;
    readonly message: string;
    readonly tone?: "neutral" | "error";
    readonly action?: PageStatusAction;
    /** A quieter alternative beside the action. It is shown only when there is a primary action to stand next to. */
    readonly secondaryAction?: PageStatusAction;
}

export function PageStatus({
    eyebrow,
    title,
    message,
    tone = "neutral",
    action,
    secondaryAction,
}: PageStatusProps): ReactElement {
    const titleId = useId();

    return (
        <section className={styles.section} aria-labelledby={titleId} role={tone === "error" ? "alert" : undefined}>
            {eyebrow !== undefined ? (
                <p className="eyebrow">
                    <ClueTermText text={eyebrow} />
                </p>
            ) : null}
            <h1 id={titleId}>
                <ClueTermText text={title} />
            </h1>
            <p className={styles.message}>
                <ClueTermText text={message} />
            </p>
            {action === undefined ? null : secondaryAction === undefined ? (
                <button type="button" onClick={action.onClick}>
                    {action.label}
                </button>
            ) : (
                <div className={styles.actions}>
                    <button type="button" onClick={action.onClick}>
                        {action.label}
                    </button>
                    <button type="button" className="buttonSecondary" onClick={secondaryAction.onClick}>
                        {secondaryAction.label}
                    </button>
                </div>
            )}
        </section>
    );
}
