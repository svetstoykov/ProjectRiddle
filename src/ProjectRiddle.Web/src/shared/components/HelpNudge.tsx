import { useEffect, useState, type ReactElement, type ReactNode } from "react";

import styles from "./HelpNudge.module.css";

const firstShowDelayMs = 15_000;
const repeatDelayMs = 45_000;
const visibleMs = 2_500;

export interface HelpNudgeProps {
    /** The help control the bubble points at. */
    readonly children: ReactNode;
    /** Stops the cycle, for example while the help it points at is already open. Resuming starts the wait again. */
    readonly paused: boolean;
}

/**
 * Every so often a small bubble appears under a help control for a moment, so a solver who is stuck remembers that
 * help is one tap away. The control keeps its own accessible name, so the bubble is decorative and never announced.
 */
export function HelpNudge({ children, paused }: HelpNudgeProps): ReactElement {
    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        if (paused) {
            setIsVisible(false);
            return;
        }

        let timer: number;
        const show = (): void => {
            setIsVisible(true);
            timer = window.setTimeout(hide, visibleMs);
        };
        const hide = (): void => {
            setIsVisible(false);
            timer = window.setTimeout(show, repeatDelayMs);
        };

        timer = window.setTimeout(show, firstShowDelayMs);
        return () => {
            window.clearTimeout(timer);
        };
    }, [paused]);

    return (
        <span className={styles.anchor}>
            {children}
            <span className={styles.bubble} data-visible={isVisible} aria-hidden="true">
                Нужна помощ? Натисни тук
            </span>
        </span>
    );
}
