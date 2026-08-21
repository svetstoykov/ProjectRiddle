import type { ReactElement } from "react";
import { useQuery } from "@tanstack/react-query";

import { getWalkingSkeleton, requestWalkingSkeletonFailure } from "../api/walkingSkeletonApi";
import { ApplicationError, isApplicationError } from "../../../shared/api/errors";
import styles from "./WalkingSkeletonStatus.module.css";

function describeError(error: unknown): string {
    if (isApplicationError(error)) {
        return error.message;
    }

    if (error instanceof Error) {
        return error.message;
    }

    return "The readiness request failed.";
}

export function WalkingSkeletonStatus(): ReactElement {
    const readinessQuery = useQuery({
        queryKey: ["system", "walking-skeleton"],
        queryFn: ({ signal }) => getWalkingSkeleton(signal),
    });
    const failureQuery = useQuery({
        queryKey: ["system", "walking-skeleton-failure"],
        queryFn: ({ signal }) => requestWalkingSkeletonFailure(signal),
        enabled: false,
        retry: false,
    });

    return (
        <section className={styles.card} aria-labelledby="walking-skeleton-title">
            <h2 id="walking-skeleton-title">Walking skeleton</h2>
            {readinessQuery.isPending && <p role="status">Checking the API…</p>}
            {readinessQuery.isError && <p className={styles.error}>{describeError(readinessQuery.error)}</p>}
            {readinessQuery.data !== undefined && (
                <>
                    <p>{readinessQuery.data.message}</p>
                    <p className={styles.metadata}>Publication date: {readinessQuery.data.publicationDate}</p>
                </>
            )}
            {failureQuery.isError && (
                <p className={styles.error} role="alert">
                    Typed API error: {describeError(failureQuery.error)}
                    {failureQuery.error instanceof ApplicationError && failureQuery.error.code !== undefined
                        ? ` (${failureQuery.error.code})`
                        : ""}
                </p>
            )}
            <button className="button buttonSecondary" type="button" onClick={() => void failureQuery.refetch()}>
                Test Problem Details
            </button>
        </section>
    );
}
