import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { Outlet } from "react-router-dom";

import { sessionQueryOptions } from "../../features/auth/api/sessionQuery";
import { ClueBlockSkeleton } from "../../shared/components/ContentSkeletons";
import { PageStatus } from "../../shared/components/PageStatus";

export function SessionBoundary(): ReactElement {
    const sessionQuery = useQuery(sessionQueryOptions);

    if (sessionQuery.isPending) {
        return (
            <>
                <p className="visuallyHidden" role="status">
                    Проверяваме сесията…
                </p>
                <ClueBlockSkeleton />
            </>
        );
    }

    if (sessionQuery.isError) {
        return (
            <PageStatus
                tone="error"
                eyebrow="Сесия"
                title="Сесията временно не е достъпна"
                message="Неуспешна проверка на текущия профил. Опитайте отново."
                action={{
                    label: "Опитай отново",
                    onClick: () => {
                        void sessionQuery.refetch();
                    },
                }}
            />
        );
    }

    return <Outlet />;
}
