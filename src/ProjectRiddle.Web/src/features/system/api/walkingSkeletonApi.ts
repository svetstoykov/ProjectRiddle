import { request } from "../../../shared/api/client";

export interface WalkingSkeletonResponse {
    readonly message: string;
    readonly publicationDate: string;
}

export function getWalkingSkeleton(signal?: AbortSignal): Promise<WalkingSkeletonResponse> {
    return request<WalkingSkeletonResponse>({
        method: "GET",
        url: "/api/system/ping",
        signal,
    });
}

export function requestWalkingSkeletonFailure(signal?: AbortSignal): Promise<WalkingSkeletonResponse> {
    return request<WalkingSkeletonResponse>({
        method: "GET",
        url: "/api/system/ping?fail=true",
        signal,
    });
}
