import { useState } from "react";

type Props = { onSaved: () => void };

export default function OwnerSetupPage({ onSaved }: Props) {
    const [ownerId, setOwnerId] = useState(localStorage.getItem("ownerId") ?? "demo-admin-1");

    return (
        <div>
            <h2>Admin Owner Setup (MVP)</h2>
            <input value={ownerId} onChange={(e) => setOwnerId(e.target.value)} />
            <button
                onClick={() => {
                    localStorage.setItem("ownerId", ownerId.trim());
                    onSaved();
                }}
            >
                Save Owner ID
            </button>
        </div>
    );
}
