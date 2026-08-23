const accountMessages = {
    "users.email.invalid": "Въведете валиден имейл адрес.",
    "users.email.conflict": "Вече има профил с този имейл адрес.",
    "users.password.invalid": "Паролата трябва да е между 8 и 256 знака.",
    "users.credentials.invalid": "Невалиден имейл или парола.",
    "users.unauthorized": "Сесията е изтекла. Влезте отново.",
} as const;

export type AccountMessageCode = keyof typeof accountMessages;

export function accountMessageForCode(code: string | undefined): string | undefined {
    if (code === undefined) {
        return undefined;
    }

    return Object.hasOwn(accountMessages, code) ? accountMessages[code as AccountMessageCode] : undefined;
}

export function unknownAccountFailure(traceId: string | undefined): string {
    const fallback = "Заявката не може да бъде изпълнена.";
    return traceId === undefined ? fallback : `${fallback} Код за проследяване: ${traceId}`;
}
