import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { adminApi, type SessionDto } from "../api/adminApi";
import { toErrorMessage } from "../types/problemDetails";
import { useSignalRHub } from "../hooks/useSignalRHub";
import "./admin.css";

function stateLabel(state: number): string {
    switch (state) {
        case 0: return "Created";
        case 1: return "LobbyOpen";
        case 2: return "QuestionOpen";
        case 3: return "QuestionClosed";
        case 4: return "Finished";
        default: return `Unknown(${state})`;
    }
}

export default function AdminSessionControlPage() {
    const { sessionId } = useParams<{ sessionId: string }>();
    const numericSessionId = Number(sessionId);

    const [session, setSession] = useState<SessionDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [busyAction, setBusyAction] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const { isConnected } = useSignalRHub({
        sessionId: numericSessionId,
        onParticipantJoined: (data) => {
            console.log("Participant joined:", data);
        },
        onAnswerSubmitted: (data) => {
            console.log("Answer submitted:", data);
        },
        onQuestionStarted: () => {
            loadSession();
        },
        onQuestionClosed: () => {
            loadSession();
        },
        onSessionFinished: () => {
            loadSession();
        }
    });

    async function loadSession() {
        if (!numericSessionId) return;
        setLoading(true);
        setError(null);

        try {
            const data = await adminApi.getSessionById(numericSessionId);
            setSession(data);
        } catch (e: unknown) {
            setError(toErrorMessage(e));
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        loadSession();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [numericSessionId]);

    async function runAction(name: string, action: () => Promise<SessionDto>) {
        setBusyAction(name);
        setError(null);

        try {
            const updated = await action();
            setSession(updated);
        } catch (e: unknown) {
            setError(toErrorMessage(e));
        } finally {
            setBusyAction(null);
        }
    }

    if (loading) return <div>Betöltés...</div>;
    if (!session) return <div>Nincs session adat.</div>;

    return (
        <div className="admin-page">
            <div className="admin-shell">
                <section className="admin-card">
                    <Link className="admin-link" to={`/admin/quizzes/${session.quizId}`}>← Vissza a kvzhez</Link>
                    <h1 className="admin-title">Session #{session.id}</h1>

                    <div className="admin-kpi">
                        <span><strong>SignalR:</strong> <span style={{ color: isConnected ? "green" : "red" }}>{isConnected ? "Connected" : "Disconnected"}</span></span>
                        <span><strong>PIN:</strong> {session.joinPin}</span>
                        <span><strong>Allapot:</strong> {stateLabel(session.state)}</span>
                        <span><strong>Aktualis kerdes:</strong> {session.currentQuestionText ?? "-"}</span>
                        <span><strong>Kerdes index:</strong> {session.currentQuestionOrderIndex ?? "-"}</span>
                    </div>
                </section>

                <section className="admin-card">
                    <div className="admin-actions">
                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() => runAction("openLobby", () => adminApi.openLobby(session.id))}
                        >
                            Lobby megnyitas
                        </button>

                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() => runAction("startSession", () => adminApi.startSession(session.id))}
                        >
                            Inditas
                        </button>

                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() =>
                                runAction("closeCurrentQuestion", () => adminApi.closeCurrentQuestion(session.id))
                            }
                        >
                            Kerdes lezarasa
                        </button>

                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() => runAction("advance", () => adminApi.advance(session.id))}
                        >
                            Kovetkezo kerdes
                        </button>

                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() => runAction("finish", () => adminApi.finish(session.id))}
                        >
                            Befejezes
                        </button>

                        <button className="admin-button admin-button-secondary" disabled={!!busyAction} onClick={loadSession}>
                            Frissites
                        </button>
                    </div>
                </section>

                {busyAction && <section className="admin-card admin-muted">Futo muvelet: {busyAction}</section>}
                {error && <p className="admin-error">{error}</p>}
            </div>
        </div>
    );
}
