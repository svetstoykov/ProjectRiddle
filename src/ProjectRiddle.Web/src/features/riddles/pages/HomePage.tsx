import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import { isApplicationError } from "../../../shared/api/errors";
import { CourseCarouselSkeleton, HomePageSkeleton } from "../../../shared/components/ContentSkeletons";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { sessionQueryOptions } from "../../auth/api/sessionQuery";
import { courseCatalogQueryOptions } from "../../courses/api/courseQueries";
import { courseMessages } from "../../courses/messages/courseMessages";
import { CourseCarousel } from "../../courses/components/CourseCarousel";
import { riddleWeekQueryOptions, todayRiddleQueryOptions } from "../api/riddleQueries";
import { TodayCard } from "../components/TodayCard";
import { WeekStrip } from "../components/WeekStrip";
import styles from "./HomePage.module.css";

export function HomePage(): ReactElement {
    const sessionQuery = useQuery(sessionQueryOptions);
    const weekQuery = useQuery(riddleWeekQueryOptions());
    const todayQuery = useQuery(todayRiddleQueryOptions());
    const catalogQuery = useQuery(courseCatalogQueryOptions());
    const isAuthenticated = (sessionQuery.data ?? null) !== null;
    const todayUnavailable =
        isApplicationError(todayQuery.error) && todayQuery.error.code === "riddles.today.unavailable";

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
            <TodayCard today={todayQuery.data} isUnavailable={todayUnavailable} />
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
        </div>
    );
}
