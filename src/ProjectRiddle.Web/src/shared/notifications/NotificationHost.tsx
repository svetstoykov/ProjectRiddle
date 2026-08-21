import type { PropsWithChildren, ReactElement } from "react";
import { Toaster } from "sonner";

export function NotificationHost({ children }: PropsWithChildren): ReactElement {
    return (
        <>
            <Toaster closeButton position="bottom-right" />
            {children}
        </>
    );
}
