import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import { isApplicationError } from "../../../shared/api/errors";
import { PageStatus } from "../../../shared/components/PageStatus";
import { sessionQueryOptions } from "../../auth/api/sessionQuery";
import { riddleWeekQueryOptions, todayRiddleQueryOptions } from "../api/riddleQueries";
import { TodayCard } from "../components/TodayCard";
import { WeekStrip } from "../components/WeekStrip";
import styles from "./HomePage.module.css";

export function HomePage(): ReactElement {
    const sessionQuery = useQuery(sessionQueryOptions);
    const weekQuery = useQuery(riddleWeekQueryOptions());
    const todayQuery = useQuery(todayRiddleQueryOptions());
    const isAuthenticated = (sessionQuery.data ?? null) !== null;
    const todayUnavailable =
        isApplicationError(todayQuery.error) && todayQuery.error.code === "riddles.today.unavailable";

    if (weekQuery.isError) {
        return (
            <PageStatus
                tone="error"
                eyebrow="Начало"
                title="Седмицата временно не е достъпна"
                message="Заявката не можа да бъде изпълнена."
                action={{
                    label: "Опитай отново",
                    onClick: () => {
                        void weekQuery.refetch();
                    },
                }}
            />
        );
    }

    if (todayQuery.isError && !todayUnavailable) {
        return (
            <PageStatus
                tone="error"
                eyebrow="Начало"
                title="Днешната загадка временно не е достъпна"
                message="Заявката не можа да бъде изпълнена."
                action={{
                    label: "Опитай отново",
                    onClick: () => {
                        void todayQuery.refetch();
                    },
                }}
            />
        );
    }

    // The pending gate is written as a data check rather than `isPending`, because a disjunction of two queries'
    // pending flags does not narrow `weekQuery.data` to a defined value.
    if (weekQuery.data === undefined || todayQuery.isPending) {
        return <PageStatus eyebrow="Начало" title="Зареждаме седмицата…" message="Изчакваме днешната загадка." />;
    }

    return (
        <div className={styles.page}>
            <TodayCard today={todayQuery.data} isUnavailable={todayUnavailable} />
            <WeekStrip week={weekQuery.data} isAuthenticated={isAuthenticated} />
        </div>
    );
}
