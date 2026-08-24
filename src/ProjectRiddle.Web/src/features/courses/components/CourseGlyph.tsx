import type { ReactElement } from "react";

import { glyphMarkForLesson } from "../messages/coursePresentation";
import styles from "./CourseGlyph.module.css";

export type CourseGlyphTone = "preview" | "hub";

export interface CourseGlyphProps {
    readonly lessonKey: string;
}

export function CourseGlyph({ lessonKey }: CourseGlyphProps): ReactElement {
    return (
        <span className={styles.glyph} aria-hidden="true">
            {glyphMarkForLesson(lessonKey)}
        </span>
    );
}

export interface CourseGlyphStackProps {
    readonly lessonKeys: readonly string[];
    readonly tone: CourseGlyphTone;
}

export function CourseGlyphStack({ lessonKeys, tone }: CourseGlyphStackProps): ReactElement {
    return (
        <span className={`${styles.stack} ${styles[tone]}`} aria-hidden="true">
            {lessonKeys.map((lessonKey, index) => (
                <CourseGlyph key={`${lessonKey}-${index}`} lessonKey={lessonKey} />
            ))}
        </span>
    );
}
