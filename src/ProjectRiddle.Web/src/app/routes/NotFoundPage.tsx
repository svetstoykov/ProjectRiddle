import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import styles from "./NotFoundPage.module.css";

export function NotFoundPage(): ReactElement {
    return (
        <section className={styles.content} aria-labelledby="not-found-title">
            <p className="eyebrow">404</p>
            <h1 id="not-found-title">Page not found</h1>
            <p>The requested page does not exist.</p>
            <Link className="button" to="/">
                Return home
            </Link>
        </section>
    );
}
