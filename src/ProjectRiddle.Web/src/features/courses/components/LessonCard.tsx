import type { ReactElement } from "react";
import { Link } from "react-router-dom";

import { ClueTermText } from "../../../shared/components/ClueTermText";
import type { CourseLessonSummary } from "../models/courseCatalog";
import type { ResolvedLessonProgress } from "../models/courseProgress";
import { CourseGlyphStack } from "./CourseGlyph";
import { CourseProgressPill } from "./CourseProgressPill";
import styles from "./LessonCard.module.css";

export interface LessonCardProps {
    readonly courseKey: string;
    readonly lesson: CourseLessonSummary;
    readonly progress: ResolvedLessonProgress;
    readonly glyphKeys: readonly string[];
    readonly lockedReason?: string;
}

export function LessonCard(props: LessonCardProps): ReactElement {
    const body = (
        <>
            <CourseGlyphStack lessonKeys={props.glyphKeys} tone="hub" />
            <h3>
                <ClueTermText text={props.lesson.title} />
            </h3>
            <CourseProgressPill completed={props.progress.completedExerciseCount} total={props.lesson.exerciseCount} />
        </>
    );

    return props.progress.isAvailable ? (
        <Link className={styles.card} to={`/courses/${props.courseKey}/${props.lesson.key}/1`}>
            {body}
        </Link>
    ) : (
        <article
            className={styles.locked}
            aria-label={`${props.lesson.title}, заключено. ${props.lockedReason ?? ""}`}
            aria-disabled="true"
        >
            {body}
        </article>
    );
}
