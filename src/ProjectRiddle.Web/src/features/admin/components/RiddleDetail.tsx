import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState, type ReactElement } from "react";
import { Link } from "react-router-dom";

import { reconcileSessionAfterAuthorizationFailure } from "../../auth/api/sessionQuery";
import { isApplicationError } from "../../../shared/api/errors";
import { ClueTermText } from "../../../shared/components/ClueTermText";
import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";
import { adminRiddleDetailQueryOptions } from "../api/adminRiddleQueries";
import { riddleMessageForCode, unknownRiddleFailure } from "../messages/adminMessages";
import { PublicationPanel } from "./PublicationPanel";
import { RiddlePreview } from "./RiddlePreview";
import styles from "./RiddleDetail.module.css";

export interface RiddleDetailProps {
    readonly riddleId: string;
}

/**
 * Presents a saved riddle. Content is immutable once written, so the screen reads the riddle and offers publication
 * commands only — never an edit affordance.
 */
export function RiddleDetail({ riddleId }: RiddleDetailProps): ReactElement {
    const queryClient = useQueryClient();
    const [missing, setMissing] = useState(false);
    const detailQuery = useQuery(adminRiddleDetailQueryOptions(riddleId));
    const authorizationFailure =
        isApplicationError(detailQuery.error) && (detailQuery.error.status === 401 || detailQuery.error.status === 403);
    const notFound =
        missing ||
        (isApplicationError(detailQuery.error) &&
            (detailQuery.error.status === 404 || detailQuery.error.code === "riddles.notFound"));

    useEffect(() => {
        if (detailQuery.error !== null && authorizationFailure) {
            reconcileSessionAfterAuthorizationFailure(queryClient, detailQuery.error);
        }
    }, [authorizationFailure, detailQuery.error, queryClient]);

    if (notFound) {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    tone="error"
                    eyebrow="Администрация"
                    title="Загадката не е намерена"
                    message={riddleMessageForCode("riddles.notFound") ?? "Загадката не е намерена."}
                />
            </>
        );
    }

    if (detailQuery.isPending) {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    eyebrow="Администрация"
                    title="Зареждаме загадката…"
                    message="Изчакваме текущото съдържание."
                />
            </>
        );
    }

    if (authorizationFailure) {
        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    tone="error"
                    eyebrow="Администрация"
                    title="Няма достъп до загадката"
                    message="Сесията или правата трябва да бъдат проверени отново."
                />
            </>
        );
    }

    if (detailQuery.isError) {
        const message = isApplicationError(detailQuery.error)
            ? unknownRiddleFailure(detailQuery.error.traceId)
            : unknownRiddleFailure(undefined);

        return (
            <>
                <DocumentTitle title="Загадка" />
                <PageStatus
                    tone="error"
                    eyebrow="Администрация"
                    title="Загадката временно не е достъпна"
                    message={message}
                    action={{
                        label: "Опитай отново",
                        onClick: () => {
                            void detailQuery.refetch();
                        },
                    }}
                />
            </>
        );
    }

    const riddle = detailQuery.data;

    return (
        <section className={styles.detail} aria-labelledby="riddle-detail-title">
            <DocumentTitle title="Загадка" />
            <div className={styles.header}>
                <p className="eyebrow">Администрация</p>
                <h1 id="riddle-detail-title">Загадка</h1>
                <p className={styles.immutableNotice}>
                    Загадките са неизменими. Съдържанието не може да бъде променяно — само насрочено, публикувано,
                    свалено или изтрито.
                </p>
                <Link className={styles.back} to="/admin/riddles">
                    Всички загадки
                </Link>
            </div>
            <div className={styles.layout}>
                <RiddlePreview clue={riddle.clue} answer={riddle.answer} ranges={riddle.ranges} />
                <section className={styles.content} aria-labelledby="riddle-content-title">
                    <h2 id="riddle-content-title">Съдържание</h2>
                    <dl className={styles.facts}>
                        <dt>Отговор</dt>
                        <dd lang="bg">{riddle.answer}</dd>
                        <dt>Обяснение</dt>
                        <dd lang="bg">
                            <ClueTermText text={riddle.explanation} />
                        </dd>
                    </dl>
                </section>
            </div>
            <PublicationPanel
                riddle={riddle}
                onMissing={() => {
                    setMissing(true);
                }}
            />
        </section>
    );
}
