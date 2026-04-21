import { Navigate, Route, Routes, useLocation } from "react-router-dom";
import type { ReactNode } from "react";
import AdminQuizListPage from "./pages/AdminQuizListPage";
import AdminQuizDetailsPage from "./pages/AdminQuizDetailsPage";
import AdminSessionControlPage from "./pages/AdminSessionControlPage";
import ParticipantJoinPage from "./pages/ParticipantJoinPage";
import ParticipantSessionPage from "./pages/ParticipantSessionPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import { hasAccessToken } from "./state/authStorage";

function RequireAuth({ children }: { children: ReactNode }) {
    const location = useLocation();

    if (!hasAccessToken()) {
        const from = `${location.pathname}${location.search}${location.hash}`;
        return <Navigate to="/login" replace state={{ from }} />;
    }

    return children;
}

function RequireGuest({ children }: { children: ReactNode }) {
    if (hasAccessToken()) {
        return <Navigate to="/admin/quizzes" replace />;
    }

    return children;
}

export default function App() {
    return (
        <Routes>
            <Route
                path="/login"
                element={
                    <RequireGuest>
                        <LoginPage />
                    </RequireGuest>
                }
            />

            <Route
                path="/register"
                element={
                    <RequireGuest>
                        <RegisterPage />
                    </RequireGuest>
                }
            />

            <Route path="/participant/join" element={<ParticipantJoinPage />} />
            <Route path="/participant/session/:participantId" element={<ParticipantSessionPage />} />

            <Route
                path="/admin/quizzes"
                element={
                    <RequireAuth>
                        <AdminQuizListPage />
                    </RequireAuth>
                }
            />

            <Route
                path="/admin/quizzes/:quizId"
                element={
                    <RequireAuth>
                        <AdminQuizDetailsPage />
                    </RequireAuth>
                }
            />

            <Route
                path="/admin/sessions/:sessionId"
                element={
                    <RequireAuth>
                        <AdminSessionControlPage />
                    </RequireAuth>
                }
            />

            <Route
                path="*"
                element={<Navigate to={hasAccessToken() ? "/admin/quizzes" : "/login"} replace />}
            />
        </Routes>
    );
}
