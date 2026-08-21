import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { Outlet } from "react-router-dom";

import { sessionQueryOptions } from "../../features/auth/api/sessionQuery";
import { PageStatus } from "../../shared/components/PageStatus";

export function SessionBoundary(): ReactElement {
    const sessionQuery = useQuery(sessionQueryOptions);

    if (sessionQuery.isPending) {
        return (
            <PageStatus eyebrow="Сесия" title="Проверяваме сесията…" message="Изчакваме отговора за текущия профил." />
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
