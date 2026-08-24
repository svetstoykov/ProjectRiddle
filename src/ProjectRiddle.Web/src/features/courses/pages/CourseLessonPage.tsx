import { useQuery } from "@tanstack/react-query";
import type { ReactElement, ReactNode } from "react";
import { useNavigate, useParams } from "react-router-dom";

import { NotFoundPage } from "../../../app/routes/NotFoundPage";
import { isApplicationError } from "../../../shared/api/errors";
import { PlayerPageSkeleton } from "../../../shared/components/ContentSkeletons";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { sessionQueryOptions } from "../../auth/api/sessionQuery";
import { RiddlePlayer } from "../../riddles/components/RiddlePlayer";
import { courseLessonQueryOptions, courseCatalogQueryOptions } from "../api/courseQueries";
import { useCoursePlaySession } from "../api/coursePlaySession";
import { useResolvedCourseProgress } from "../api/courseProgress";
import { CourseLessonHeader } from "../components/CourseLessonHeader";
import { LessonOutcome } from "../components/LessonOutcome";
import { courseMessages, lockedReason, successLine } from "../messages/courseMessages";
import type { CourseLessonSummary } from "../models/courseCatalog";
import styles from "./CourseLessonPage.module.css";

interface LessonFrameProps {
    readonly courseKey: string;
    readonly lesson: CourseLessonSummary;
    readonly ordinal: number;
    readonly total: number;
    readonly children: ReactNode;
}

function LessonFrame({ courseKey, lesson, ordinal, total, children }: LessonFrameProps): ReactElement {
    return (
        <div className={styles.screen}>
            <CourseLessonHeader
                courseKey={courseKey}
                lessonKey={lesson.key}
                title={lesson.title}
                ordinal={ordinal}
                total={total}
            />
            <main className={styles.stage}>{children}</main>
        </div>
    );
}

function failureMessage(error: unknown): string {
    const traceId = isApplicationError(error) ? error.traceId : undefined;
    return traceId === undefined
        ? "Заявката не може да бъде изпълнена."
        : `Заявката не може да бъде изпълнена. Код за проследяване: ${traceId}`;
}

function isMissingLessonOrExercise(error: unknown): boolean {
    if (!isApplicationError(error)) {
        return false;
    }

    return error.code === "courses.lesson.notFound" || error.code === "courses.exercise.notFound";
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

export function CourseLessonPage(): ReactElement {
    const {
        courseKey,
        lessonKey,
        ordinal: ordinalParam,
    } = useParams<{
        courseKey: string;
        lessonKey: string;
        ordinal: string;
    }>();
    const navigate = useNavigate();
    const sessionQuery = useQuery(sessionQueryOptions);
    const catalogQuery = useQuery(courseCatalogQueryOptions());
    const isAuthenticated = (sessionQuery.data ?? null) !== null;
    const course = catalogQuery.data?.courses.find((item) => item.key === courseKey);
    const lessonSummary = course?.lessons.find((lesson) => lesson.key === lessonKey);
    const ordinal = /^\d+$/u.test(ordinalParam ?? "") ? Number(ordinalParam) : undefined;
    const lessonQuery = useQuery({
        ...courseLessonQueryOptions(lessonSummary?.id ?? ""),
        enabled: lessonSummary !== undefined && ordinal !== undefined,
    });
    const resolvedProgress = useResolvedCourseProgress(catalogQuery.data, course, isAuthenticated);
    const detail = lessonQuery.data;
    const exercise = ordinal === undefined ? undefined : detail?.exercises.find((item) => item.ordinal === ordinal);
    const lessonProgress = lessonSummary === undefined ? undefined : resolvedProgress.lessons.get(lessonSummary.key);
    const playableExercise = lessonProgress?.isAvailable === true ? exercise : undefined;
    const courseSession = useCoursePlaySession(playableExercise, isAuthenticated);
    const total = detail?.exercises.length ?? lessonSummary?.exerciseCount ?? 0;

    if (catalogQuery.isPending) {
        return (
            <div className={styles.screen}>
                <main className={styles.stage}>
                    <p className="visuallyHidden" role="status">
                        Зареждаме курса…
                    </p>
                    <PlayerPageSkeleton />
                </main>
            </div>
        );
    }

    if (catalogQuery.isError) {
        return (
            <div className={styles.screen}>
                <main className={styles.stage}>
                    <PageStatus
                        tone="error"
                        eyebrow="Курсове"
                        title="Курсът временно не е достъпен"
                        message={failureMessage(catalogQuery.error)}
                        action={{
                            label: courseMessages.retry,
                            onClick: () => {
                                void catalogQuery.refetch();
                            },
                        }}
                    />
                </main>
            </div>
        );
    }

    if (course === undefined || lessonSummary === undefined || ordinal === undefined) {
        return <NotFoundPage />;
    }

    if (lessonQuery.isError) {
        return (
            <LessonFrame courseKey={course.key} lesson={lessonSummary} ordinal={ordinal} total={total}>
                <DocumentTitle title={lessonSummary.title} />
                <PageStatus
                    tone="error"
                    eyebrow={lessonSummary.title}
                    title={
                        isMissingLessonOrExercise(lessonQuery.error)
                            ? "Урокът не е достъпен"
                            : "Урокът временно не е достъпен"
                    }
                    message={
                        isMissingLessonOrExercise(lessonQuery.error)
                            ? "Това съдържание вече не е налично."
                            : failureMessage(lessonQuery.error)
                    }
                    action={
                        isMissingLessonOrExercise(lessonQuery.error)
                            ? undefined
                            : {
                                  label: courseMessages.retry,
                                  onClick: () => {
                                      void lessonQuery.refetch();
                                  },
                              }
                    }
                />
            </LessonFrame>
        );
    }

    if (lessonQuery.isPending || resolvedProgress.isPending || detail === undefined) {
        return (
            <LessonFrame courseKey={course.key} lesson={lessonSummary} ordinal={ordinal} total={total}>
                <DocumentTitle title={lessonSummary.title} />
                <p className="visuallyHidden" role="status">
                    Зареждаме задачата…
                </p>
                <PlayerPageSkeleton />
            </LessonFrame>
        );
    }

    if (resolvedProgress.error !== undefined) {
        return (
            <LessonFrame courseKey={course.key} lesson={lessonSummary} ordinal={ordinal} total={total}>
                <DocumentTitle title={lessonSummary.title} />
                <PageStatus
                    tone="error"
                    eyebrow={lessonSummary.title}
                    title="Напредъкът временно не е достъпен"
                    message={failureMessage(resolvedProgress.error)}
                    action={{ label: courseMessages.retry, onClick: resolvedProgress.retry }}
                />
            </LessonFrame>
        );
    }

    if (exercise === undefined) {
        return <NotFoundPage />;
    }

    if (lessonProgress?.isAvailable !== true) {
        const missingTitles = prerequisiteTitles(
            catalogQuery.data.courses,
            lessonSummary,
            (key) => resolvedProgress.lessons.get(key)?.isComplete === true,
        );

        return (
            <LessonFrame
                courseKey={course.key}
                lesson={lessonSummary}
                ordinal={ordinal}
                total={detail.exercises.length}
            >
                <DocumentTitle title={lessonSummary.title} />
                <PageStatus
                    eyebrow={lessonSummary.title}
                    title={courseMessages.lockedHeading}
                    message={lockedReason(missingTitles)}
                    action={{
                        label: "Към курса",
                        onClick: () => {
                            void navigate(`/courses/${course.key}`);
                        },
                    }}
                />
            </LessonFrame>
        );
    }

    if (courseSession.error !== undefined) {
        return (
            <LessonFrame
                courseKey={course.key}
                lesson={lessonSummary}
                ordinal={ordinal}
                total={detail.exercises.length}
            >
                <DocumentTitle title={`${lessonSummary.title} · ${ordinal} от ${detail.exercises.length}`} />
                <PageStatus
                    tone="error"
                    eyebrow={lessonSummary.title}
                    title="Задачата временно не е достъпна"
                    message={failureMessage(courseSession.error)}
                    action={{ label: courseMessages.retry, onClick: courseSession.retry }}
                />
            </LessonFrame>
        );
    }

    if (courseSession.isPending || courseSession.playerView === undefined || courseSession.playerState === undefined) {
        return (
            <LessonFrame
                courseKey={course.key}
                lesson={lessonSummary}
                ordinal={ordinal}
                total={detail.exercises.length}
            >
                <DocumentTitle title={`${lessonSummary.title} · ${ordinal} от ${detail.exercises.length}`} />
                <p className="visuallyHidden" role="status">
                    Зареждаме задачата…
                </p>
                <PlayerPageSkeleton />
            </LessonFrame>
        );
    }

    const completedAfterPlay =
        courseSession.playState !== undefined &&
        courseSession.playState.progress.status !== "inProgress" &&
        lessonProgress !== undefined &&
        !lessonProgress.completedExerciseIds.has(exercise.id)
            ? Math.min(lessonProgress.completedExerciseCount + 1, detail.exercises.length)
            : (lessonProgress?.completedExerciseCount ?? 0);
    const isInProgress = courseSession.playState?.progress.status === "inProgress";
    const actionLabel =
        exercise.ordinal < detail.exercises.length
            ? "Напред"
            : lessonSummary.kind === "finalMix"
              ? "Готово"
              : "Към курса";
    const actionTo =
        exercise.ordinal < detail.exercises.length
            ? `/courses/${course.key}/${lessonSummary.key}/${exercise.ordinal + 1}`
            : `/courses/${course.key}`;

    return (
        <LessonFrame courseKey={course.key} lesson={lessonSummary} ordinal={ordinal} total={detail.exercises.length}>
            <DocumentTitle title={`${lessonSummary.title} · ${ordinal} от ${detail.exercises.length}`} />
            {isInProgress && exercise.setup === undefined ? null : isInProgress ? (
                <p className={styles.setup}>{exercise.setup}</p>
            ) : null}
            <RiddlePlayer
                play={courseSession.playerView}
                playState={courseSession.playerState}
                terminalSupplement={
                    <LessonOutcome
                        completed={completedAfterPlay}
                        total={detail.exercises.length}
                        message={successLine(
                            lessonSummary.title,
                            lessonSummary.kind,
                            exercise.ordinal,
                            detail.exercises.length,
                        )}
                        teachingNote={courseSession.teachingNote}
                        actionLabel={actionLabel}
                        actionTo={actionTo}
                    />
                }
                pendingHint={courseSession.pendingHint}
                isSubmitting={courseSession.isSubmitting}
                isRevealing={courseSession.isRevealing}
                onSubmitAnswer={courseSession.submitAnswer}
                onUseHint={courseSession.useHint}
                onRevealLetter={courseSession.revealLetter}
            />
        </LessonFrame>
    );
}
