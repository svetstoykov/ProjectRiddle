import type { ReactElement } from "react";

import { CardGridSkeleton, CourseCarouselSkeleton } from "../../../shared/components/ContentSkeletons";

export function CourseHubSkeleton(): ReactElement {
    return (
        <div aria-hidden="true">
            <CardGridSkeleton />
            <CourseCarouselSkeleton />
        </div>
    );
}
