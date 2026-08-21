import { request } from "../../../shared/api/client";

export interface InternalStatusResponse {
    readonly message: string;
    readonly utcDateTime: string;
    readonly localDateTime: string;
}

export function getInternalStatus(signal?: AbortSignal): Promise<InternalStatusResponse> {
    return request<InternalStatusResponse>({
        method: "GET",
        url: "/api/system/health",
        signal,
    });
}
