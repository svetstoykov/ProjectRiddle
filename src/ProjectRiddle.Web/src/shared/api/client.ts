import axios, { AxiosHeaders, type AxiosRequestConfig } from "axios";

import { ApplicationError, isInvalidAntiforgeryError, parseProblemDetails } from "./errors";

export interface HttpRequestConfig extends AxiosRequestConfig {
    readonly csrf?: "omit";
}

interface AntiforgeryTokenResponse {
    readonly token: string;
}

let antiforgeryToken: string | undefined;
let antiforgeryRequest: Promise<string> | undefined;

const unsafeMethods = new Set(["DELETE", "PATCH", "POST", "PUT"]);

const httpClient = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL ?? "/",
    withCredentials: true,
    headers: {
        Accept: "application/json",
    },
});

httpClient.interceptors.response.use(
    (response) => response,
    (error: unknown) => {
        if (axios.isAxiosError(error)) {
            const problemDetails = parseProblemDetails(error.response?.data);

            if (problemDetails !== undefined) {
                const applicationError = new ApplicationError(problemDetails);

                if (isInvalidAntiforgeryError(applicationError)) {
                    clearAntiforgeryToken();
                }

                return Promise.reject(applicationError);
            }
        }

        return Promise.reject(error);
    },
);

async function getAntiforgeryToken(): Promise<string> {
    if (antiforgeryToken !== undefined) {
        return antiforgeryToken;
    }

    if (antiforgeryRequest === undefined) {
        antiforgeryRequest = httpClient
            .get<AntiforgeryTokenResponse>("/api/auth/antiforgery")
            .then((response) => {
                antiforgeryToken = response.data.token;
                return antiforgeryToken;
            })
            .finally(() => {
                antiforgeryRequest = undefined;
            });
    }

    return antiforgeryRequest;
}

export function clearAntiforgeryToken(): void {
    antiforgeryToken = undefined;
    antiforgeryRequest = undefined;
}

export async function request<T>(config: HttpRequestConfig): Promise<T> {
    const method = (config.method ?? "GET").toUpperCase();
    const { csrf, headers, ...rest } = config;
    let prepared: AxiosRequestConfig = { ...rest, method, headers };

    if (unsafeMethods.has(method) && csrf !== "omit") {
        const token = await getAntiforgeryToken();
        const axiosHeaders = AxiosHeaders.from(
            headers instanceof AxiosHeaders ? headers : (headers as Record<string, string> | undefined),
        );
        axiosHeaders.set("X-CSRF-TOKEN", token);
        prepared = { ...prepared, headers: axiosHeaders };
    }

    const response = await httpClient.request<T>(prepared);
    return response.data;
}
