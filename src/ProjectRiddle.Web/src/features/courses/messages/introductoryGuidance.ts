import type { RiddleRangeKind } from "../../riddles/models/riddleRange";
import type { ClueParts } from "../models/clueParts";

/**
 * The next thing to try in the introductory exercise, given what the solver has already opened. That exercise is a
 * rearrangement, so once the indicator is shown the guidance names the operation instead of only pointing at a term.
 * It never says more than the opened hints already show on the board, and it never names the answer.
 */
export function introductoryStep(parts: ClueParts, usedHints: readonly RiddleRangeKind[]): string {
    const { definition, indicator, fodder, wordplay } = parts;
    const definitionShown = usedHints.includes("definition") && definition !== undefined;

    if (usedHints.includes("indicator") && indicator !== undefined && fodder !== undefined) {
        const target = definitionShown ? `в дума, която значи „${definition}“` : "в нова дума";
        return `„${indicator}“ е индикаторът: казва ти да пренаредиш буквите на „${fodder}“ ${target}.`;
    }

    if (usedHints.includes("fodder") && fodder !== undefined) {
        return `„${fodder}“ е материалът: буквите, от които се сглобява отговорът. Индикаторът в „Подсказки“ казва какво да правиш с тях.`;
    }

    if (definitionShown) {
        const rest = wordplay === undefined ? "Останалите думи казват" : `„${wordplay}“ казва`;
        return `„${definition}“ е дефиницията: описва отговора. ${rest} как да го сглобиш. Индикаторът показва как.`;
    }

    return "Започни оттук: кои думи в началото или в края описват отговора? Ако се колебаеш, покажи дефиницията от „Подсказки“.";
}
