import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef, useState, type ReactElement } from "react";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";

import { authApi } from "../../features/auth/api/authApi";
import {
    reconcileSessionAfterAuthorizationFailure,
    sessionKey,
    sessionQueryOptions,
} from "../../features/auth/api/sessionQuery";
import { clearAntiforgeryToken } from "../../shared/api/client";
import { notifications } from "../../shared/notifications/notifications";
import styles from "./ApplicationLayout.module.css";

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
            clearAntiforgeryToken();
            notifications.success("Излязохте от профила си.");
            void navigate("/", { replace: true });
        },
        onError: (error: unknown) => {
            if (reconcileSessionAfterAuthorizationFailure(queryClient, error)) {
                setSignOutError(null);
                return;
            }

            setSignOutError("Изходът не можа да бъде потвърден. Опитайте отново.");
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
                        Project Riddle
                    </NavLink>
                    <div className={styles.links}>
                        <NavLink className={styles.link} to="/">
                            Начало
                        </NavLink>
                        <NavLink className={styles.link} to="/archive">
                            Архив
                        </NavLink>
                        {session === null ? (
                            <>
                                <NavLink className={styles.link} to="/register">
                                    Регистрация
                                </NavLink>
                                <NavLink className={styles.link} to="/sign-in">
                                    Вход
                                </NavLink>
                            </>
                        ) : (
                            <>
                                {session.role === "admin" ? (
                                    <NavLink className={styles.link} to="/admin/riddles">
                                        Администрация
                                    </NavLink>
                                ) : null}
                                <p className={styles.email}>{session.email}</p>
                                <button
                                    type="button"
                                    className={styles.signOut}
                                    aria-busy={signOutMutation.isPending}
                                    disabled={signOutMutation.isPending}
                                    onClick={() => {
                                        signOutMutation.mutate();
                                    }}
                                >
                                    {signOutMutation.isPending ? "Излизане…" : "Изход"}
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
