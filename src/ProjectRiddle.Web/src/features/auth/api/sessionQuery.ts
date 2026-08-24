import { queryOptions, type QueryClient } from "@tanstack/react-query";

import { clearAntiforgeryToken } from "../../../shared/api/client";
import { isApplicationError } from "../../../shared/api/errors";
import { courseKeys } from "../../courses/api/courseQueries";
import { riddleKeys } from "../../riddles/api/riddleQueries";
import { authApi } from "./authApi";

export const sessionKey = ["session"] as const;

export const sessionQueryOptions = queryOptions({
    queryKey: sessionKey,
    queryFn: authApi.getSession,
    staleTime: 30_000,
});

/**
 * Capability reads are answered for the caller the server sees: the archive withholds account-only clues from a
 * signed-out visitor, and both riddle and course progress belong to the account. Signing in or out therefore
 * discards both capability roots, so an old caller's data cannot remain visible after the session changes.
 */
export function discardSessionScopedQueries(queryClient: QueryClient): void {
    queryClient.removeQueries({ queryKey: riddleKeys.all() });
    queryClient.removeQueries({ queryKey: courseKeys.all() });
}

export function reconcileSessionAfterAuthorizationFailure(queryClient: QueryClient, error: unknown): boolean {
    if (!isApplicationError(error) || (error.status !== 401 && error.status !== 403)) {
        return false;
    }

    clearAntiforgeryToken();
    void queryClient.invalidateQueries({ queryKey: sessionKey });
    return true;
}
