import type { ReactElement } from "react";
import { RouterProvider } from "react-router-dom";

import { appRouter } from "./routes/AppRoutes";

export function App(): ReactElement {
    return <RouterProvider router={appRouter} />;
}
