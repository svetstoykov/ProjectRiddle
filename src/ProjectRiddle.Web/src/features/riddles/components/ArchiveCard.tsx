import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import { formatFullDate } from "../models/localDate";
import type { PublicRiddleDiscoveryItem } from "../models/publicRiddle";
import type { RiddleProgressStatus } from "../models/riddleProgress";
import styles from "./ArchiveCard.module.css";

export interface ArchiveCardProps {
    readonly item: PublicRiddleDiscoveryItem;
    readonly outcome: RiddleProgressStatus | undefined;
    readonly isAuthenticated: boolean;
}

const outcomeLabels: Record<RiddleProgressStatus, string> = {
    inProgress: "Започната",
    fullyRevealed: "Разкрита",
    solved: "Решена",
};

/**
 * A signed-out visitor sees the day and the letter count, while the clue itself stays veiled: enough to show that the
 * archive holds real riddles, and a reason to make an account. The veil is drawn rather than applied over the text,
 * because the server withholds the excerpt for a riddle the caller cannot open, so there is nothing to uncover in the
 * page source, the network response, or the query cache.
 */
export function ArchiveCard({ item, outcome, isAuthenticated }: ArchiveCardProps): ReactElement {
    const excerpt = item.clueExcerpt;

    return (
        <Link
            className={styles.card}
            to={`/riddles/${item.id}`}
            state={isAuthenticated ? undefined : { requiresAccount: true }}
        >
            <span className={styles.date}>{formatFullDate(item.publicationDate)}</span>
            {excerpt === null ? (
                <>
                    <span className={styles.veil} aria-hidden="true">
                        <span className={styles.veilLineWide} />
                        <span className={styles.veilLineMid} />
                        <span className={styles.veilLineShort} />
                    </span>
                    <span className="visuallyHidden">Условието се отключва с профил.</span>
                </>
            ) : (
                <span className={styles.excerpt} lang="bg">
                    {excerpt}
                </span>
            )}
            <span className={styles.meta}>
                <span className={styles.pattern}>Брой букви: ({item.answerPattern})</span>
                {outcome === undefined ? null : (
                    <span className={outcome === "solved" ? styles.solved : styles.outcome}>
                        {outcomeLabels[outcome]}
                    </span>
                )}
            </span>
        </Link>
    );
}
