import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, type ReactElement } from "react";
import { Link } from "react-router-dom";

import { reconcileSessionAfterAuthorizationFailure } from "../../auth/api/sessionQuery";
import { isApplicationError } from "../../../shared/api/errors";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
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
            <>
                <DocumentTitle title="Администрация" />
                <PageStatus eyebrow="Администрация" title="Зареждаме криптиките…" message="Изчакваме списъка." />
            </>
        );
    }

    if (authorizationFailure) {
        return (
            <>
                <DocumentTitle title="Администрация" />
                <PageStatus
                    tone="error"
                    eyebrow="Администрация"
                    title="Няма достъп до списъка"
                    message="Влез отново или провери правата си."
                />
            </>
        );
    }

    if (listQuery.isError) {
        const message = isApplicationError(listQuery.error)
            ? unknownRiddleFailure(listQuery.error.traceId)
            : unknownRiddleFailure(undefined);

        return (
            <>
                <DocumentTitle title="Администрация" />
                <PageStatus
                    tone="error"
                    eyebrow="Администрация"
                    title="Списъкът не се зарежда."
                    message={message}
                    action={{
                        label: "Пробвай пак",
                        onClick: () => {
                            void listQuery.refetch();
                        },
                    }}
                />
            </>
        );
    }

    return (
        <section aria-labelledby="admin-riddle-list-title">
            <DocumentTitle title="Администрация" />
            <div className={styles.pageHeader}>
                <div className={styles.titles}>
                    <p className="eyebrow">Администрация</p>
                    <h1 id="admin-riddle-list-title">Криптики</h1>
                </div>
                <Link className="button" to="/admin/riddles/new">
                    Нова криптика
                </Link>
            </div>
            {listQuery.data.length === 0 ? (
                <p role="status">Още няма криптики.</p>
            ) : (
                <AdminRiddleList riddles={listQuery.data} />
            )}
        </section>
    );
}
