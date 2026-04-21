import { useEffect, useMemo, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import MatrixLoader from "../components/auth/MatrixLoader";
import ProblemAlert from "../components/ProblemAlert";
import { authApi } from "../api/authApi";
import { saveAuthSession } from "../state/authStorage";
import { toErrorMessage } from "../types/problemDetails";
import "./auth.css";

const bootMessages = [
    "[boot] neural gateway online",
    "[tls] validating secure tunnel",
    "[auth] checking identity signature",
    "[auth] generating access vectors",
];

export default function LoginPage() {
    const navigate = useNavigate();
    const location = useLocation();
    const from = useMemo(
        () => (location.state as { from?: string } | null)?.from || "/admin/quizzes",
        [location.state]
    );

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [isSuccess, setIsSuccess] = useState(false);
    const [messages, setMessages] = useState<string[]>([]);

    useEffect(() => {
        if (!isLoading) return;

        let index = 0;
        setMessages([bootMessages[0]]);

        const timer = window.setInterval(() => {
            index += 1;
            if (index < bootMessages.length) {
                setMessages((prev) => [...prev, bootMessages[index]]);
                return;
            }
            window.clearInterval(timer);
        }, 220);

        return () => window.clearInterval(timer);
    }, [isLoading]);

    async function handleSubmit() {
        setError(null);

        if (!email.trim() || !password.trim()) {
            setError("Email and password are required.");
            return;
        }

        try {
            setIsLoading(true);
            setIsSuccess(false);

            const result = await authApi.login({
                email: email.trim(),
                password,
            });

            saveAuthSession(result);
            setMessages((prev) => [...prev, "[ok] connection established"]);
            setIsSuccess(true);

            window.setTimeout(() => {
                navigate(from, { replace: true });
            }, 450);
        } catch (err) {
            setError(toErrorMessage(err));
            setIsLoading(false);
            setIsSuccess(false);
            setMessages([]);
        }
    }

    return (
        <div className="auth-page">
            <div className="auth-bg-grid" />
            <div className="auth-bg-glow-cyan" />
            <div className="auth-bg-glow-pink" />

            <section className="auth-card">
                <h1 className="auth-title">
                    <span className="auth-title-bracket">[</span> Neural Link <span className="auth-title-bracket">]</span>
                </h1>
                <p className="auth-subtitle">Establish secure admin connection</p>

                {isLoading ? (
                    <MatrixLoader lines={messages} isSuccess={isSuccess} />
                ) : (
                    <form
                        className="auth-form"
                        onSubmit={(e) => {
                            e.preventDefault();
                            void handleSubmit();
                        }}
                    >
                        <label className="auth-field">
                            <span className="auth-label">Email</span>
                            <input
                                className="auth-input"
                                type="email"
                                autoComplete="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                placeholder="pilot@realtimequiz.dev"
                            />
                        </label>

                        <label className="auth-field">
                            <span className="auth-label">Password</span>
                            <input
                                className="auth-input"
                                type="password"
                                autoComplete="current-password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                placeholder="••••••••"
                            />
                        </label>

                        <ProblemAlert message={error} />

                        <button className="auth-button" type="submit">
                            Initiate connection
                        </button>
                    </form>
                )}

                {!isLoading && (
                    <p className="auth-footer">
                        No account yet? <Link className="auth-link" to="/register">Register here</Link>
                    </p>
                )}
            </section>
        </div>
    );
}
