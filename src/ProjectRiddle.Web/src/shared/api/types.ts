export type FieldErrors = Readonly<Record<string, readonly string[]>>;

export interface ProblemDetails {
    readonly type?: string;
    readonly title?: string;
    readonly status?: number;
    readonly detail?: string;
    readonly instance?: string;
    readonly code?: string;
    readonly traceId?: string;
    readonly errors?: FieldErrors;
}
