function letterCount(word: string): number {
    let count = 0;

    for (const character of word) {
        if (/\p{L}/u.test(character)) {
            count += 1;
        }
    }

    return count;
}

export function deriveAnswerPattern(answer: string): string {
    const collapsed = answer.trim().replace(/\s+/g, " ");
    if (collapsed.length === 0) {
        return "";
    }

    const counts: number[] = [];

    for (const word of collapsed.split(" ")) {
        const count = letterCount(word);
        if (count <= 0) {
            return "";
        }

        counts.push(count);
    }

    return counts.join(",");
}
