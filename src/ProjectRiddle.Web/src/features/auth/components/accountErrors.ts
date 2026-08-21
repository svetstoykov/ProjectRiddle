import { isApplicationError } from "../../../shared/api/errors";
import { accountMessageForCode, unknownAccountFailure } from "../messages/authMessages";

export interface MappedAccountErrors {
    readonly email: readonly string[];
    readonly password: readonly string[];
    readonly summary: readonly string[];
}

function fieldMessages(fieldErrors: Record<string, readonly string[]> | undefined, fieldName: string): string[] {
    if (fieldErrors === undefined) {
        return [];
    }

    return Object.entries(fieldErrors)
        .filter(([key]) => key.toLowerCase() === fieldName.toLowerCase())
        .flatMap(([, messages]) => [...messages]);
}

function unique(messages: readonly string[]): string[] {
    return [...new Set(messages.filter((message) => message.length > 0))];
}

export function mapAccountErrors(error: unknown, fieldNames: readonly string[]): MappedAccountErrors {
    if (error === undefined || error === null) {
        return { email: [], password: [], summary: [] };
    }

    if (!isApplicationError(error)) {
        return { email: [], password: [], summary: [unknownAccountFailure(undefined)] };
    }

    const includeEmail = fieldNames.some((name) => name.toLowerCase() === "email");
    const includePassword = fieldNames.some((name) => name.toLowerCase() === "password");
    const email = includeEmail ? fieldMessages(error.fieldErrors, "email") : [];
    const password = includePassword ? fieldMessages(error.fieldErrors, "password") : [];
    const summary: string[] = [];
    const mappedCode = accountMessageForCode(error.code);

    if (error.code === "users.email.invalid" || error.code === "users.email.conflict") {
        if (mappedCode !== undefined) {
            email.push(mappedCode);
        }
    } else if (error.code === "users.password.invalid") {
        if (mappedCode !== undefined) {
            password.push(mappedCode);
        }
    } else if (error.code === "users.credentials.invalid" || error.code === "users.unauthorized") {
        if (mappedCode !== undefined) {
            summary.push(mappedCode);
        }
    }

    if (email.length === 0 && password.length === 0 && summary.length === 0) {
        summary.push(mappedCode ?? unknownAccountFailure(error.traceId));
    }

    return {
        email: unique(email),
        password: unique(password),
        summary: unique(summary),
    };
}
