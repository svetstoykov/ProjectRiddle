import type { ReactElement } from "react";

import { InternalStatus } from "../components/InternalStatus";
import styles from "./HomePage.module.css";

export function HomePage(): ReactElement {
    return (
        <section className={styles.hero} aria-labelledby="home-title">
            <p className="eyebrow">Project Riddle</p>
            <h1 id="home-title">Български загадки с характер.</h1>
            <p className={styles.lead}>Приложението помага да се създават и решават български криптограми.</p>
            <InternalStatus />
        </section>
    );
}
