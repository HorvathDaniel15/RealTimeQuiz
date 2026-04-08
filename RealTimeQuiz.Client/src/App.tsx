import { Navigate, Route, Routes, useLocation } from "react-router-dom";
import type { ReactNode } from "react";
import OwnerSetupPage from "./pages/OwnerSetupPage";
import AdminQuizListPage from "./pages/AdminQuizListPage";
import AdminQuizDetailsPage from "./pages/AdminQuizDetailsPage";
import AdminSessionControlPage from "./pages/AdminSessionControlPage";

function RequireOwner({ children }: { children: ReactNode }) {
    const location = useLocation();
    const ownerId = localStorage.getItem("ownerId");
    if (!ownerId) {
        const from = `${location.pathname}${location.search}${location.hash}`;
        return <Navigate to="/owner-setup" replace state={{ from }} />;
    }

    return children;
}

export default function App() {
    return (
        <Routes>
            <Route path="/owner-setup" element={<OwnerSetupPage />} />

            <Route
                path="/admin/quizzes"
                element={
                    <RequireOwner>
                        <AdminQuizListPage />
                    </RequireOwner>
                }
            />

            <Route
                path="/admin/quizzes/:quizId"
                element={
                    <RequireOwner>
                        <AdminQuizDetailsPage />
                    </RequireOwner>
                }
            />

            <Route
                path="/admin/sessions/:sessionId"
                element={
                    <RequireOwner>
                        <AdminSessionControlPage />
                    </RequireOwner>
                }
            />

            <Route path="*" element={<Navigate to="/admin/quizzes" replace />} />
        </Routes>
    );
}
