import type { RiddleProgressStatus } from "./riddleProgress";

export type RiddleOutcomeStatus = Exclude<RiddleProgressStatus, "inProgress">;

/**
 * The result line of a finished riddle. It confirms a solve without grading it and treats a full reveal as a way into
 * the explanation, so the two states stay distinct without either one reading as a score.
 */
export function outcomeHeadline(status: RiddleOutcomeStatus): string {
    return status === "solved" ? "Точно така!" : "Отговорът е разкрит.";
}

export const revealedOutcomeLead = "Виж как работи уликата.";
