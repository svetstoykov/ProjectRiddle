import type { ReactElement } from "react";
import { Link } from "react-router-dom";

export function AccessDeniedPage(): ReactElement {
    return (
        <section aria-labelledby="access-denied-title">
            <p className="eyebrow">Достъп</p>
            <h1 id="access-denied-title">Няма достъп</h1>
            <p>Този профил няма право да отваря администрацията.</p>
            <Link className="button" to="/">
                Към началото
            </Link>
        </section>
    );
}
