import { createBrowserRouter, createRoutesFromElements, Navigate, Route } from "react-router-dom";

import { AdminRiddleListPage } from "../../features/admin/pages/AdminRiddleListPage";
import { NewRiddlePage } from "../../features/admin/pages/NewRiddlePage";
import { RiddleDetailPage } from "../../features/admin/pages/RiddleDetailPage";
import { AnonymousOnlyRoute, AdminRoute, AuthenticatedRoute } from "../../features/auth/components/ProtectedRoute";
import { AccessDeniedPage } from "../../features/auth/pages/AccessDeniedPage";
import { RegisterPage } from "../../features/auth/pages/RegisterPage";
import { SignInPage } from "../../features/auth/pages/SignInPage";
import { ArchivePage } from "../../features/riddles/pages/ArchivePage";
import { HomePage } from "../../features/riddles/pages/HomePage";
import { RiddlePlayerPage } from "../../features/riddles/pages/RiddlePlayerPage";
import { ApplicationLayout } from "./ApplicationLayout";
import { NotFoundPage } from "./NotFoundPage";
import { SessionBoundary } from "./SessionBoundary";
import { SolvingLayout } from "./SolvingLayout";

export const appRouter = createBrowserRouter(
    createRoutesFromElements(
        <Route>
            {/* Solving runs in its own shell: the riddle takes the whole screen and the page draws its own way back. */}
            <Route element={<SolvingLayout />}>
                <Route element={<SessionBoundary />}>
                    <Route path="riddles/today" element={<RiddlePlayerPage />} />
                    <Route path="riddles/:riddleId" element={<RiddlePlayerPage />} />
                </Route>
            </Route>
            <Route element={<ApplicationLayout />}>
                <Route element={<SessionBoundary />}>
                    <Route index element={<HomePage />} />
                    <Route path="archive" element={<ArchivePage />} />
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
                        <Route path="admin/riddles/:riddleId" element={<RiddleDetailPage />} />
                    </Route>
                    <Route path="*" element={<NotFoundPage />} />
                </Route>
            </Route>
        </Route>,
    ),
);
