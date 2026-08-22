import { useId, type ReactElement } from "react";
import { Link } from "react-router-dom";

import { safeReturnPath } from "../../features/auth/routing/returnPath";
import styles from "./AuthenticationRequiredNotice.module.css";

export interface AuthenticationRequiredNoticeProps {
    readonly title: string;
    readonly message: string;
    readonly returnTo: string;
}

/**
 * Renders the access prompt inline rather than in a modal: it is a page-level outcome, and a modal would compete with
 * the on-screen keyboard on a narrow viewport. The prompt is a navigation convenience; the backend enforces the gate.
 */
export function AuthenticationRequiredNotice({
    title,
    message,
    returnTo,
}: AuthenticationRequiredNoticeProps): ReactElement {
    const titleId = useId();
    const target = encodeURIComponent(safeReturnPath(returnTo));

    return (
        <section className={styles.notice} aria-labelledby={titleId}>
            <p className="eyebrow">Нужен е профил</p>
            <h1 id={titleId}>{title}</h1>
            <p className={styles.message}>{message}</p>
            <div className={styles.actions}>
                <Link className="button" to={`/sign-in?returnTo=${target}`}>
                    Вход
                </Link>
                <Link className="button buttonSecondary" to={`/register?returnTo=${target}`}>
                    Регистрация
                </Link>
            </div>
        </section>
    );
}
