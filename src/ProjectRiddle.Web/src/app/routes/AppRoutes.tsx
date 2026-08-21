import type { ReactElement } from "react";
import { Route, Routes } from "react-router-dom";

import { ApplicationLayout } from "./ApplicationLayout";
import { NotFoundPage } from "./NotFoundPage";
import { HomePage } from "../../features/system/pages/HomePage";

export function AppRoutes(): ReactElement {
    return (
        <Routes>
            <Route element={<ApplicationLayout />}>
                <Route index element={<HomePage />} />
                <Route path="*" element={<NotFoundPage />} />
            </Route>
        </Routes>
    );
}
