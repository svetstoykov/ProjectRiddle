export const recommendedStartByCourseKey: Readonly<Record<string, string>> = {
    letterplay: "„Основи“ е добро начало.",
    wordplay: "Започни със синонимите.",
    weirdplay: "Започни с преводите.",
};

export const carouselLabelByCourseKey: Readonly<Record<string, string>> = {
    letterplay: "Следващи курсове",
    wordplay: "Още курсове",
    weirdplay: "Предишни курсове",
    finale: "Предишни курсове",
};

export const glyphMarkByLessonKey: Readonly<Record<string, string>> = {
    basics: "?",
    anagrams: "↻",
    selectors: "⌁",
    hiddens: "⊂",
    reversals: "↤",
    synonyms: "≈",
    symbols: "#",
    containers: "□",
    deletions: "−",
    homophones: "≋",
    translations: "А↔A",
    homoglyphs: "Aa",
    "double-definitions": "2×",
    "and-lits": "&",
    rebuses: "▦",
    "letterplay-mix": "Б",
    "wordplay-mix": "Д",
    "weirdplay-mix": "Я",
};

export const glyphMarkForLesson = (lessonKey: string): string => glyphMarkByLessonKey[lessonKey] ?? "•";
