export type UserRole = "user" | "admin";

export interface Session {
    readonly id: string;
    readonly email: string;
    readonly role: UserRole;
}

export interface RegisterRequest {
    readonly email: string;
    readonly password: string;
}

export interface SignInRequest {
    readonly email: string;
    readonly password: string;
}
