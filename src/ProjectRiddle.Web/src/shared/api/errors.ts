import type { FieldErrors, ProblemDetails } from "./types";

type UnknownRecord = Record<string, unknown>;

function isRecord(value: unknown): value is UnknownRecord {
    return typeof value === "object" && value !== null;
}

function readString(value: unknown): string | undefined {
    return typeof value === "string" && value.length > 0 ? value : undefined;
}

function readNumber(value: unknown): number | undefined {
    return typeof value === "number" && Number.isFinite(value) ? value : undefined;
}

function readFieldErrors(value: unknown): FieldErrors | undefined {
    if (!isRecord(value)) {
        return undefined;
    }

    const entries = Object.entries(value).flatMap(([key, messages]) => {
        if (!Array.isArray(messages) || !messages.every((message): message is string => typeof message === "string")) {
            return [];
        }

        return [[key, messages] as const];
    });

    return entries.length > 0 ? Object.fromEntries(entries) : undefined;
}

export function parseProblemDetails(value: unknown): ProblemDetails | undefined {
    let candidate: unknown = value;

    if (typeof candidate === "string") {
        try {
            candidate = JSON.parse(candidate) as unknown;
        } catch {
            return undefined;
        }
    }

    if (!isRecord(candidate)) {
        return undefined;
    }

    const problem: ProblemDetails = {
        type: readString(candidate.type),
        title: readString(candidate.title),
        status: readNumber(candidate.status),
        detail: readString(candidate.detail),
        instance: readString(candidate.instance),
        code: readString(candidate.code),
        traceId: readString(candidate.traceId),
        errors: readFieldErrors(candidate.errors),
    };

    const hasProblemDetailsMember = Object.values(problem).some((member) => member !== undefined);
    return hasProblemDetailsMember ? problem : undefined;
}

export class ApplicationError extends Error {
    public readonly problem: ProblemDetails;

    public constructor(problem: ProblemDetails) {
        super(problem.detail ?? problem.title ?? "The request could not be completed.");
        this.name = "ApplicationError";
        this.problem = problem;
    }

    public get status(): number | undefined {
        return this.problem.status;
    }

    public get code(): string | undefined {
        return this.problem.code;
    }

    public get traceId(): string | undefined {
        return this.problem.traceId;
    }

    public get fieldErrors(): FieldErrors | undefined {
        return this.problem.errors;
    }
}

export function isApplicationError(error: unknown): error is ApplicationError {
    return error instanceof ApplicationError;
}
