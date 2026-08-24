import type { ReactElement, RefObject } from "react";

import { ClueTermText } from "../../../shared/components/ClueTermText";
import { InfoDialog } from "../../../shared/components/InfoDialog";
import type { CourseLessonDetail } from "../models/courseCatalog";
import { dismissLessonIntro } from "../storage/anonymousCourseProgress";

export interface LessonIntroDialogProps {
    readonly lesson: CourseLessonDetail;
    readonly open: boolean;
    readonly onDismiss: () => void;
    readonly returnFocusRef: RefObject<HTMLElement | null>;
}

export function LessonIntroDialog({ lesson, open, onDismiss, returnFocusRef }: LessonIntroDialogProps): ReactElement {
    return (
        <InfoDialog
            open={open}
            title={lesson.title}
            onClose={() => {
                dismissLessonIntro(lesson.key);
                onDismiss();
            }}
            returnFocusRef={returnFocusRef}
        >
            <p>{lesson.intro === undefined ? null : <ClueTermText text={lesson.intro} />}</p>
        </InfoDialog>
    );
}
