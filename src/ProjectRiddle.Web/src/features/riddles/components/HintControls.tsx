import type { ReactElement } from "react";

import type { RiddleRangeKind } from "../models/riddleRange";
import styles from "./HintControls.module.css";

export interface HintControlsProps {
    readonly usedHints: readonly RiddleRangeKind[];
    readonly visibleHints: ReadonlySet<RiddleRangeKind>;
    readonly pendingHint: RiddleRangeKind | undefined;
    readonly disabled: boolean;
    readonly onUnlock: (kind: RiddleRangeKind) => void;
    readonly onToggle: (kind: RiddleRangeKind) => void;
}

const hintOrder: readonly RiddleRangeKind[] = ["definition", "indicator", "fodder"];

const unlockLabels: Record<RiddleRangeKind, string> = {
    definition: "Покажи дефиницията",
    indicator: "Покажи индикатора",
    fodder: "Покажи материала",
};

const usedLabels: Record<RiddleRangeKind, string> = {
    definition: "Дефиниция",
    indicator: "Индикатор",
    fodder: "Материал",
};

export function HintControls({
    usedHints,
    visibleHints,
    pendingHint,
    disabled,
    onUnlock,
    onToggle,
}: HintControlsProps): ReactElement {
    return (
        <div className={styles.controls} role="group" aria-label="Подсказки">
            {hintOrder.map((kind) => {
                const isUsed = usedHints.includes(kind);
                const isPending = pendingHint === kind;

                if (!isUsed) {
                    return (
                        <button
                            key={kind}
                            type="button"
                            className={styles.unlock}
                            aria-busy={isPending}
                            disabled={disabled || isPending}
                            onClick={() => {
                                onUnlock(kind);
                            }}
                        >
                            {isPending ? "Отключваме…" : unlockLabels[kind]}
                        </button>
                    );
                }

                const isVisible = visibleHints.has(kind);

                return (
                    <button
                        key={kind}
                        type="button"
                        className={[styles.chip, isVisible ? styles.shown : undefined].join(" ")}
                        aria-pressed={isVisible}
                        onClick={() => {
                            onToggle(kind);
                        }}
                    >
                        {usedLabels[kind]}
                        <span className={styles.chipState}>{isVisible ? "показана" : "скрита"}</span>
                    </button>
                );
            })}
        </div>
    );
}
