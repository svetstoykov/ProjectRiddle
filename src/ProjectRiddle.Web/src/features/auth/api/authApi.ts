import { request } from "../../../shared/api/client";
import { isApplicationError } from "../../../shared/api/errors";
import type { RegisterRequest, Session, SignInRequest } from "../models/auth";

export const authApi = {
    register(input: RegisterRequest): Promise<Session> {
        return request<Session>({
            method: "POST",
            url: "/api/auth/register",
            data: input,
            csrf: "omit",
        });
    },

    signIn(input: SignInRequest): Promise<Session> {
        return request<Session>({
            method: "POST",
            url: "/api/auth/sign-in",
            data: input,
            csrf: "omit",
        });
    },

    signOut(): Promise<void> {
        return request<void>({
            method: "POST",
            url: "/api/auth/sign-out",
        });
    },

    async getSession(): Promise<Session | null> {
        try {
            return await request<Session>({
                method: "GET",
                url: "/api/auth/session",
            });
        } catch (error: unknown) {
            if (isApplicationError(error) && error.status === 401) {
                return null;
            }

            throw error;
        }
    },
};
