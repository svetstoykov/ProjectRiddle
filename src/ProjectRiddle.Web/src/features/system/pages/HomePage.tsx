import type { ReactElement } from "react";

import { InternalStatus } from "../components/InternalStatus";
import styles from "./HomePage.module.css";

export function HomePage(): ReactElement {
    return (
        <section className={styles.hero} aria-labelledby="home-title">
            <p className="eyebrow">Project Riddle</p>
            <h1 id="home-title">A small, dependable starting point.</h1>
            <p className={styles.lead}>
                The application is ready to help people create and solve Bulgarian cryptograms.
            </p>
            <InternalStatus />
        </section>
    );
}
