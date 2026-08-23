/*
 * Publication days are Sofia calendar days, so the default offered to an author is read in that zone rather than in
 * the browser's. `en-CA` is the locale whose numeric date format is already `YYYY-MM-DD`, the form the date input and
 * the publication commands both use.
 */
const sofiaDateFormat = new Intl.DateTimeFormat("en-CA", {
    timeZone: "Europe/Sofia",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
});

export function todayInSofia(): string {
    return sofiaDateFormat.format(new Date());
}
