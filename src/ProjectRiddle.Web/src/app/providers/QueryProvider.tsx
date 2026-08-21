import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { PropsWithChildren, ReactElement } from "react";

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            retry: 1,
            staleTime: 30_000,
            refetchOnWindowFocus: false,
        },
        mutations: {
            retry: false,
        },
    },
});

export function QueryProvider({ children }: PropsWithChildren): ReactElement {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}
