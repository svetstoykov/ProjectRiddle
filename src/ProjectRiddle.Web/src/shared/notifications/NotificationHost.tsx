import type { PropsWithChildren, ReactElement } from "react";
import { Toaster } from "sonner";

export function NotificationHost({ children }: PropsWithChildren): ReactElement {
    return (
        <>
            <Toaster
                closeButton
                position="bottom-right"
                toastOptions={{
                    style: {
                        border: "3px solid #2d0066",
                        borderRadius: "1rem",
                        boxShadow: "4px 4px 0 #2d0066",
                        color: "#1d1a25",
                        background: "#fdf7ff",
                    },
                }}
            />
            {children}
        </>
    );
}
