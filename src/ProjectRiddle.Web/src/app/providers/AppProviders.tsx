import type { PropsWithChildren, ReactElement } from "react";

import { NotificationHost } from "../../shared/notifications/NotificationHost";
import { QueryProvider } from "./QueryProvider";

export function AppProviders({ children }: PropsWithChildren): ReactElement {
    return (
        <QueryProvider>
            <NotificationHost>{children}</NotificationHost>
        </QueryProvider>
    );
}
