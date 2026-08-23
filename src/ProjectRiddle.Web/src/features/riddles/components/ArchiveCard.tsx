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

export function ArchiveCard({ item, outcome, isAuthenticated }: ArchiveCardProps): ReactElement {
    return (
        <Link
            className={styles.card}
            to={`/riddles/${item.id}`}
            state={isAuthenticated ? undefined : { requiresAccount: true }}
        >
            <span className={styles.date}>{formatFullDate(item.publicationDate)}</span>
            <span className={styles.excerpt} lang="bg">
                {item.clueExcerpt}
            </span>
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
