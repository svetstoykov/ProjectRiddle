import { useState, type ReactElement } from "react";

import { CluePresentation } from "../../riddles/components/CluePresentation";
import type { RiddleRangeKind } from "../../riddles/models/riddleRange";
import { rangeKindLabels } from "../messages/adminMessages";
import { deriveAnswerPattern } from "../models/answerPattern";
import type { RiddleRange } from "../models/adminRiddle";
import styles from "./RiddlePreview.module.css";

export interface RiddlePreviewProps {
    readonly clue: string;
    readonly answer: string;
    readonly ranges: readonly Pick<RiddleRange, "kind" | "start" | "end">[];
}

const allKinds: readonly RiddleRangeKind[] = ["definition", "indicator", "fodder"];

export function RiddlePreview({ clue, answer, ranges }: RiddlePreviewProps): ReactElement {
    const [activeKinds, setActiveKinds] = useState<ReadonlySet<RiddleRangeKind>>(() => new Set(allKinds));

    return (
        <section className={styles.preview} aria-labelledby="riddle-preview-title">
            <h2 id="riddle-preview-title">Преглед</h2>
            <div className={styles.toggles}>
                {allKinds.map((kind) => {
                    const pressed = activeKinds.has(kind);
                    return (
                        <button
                            key={kind}
                            type="button"
                            className={pressed ? styles.toggleActive : styles.toggle}
                            aria-pressed={pressed}
                            onClick={() => {
                                setActiveKinds((current) => {
                                    const next = new Set(current);
                                    if (next.has(kind)) {
                                        next.delete(kind);
                                    } else {
                                        next.add(kind);
                                    }
                                    return next;
                                });
                            }}
                        >
                            {rangeKindLabels[kind]}
                        </button>
                    );
                })}
            </div>
            <CluePresentation
                clue={clue}
                answerPattern={deriveAnswerPattern(answer)}
                ranges={ranges}
                activeKinds={activeKinds}
            />
        </section>
    );
}
