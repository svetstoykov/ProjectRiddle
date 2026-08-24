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
}

export function PageStatus({ eyebrow, title, message, tone = "neutral", action }: PageStatusProps): ReactElement {
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
            {action !== undefined ? (
                <button type="button" onClick={action.onClick}>
                    {action.label}
                </button>
            ) : null}
        </section>
    );
}
