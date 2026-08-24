import { useQuery } from "@tanstack/react-query";
import { useState, type ReactElement, type RefObject } from "react";

import { ClueTermText } from "../../../shared/components/ClueTermText";
import { InfoDialog } from "../../../shared/components/InfoDialog";
import { coursePrimerQueryOptions } from "../api/courseQueries";
import { dismissCoursePrimer } from "../storage/anonymousCourseProgress";
import { PrimerFigure } from "./PrimerFigure";
import styles from "./CoursePrimerDialog.module.css";

export interface CoursePrimerDialogProps {
    readonly open: boolean;
    readonly onDismiss: () => void;
    readonly returnFocusRef: RefObject<HTMLElement | null>;
}

export function CoursePrimerDialog({ open, onDismiss, returnFocusRef }: CoursePrimerDialogProps): ReactElement {
    const [pageIndex, setPageIndex] = useState(0);
    const primerQuery = useQuery({ ...coursePrimerQueryOptions(), enabled: open });
    const pages = primerQuery.data?.pages ?? [];
    const page = pages[pageIndex];

    const close = (): void => {
        dismissCoursePrimer();
        setPageIndex(0);
        onDismiss();
    };

    return (
        <InfoDialog open={open} title={page?.title ?? "Увод в уликите"} onClose={close} returnFocusRef={returnFocusRef}>
            {primerQuery.isPending ? <p role="status">Зареждаме увода…</p> : null}
            {primerQuery.isError || (primerQuery.data !== undefined && pages.length === 0) ? (
                <div role="alert">
                    <p>Уводът не се зарежда.</p>
                    <button
                        type="button"
                        onClick={() => {
                            void primerQuery.refetch();
                        }}
                    >
                        Пробвай пак
                    </button>
                </div>
            ) : null}
            {page === undefined ? null : (
                <>
                    <p>
                        <ClueTermText text={page.body} />
                    </p>
                    <PrimerFigure figureKey={page.figure} />
                    <footer className={styles.footer}>
                        <button
                            type="button"
                            disabled={pageIndex === 0}
                            onClick={() => {
                                setPageIndex((value) => Math.max(0, value - 1));
                            }}
                        >
                            Назад
                        </button>
                        <span>
                            {pageIndex + 1} от {pages.length}
                        </span>
                        {pageIndex === pages.length - 1 ? (
                            <button type="button" onClick={close}>
                                Готово
                            </button>
                        ) : (
                            <button
                                type="button"
                                onClick={() => {
                                    setPageIndex((value) => Math.min(pages.length - 1, value + 1));
                                }}
                            >
                                Напред
                            </button>
                        )}
                    </footer>
                </>
            )}
        </InfoDialog>
    );
}
