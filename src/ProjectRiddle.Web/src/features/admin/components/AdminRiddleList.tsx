import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import { publicationStateLabels } from "../messages/adminMessages";
import type { Riddle, RiddlePublicationState } from "../models/adminRiddle";
import styles from "./AdminRiddleList.module.css";

const groupOrder = ["published", "scheduled", "draft", "unpublished"] as const;

const groupHeadings: Record<(typeof groupOrder)[number], string> = {
    published: "Публикувани",
    scheduled: "Насрочени",
    draft: "Чернови",
    unpublished: "Свалени",
};

const groupEmptyMessages: Record<(typeof groupOrder)[number], string> = {
    published: "Няма публикувани загадки.",
    scheduled: "Няма насрочени загадки.",
    draft: "Няма чернови.",
    unpublished: "Няма свалени загадки.",
};

const updatedFormatter = new Intl.DateTimeFormat("bg-BG", {
    dateStyle: "medium",
    timeStyle: "short",
    timeZone: "Europe/Sofia",
});

export interface AdminRiddleListProps {
    readonly riddles: readonly Riddle[];
}

function excerpt(clue: string): string {
    const normalized = clue.replace(/\s+/g, " ").trim();
    return normalized.length <= 80 ? normalized : `${normalized.slice(0, 80)}…`;
}

function compareRiddles(left: Riddle, right: Riddle): number {
    if (left.sofiaPublicationDate !== null && right.sofiaPublicationDate !== null) {
        if (left.sofiaPublicationDate !== right.sofiaPublicationDate) {
            return left.sofiaPublicationDate < right.sofiaPublicationDate ? -1 : 1;
        }
    } else if (left.sofiaPublicationDate !== null) {
        return -1;
    } else if (right.sofiaPublicationDate !== null) {
        return 1;
    }

    if (left.updatedAtUtc !== right.updatedAtUtc) {
        return left.updatedAtUtc > right.updatedAtUtc ? -1 : 1;
    }

    if (left.id === right.id) {
        return 0;
    }

    return left.id < right.id ? -1 : 1;
}

function groupRiddles(riddles: readonly Riddle[]): Record<(typeof groupOrder)[number], Riddle[]> {
    const grouped: Record<(typeof groupOrder)[number], Riddle[]> = {
        published: [],
        scheduled: [],
        draft: [],
        unpublished: [],
    };

    for (const riddle of riddles) {
        grouped[riddle.publicationState].push(riddle);
    }

    for (const state of groupOrder) {
        grouped[state].sort(compareRiddles);
    }

    return grouped;
}

function dateLabel(state: RiddlePublicationState): string {
    if (state === "unpublished") {
        return "Предишна дата";
    }

    return "Дата в София";
}

export function AdminRiddleList({ riddles }: AdminRiddleListProps): ReactElement {
    const grouped = groupRiddles(riddles);

    return (
        <div className={styles.groups}>
            {groupOrder.map((state) => {
                const items = grouped[state];
                return (
                    <section key={state} className={styles.group} aria-labelledby={`admin-riddle-group-${state}`}>
                        <div className={styles.groupHeader}>
                            <h2 id={`admin-riddle-group-${state}`}>{groupHeadings[state]}</h2>
                            <p className={styles.count}>{items.length}</p>
                        </div>
                        {items.length === 0 ? (
                            <p className={styles.empty}>{groupEmptyMessages[state]}</p>
                        ) : (
                            <table className={styles.table}>
                                <thead>
                                    <tr>
                                        <th>Условие</th>
                                        <th>Шаблон</th>
                                        <th>Състояние</th>
                                        <th>{dateLabel(state)}</th>
                                        <th>Обновена</th>
                                        <th>Действие</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {items.map((riddle) => {
                                        const clueExcerpt = excerpt(riddle.clue);
                                        return (
                                            <tr key={riddle.id}>
                                                <td data-label="Условие">{clueExcerpt}</td>
                                                <td data-label="Шаблон">{riddle.answerPattern}</td>
                                                <td data-label="Състояние">
                                                    <span className={styles.chip}>
                                                        {publicationStateLabels[riddle.publicationState]}
                                                    </span>
                                                </td>
                                                <td data-label={dateLabel(state)}>
                                                    {riddle.sofiaPublicationDate ?? "—"}
                                                </td>
                                                <td data-label="Обновена">
                                                    {updatedFormatter.format(new Date(riddle.updatedAtUtc))}
                                                </td>
                                                <td data-label="Действие">
                                                    <Link
                                                        to={`/admin/riddles/${riddle.id}`}
                                                        aria-label={`Редактирай: ${clueExcerpt}`}
                                                    >
                                                        Редактирай
                                                    </Link>
                                                </td>
                                            </tr>
                                        );
                                    })}
                                </tbody>
                            </table>
                        )}
                    </section>
                );
            })}
        </div>
    );
}
