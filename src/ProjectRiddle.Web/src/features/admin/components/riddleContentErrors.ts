import { isApplicationError } from "../../../shared/api/errors";
import { riddleMessageForCode, unknownRiddleFailure } from "../messages/adminMessages";
import { deriveAnswerPattern } from "../models/answerPattern";
import { emptyRiddleContentErrors, type RiddleContentErrors, type RiddleDraft } from "../models/riddleDraft";

function fieldMessages(fieldErrors: Record<string, readonly string[]> | undefined, fieldName: string): string[] {
    if (fieldErrors === undefined) {
        return [];
    }

    return Object.entries(fieldErrors)
        .filter(([key]) => key.toLowerCase() === fieldName.toLowerCase())
        .flatMap(([, messages]) => [...messages]);
}

function unique(messages: readonly string[]): string[] {
    return [...new Set(messages.filter((message) => message.length > 0))];
}

export function requiredContentErrors(draft: RiddleDraft): RiddleContentErrors {
    const answer = draft.answer.trim();
    const errors: RiddleContentErrors = {
        clue: draft.clue.trim().length === 0 ? ["Въведете условие."] : [],
        answer:
            answer.length === 0
                ? ["Въведете отговор."]
                : deriveAnswerPattern(answer).length === 0
                  ? ["Въведете валиден отговор."]
                  : [],
        explanation: draft.explanation.trim().length === 0 ? ["Въведете обяснение."] : [],
        ranges: [],
        summary: [],
    };

    return {
        ...errors,
        summary: hasContentErrors(errors) ? ["Попълнете задължителните полета."] : [],
    };
}

export function hasContentErrors(errors: RiddleContentErrors): boolean {
    return (
        errors.clue.length > 0 ||
        errors.answer.length > 0 ||
        errors.explanation.length > 0 ||
        errors.ranges.length > 0 ||
        errors.summary.length > 0
    );
}

export function mapRiddleContentErrors(error: unknown): RiddleContentErrors {
    if (!isApplicationError(error)) {
        return { ...emptyRiddleContentErrors(), summary: [unknownRiddleFailure(undefined)] };
    }

    const mappedCode = riddleMessageForCode(error.code);
    const clue = fieldMessages(error.fieldErrors, "clue");
    const answer = fieldMessages(error.fieldErrors, "answer");
    const explanation = fieldMessages(error.fieldErrors, "explanation");
    const ranges = fieldMessages(error.fieldErrors, "ranges");
    const summary: string[] = [];

    if (error.code === "riddles.clue.invalid" && mappedCode !== undefined) {
        clue.push(mappedCode);
    } else if (error.code === "riddles.answer.invalid" && mappedCode !== undefined) {
        answer.push(mappedCode);
    } else if (error.code === "riddles.explanation.invalid" && mappedCode !== undefined) {
        explanation.push(mappedCode);
    } else if (error.code === "riddles.ranges.invalid" && mappedCode !== undefined) {
        ranges.push(mappedCode);
        summary.push(mappedCode);
    } else if (mappedCode !== undefined) {
        summary.push(mappedCode);
    }

    if (
        clue.length === 0 &&
        answer.length === 0 &&
        explanation.length === 0 &&
        ranges.length === 0 &&
        summary.length === 0
    ) {
        summary.push(unknownRiddleFailure(error.traceId));
    }

    return {
        clue: unique(clue),
        answer: unique(answer),
        explanation: unique(explanation),
        ranges: unique(ranges),
        summary: unique(summary),
    };
}
