import type { PropsWithChildren, ReactElement } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";

import { PlayerPageSkeleton } from "../../../shared/components/ContentSkeletons";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { MembershipDialog } from "../../auth/components/MembershipDialog";
import { useRiddlePlaySession } from "../api/riddlePlaySession";
import { RiddlePlayer } from "../components/RiddlePlayer";
import { SolvingTopBar } from "../components/SolvingTopBar";
import styles from "./RiddlePlayerPage.module.css";

interface SolvingScreenProps {
    readonly publicationDate: string | undefined;
}

/** The screen every state of this page shares: a way back at the top and one region beneath it. */
function SolvingScreen({ publicationDate, children }: PropsWithChildren<SolvingScreenProps>): ReactElement {
    return (
        <div className={styles.screen}>
            <SolvingTopBar publicationDate={publicationDate} />
            <main className={styles.stage}>{children}</main>
        </div>
    );
}

export function RiddlePlayerPage(): ReactElement {
    const { riddleId } = useParams<{ riddleId: string }>();
    const location = useLocation();
    const navigate = useNavigate();
    const session = useRiddlePlaySession(riddleId);

    // A riddle reached without an account — by link, or after the free day rolled over mid-play — is answered by the
    // same prompt the rest of the application uses. The board is what an account unlocks, so dismissing it leaves.
    if (session.status === "authenticationRequired") {
        return (
            <SolvingScreen publicationDate={undefined}>
                <DocumentTitle title="Загадка" />
                <MembershipDialog
                    returnTo={location.pathname}
                    onClose={() => {
                        void navigate("/");
                    }}
                />
            </SolvingScreen>
        );
    }

    if (session.status === "todayUnavailable") {
        return (
            <SolvingScreen publicationDate={undefined}>
                <DocumentTitle title="Загадка" />
                <div className={styles.notice}>
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
                </div>
            </SolvingScreen>
        );
    }

    if (session.status === "notFound") {
        return (
            <SolvingScreen publicationDate={undefined}>
                <DocumentTitle title="Загадка" />
                <div className={styles.notice}>
                    <PageStatus
                        tone="error"
                        eyebrow="Загадка"
                        title="Загадката не е достъпна"
                        message="Тази загадка не е налична."
                    />
                </div>
            </SolvingScreen>
        );
    }

    if (session.status === "failed") {
        const trace = session.traceId === undefined ? "" : ` Код за проследяване: ${session.traceId}`;

        return (
            <SolvingScreen publicationDate={undefined}>
                <DocumentTitle title="Загадка" />
                <div className={styles.notice}>
                    <PageStatus
                        tone="error"
                        eyebrow="Загадка"
                        title="Загадката временно не е достъпна"
                        message={`Заявката не може да бъде изпълнена.${trace}`}
                        action={{ label: "Опитай отново", onClick: session.retry }}
                    />
                </div>
            </SolvingScreen>
        );
    }

    if (session.play === undefined || session.playState === undefined) {
        return (
            <SolvingScreen publicationDate={undefined}>
                <DocumentTitle title="Загадка" />
                <div className={styles.notice}>
                    <p className="visuallyHidden" role="status">
                        Зареждаме загадката…
                    </p>
                    <PlayerPageSkeleton />
                </div>
            </SolvingScreen>
        );
    }

    return (
        <SolvingScreen publicationDate={session.play.publicationDate}>
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
        </SolvingScreen>
    );
}
