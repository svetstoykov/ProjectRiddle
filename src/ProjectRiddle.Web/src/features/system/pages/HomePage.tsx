import type { ReactElement } from "react";

import { WalkingSkeletonStatus } from "../components/WalkingSkeletonStatus";
import styles from "./HomePage.module.css";

export function HomePage(): ReactElement {
    return (
        <section className={styles.hero} aria-labelledby="home-title">
            <p className="eyebrow">Project Riddle V1</p>
            <h1 id="home-title">A small, dependable starting point.</h1>
            <p className={styles.lead}>
                The application shell is ready for the later riddle, account, and course capabilities.
            </p>
            <WalkingSkeletonStatus />
        </section>
    );
}
