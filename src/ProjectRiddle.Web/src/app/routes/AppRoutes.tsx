import { createBrowserRouter, createRoutesFromElements, Navigate, Route } from "react-router-dom";

import { AdminRiddleListPage } from "../../features/admin/pages/AdminRiddleListPage";
import { EditRiddlePage } from "../../features/admin/pages/EditRiddlePage";
import { NewRiddlePage } from "../../features/admin/pages/NewRiddlePage";
import { AnonymousOnlyRoute, AdminRoute, AuthenticatedRoute } from "../../features/auth/components/ProtectedRoute";
import { AccessDeniedPage } from "../../features/auth/pages/AccessDeniedPage";
import { RegisterPage } from "../../features/auth/pages/RegisterPage";
import { SignInPage } from "../../features/auth/pages/SignInPage";
import { HomePage } from "../../features/system/pages/HomePage";
import { ApplicationLayout } from "./ApplicationLayout";
import { NotFoundPage } from "./NotFoundPage";
import { SessionBoundary } from "./SessionBoundary";

export const appRouter = createBrowserRouter(
    createRoutesFromElements(
        <Route element={<SessionBoundary />}>
            <Route element={<ApplicationLayout />}>
                <Route index element={<HomePage />} />
                <Route element={<AnonymousOnlyRoute />}>
                    <Route path="register" element={<RegisterPage />} />
                    <Route path="sign-in" element={<SignInPage />} />
                </Route>
                <Route element={<AuthenticatedRoute />}>
                    <Route path="access-denied" element={<AccessDeniedPage />} />
                </Route>
                <Route element={<AdminRoute />}>
                    <Route path="admin" element={<Navigate replace to="/admin/riddles" />} />
                    <Route path="admin/riddles" element={<AdminRiddleListPage />} />
                    <Route path="admin/riddles/new" element={<NewRiddlePage />} />
                    <Route path="admin/riddles/:riddleId" element={<EditRiddlePage />} />
                </Route>
                <Route path="*" element={<NotFoundPage />} />
            </Route>
        </Route>,
    ),
);
