import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, type ReactElement } from "react";
import { Link } from "react-router-dom";

import { reconcileSessionAfterAuthorizationFailure } from "../../auth/api/sessionQuery";
import { isApplicationError } from "../../../shared/api/errors";
import { PageStatus } from "../../../shared/components/PageStatus";
import { AdminRiddleList } from "../components/AdminRiddleList";
import { adminRiddleListQueryOptions } from "../api/adminRiddleQueries";
import { unknownRiddleFailure } from "../messages/adminMessages";
import styles from "../components/AdminRiddleList.module.css";

export function AdminRiddleListPage(): ReactElement {
    const queryClient = useQueryClient();
    const listQuery = useQuery(adminRiddleListQueryOptions());
    const authorizationFailure =
        isApplicationError(listQuery.error) && (listQuery.error.status === 401 || listQuery.error.status === 403);

    useEffect(() => {
        if (listQuery.error !== null && authorizationFailure) {
            reconcileSessionAfterAuthorizationFailure(queryClient, listQuery.error);
        }
    }, [authorizationFailure, listQuery.error, queryClient]);

    if (listQuery.isPending) {
        return (
            <PageStatus
                eyebrow="Администрация"
                title="Зареждаме загадките…"
                message="Изчакваме списъка с административни загадки."
            />
        );
    }

    if (authorizationFailure) {
        return (
            <PageStatus
                tone="error"
                eyebrow="Администрация"
                title="Няма достъп до списъка"
                message="Сесията или правата трябва да бъдат проверени отново."
            />
        );
    }

    if (listQuery.isError) {
        const message = isApplicationError(listQuery.error)
            ? unknownRiddleFailure(listQuery.error.traceId)
            : unknownRiddleFailure(undefined);

        return (
            <PageStatus
                tone="error"
                eyebrow="Администрация"
                title="Списъкът временно не е достъпен"
                message={message}
                action={{
                    label: "Опитай отново",
                    onClick: () => {
                        void listQuery.refetch();
                    },
                }}
            />
        );
    }

    return (
        <section aria-labelledby="admin-riddle-list-title">
            <div className={styles.pageHeader}>
                <div>
                    <p className="eyebrow">Администрация</p>
                    <h1 id="admin-riddle-list-title">Загадки</h1>
                </div>
                <Link className="button" to="/admin/riddles/new">
                    Нова загадка
                </Link>
            </div>
            {listQuery.data.length === 0 ? (
                <p role="status">Все още няма загадки.</p>
            ) : (
                <AdminRiddleList riddles={listQuery.data} />
            )}
        </section>
    );
}
