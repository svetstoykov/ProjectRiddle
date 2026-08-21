import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";

import { sessionQueryOptions } from "../api/sessionQuery";
import type { Session } from "../models/auth";

function useResolvedSession(): Session | null | undefined {
    const sessionQuery = useQuery(sessionQueryOptions);
    return sessionQuery.data;
}

function signInLocation(pathname: string, search: string, hash: string): string {
    const returnTo = `${pathname}${search}${hash}`;
    return `/sign-in?returnTo=${encodeURIComponent(returnTo)}`;
}

export function AnonymousOnlyRoute(): ReactElement | null {
    const session = useResolvedSession();

    if (session === undefined) {
        return null;
    }

    if (session !== null) {
        return <Navigate replace to="/" />;
    }

    return <Outlet />;
}

export function AuthenticatedRoute(): ReactElement | null {
    const session = useResolvedSession();
    const location = useLocation();

    if (session === undefined) {
        return null;
    }

    if (session === null) {
        return <Navigate replace to={signInLocation(location.pathname, location.search, location.hash)} />;
    }

    return <Outlet />;
}

export function AdminRoute(): ReactElement | null {
    const session = useResolvedSession();
    const location = useLocation();

    if (session === undefined) {
        return null;
    }

    if (session === null) {
        return <Navigate replace to={signInLocation(location.pathname, location.search, location.hash)} />;
    }

    if (session.role !== "admin") {
        return <Navigate replace to="/access-denied" />;
    }

    return <Outlet />;
}
