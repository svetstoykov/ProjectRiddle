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
    /** Called instead of opening the riddle when the visitor has no account, with the card as the prompt's origin. */
    readonly onAccountRequired: (trigger: HTMLAnchorElement) => void;
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
 * page source, the network response, or the query cache. The card keeps its destination either way and answers a
 * signed-out click with the account prompt rather than a screen the visitor did not ask for.
 */
export function ArchiveCard({ item, outcome, isAuthenticated, onAccountRequired }: ArchiveCardProps): ReactElement {
    const excerpt = item.clueExcerpt;

    return (
        <Link
            className={styles.card}
            to={`/riddles/${item.id}`}
            onClick={
                isAuthenticated
                    ? undefined
                    : (event) => {
                          event.preventDefault();
                          onAccountRequired(event.currentTarget);
                      }
            }
        >
            <span className={styles.date}>{formatFullDate(item.publicationDate)}</span>
            {excerpt === null ? (
                <>
                    <span className={styles.veil} aria-hidden="true">
                        <span className={styles.veilLineWide} />
                        <span className={styles.veilLineMid} />
                        <span className={styles.veilLineShort} />
                    </span>
                    <span className="visuallyHidden">С профил виждаш уликата.</span>
                </>
            ) : (
                <span className={styles.excerpt} lang="bg">
                    {excerpt}
                </span>
            )}
            <span className={styles.meta}>
                <span className={styles.pattern}>Букви: ({item.answerPattern})</span>
                {outcome === undefined ? null : (
                    <span className={outcome === "solved" ? styles.solved : styles.outcome}>
                        {outcomeLabels[outcome]}
                    </span>
                )}
            </span>
        </Link>
    );
}
