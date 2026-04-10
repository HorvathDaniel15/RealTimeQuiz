import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";

type Props = { onSaved?: () => void };

export default function OwnerSetupPage({ onSaved }: Props) {
    const navigate = useNavigate();
    const location = useLocation();
    const [ownerId, setOwnerId] = useState(localStorage.getItem("ownerId") ?? "demo-admin-1");
    const from = (location.state as { from?: string } | null)?.from;

    return (
        <div>
            <h2>Admin Owner Setup (MVP)</h2>
            <input value={ownerId} onChange={(e) => setOwnerId(e.target.value)} />
            <button
                onClick={() => {
                    localStorage.setItem("ownerId", ownerId.trim());
                    onSaved?.();
                    navigate(from || "/admin/quizzes", { replace: true });
                }}
            >
                Save Owner ID
            </button>
        </div>
    );
}
