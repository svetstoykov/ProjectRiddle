import { queryOptions, type QueryClient } from "@tanstack/react-query";

import { clearAntiforgeryToken } from "../../../shared/api/client";
import { isApplicationError } from "../../../shared/api/errors";
import { riddleKeys } from "../../riddles/api/riddleQueries";
import { authApi } from "./authApi";

export const sessionKey = ["session"] as const;

export const sessionQueryOptions = queryOptions({
    queryKey: sessionKey,
    queryFn: authApi.getSession,
    staleTime: 30_000,
});

/**
 * Riddle reads are answered for the caller the server sees: the archive withholds account-only clues from a signed-out
 * visitor, and progress belongs to the account. Signing in or out therefore discards the riddle cache, so a card is
 * never left veiled after an unlock or readable after a sign-out.
 */
export function discardSessionScopedRiddleQueries(queryClient: QueryClient): void {
    queryClient.removeQueries({ queryKey: riddleKeys.all() });
}

export function reconcileSessionAfterAuthorizationFailure(queryClient: QueryClient, error: unknown): boolean {
    if (!isApplicationError(error) || (error.status !== 401 && error.status !== 403)) {
        return false;
    }

    clearAntiforgeryToken();
    void queryClient.invalidateQueries({ queryKey: sessionKey });
    return true;
}
