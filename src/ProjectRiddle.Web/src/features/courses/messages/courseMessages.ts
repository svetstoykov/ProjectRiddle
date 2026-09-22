import type { CourseLessonKind } from "../models/courseCatalog";
import type { LessonRecommendation } from "../models/courseProgress";

export const courseMessages = {
    homeHeading: "Започни оттук",
    homeLead: "Избери курс, с който да започнеш",
    chooseStart: "Избери начало",
    lockedHeading: "Заключено засега",
    completionHeading: "Курсът е завършен",
    completionLead: "Можеш да повториш смесената практика или да избереш друг курс.",
    primerLabel: "Увод в уликите",
    retry: "Пробвай пак",
    startCue: "Начало",
    continueCue: "Продължи",
    techniquesComplete: "Всички техники са готови.",
} as const;

/** The hub's lead sentence, which always names the card that carries the recommendation cue. */
export function recommendationLead(recommendation: LessonRecommendation, recommendedStart: string | undefined): string {
    if (recommendation.lesson.kind !== "technique") {
        return "Техниките са готови. Продължи със смесената практика.";
    }

    if (recommendation.cue === "start") {
        return recommendedStart ?? `Започни с „${recommendation.lesson.title}“.`;
    }

    return `Продължи с „${recommendation.lesson.title}“.`;
}

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
