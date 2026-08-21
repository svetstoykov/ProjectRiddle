import type { ReactElement } from "react";
import { NavLink, Outlet } from "react-router-dom";

import styles from "./ApplicationLayout.module.css";

export function ApplicationLayout(): ReactElement {
    return (
        <div className={styles.shell}>
            <header className={styles.header}>
                <nav className={styles.navigation} aria-label="Main navigation">
                    <NavLink className={styles.brand} to="/">
                        Project Riddle
                    </NavLink>
                    <div className={styles.links}>
                        <NavLink className={styles.link} to="/">
                            Home
                        </NavLink>
                    </div>
                </nav>
            </header>
            <main className={styles.main}>
                <Outlet />
            </main>
            <footer className={styles.footer}>Project Riddle</footer>
        </div>
    );
}
