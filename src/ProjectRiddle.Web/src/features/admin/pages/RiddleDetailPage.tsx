import type { ReactElement } from "react";
import { useParams } from "react-router-dom";

import { PageStatus } from "../../../shared/components/PageStatus";
import { RiddleDetail } from "../components/RiddleDetail";

export function RiddleDetailPage(): ReactElement {
    const { riddleId } = useParams();

    if (riddleId === undefined || riddleId.length === 0) {
        return (
            <PageStatus
                tone="error"
                eyebrow="Администрация"
                title="Криптиката не е намерена"
                message="Липсва идентификатор на криптиката."
            />
        );
    }

    return <RiddleDetail riddleId={riddleId} />;
}
