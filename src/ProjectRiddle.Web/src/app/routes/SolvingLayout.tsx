import type { ReactElement } from "react";
import { Outlet } from "react-router-dom";

import styles from "./SolvingLayout.module.css";

/**
 * Solving is the only screen without the application's navigation: the riddle takes the whole viewport and the way
 * out is the back control the page itself draws. The shell exists to hold that viewport and nothing else.
 */
export function SolvingLayout(): ReactElement {
    return (
        <div className={styles.shell}>
            <Outlet />
        </div>
    );
}
