import { useId, type ReactElement } from "react";

import type { Course } from "../models/courseCatalog";
import { CoursePreviewCard } from "./CoursePreviewCard";
import styles from "./CourseCarousel.module.css";

export interface CourseCarouselProps {
    readonly courses: readonly Course[];
    readonly heading: string;
    readonly lead?: string;
}

export function CourseCarousel({ courses, heading, lead }: CourseCarouselProps): ReactElement {
    const headingId = useId();

    return (
        <section className={styles.section} aria-labelledby={headingId}>
            <h2 id={headingId}>{heading}</h2>
            {lead === undefined ? null : <p className={styles.lead}>{lead}</p>}
            <div className={styles.track}>
                {courses.map((course) => (
                    <CoursePreviewCard key={course.id} course={course} />
                ))}
            </div>
        </section>
    );
}
