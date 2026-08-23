const wordLengthPattern = /^\d+$/;
const separator = /\s/u;

export interface AnswerWord {
    readonly wordIndex: number;
    readonly positions: readonly number[];
}

/**
 * Parses a public answer pattern such as `"5,3"` into word lengths. A malformed pattern yields an empty list, which
 * callers render as an unavailable board rather than a partly usable one.
 */
export function parseAnswerPattern(answerPattern: string): readonly number[] {
    const parts = answerPattern.split(",");
    const wordLengths: number[] = [];

    for (const part of parts) {
        const trimmed = part.trim();

        if (!wordLengthPattern.test(trimmed)) {
            return [];
        }

        const wordLength = Number.parseInt(trimmed, 10);

        if (wordLength <= 0) {
            return [];
        }

        wordLengths.push(wordLength);
    }

    return wordLengths;
}

/**
 * Splits a released answer into one character per tile position, dropping the spaces that separate its words. The
 * server releases the answer once a riddle ends, so a finished board can show the solution instead of empty tiles.
 */
export function answerCharacters(answer: string): readonly string[] {
    return Array.from(answer).filter((character) => !separator.test(character));
}

export function letterCountOf(wordLengths: readonly number[]): number {
    return wordLengths.reduce((total, wordLength) => total + wordLength, 0);
}

/**
 * Maps flat letter positions onto words by running offset, so position `n` lands in the word whose cumulative length
 * first exceeds `n`.
 */
export function buildAnswerWords(wordLengths: readonly number[]): readonly AnswerWord[] {
    const words: AnswerWord[] = [];
    let position = 0;

    for (const [wordIndex, wordLength] of wordLengths.entries()) {
        const positions: number[] = [];

        for (let indexInWord = 0; indexInWord < wordLength; indexInWord += 1) {
            positions.push(position);
            position += 1;
        }

        words.push({ wordIndex, positions });
    }

    return words;
}

/**
 * Rebuilds a submission by joining word slices with a single space and uppercasing with Bulgarian casing, which
 * reproduces the server's answer normalization so a correct entry cannot be lost to casing or spacing.
 */
export function buildSubmission(wordLengths: readonly number[], characters: readonly string[]): string {
    return buildAnswerWords(wordLengths)
        .map((word) => word.positions.map((position) => characters[position] ?? "").join(""))
        .join(" ")
        .toLocaleUpperCase("bg");
}
