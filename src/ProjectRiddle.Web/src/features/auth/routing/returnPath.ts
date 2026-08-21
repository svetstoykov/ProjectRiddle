export function safeReturnPath(value: string | null | undefined): string {
    return value?.startsWith("/") === true && !value.startsWith("//") ? value : "/";
}
