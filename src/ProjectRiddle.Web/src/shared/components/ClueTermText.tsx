import type { ReactElement, ReactNode } from "react";

import styles from "./ClueTermText.module.css";

type ClueTermKind = "definition" | "indicator" | "fodder";

export interface ClueTermTextProps {
    readonly text: string;
}

/**
 * Longest inflected forms first so a definite article or plural ending is not left behind. Lookarounds use letters
 * rather than `\\b`, because JavaScript word boundaries ignore Cyrillic.
 */
const clueTermPattern =
    /(?<!\p{L})(дефинициите|дефиницията|дефиниции|дефиниция|индикаторите|индикаторът|индикатори|индикатора|индикатор|материалите|материалът|материали|материала|материал)(?!\p{L})/giu;

const classByKind: Record<ClueTermKind, string> = {
    definition: styles.definition ?? "definition",
    indicator: styles.indicator ?? "indicator",
    fodder: styles.fodder ?? "fodder",
};

function kindForTerm(term: string): ClueTermKind {
    const folded = term.toLocaleLowerCase("bg");

    if (folded.startsWith("дефиниц")) {
        return "definition";
    }

    if (folded.startsWith("индикатор")) {
        return "indicator";
    }

    return "fodder";
}

/**
 * Paints the three clue-part names, and their inflections, with the same highlight language as the clue.
 */
export function ClueTermText({ text }: ClueTermTextProps): ReactElement {
    const nodes: ReactNode[] = [];
    let lastIndex = 0;

    for (const match of text.matchAll(clueTermPattern)) {
        const value = match[0];
        const start = match.index ?? 0;

        if (start > lastIndex) {
            nodes.push(text.slice(lastIndex, start));
        }

        nodes.push(
            <span key={`${start}-${value}`} className={classByKind[kindForTerm(value)]}>
                {value}
            </span>,
        );
        lastIndex = start + value.length;
    }

    if (lastIndex < text.length) {
        nodes.push(text.slice(lastIndex));
    }

    return <>{nodes}</>;
}
