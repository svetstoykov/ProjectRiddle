import type { ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import { DocumentTitle } from "../../../shared/components/DocumentTitle";
import { PageStatus } from "../../../shared/components/PageStatus";

export function AccessDeniedPage(): ReactElement {
    const navigate = useNavigate();

    return (
        <>
            <DocumentTitle title="Няма достъп" />
            <PageStatus
                eyebrow="Достъп"
                title="Няма достъп"
                message="Няма достъп."
                action={{
                    label: "Към началото",
                    onClick: () => {
                        void navigate("/");
                    },
                }}
            />
        </>
    );
}
