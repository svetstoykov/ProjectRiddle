import { useQuery } from "@tanstack/react-query";

import { readAnonymousCourseProgress } from "../storage/anonymousCourseProgress";
import { accountCourseProgressQueryOptions } from "./courseQueries";

export interface LearningStatus {
    /** Whether every source below has answered. Until then callers keep a fallback rather than guessing. */
    readonly isResolved: boolean;
    readonly hasStartedLearning: boolean;
    readonly invitationDismissed: boolean;
}

/**
 * Whether this visitor has already begun the courses: an exercise opened or completed in this browser, or a completion
 * on the signed-in account. Being signed in, or visiting this browser for the first time, says nothing on its own.
 */
export function useLearningStatus(isSessionResolved: boolean, isAuthenticated: boolean): LearningStatus {
    const accountQuery = useQuery(accountCourseProgressQueryOptions(isAuthenticated));
    const stored = readAnonymousCourseProgress();
    const hasAccountCompletion = (accountQuery.data?.completedExerciseIds.length ?? 0) > 0;

    return {
        // A failed account read stays unresolved, so an existing learner is never mistaken for a newcomer.
        isResolved: isSessionResolved && (!isAuthenticated || accountQuery.isSuccess),
        hasStartedLearning: stored.courseStarted || stored.completedExercises.length > 0 || hasAccountCompletion,
        invitationDismissed: stored.invitationDismissed,
    };
}
