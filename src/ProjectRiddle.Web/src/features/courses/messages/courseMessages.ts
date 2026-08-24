import type { CourseLessonKind } from "../models/courseCatalog";

export const courseMessages = {
    homeHeading: "Започни оттук",
    homeLead: "Избери курс, с който да започнеш",
    chooseStart: "Избери начало",
    lockedHeading: "Заключено засега",
    completionHeading: "Курсът е завършен",
    completionLead: "Можеш да повториш смесената практика или да избереш друг курс.",
    primerLabel: "Увод в уликите",
    retry: "Опитай отново",
} as const;

export function successLine(title: string, kind: CourseLessonKind, ordinal: number, total: number): string {
    if (kind === "finalMix" && ordinal === total) return "Финалният набор е завършен.";
    if (kind === "mix" && ordinal === total) return "Курсът е завършен.";
    if (kind === "mix") return `Смесена практика: ${ordinal} от ${total}.`;
    if (ordinal === total) return `„${title}“ е готово. Избери следващата техника.`;
    if (ordinal === 1) return `Реши първата улика за „${title}“. Следва още.`;
    return `Още една за „${title}“.`;
}

export function lockedReason(prerequisiteTitles: readonly string[]): string {
    return `Първо завърши: ${prerequisiteTitles.join(", ")}.`;
}
