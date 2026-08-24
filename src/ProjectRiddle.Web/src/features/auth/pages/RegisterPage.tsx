import { useMutation } from "@tanstack/react-query";
import { useEffect, useId, useRef, useState, type FormEvent, type ReactElement } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";

import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { ErrorSummary } from "../../../shared/components/ErrorSummary";
import { FieldError } from "../../../shared/components/FieldError";
import { clearAntiforgeryToken } from "../../../shared/api/client";
import { authApi } from "../api/authApi";
import { AccountErrorSummary } from "../components/AccountErrorSummary";
import { mapAccountErrors } from "../components/accountErrors";
import { PasswordField } from "../components/PasswordField";
import { safeReturnPath } from "../routing/returnPath";
import styles from "./AuthPage.module.css";
import formStyles from "../components/AccountForm.module.css";

function hasBrowserEmailShape(value: string): boolean {
    const probe = document.createElement("input");
    probe.type = "email";
    probe.value = value;
    return probe.checkValidity();
}

export function RegisterPage(): ReactElement {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const emailId = useId();
    const passwordId = useId();
    const confirmationId = useId();
    const emailErrorId = useId();
    const summaryRef = useRef<HTMLDivElement>(null);
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmation, setConfirmation] = useState("");
    const [emailErrors, setEmailErrors] = useState<readonly string[]>([]);
    const [passwordErrors, setPasswordErrors] = useState<readonly string[]>([]);
    const [confirmationErrors, setConfirmationErrors] = useState<readonly string[]>([]);
    const [serverError, setServerError] = useState<unknown>(null);
    const [shouldFocusSummary, setShouldFocusSummary] = useState(false);

    useEffect(() => {
        if (shouldFocusSummary) {
            summaryRef.current?.focus();
            setShouldFocusSummary(false);
        }
    }, [shouldFocusSummary]);

    const registerMutation = useMutation({
        mutationFn: authApi.register,
        onSuccess: () => {
            clearAntiforgeryToken();
            const returnTo = searchParams.get("returnTo");
            const signInPath =
                returnTo === null ? "/sign-in" : `/sign-in?returnTo=${encodeURIComponent(safeReturnPath(returnTo))}`;
            void navigate(signInPath, {
                replace: true,
                state: {
                    registrationEmail: email.trim(),
                    registrationSucceeded: true,
                },
            });
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
        const nextConfirmationErrors: string[] = [];
        const trimmedEmail = email.trim();

        if (trimmedEmail.length === 0 || !hasBrowserEmailShape(trimmedEmail)) {
            nextEmailErrors.push("Този имейл не изглежда валиден.");
        }

        // The server reports both length bounds under one error code, so the message resolved from that code can only
        // name one of them. The client checks each bound separately and says which one was missed.
        if (password.length < 8) {
            nextPasswordErrors.push("Паролата трябва да е поне 8 знака.");
        } else if (password.length > 256) {
            nextPasswordErrors.push("Паролата е най-много 256 знака.");
        }

        if (confirmation.length === 0) {
            nextConfirmationErrors.push("Потвърди паролата.");
        } else if (confirmation !== password) {
            nextConfirmationErrors.push("Паролите не съвпадат.");
        }

        setServerError(null);
        setEmailErrors(nextEmailErrors);
        setPasswordErrors(nextPasswordErrors);
        setConfirmationErrors(nextConfirmationErrors);

        if (nextEmailErrors.length > 0 || nextPasswordErrors.length > 0 || nextConfirmationErrors.length > 0) {
            setShouldFocusSummary(true);
            return;
        }

        registerMutation.mutate({ email: trimmedEmail, password });
    }

    const mappedSummary = mapAccountErrors(serverError, ["email", "password"]).summary;
    const localSummary = [
        ...emailErrors.filter((message) => !mappedSummary.includes(message)),
        ...passwordErrors.filter((message) => !mappedSummary.includes(message)),
        ...confirmationErrors,
    ];
    const busy = registerMutation.isPending;

    return (
        <section className={styles.page}>
            <div className={styles.card}>
                <DocumentTitle title="Регистрация" />
                <p className="eyebrow">Профил</p>
                <h1>Регистрация</h1>
                <AccountErrorSummary error={serverError} fieldNames={["email", "password"]} summaryRef={summaryRef} />
                {serverError === null ? (
                    <ErrorSummary
                        messages={localSummary}
                        headingId="register-local-error-heading"
                        summaryRef={summaryRef}
                    />
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
                        autoComplete="new-password"
                        onChange={setPassword}
                    />
                    <PasswordField
                        id={confirmationId}
                        label="Повтори паролата"
                        value={confirmation}
                        disabled={busy}
                        errors={confirmationErrors}
                        autoComplete="new-password"
                        onChange={setConfirmation}
                    />
                    <button type="submit" className={styles.submit} aria-busy={busy} disabled={busy}>
                        {busy ? "Създаваме профила…" : "Създай профил"}
                    </button>
                </form>
                <p className={styles.switch}>
                    Вече имаш профил? <Link to="/sign-in">Вход</Link>
                </p>
            </div>
        </section>
    );
}
