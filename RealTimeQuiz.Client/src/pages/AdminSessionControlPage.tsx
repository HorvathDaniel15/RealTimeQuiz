import { useEffect, useState, useRef } from "react";
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
    const [timeLeft, setTimeLeft] = useState<number | null>(null);
    const autoCloseTriggered = useRef(false);

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
    }, [numericSessionId]);

    useEffect(() => {
        const timeLimit = session?.currentQuestionTimeLimitSeconds ?? session?.currentQuestion?.timeLimitSeconds;
        if (!session || session.state !== 2 || !timeLimit || !session.questionOpenedAtUtc) {
            setTimeLeft(null);
            autoCloseTriggered.current = false;
            return;
        }

        const limit = timeLimit;
        const openedTimeStr = session.questionOpenedAtUtc.endsWith("Z")
            ? session.questionOpenedAtUtc
            : session.questionOpenedAtUtc + "Z";
        const openedAt = new Date(openedTimeStr).getTime();
        const expiresAt = openedAt + limit * 1000;

        const updateTimer = () => {
            const now = new Date().getTime();
            const remaining = Math.max(0, Math.floor((expiresAt - now) / 1000));
            setTimeLeft(remaining);

            // Ha lejárt az idő ÉS még nem zártuk le automatikusan
            if (remaining === 0 && !autoCloseTriggered.current) {
                autoCloseTriggered.current = true; // Jelezzük, hogy elsütöttük

                // Meghívjuk pontosan azt a funkciót, amit a gomb is csinálna:
                runAction("closeCurrentQuestion", () => adminApi.closeCurrentQuestion(session.id));
            }
        };

        updateTimer();
        const intervalId = setInterval(updateTimer, 1000);

        return () => clearInterval(intervalId);
    }, [session]); // A UseEffect újratölt, ha a session változik

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
                        <span><strong>Aktualis kerdes:</strong> {session.currentQuestionText ?? session.currentQuestion?.text ?? "-"}</span>
                        <span><strong>Kerdes index:</strong> {(session.currentQuestionOrderIndex ?? session.currentQuestion?.orderIndex)?.toString() ?? "-"}</span>
                        {timeLeft !== null &&(
                            <span>
                                <strong>Hátralévő idő:</strong>
                                <span style={{color: timeLeft <= 5 ? "red" : "inherit"}}>
                                    {timeLeft} másodperc
                                </span>
                            </span>
                        )}
                    </div>
                </section>

                <section className="admin-card">
                    <div className="admin-actions">
                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() => runAction("openLobby", () => adminApi.openLobby(session.id))}
                        >
                            Open to Lobby
                        </button>

                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() => runAction("startSession", () => adminApi.startSession(session.id))}
                        >
                            Start
                        </button>

                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() =>
                                runAction("closeCurrentQuestion", () => adminApi.closeCurrentQuestion(session.id))
                            }
                        >
                            Close Question
                        </button>

                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() => runAction("advance", () => adminApi.advance(session.id))}
                        >
                            Next Question
                        </button>

                        <button
                            className="admin-button"
                            disabled={!!busyAction}
                            onClick={() => runAction("finish", () => adminApi.finish(session.id))}
                        >
                            Finish
                        </button>

                        <button className="admin-button admin-button-secondary" disabled={!!busyAction} onClick={loadSession}>
                            Update
                        </button>
                    </div>
                </section>

                {busyAction && <section className="admin-card admin-muted">Futo muvelet: {busyAction}</section>}
                {error && <p className="admin-error">{error}</p>}
            </div>
        </div>
    );
}
