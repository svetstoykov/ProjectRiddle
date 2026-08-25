const accountMessages = {
    "users.email.invalid": "Този имейл не изглежда валиден.",
    "users.email.conflict": "Вече имаш профил с този имейл.",
    "users.password.invalid": "Паролата трябва да е поне 8 знака.",
    "users.credentials.invalid": "Имейлът или паролата не съвпадат.",
    "users.unauthorized": "Сесията изтече. Влез пак.",
} as const;

export type AccountMessageCode = keyof typeof accountMessages;

export function accountMessageForCode(code: string | undefined): string | undefined {
    if (code === undefined) {
        return undefined;
    }

    return Object.hasOwn(accountMessages, code) ? accountMessages[code as AccountMessageCode] : undefined;
}

export function unknownAccountFailure(traceId: string | undefined): string {
    const fallback = "Нещо се обърка от наша страна.";
    return traceId === undefined ? fallback : `${fallback} Код: ${traceId}`;
}
