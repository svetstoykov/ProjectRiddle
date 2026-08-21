import { queryOptions, type QueryClient } from "@tanstack/react-query";

import { clearAntiforgeryToken } from "../../../shared/api/client";
import { isApplicationError } from "../../../shared/api/errors";
import { authApi } from "./authApi";

export const sessionKey = ["session"] as const;

export const sessionQueryOptions = queryOptions({
    queryKey: sessionKey,
    queryFn: authApi.getSession,
    staleTime: 30_000,
});

export function reconcileSessionAfterAuthorizationFailure(queryClient: QueryClient, error: unknown): boolean {
    if (!isApplicationError(error) || (error.status !== 401 && error.status !== 403)) {
        return false;
    }

    clearAntiforgeryToken();
    void queryClient.invalidateQueries({ queryKey: sessionKey });
    return true;
}
