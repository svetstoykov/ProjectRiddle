import { useQuery } from "@tanstack/react-query";
import { useRef, useState, type ReactElement } from "react";
import { useParams } from "react-router-dom";

import { NotFoundPage } from "../../../app/routes/NotFoundPage";
import { isApplicationError } from "../../../shared/api/errors";
import { ClueTermText } from "../../../shared/components/ClueTermText";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { sessionQueryOptions } from "../../auth/api/sessionQuery";
import { courseCatalogQueryOptions } from "../api/courseQueries";
import { useResolvedCourseProgress } from "../api/courseProgress";
import { CourseCarousel } from "../components/CourseCarousel";
import { CourseHubSkeleton } from "../components/CourseHubSkeleton";
import { LessonCard } from "../components/LessonCard";
import { CoursePrimerDialog } from "../components/CoursePrimerDialog";
import { courseMessages, lockedReason } from "../messages/courseMessages";
import { carouselLabelByCourseKey, recommendedStartByCourseKey } from "../messages/coursePresentation";
import type { CourseLessonSummary } from "../models/courseCatalog";
import { readAnonymousCourseProgress } from "../storage/anonymousCourseProgress";
import styles from "./CourseHubPage.module.css";

function failureMessage(error: unknown): string {
    const traceId = isApplicationError(error) ? error.traceId : undefined;
    return traceId === undefined ? "Нещо се обърка от наша страна." : `Нещо се обърка от наша страна. Код: ${traceId}`;
}

function lessonProgressFallback(): {
    readonly completedExerciseCount: number;
    readonly completedExerciseIds: ReadonlySet<string>;
    readonly isComplete: boolean;
    readonly isAvailable: boolean;
} {
    return {
        completedExerciseCount: 0,
        completedExerciseIds: new Set(),
        isComplete: false,
        isAvailable: false,
    };
}

function prerequisiteTitles(
    catalogCourses: readonly { readonly lessons: readonly CourseLessonSummary[] }[],
    lesson: CourseLessonSummary,
    isComplete: (lessonKey: string) => boolean,
): readonly string[] {
    const lessonsByKey = new Map(catalogCourses.flatMap((course) => course.lessons).map((item) => [item.key, item]));

    return lesson.prerequisiteLessonKeys
        .filter((key) => !isComplete(key))
        .map((key) => lessonsByKey.get(key)?.title ?? key);
}

export function CourseHubPage(): ReactElement {
    const { courseKey } = useParams<{ courseKey: string }>();
    const sessionQuery = useQuery(sessionQueryOptions);
    const catalogQuery = useQuery(courseCatalogQueryOptions());
    const isAuthenticated = (sessionQuery.data ?? null) !== null;
    const course = catalogQuery.data?.courses.find((item) => item.key === courseKey);
    const headingRef = useRef<HTMLHeadingElement | null>(null);
    const [isPrimerOpen, setIsPrimerOpen] = useState(() => !readAnonymousCourseProgress().primerDismissed);
    const resolvedProgress = useResolvedCourseProgress(catalogQuery.data, course, isAuthenticated);

    if (catalogQuery.isPending || resolvedProgress.isPending) {
        return (
            <>
                <DocumentTitle title="Курсове" />
                <p className="visuallyHidden" role="status">
                    Зареждаме курса…
                </p>
                <CourseHubSkeleton />
            </>
        );
    }

    if (catalogQuery.isError || resolvedProgress.error != null) {
        return (
            <>
                <DocumentTitle title="Курсове" />
                <PageStatus
                    tone="error"
                    eyebrow="Курсове"
                    title="Курсовете не се зареждат."
                    message={failureMessage(catalogQuery.error ?? resolvedProgress.error)}
                    action={{
                        label: courseMessages.retry,
                        onClick: () => {
                            if (catalogQuery.isError) {
                                void catalogQuery.refetch();
                            }
                            resolvedProgress.retry();
                        },
                    }}
                />
            </>
        );
    }

    if (catalogQuery.data.courses.length === 0) {
        return (
            <>
                <DocumentTitle title="Курсове" />
                <PageStatus eyebrow="Курсове" title="Още няма курсове." message="Наминавай пак — работим по тях." />
            </>
        );
    }

    if (course === undefined) {
        return <NotFoundPage />;
    }

    const techniqueLessons = course.lessons.filter((lesson) => lesson.kind === "technique");
    const mixLesson = course.lessons.find((lesson) => lesson.kind === "mix" || lesson.kind === "finalMix");
    const mixProgress =
        mixLesson === undefined ? undefined : (resolvedProgress.lessons.get(mixLesson.key) ?? lessonProgressFallback());
    const mixLockedTitles =
        mixLesson === undefined
            ? []
            : prerequisiteTitles(
                  catalogQuery.data.courses,
                  mixLesson,
                  (key) => resolvedProgress.lessons.get(key)?.isComplete === true,
              );
    const mixLockedReason = lockedReason(mixLockedTitles);
    const mixGlyphKeys = mixLesson?.prerequisiteLessonKeys.filter((key) => key !== "basics") ?? [];
    const recommendedStart = recommendedStartByCourseKey[course.key];

    return (
        <div className={styles.page}>
            <DocumentTitle title={course.title} />
            <header className={styles.intro}>
                <p className="eyebrow">Курс {course.ordinal}</p>
                <h1 ref={headingRef} tabIndex={-1}>
                    {course.title}
                </h1>
                <p>
                    <ClueTermText text={course.intro} />
                </p>
            </header>
            {course.key === "finale" ? null : (
                <section aria-labelledby="technique-lessons-heading" className={styles.section}>
                    <h2 id="technique-lessons-heading">{courseMessages.chooseStart}</h2>
                    {recommendedStart === undefined ? null : (
                        <p className={styles.lead}>
                            <ClueTermText text={recommendedStart} />
                        </p>
                    )}
                    <div className={styles.lessonGrid}>
                        {techniqueLessons.map((lesson) => (
                            <LessonCard
                                key={lesson.id}
                                courseKey={course.key}
                                lesson={lesson}
                                progress={resolvedProgress.lessons.get(lesson.key) ?? lessonProgressFallback()}
                                glyphKeys={[lesson.key]}
                            />
                        ))}
                    </div>
                </section>
            )}
            {mixLesson === undefined || mixProgress === undefined ? null : (
                <section aria-labelledby="mix-lesson-heading" className={styles.section}>
                    <h2 id="mix-lesson-heading">
                        {mixProgress.isComplete ? (
                            courseMessages.completionHeading
                        ) : mixProgress.isAvailable ? (
                            <ClueTermText text={mixLesson.title} />
                        ) : (
                            courseMessages.lockedHeading
                        )}
                    </h2>
                    {mixProgress.isComplete ? (
                        <p className={styles.lead}>{courseMessages.completionLead}</p>
                    ) : mixProgress.isAvailable ? null : (
                        <p className={styles.lead}>
                            <ClueTermText text={mixLockedReason} />
                        </p>
                    )}
                    <LessonCard
                        courseKey={course.key}
                        lesson={mixLesson}
                        progress={mixProgress}
                        glyphKeys={mixGlyphKeys}
                        lockedReason={mixLockedReason}
                    />
                </section>
            )}
            <CourseCarousel
                courses={catalogQuery.data.courses.filter((item) => item.id !== course.id)}
                heading={carouselLabelByCourseKey[course.key] ?? "Още курсове"}
            />
            <CoursePrimerDialog
                open={isPrimerOpen}
                onDismiss={() => {
                    setIsPrimerOpen(false);
                }}
                returnFocusRef={headingRef}
            />
        </div>
    );
}
