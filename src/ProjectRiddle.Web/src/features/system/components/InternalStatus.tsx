import type { ReactElement } from "react";
import { useQuery } from "@tanstack/react-query";

import { getInternalStatus } from "../api/internalStatusApi";
import { isApplicationError } from "../../../shared/api/errors";
import styles from "./InternalStatus.module.css";

function describeError(error: unknown): string {
    if (isApplicationError(error)) {
        return error.message;
    }

    if (error instanceof Error) {
        return error.message;
    }

    return "The health check failed.";
}

export function InternalStatus(): ReactElement {
    const statusQuery = useQuery({
        queryKey: ["system", "health"],
        queryFn: ({ signal }) => getInternalStatus(signal),
    });

    return (
        <section className={styles.card} aria-labelledby="internal-status-title">
            <h2 id="internal-status-title">System health</h2>
            {statusQuery.isPending && <p role="status">Checking the API…</p>}
            {statusQuery.isError && <p className={styles.error}>{describeError(statusQuery.error)}</p>}
            {statusQuery.data !== undefined && (
                <>
                    <p>{statusQuery.data.message}</p>
                    <p className={styles.metadata}>UTC: {statusQuery.data.utcDateTime}</p>
                    <p className={styles.metadata}>Local time: {statusQuery.data.localDateTime}</p>
                </>
            )}
        </section>
    );
}
