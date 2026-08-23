import { useRef, type ReactElement, type RefObject } from "react";
import { Link } from "react-router-dom";

import { InfoDialog } from "../../../shared/components/InfoDialog";
import { safeReturnPath } from "../routing/returnPath";
import styles from "./MembershipDialog.module.css";

export interface MembershipDialogProps {
    /** The path the visitor tried to open, kept so signing in lands them on it. `null` keeps the prompt closed. */
    readonly returnTo: string | null;
    readonly onClose: () => void;
    /** The control the prompt was opened from, when there is one. Focus returns to it on dismissal. */
    readonly returnFocusRef?: RefObject<HTMLElement | null>;
}

/**
 * The single answer to "this needs an account": a centred prompt over the blurred page the visitor is already on, so
 * a locked riddle costs them neither their place nor a screen of its own. The prompt is a courtesy; the server is
 * what withholds the riddle.
 */
export function MembershipDialog({ returnTo, onClose, returnFocusRef }: MembershipDialogProps): ReactElement {
    const unopenedRef = useRef<HTMLElement>(null);
    const target = encodeURIComponent(safeReturnPath(returnTo));

    return (
        <InfoDialog
            open={returnTo !== null}
            title="Нужен е профил"
            onClose={onClose}
            returnFocusRef={returnFocusRef ?? unopenedRef}
        >
            <p>
                За да отвориш тази загадка, трябва да си член. Днешната загадка е свободна за всички, а предишните дни
                се играят с профил, в който напредъкът ти остава запазен.
            </p>
            <div className={styles.actions}>
                <Link className="button" to={`/register?returnTo=${target}`}>
                    Регистрация
                </Link>
                <Link className="button buttonSecondary" to={`/sign-in?returnTo=${target}`}>
                    Вход
                </Link>
            </div>
        </InfoDialog>
    );
}
