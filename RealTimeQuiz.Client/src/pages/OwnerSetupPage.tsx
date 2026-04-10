import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import "./admin.css";

type Props = { onSaved?: () => void };

export default function OwnerSetupPage({ onSaved }: Props) {
    const navigate = useNavigate();
    const location = useLocation();
    const [ownerId, setOwnerId] = useState(localStorage.getItem("ownerId") ?? "demo-admin-1");
    const from = (location.state as { from?: string } | null)?.from;

    return (
        <div className="admin-page">
            <div className="admin-shell">
                <section className="admin-card">
                    <h1 className="admin-title">Admin owner setup</h1>
                    <p className="admin-subtitle">MVP azonositashoz add meg az owner ID-t.</p>
                </section>

                <section className="admin-card admin-form-grid">
                    <label className="admin-field">
                        <span className="admin-label">Owner ID</span>
                        <input
                            className="admin-input"
                            value={ownerId}
                            onChange={(e) => setOwnerId(e.target.value)}
                        />
                    </label>

                    <div className="admin-actions">
                        <button
                            className="admin-button"
                            onClick={() => {
                                localStorage.setItem("ownerId", ownerId.trim());
                                onSaved?.();
                                navigate(from || "/admin/quizzes", { replace: true });
                            }}
                        >
                            Save Owner ID
                        </button>
                    </div>
                </section>
            </div>
        </div>
    );
}
