import type { ReactElement, RefObject } from "react";
import { Link } from "react-router-dom";

import { ClueTermText } from "../../../shared/components/ClueTermText";
import styles from "./CourseLessonHeader.module.css";

export interface CourseLessonHeaderProps {
    readonly courseKey: string;
    readonly title: string;
    readonly ordinal: number;
    readonly total: number;
    readonly onOpenIntro?: () => void;
    readonly introTriggerRef?: RefObject<HTMLButtonElement | null>;
}

export function CourseLessonHeader({
    courseKey,
    title,
    ordinal,
    total,
    onOpenIntro,
    introTriggerRef,
}: CourseLessonHeaderProps): ReactElement {
    const hasTechniqueIntro = onOpenIntro !== undefined && introTriggerRef !== undefined;

    return (
        <header className={styles.header}>
            <Link className={styles.close} to={`/courses/${courseKey}`} aria-label="Назад към курса">
                <svg className={styles.icon} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="M20 12H5M11 5l-7 7 7 7" />
                </svg>
            </Link>
            <div className={styles.heading}>
                <h1>
                    <ClueTermText text={title} />
                </h1>
                <p>
                    {ordinal} от {total}
                </p>
            </div>
            {hasTechniqueIntro ? (
                <button
                    ref={introTriggerRef}
                    type="button"
                    className={styles.info}
                    aria-label="За тази техника"
                    onClick={onOpenIntro}
                >
                    i
                </button>
            ) : (
                <span className={styles.infoPlaceholder} aria-hidden="true" />
            )}
        </header>
    );
}
