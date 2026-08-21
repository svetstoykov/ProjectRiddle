import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useId, useRef, useState, type FormEvent, type ReactElement } from "react";
import { Link, useLocation, useNavigate, useSearchParams } from "react-router-dom";

import { FieldError } from "../../../shared/components/FieldError";
import { clearAntiforgeryToken } from "../../../shared/api/client";
import { authApi } from "../api/authApi";
import { sessionKey } from "../api/sessionQuery";
import { AccountErrorSummary } from "../components/AccountErrorSummary";
import { mapAccountErrors } from "../components/accountErrors";
import { PasswordField } from "../components/PasswordField";
import { safeReturnPath } from "../routing/returnPath";
import formStyles from "../components/AccountForm.module.css";
import styles from "./AuthPage.module.css";

function readRegistrationState(state: unknown): { readonly email: string; readonly succeeded: boolean } {
    if (typeof state !== "object" || state === null) {
        return { email: "", succeeded: false };
    }

    const record = state as Record<string, unknown>;
    return {
        email: typeof record.registrationEmail === "string" ? record.registrationEmail : "",
        succeeded: record.registrationSucceeded === true,
    };
}

function hasBrowserEmailShape(value: string): boolean {
    const probe = document.createElement("input");
    probe.type = "email";
    probe.value = value;
    return probe.checkValidity();
}

export function SignInPage(): ReactElement {
    const navigate = useNavigate();
    const location = useLocation();
    const [searchParams] = useSearchParams();
    const queryClient = useQueryClient();
    const registration = readRegistrationState(location.state);
    const emailId = useId();
    const passwordId = useId();
    const emailErrorId = useId();
    const summaryRef = useRef<HTMLDivElement>(null);
    const [email, setEmail] = useState(registration.email);
    const [password, setPassword] = useState("");
    const [emailErrors, setEmailErrors] = useState<readonly string[]>([]);
    const [passwordErrors, setPasswordErrors] = useState<readonly string[]>([]);
    const [serverError, setServerError] = useState<unknown>(null);
    const [shouldFocusSummary, setShouldFocusSummary] = useState(false);

    useEffect(() => {
        if (shouldFocusSummary) {
            summaryRef.current?.focus();
            setShouldFocusSummary(false);
        }
    }, [shouldFocusSummary]);

    const signInMutation = useMutation({
        mutationFn: authApi.signIn,
        onSuccess: (session) => {
            queryClient.setQueryData(sessionKey, session);
            clearAntiforgeryToken();
            void navigate(safeReturnPath(searchParams.get("returnTo")), { replace: true });
        },
        onError: (error: unknown) => {
            const mapped = mapAccountErrors(error, ["email", "password"]);
            setServerError(error);
            setEmailErrors(mapped.email);
            setPasswordErrors(mapped.password);
            setShouldFocusSummary(true);
        },
    });

    function handleSubmit(event: FormEvent<HTMLFormElement>): void {
        event.preventDefault();

        const nextEmailErrors: string[] = [];
        const nextPasswordErrors: string[] = [];
        const trimmedEmail = email.trim();

        if (trimmedEmail.length === 0 || !hasBrowserEmailShape(trimmedEmail)) {
            nextEmailErrors.push("Въведете валиден имейл адрес.");
        }

        if (password.length === 0) {
            nextPasswordErrors.push("Въведете парола.");
        }

        setServerError(null);
        setEmailErrors(nextEmailErrors);
        setPasswordErrors(nextPasswordErrors);

        if (nextEmailErrors.length > 0 || nextPasswordErrors.length > 0) {
            setShouldFocusSummary(true);
            return;
        }

        signInMutation.mutate({ email: trimmedEmail, password });
    }

    const busy = signInMutation.isPending;
    const localSummary = [...emailErrors, ...passwordErrors];

    return (
        <section className={styles.page}>
            <div className={styles.card}>
                <p className="eyebrow">Профил</p>
                <h1>Вход</h1>
                {registration.succeeded ? (
                    <p className={styles.success} role="status">
                        Профилът е създаден. Влезте с новите си данни.
                    </p>
                ) : null}
                <AccountErrorSummary error={serverError} fieldNames={["email", "password"]} summaryRef={summaryRef} />
                {serverError === null && localSummary.length > 0 ? (
                    <div
                        ref={summaryRef}
                        className={formStyles.summary}
                        tabIndex={-1}
                        role="alert"
                        aria-labelledby="sign-in-local-error-heading"
                    >
                        <h2 id="sign-in-local-error-heading">Има проблем със заявката</h2>
                        <ul>
                            {localSummary.map((message) => (
                                <li key={message}>{message}</li>
                            ))}
                        </ul>
                    </div>
                ) : null}
                <form className={styles.form} onSubmit={handleSubmit} noValidate>
                    <div className={formStyles.field}>
                        <label htmlFor={emailId}>Имейл</label>
                        <input
                            id={emailId}
                            type="email"
                            autoComplete="email"
                            value={email}
                            disabled={busy}
                            aria-invalid={emailErrors.length > 0}
                            aria-describedby={emailErrors.length > 0 ? emailErrorId : undefined}
                            onChange={(event) => {
                                setEmail(event.target.value);
                            }}
                        />
                        <FieldError id={emailErrorId} messages={emailErrors} />
                    </div>
                    <PasswordField
                        id={passwordId}
                        label="Парола"
                        value={password}
                        disabled={busy}
                        errors={passwordErrors}
                        onChange={setPassword}
                    />
                    <button type="submit" aria-busy={busy} disabled={busy}>
                        {busy ? "Влизане…" : "Вход"}
                    </button>
                </form>
                <p className={styles.switch}>
                    Нямате профил? <Link to="/register">Регистрация</Link>
                </p>
            </div>
        </section>
    );
}
