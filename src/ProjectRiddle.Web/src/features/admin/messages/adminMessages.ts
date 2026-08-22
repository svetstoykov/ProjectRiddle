import type { RiddlePublicationState } from "../models/adminRiddle";
import type { RiddleRangeKind } from "../../riddles/models/riddleRange";

const riddleMessages = {
    "riddles.notFound": "Загадката не е намерена.",
    "riddles.clue.invalid": "Въведете валидно условие.",
    "riddles.answer.invalid": "Въведете валиден отговор.",
    "riddles.answer.format.invalid": "Отговорът може да съдържа само букви, разделени с по един интервал.",
    "riddles.explanation.invalid": "Въведете валидно обяснение.",
    "riddles.ranges.invalid": "Етикетите по откъси са невалидни.",
    "riddles.publicationDate.invalid": "Въведете валидна дата на публикуване в София.",
    "riddles.publicationDate.conflict": "Тази дата в София вече е заета.",
    "riddles.transition.invalid": "Това действие не е позволено в текущото състояние.",
    "riddles.delete.notPermitted": "Загадката не може да бъде изтрита в това състояние.",
} as const;

export type RiddleMessageCode = keyof typeof riddleMessages;

export const publicationStateLabels: Record<RiddlePublicationState, string> = {
    draft: "Чернова",
    scheduled: "Насрочена",
    published: "Публикувана",
    unpublished: "Свалена",
};

export const rangeKindLabels: Record<RiddleRangeKind, string> = {
    definition: "Дефиниция",
    indicator: "Индикатор",
    fodder: "Материал",
};

export function riddleMessageForCode(code: string | undefined): string | undefined {
    if (code === undefined) {
        return undefined;
    }

    return Object.hasOwn(riddleMessages, code) ? riddleMessages[code as RiddleMessageCode] : undefined;
}

export function unknownRiddleFailure(traceId: string | undefined): string {
    const fallback = "Заявката не можа да бъде изпълнена.";
    return traceId === undefined ? fallback : `${fallback} Код за проследяване: ${traceId}`;
}
