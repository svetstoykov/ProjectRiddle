import { useRef, useState, type PropsWithChildren, type ReactElement, type RefObject } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";

import { PlayerPageSkeleton } from "../../../shared/components/ContentSkeletons";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { MembershipDialog } from "../../auth/components/MembershipDialog";
import { CoursePrimerDialog } from "../../courses/components/CoursePrimerDialog";
import { useRiddlePlaySession } from "../api/riddlePlaySession";
import { RiddlePlayer } from "../components/RiddlePlayer";
import { SolvingTopBar } from "../components/SolvingTopBar";
import styles from "./RiddlePlayerPage.module.css";

interface SolvingScreenProps {
    readonly publicationDate: string | undefined;
    readonly onOpenPrimer: () => void;
    readonly primerOpen: boolean;
    readonly onDismissPrimer: () => void;
    readonly primerTriggerRef: RefObject<HTMLButtonElement | null>;
}

/** The screen every state of this page shares: a way back at the top and one region beneath it. */
function SolvingScreen({
    publicationDate,
    onOpenPrimer,
    primerOpen,
    onDismissPrimer,
    primerTriggerRef,
    children,
}: PropsWithChildren<SolvingScreenProps>): ReactElement {
    return (
        <div className={styles.screen}>
            <SolvingTopBar
                publicationDate={publicationDate}
                onOpenPrimer={onOpenPrimer}
                primerTriggerRef={primerTriggerRef}
            />
            <main className={styles.stage}>{children}</main>
            <CoursePrimerDialog open={primerOpen} onDismiss={onDismissPrimer} returnFocusRef={primerTriggerRef} />
        </div>
    );
}

export function RiddlePlayerPage(): ReactElement {
    const { riddleId } = useParams<{ riddleId: string }>();
    const location = useLocation();
    const navigate = useNavigate();
    const session = useRiddlePlaySession(riddleId);
    const primerTriggerRef = useRef<HTMLButtonElement | null>(null);
    const [isPrimerOpen, setIsPrimerOpen] = useState(false);
    const openPrimer = (): void => {
        setIsPrimerOpen(true);
    };
    const dismissPrimer = (): void => {
        setIsPrimerOpen(false);
    };

    // A riddle reached without an account — by link, or after the free day rolled over mid-play — is answered by the
    // same prompt the rest of the application uses. The board is what an account unlocks, so dismissing it leaves.
    if (session.status === "authenticationRequired") {
        return (
            <SolvingScreen
                publicationDate={undefined}
                onOpenPrimer={openPrimer}
                primerOpen={isPrimerOpen}
                onDismissPrimer={dismissPrimer}
                primerTriggerRef={primerTriggerRef}
            >
                <DocumentTitle title="Криптика" />
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
            <SolvingScreen
                publicationDate={undefined}
                onOpenPrimer={openPrimer}
                primerOpen={isPrimerOpen}
                onDismissPrimer={dismissPrimer}
                primerTriggerRef={primerTriggerRef}
            >
                <DocumentTitle title="Криптика" />
                <div className={styles.notice}>
                    <PageStatus
                        eyebrow="Днешната криптика"
                        title="Днес няма криптика."
                        message="Виж архива, докато чакаш."
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
            <SolvingScreen
                publicationDate={undefined}
                onOpenPrimer={openPrimer}
                primerOpen={isPrimerOpen}
                onDismissPrimer={dismissPrimer}
                primerTriggerRef={primerTriggerRef}
            >
                <DocumentTitle title="Криптика" />
                <div className={styles.notice}>
                    <PageStatus
                        tone="error"
                        eyebrow="Криптика"
                        title="Тази криптика не е достъпна."
                        message="Може да е свалена или още непубликувана."
                    />
                </div>
            </SolvingScreen>
        );
    }

    if (session.status === "failed") {
        const trace = session.traceId === undefined ? "" : ` Код: ${session.traceId}`;

        return (
            <SolvingScreen
                publicationDate={undefined}
                onOpenPrimer={openPrimer}
                primerOpen={isPrimerOpen}
                onDismissPrimer={dismissPrimer}
                primerTriggerRef={primerTriggerRef}
            >
                <DocumentTitle title="Криптика" />
                <div className={styles.notice}>
                    <PageStatus
                        tone="error"
                        eyebrow="Криптика"
                        title="Криптиката не се зарежда."
                        message={`Нещо се обърка от наша страна.${trace}`}
                        action={{ label: "Пробвай пак", onClick: session.retry }}
                    />
                </div>
            </SolvingScreen>
        );
    }

    if (session.play === undefined || session.playState === undefined) {
        return (
            <SolvingScreen
                publicationDate={undefined}
                onOpenPrimer={openPrimer}
                primerOpen={isPrimerOpen}
                onDismissPrimer={dismissPrimer}
                primerTriggerRef={primerTriggerRef}
            >
                <DocumentTitle title="Криптика" />
                <div className={styles.notice}>
                    <p className="visuallyHidden" role="status">
                        Зареждаме криптиката…
                    </p>
                    <PlayerPageSkeleton />
                </div>
            </SolvingScreen>
        );
    }

    return (
        <SolvingScreen
            publicationDate={session.play.publicationDate}
            onOpenPrimer={openPrimer}
            primerOpen={isPrimerOpen}
            onDismissPrimer={dismissPrimer}
            primerTriggerRef={primerTriggerRef}
        >
            <DocumentTitle title="Криптика" />
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
