import { createBrowserRouter, createRoutesFromElements, Route } from "react-router-dom";

import { AuthenticatedRoute } from "../../features/auth/components/ProtectedRoute";
import { AccessDeniedPage } from "../../features/auth/pages/AccessDeniedPage";
import { HomePage } from "../../features/system/pages/HomePage";
import { ApplicationLayout } from "./ApplicationLayout";
import { NotFoundPage } from "./NotFoundPage";
import { SessionBoundary } from "./SessionBoundary";

export const appRouter = createBrowserRouter(
    createRoutesFromElements(
        <Route element={<SessionBoundary />}>
            <Route element={<ApplicationLayout />}>
                <Route index element={<HomePage />} />
                <Route element={<AuthenticatedRoute />}>
                    <Route path="access-denied" element={<AccessDeniedPage />} />
                </Route>
                <Route path="*" element={<NotFoundPage />} />
            </Route>
        </Route>,
    ),
);
