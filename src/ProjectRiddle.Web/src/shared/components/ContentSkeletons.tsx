import type { ReactElement } from "react";

import styles from "./ContentSkeletons.module.css";

export function ClueBlockSkeleton(): ReactElement {
    return (
        <div className={styles.clue} aria-hidden="true">
            <div className={styles.lineWide} />
            <div className={styles.lineMid} />
            <div className={styles.lineShort} />
        </div>
    );
}

export function TileRowSkeleton(): ReactElement {
    return (
        <div className={styles.tiles} aria-hidden="true">
            {Array.from({ length: 6 }, (_unused, index) => (
                <div key={index} className={styles.tile} />
            ))}
        </div>
    );
}

export function CardGridSkeleton(): ReactElement {
    return (
        <div className={styles.grid} aria-hidden="true">
            {Array.from({ length: 6 }, (_unused, index) => (
                <div key={index} className={styles.card}>
                    <div className={styles.lineShort} />
                    <div className={styles.lineWide} />
                    <div className={styles.lineMid} />
                </div>
            ))}
        </div>
    );
}

export function HomePageSkeleton(): ReactElement {
    return (
        <div className={styles.home} aria-hidden="true">
            <ClueBlockSkeleton />
            <div className={styles.week}>
                {Array.from({ length: 7 }, (_unused, index) => (
                    <div key={index} className={styles.day} />
                ))}
            </div>
        </div>
    );
}

export function CourseCarouselSkeleton(): ReactElement {
    return (
        <div className={styles.carousel} aria-hidden="true">
            {Array.from({ length: 3 }, (_value, index) => (
                <div key={index} className={styles.courseCard} />
            ))}
        </div>
    );
}

export function PlayerPageSkeleton(): ReactElement {
    return (
        <div className={styles.player} aria-hidden="true">
            <ClueBlockSkeleton />
            <TileRowSkeleton />
        </div>
    );
}
