import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { participantApi } from "../api/participantApi";
import ProblemAlert from "../components/ProblemAlert";
import { toErrorMessage } from "../types/problemDetails";
import { saveParticipantContext } from "../types/participant";
import "./participant.css";

export default function ParticipantJoinPage() {
    const navigate = useNavigate();
    const [joinPin, setJoinPin] = useState("");
    const [displayName, setDisplayName] = useState("");
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function onSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        const normalizedPin = joinPin.trim();
        const normalizedDisplayName = displayName.trim();

        if (normalizedPin.length !== 6) {
            setError("A PIN pontosan 6 karakter legyen.");
            return;
        }

        if (!normalizedDisplayName) {
            setError("A megjelenitendo nev kotelezo.");
            return;
        }

        setBusy(true);
        setError(null);

        try {
            const joined = await participantApi.joinSession({
                joinPin: normalizedPin,
                displayName: normalizedDisplayName,
            });

            saveParticipantContext({
                participantId: joined.participantId,
                sessionId: joined.sessionId,
                joinPin: joined.joinPin,
                displayName: joined.displayName,
            });

            navigate(`/participant/session/${joined.participantId}`);
        } catch (e: unknown) {
            setError(toErrorMessage(e));
        } finally {
            setBusy(false);
        }
    }

    return (
        <div className="participant-page">
            <div className="participant-shell">
                <section className="participant-card">
                    <h1 className="participant-title">Join Quiz Session</h1>
                    <p className="participant-subtitle">
                        Add meg a PIN-t es a neved, utana csatlakozol az aktualis sessionhoz.
                    </p>
                </section>

                <section className="participant-card">
                    <form onSubmit={onSubmit} className="participant-form">
                        <label className="participant-field">
                            <span className="participant-field-label">Session PIN</span>
                            <input
                                className="participant-input"
                                value={joinPin}
                                maxLength={6}
                                placeholder="pl. 376675"
                                onChange={(e) => setJoinPin(e.target.value)}
                            />
                        </label>

                        <label className="participant-field">
                            <span className="participant-field-label">Display name</span>
                            <input
                                className="participant-input"
                                value={displayName}
                                maxLength={100}
                                placeholder="pl. Dani"
                                onChange={(e) => setDisplayName(e.target.value)}
                            />
                        </label>

                        <div className="participant-actions">
                            <button className="participant-button" type="submit" disabled={busy}>
                                {busy ? "Joining..." : "Join"}
                            </button>
                            <Link className="participant-button participant-button-secondary" to="/admin/quizzes">
                                Go to admin
                            </Link>
                        </div>
                    </form>
                </section>

                <ProblemAlert message={error} />
            </div>
        </div>
    );
}
