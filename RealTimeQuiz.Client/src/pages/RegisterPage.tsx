import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import ProblemAlert from "../components/ProblemAlert";
import { authApi } from "../api/authApi";
import { toErrorMessage } from "../types/problemDetails";
import "./auth.css";

export default function RegisterPage() {
    const navigate = useNavigate();

    const [email, setEmail] = useState("");
    const [userName, setUserName] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [isBusy, setIsBusy] = useState(false);

    async function handleSubmit() {
        setError(null);
        setSuccess(null);

        if (!email.trim() || !userName.trim() || !password.trim()) {
            setError("Email, user name and password are required.");
            return;
        }

        if (password !== confirmPassword) {
            setError("Passwords do not match.");
            return;
        }

        try {
            setIsBusy(true);
            await authApi.register({
                email: email.trim(),
                userName: userName.trim(),
                password,
            });

            setSuccess("Registration completed. Redirecting to login...");
            window.setTimeout(() => navigate("/login", { replace: true }), 900);
        } catch (err) {
            setError(toErrorMessage(err));
        } finally {
            setIsBusy(false);
        }
    }

    return (
        <div className="auth-page">
            <div className="auth-bg-grid" />
            <div className="auth-bg-glow-cyan" />
            <div className="auth-bg-glow-pink" />

            <section className="auth-card">
                <h1 className="auth-title">Register</h1>
                <p className="auth-subtitle">Create a new admin identity</p>

                <form
                    className="auth-form"
                    onSubmit={(e) => {
                        e.preventDefault();
                        void handleSubmit();
                    }}
                >
                    {success && <p className="auth-success">{success}</p>}
                    <ProblemAlert message={error} />

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
                        <span className="auth-label">User Name</span>
                        <input
                            className="auth-input"
                            type="text"
                            autoComplete="username"
                            value={userName}
                            onChange={(e) => setUserName(e.target.value)}
                            placeholder="danimester23"
                        />
                    </label>

                    <label className="auth-field">
                        <span className="auth-label">Password</span>
                        <input
                            className="auth-input"
                            type="password"
                            autoComplete="new-password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="At least 8 chars"
                        />
                    </label>

                    <label className="auth-field">
                        <span className="auth-label">Confirm Password</span>
                        <input
                            className="auth-input"
                            type="password"
                            autoComplete="new-password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                            placeholder="Repeat password"
                        />
                    </label>

                    <button className="auth-button" type="submit" disabled={isBusy}>
                        {isBusy ? "Processing..." : "Register"}
                    </button>
                </form>

                <p className="auth-footer">
                    Already registered? <Link className="auth-link" to="/login">Go to login</Link>
                </p>
            </section>
        </div>
    );
}
