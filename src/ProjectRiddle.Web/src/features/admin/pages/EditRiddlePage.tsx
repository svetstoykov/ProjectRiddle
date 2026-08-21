import type { ReactElement } from "react";
import { useParams } from "react-router-dom";

import { PageStatus } from "../../../shared/components/PageStatus";
import { RiddleEditor } from "../components/RiddleEditor";

export function EditRiddlePage(): ReactElement {
    const { riddleId } = useParams();

    if (riddleId === undefined || riddleId.length === 0) {
        return (
            <PageStatus
                tone="error"
                eyebrow="Администрация"
                title="Загадката не е намерена"
                message="Липсва идентификатор на загадката."
            />
        );
    }

    return <RiddleEditor riddleId={riddleId} />;
}
