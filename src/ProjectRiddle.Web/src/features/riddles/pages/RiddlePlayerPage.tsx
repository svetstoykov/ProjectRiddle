import type { ReactElement } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";

import { AuthenticationRequiredNotice } from "../../../shared/components/AuthenticationRequiredNotice";
import { PlayerPageSkeleton } from "../../../shared/components/ContentSkeletons";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { useRiddlePlaySession } from "../api/riddlePlaySession";
import { RiddlePlayer } from "../components/RiddlePlayer";

/**
 * Marks a navigation that came from a past-day tile or an archive card while signed out, so the page can show the
 * access prompt without a request whose outcome is already known.
 */
export interface ArchiveNavigationState {
    readonly requiresAccount: true;
}

function readRequiresAccount(state: unknown): boolean {
    return typeof state === "object" && state !== null && (state as ArchiveNavigationState).requiresAccount === true;
}

export function RiddlePlayerPage(): ReactElement {
    const { riddleId } = useParams<{ riddleId: string }>();
    const location = useLocation();
    const navigate = useNavigate();
    const session = useRiddlePlaySession(riddleId, readRequiresAccount(location.state));

    if (session.status === "authenticationRequired") {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <AuthenticationRequiredNotice
                    title="Тази загадка е в архива"
                    message="По-ранните загадки се играят с профил. Влез или се регистрирай, за да продължиш от същото място."
                    returnTo={location.pathname}
                />
            </>
        );
    }

    if (session.status === "todayUnavailable") {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    eyebrow="Днешната загадка"
                    title="Днес няма публикувана загадка"
                    message="Върни се по-късно или разгледай архива с предишните загадки."
                    action={{
                        label: "Към архива",
                        onClick: () => {
                            void navigate("/archive");
                        },
                    }}
                />
            </>
        );
    }

    if (session.status === "notFound") {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    tone="error"
                    eyebrow="Загадка"
                    title="Загадката не е достъпна"
                    message="Тази загадка не е налична."
                />
            </>
        );
    }

    if (session.status === "failed") {
        const trace = session.traceId === undefined ? "" : ` Код за проследяване: ${session.traceId}`;

        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    tone="error"
                    eyebrow="Загадка"
                    title="Загадката временно не е достъпна"
                    message={`Заявката не можа да бъде изпълнена.${trace}`}
                    action={{ label: "Опитай отново", onClick: session.retry }}
                />
            </>
        );
    }

    if (session.play === undefined || session.playState === undefined) {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <p className="visuallyHidden" role="status">
                    Зареждаме загадката…
                </p>
                <PlayerPageSkeleton />
            </>
        );
    }

    return (
        <>
            <DocumentTitle title="Загадка" />
            <RiddlePlayer
                key={session.play.id}
                play={session.play}
                playState={session.playState}
                pendingHint={session.pendingHint}
                isSubmitting={session.isSubmitting}
                isRevealing={session.isRevealing}
                onSubmitAnswer={session.submitAnswer}
                onUseHint={session.useHint}
                onRevealLetter={session.revealLetter}
            />
        </>
    );
}
