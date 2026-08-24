import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import type { Course } from "../models/courseCatalog";
import { CourseGlyphStack } from "./CourseGlyph";
import styles from "./CoursePreviewCard.module.css";

export interface CoursePreviewCardProps {
    readonly course: Course;
}

export function CoursePreviewCard({ course }: CoursePreviewCardProps): ReactElement {
    const glyphKeys =
        course.key === "finale"
            ? ["letterplay-mix", "wordplay-mix", "weirdplay-mix"]
            : course.lessons.filter((lesson) => lesson.kind === "technique").map((lesson) => lesson.key);

    return (
        <Link className={styles.card} to={`/courses/${course.key}`}>
            <span className={styles.ordinal}>Курс {course.ordinal}</span>
            <strong className={styles.title}>{course.title}</strong>
            <CourseGlyphStack lessonKeys={glyphKeys} tone="preview" />
        </Link>
    );
}
