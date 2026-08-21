import { queryOptions } from "@tanstack/react-query";

import { adminRiddlesApi } from "./adminRiddlesApi";

export const adminRiddleKeys = {
    all: ["admin", "riddles"] as const,
    lists: () => ["admin", "riddles", "list"] as const,
    list: () => ["admin", "riddles", "list", "all"] as const,
    details: () => ["admin", "riddles", "detail"] as const,
    detail: (id: string) => ["admin", "riddles", "detail", id] as const,
};

export function adminRiddleListQueryOptions() {
    return queryOptions({
        queryKey: adminRiddleKeys.list(),
        queryFn: adminRiddlesApi.list,
    });
}

export function adminRiddleDetailQueryOptions(id: string) {
    return queryOptions({
        queryKey: adminRiddleKeys.detail(id),
        queryFn: () => adminRiddlesApi.get(id),
    });
}
