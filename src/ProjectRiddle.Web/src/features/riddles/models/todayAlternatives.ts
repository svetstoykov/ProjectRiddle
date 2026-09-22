import { introductoryCoursePath, introductoryLessonPath } from "../../courses/messages/coursePresentation";

export interface TodayAlternativeLink {
    readonly label: string;
    readonly to: string;
}

export interface TodayAlternatives {
    readonly message: string;
    readonly primary: TodayAlternativeLink;
    readonly secondary: TodayAlternativeLink;
}

const archivePath = "/archive";

/**
 * What to offer on a day without a published riddle. Practice leads for a newcomer or a signed-out visitor; only a
 * signed-in visitor who has already started learning is sent to the archive first. Older days still need an account,
 * so the signed-out archive entry says so instead of implying it is free to play.
 */
export function todayAlternatives(isAuthenticated: boolean, hasStartedLearning: boolean): TodayAlternatives {
    const practice: TodayAlternativeLink = hasStartedLearning
        ? { label: "Към курсовете", to: introductoryCoursePath }
        : { label: "Започни с основите", to: introductoryLessonPath };

    if (isAuthenticated && hasStartedLearning) {
        return {
            message: "Наминавай пак утре. Дотогава реши някоя от архива или продължи с курсовете.",
            primary: { label: "Към архива", to: archivePath },
            secondary: practice,
        };
    }

    return isAuthenticated
        ? {
              message:
                  "Наминавай пак утре. Дотогава се упражнявай с кратки улики с подсказки или реши някоя от архива.",
              primary: practice,
              secondary: { label: "Към архива", to: archivePath },
          }
        : {
              message:
                  "Наминавай пак утре. Дотогава се упражнявай с кратки улики с подсказки. Архивът се играе с профил.",
              primary: practice,
              secondary: { label: "Разгледай архива", to: archivePath },
          };
}
