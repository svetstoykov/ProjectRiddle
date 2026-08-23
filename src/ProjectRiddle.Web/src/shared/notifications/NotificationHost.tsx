import type { PropsWithChildren, ReactElement } from "react";
import { Toaster } from "sonner";

import styles from "./NotificationHost.module.css";

export function NotificationHost({ children }: PropsWithChildren): ReactElement {
    return (
        <>
            <Toaster
                closeButton
                position="top-center"
                offset={16}
                toastOptions={{
                    className: styles.toast,
                    classNames: {
                        success: styles.toastSuccess,
                        error: styles.toastError,
                    },
                }}
            />
            {children}
        </>
    );
}
