import { useQuery } from "@tanstack/react-query";
import { useRef, useState, type ReactElement } from "react";

import { isApplicationError } from "../../../shared/api/errors";
import { CourseCarouselSkeleton, HomePageSkeleton } from "../../../shared/components/ContentSkeletons";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { sessionQueryOptions } from "../../auth/api/sessionQuery";
import { courseCatalogQueryOptions } from "../../courses/api/courseQueries";
import { useLearningStatus } from "../../courses/api/learningStatus";
import { courseMessages } from "../../courses/messages/courseMessages";
import { BeginnerInvitation } from "../../courses/components/BeginnerInvitation";
import { CourseCarousel } from "../../courses/components/CourseCarousel";
import { dismissInvitation, recordCourseStart } from "../../courses/storage/anonymousCourseProgress";
import { riddleWeekQueryOptions, todayRiddleQueryOptions } from "../api/riddleQueries";
import { TodayCard } from "../components/TodayCard";
import { todayAlternatives } from "../models/todayAlternatives";
import { WeekStrip } from "../components/WeekStrip";
import styles from "./HomePage.module.css";

export function HomePage(): ReactElement {
    const sessionQuery = useQuery(sessionQueryOptions);
    const weekQuery = useQuery(riddleWeekQueryOptions());
    const todayQuery = useQuery(todayRiddleQueryOptions());
    const catalogQuery = useQuery(courseCatalogQueryOptions());
    const isAuthenticated = (sessionQuery.data ?? null) !== null;
    const learning = useLearningStatus(!sessionQuery.isPending, isAuthenticated);
    const [isInvitationClosed, setIsInvitationClosed] = useState(false);
    const todayHeadingRef = useRef<HTMLHeadingElement | null>(null);
    const todayUnavailable =
        isApplicationError(todayQuery.error) && todayQuery.error.code === "riddles.today.unavailable";
    // The invitation waits for progress to resolve, and anyone who has answered it or already begun a course keeps
    // the plain home screen.
    const isInvitationOpen =
        learning.isResolved && !learning.invitationDismissed && !learning.hasStartedLearning && !isInvitationClosed;
    const closeInvitation = (): void => {
        dismissInvitation();
        setIsInvitationClosed(true);
    };
    const startBasics = (): void => {
        recordCourseStart();
        closeInvitation();
    };

    if (weekQuery.isError) {
        return (
            <>
                <DocumentTitle title="Начало" />
                <PageStatus
                    tone="error"
                    eyebrow="Начало"
                    title="Седмицата не се зарежда."
                    message="Нещо се обърка от наша страна."
                    action={{
                        label: "Пробвай пак",
                        onClick: () => {
                            void weekQuery.refetch();
                        },
                    }}
                />
            </>
        );
    }

    if (todayQuery.isError && !todayUnavailable) {
        return (
            <>
                <DocumentTitle title="Начало" />
                <PageStatus
                    tone="error"
                    eyebrow="Начало"
                    title="Днешната криптика не се зарежда."
                    message="Нещо се обърка от наша страна."
                    action={{
                        label: "Пробвай пак",
                        onClick: () => {
                            void todayQuery.refetch();
                        },
                    }}
                />
            </>
        );
    }

    // The pending gate is written as a data check rather than `isPending`, because a disjunction of two queries'
    // pending flags does not narrow `weekQuery.data` to a defined value.
    if (weekQuery.data === undefined || todayQuery.isPending) {
        return (
            <>
                <DocumentTitle title="Начало" />
                <p className="visuallyHidden" role="status">
                    Зареждаме седмицата…
                </p>
                <HomePageSkeleton />
            </>
        );
    }

    return (
        <div className={styles.page}>
            <DocumentTitle title="Начало" />
            <TodayCard
                today={todayQuery.data}
                isUnavailable={todayUnavailable}
                alternatives={todayAlternatives(isAuthenticated, learning.hasStartedLearning)}
                headingRef={todayHeadingRef}
            />
            <WeekStrip week={weekQuery.data} isAuthenticated={isAuthenticated} />
            {catalogQuery.isPending ? (
                <section aria-label={courseMessages.homeHeading}>
                    <p className="visuallyHidden" role="status">
                        Зареждаме курсовете…
                    </p>
                    <CourseCarouselSkeleton />
                </section>
            ) : catalogQuery.isError || catalogQuery.data.courses.length === 0 ? (
                <PageStatus
                    tone="error"
                    title="Курсовете не се зареждат."
                    message="Нещо се обърка от наша страна."
                    action={{
                        label: courseMessages.retry,
                        onClick: () => {
                            void catalogQuery.refetch();
                        },
                    }}
                />
            ) : (
                <CourseCarousel
                    courses={catalogQuery.data.courses}
                    heading={courseMessages.homeHeading}
                    lead={courseMessages.homeLead}
                />
            )}
            <BeginnerInvitation
                open={isInvitationOpen}
                onStart={startBasics}
                onDismiss={closeInvitation}
                returnFocusRef={todayHeadingRef}
            />
        </div>
    );
}
