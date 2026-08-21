import axios, { type AxiosRequestConfig } from "axios";

import { ApplicationError, parseProblemDetails } from "./errors";

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
                return Promise.reject(new ApplicationError(problemDetails));
            }
        }

        return Promise.reject(error);
    },
);

export async function request<T>(config: AxiosRequestConfig): Promise<T> {
    const response = await httpClient.request<T>(config);
    return response.data;
}
