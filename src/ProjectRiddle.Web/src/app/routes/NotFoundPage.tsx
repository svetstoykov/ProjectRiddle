import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import { DocumentTitle } from "../../shared/components/DocumentTitle";
import styles from "./NotFoundPage.module.css";

export function NotFoundPage(): ReactElement {
    return (
        <section className={styles.content} aria-labelledby="not-found-title">
            <DocumentTitle title="Страницата не е намерена" />
            <p className="eyebrow">404</p>
            <h1 id="not-found-title">Страницата не е намерена</h1>
            <p>Поисканата страница не съществува.</p>
            <Link className="button" to="/">
                Към началото
            </Link>
        </section>
    );
}
