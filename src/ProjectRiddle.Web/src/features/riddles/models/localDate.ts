const localDatePattern = /^\d{4}-\d{2}-\d{2}$/;

const dayNumberFormat = new Intl.DateTimeFormat("bg-BG", { day: "numeric", timeZone: "UTC" });
const weekdayFormat = new Intl.DateTimeFormat("bg-BG", { weekday: "short", timeZone: "UTC" });
const fullDateFormat = new Intl.DateTimeFormat("bg-BG", {
    day: "numeric",
    month: "long",
    year: "numeric",
    timeZone: "UTC",
});
const monthFormat = new Intl.DateTimeFormat("bg-BG", { month: "long", year: "numeric", timeZone: "UTC" });

/**
 * Reads a `YYYY-MM-DD` local date as midday UTC so that formatting and day arithmetic can never drift across a
 * boundary because of the visitor's own time zone.
 */
function toInstant(localDate: string): Date {
    return new Date(`${localDate}T12:00:00Z`);
}

export function isLocalDate(value: string): boolean {
    return localDatePattern.test(value) && !Number.isNaN(toInstant(value).getTime());
}

export function addLocalDays(localDate: string, days: number): string {
    const instant = toInstant(localDate);
    instant.setUTCDate(instant.getUTCDate() + days);
    return instant.toISOString().slice(0, 10);
}

export function compareLocalDates(left: string, right: string): number {
    if (left === right) {
        return 0;
    }

    return left < right ? -1 : 1;
}

export function monthKey(localDate: string): string {
    return localDate.slice(0, 7);
}

export function formatDayNumber(localDate: string): string {
    return dayNumberFormat.format(toInstant(localDate));
}

export function formatWeekday(localDate: string): string {
    return weekdayFormat.format(toInstant(localDate));
}

export function formatFullDate(localDate: string): string {
    return fullDateFormat.format(toInstant(localDate));
}

export function formatMonth(localDate: string): string {
    return monthFormat.format(toInstant(localDate));
}
