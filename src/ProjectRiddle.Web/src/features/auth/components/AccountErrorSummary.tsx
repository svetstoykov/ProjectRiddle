import type { ReactElement, RefObject } from "react";

import { ErrorSummary } from "../../../shared/components/ErrorSummary";
import { mapAccountErrors } from "./accountErrors";

export interface AccountErrorSummaryProps {
    readonly error: unknown;
    readonly fieldNames: readonly string[];
    readonly summaryRef: RefObject<HTMLDivElement | null>;
}

export function AccountErrorSummary({ error, fieldNames, summaryRef }: AccountErrorSummaryProps): ReactElement | null {
    const mapped = mapAccountErrors(error, fieldNames);

    return <ErrorSummary messages={mapped.summary} summaryRef={summaryRef} headingId="account-error-heading" />;
}
