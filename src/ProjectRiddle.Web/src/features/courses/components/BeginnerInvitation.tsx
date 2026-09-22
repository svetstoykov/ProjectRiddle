import { useRef, type ReactElement, type RefObject } from "react";
import { Link } from "react-router-dom";

import { InfoDialog } from "../../../shared/components/InfoDialog";
import { introductoryLessonPath } from "../messages/coursePresentation";
import styles from "./BeginnerInvitation.module.css";

export interface BeginnerInvitationProps {
    readonly open: boolean;
    readonly onStart: () => void;
    readonly onDismiss: () => void;
    readonly returnFocusRef: RefObject<HTMLElement | null>;
}

/**
 * The home screen's one invitation to the first practice. Starting is the recommendation, but declining is a plain
 * way back to the home screen, so an experienced solver loses nothing by arriving here first.
 */
export function BeginnerInvitation({
    open,
    onStart,
    onDismiss,
    returnFocusRef,
}: BeginnerInvitationProps): ReactElement {
    const startRef = useRef<HTMLAnchorElement>(null);

    return (
        <InfoDialog
            open={open}
            title="Първата ти криптика?"
            onClose={onDismiss}
            returnFocusRef={returnFocusRef}
            initialFocusRef={startRef}
        >
            <p>
                Започни с кратко въведение и пробвай една улика с подсказки. Ще откриеш как работи играта, докато
                решаваш.
            </p>
            <p className={styles.optional}>Ако вече ги познаваш, днешната улика те чака на началната страница.</p>
            <div className={styles.actions}>
                <Link ref={startRef} className="button" to={introductoryLessonPath} onClick={onStart}>
                    Започни с основите
                </Link>
                <button type="button" className={styles.dismiss} onClick={onDismiss}>
                    Продължи към началната страница
                </button>
            </div>
        </InfoDialog>
    );
}
