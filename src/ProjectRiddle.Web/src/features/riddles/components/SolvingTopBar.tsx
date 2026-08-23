import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import { formatFullDate } from "../models/localDate";
import styles from "./SolvingTopBar.module.css";

export interface SolvingTopBarProps {
    /** The riddle's publication date, once it is known. The bar keeps its height while the riddle is still loading. */
    readonly publicationDate: string | undefined;
}

export function SolvingTopBar({ publicationDate }: SolvingTopBarProps): ReactElement {
    return (
        <header className={styles.bar}>
            <Link className={styles.back} to="/" aria-label="Назад към началото">
                {/* A drawn arrow rather than a glyph: a text arrow sits wherever its font's side bearings put it,
                    which is never quite the middle of a round button. */}
                <svg className={styles.icon} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="M20 12H5M11 5l-7 7 7 7" />
                </svg>
            </Link>
            <p className={styles.date}>{publicationDate === undefined ? "" : formatFullDate(publicationDate)}</p>
        </header>
    );
}
