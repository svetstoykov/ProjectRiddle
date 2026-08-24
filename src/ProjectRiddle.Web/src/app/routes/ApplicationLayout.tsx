import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef, useState, type ReactElement } from "react";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";

import { authApi } from "../../features/auth/api/authApi";
import {
    discardSessionScopedQueries,
    reconcileSessionAfterAuthorizationFailure,
    sessionKey,
    sessionQueryOptions,
} from "../../features/auth/api/sessionQuery";
import { clearAntiforgeryToken } from "../../shared/api/client";
import { notifications } from "../../shared/notifications/notifications";
import styles from "./ApplicationLayout.module.css";
import { NavigationIcon } from "./NavigationIcon";

export function ApplicationLayout(): ReactElement {
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const location = useLocation();
    const mainRef = useRef<HTMLElement>(null);
    const previousPathRef = useRef<string | null>(null);
    const sessionQuery = useQuery(sessionQueryOptions);
    const session = sessionQuery.data ?? null;
    const [signOutError, setSignOutError] = useState<string | null>(null);
    const [announcement, setAnnouncement] = useState("");

    // Focus moves to the main region on navigation so keyboard and screen-reader users land on the new page. The
    // first render is skipped: focus already starts at the top of the document, and a scripted focus before any
    // pointer input counts as keyboard-initiated, which would paint the focus ring around the whole page on load.
    useEffect(() => {
        const isFirstRender = previousPathRef.current === null;
        previousPathRef.current = location.pathname;

        if (isFirstRender) {
            return;
        }

        mainRef.current?.focus({ preventScroll: true });
        setAnnouncement(document.title);
    }, [location.pathname]);

    const signOutMutation = useMutation({
        mutationFn: authApi.signOut,
        onSuccess: () => {
            setSignOutError(null);
            queryClient.setQueryData(sessionKey, null);
            discardSessionScopedQueries(queryClient);
            clearAntiforgeryToken();
            notifications.success("Излязохте от профила си.");
            void navigate("/", { replace: true });
        },
        onError: (error: unknown) => {
            if (reconcileSessionAfterAuthorizationFailure(queryClient, error)) {
                setSignOutError(null);
                return;
            }

            setSignOutError("Не успяхте да излезете от профила си. Опитайте отново.");
        },
    });

    return (
        <div className={styles.shell}>
            <a className={styles.skipLink} href="#main">
                Към съдържанието
            </a>
            <div className="visuallyHidden" aria-live="polite" aria-atomic="true">
                {announcement}
            </div>
            <header className={styles.header}>
                <nav className={styles.navigation} aria-label="Главна навигация">
                    <NavLink className={styles.brand} to="/">
                        <span className={styles.brandMark} aria-hidden="true">
                            ?
                        </span>
                        <span className={styles.brandText}>Project Riddle</span>
                    </NavLink>
                    {/* The bar carries icons alone, so every control names itself for assistive technology and shows
                        that same name as a tooltip on hover and keyboard focus. */}
                    <div className={styles.actions}>
                        <NavLink className={styles.action} to="/" end aria-label="Начало" data-tooltip="Начало">
                            <NavigationIcon className={styles.icon} name="home" />
                        </NavLink>
                        <NavLink className={styles.action} to="/archive" aria-label="Архив" data-tooltip="Архив">
                            <NavigationIcon className={styles.icon} name="archive" />
                        </NavLink>
                        {session !== null && session.role === "admin" ? (
                            <NavLink
                                className={styles.action}
                                to="/admin/riddles"
                                aria-label="Администрация"
                                data-tooltip="Администрация"
                            >
                                <NavigationIcon className={styles.icon} name="administration" />
                            </NavLink>
                        ) : null}
                        <span className={styles.divider} aria-hidden="true" />
                        {session === null ? (
                            <>
                                <NavLink
                                    className={styles.action}
                                    to="/register"
                                    aria-label="Регистрация"
                                    data-tooltip="Регистрация"
                                >
                                    <NavigationIcon className={styles.icon} name="register" />
                                </NavLink>
                                <NavLink className={styles.action} to="/sign-in" aria-label="Вход" data-tooltip="Вход">
                                    <NavigationIcon className={styles.icon} name="signIn" />
                                </NavLink>
                            </>
                        ) : (
                            <>
                                <span className={styles.avatar} data-tooltip={session.email}>
                                    <span aria-hidden="true">{session.email.slice(0, 1)}</span>
                                    <span className="visuallyHidden">Влезли сте като {session.email}</span>
                                </span>
                                <button
                                    type="button"
                                    className={styles.action}
                                    aria-label={signOutMutation.isPending ? "Излизане…" : "Изход"}
                                    data-tooltip={signOutMutation.isPending ? "Излизане…" : "Изход"}
                                    aria-busy={signOutMutation.isPending}
                                    disabled={signOutMutation.isPending}
                                    onClick={() => {
                                        signOutMutation.mutate();
                                    }}
                                >
                                    <NavigationIcon
                                        className={styles.icon}
                                        name={signOutMutation.isPending ? "busy" : "signOut"}
                                    />
                                </button>
                            </>
                        )}
                    </div>
                </nav>
                {signOutError !== null ? (
                    <p className={styles.signOutError} role="alert">
                        {signOutError}
                    </p>
                ) : null}
            </header>
            <main id="main" ref={mainRef} className={styles.main} tabIndex={-1}>
                <Outlet />
            </main>
            <footer className={styles.footer}>Project Riddle</footer>
        </div>
    );
}
