import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { adminApi, type SessionDto } from "../api/adminApi";

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

    async function loadSession() {
        if (!numericSessionId) return;
        setLoading(true);
        setError(null);

        try {
            const data = await adminApi.getSessionById(numericSessionId);
            setSession(data);
        } catch (e: any) {
            setError(e?.message ?? "Session betöltése sikertelen.");
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
        } catch (e: any) {
            setError(e?.message ?? `Sikertelen művelet: ${name}`);
        } finally {
            setBusyAction(null);
        }
    }

    if (loading) return <div>Betöltés...</div>;
    if (!session) return <div>Nincs session adat.</div>;

    return (
        <div style={{ padding: 16 }}>
            <Link to={`/admin/quizzes/${session.quizId}`}>← Vissza a kvízhez</Link>
            <h1>Session #{session.id}</h1>

            <p><strong>PIN:</strong> {session.joinPin}</p>
            <p><strong>Állapot:</strong> {stateLabel(session.state)}</p>
            <p><strong>Aktuális kérdés:</strong> {session.currentQuestionText ?? "-"}</p>
            <p><strong>Kérdés index:</strong> {session.currentQuestionOrderIndex ?? "-"}</p>

            <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                <button
                    disabled={!!busyAction}
                    onClick={() => runAction("openLobby", () => adminApi.openLobby(session.id))}
                >
                    Lobby megnyitás
                </button>

                <button
                    disabled={!!busyAction}
                    onClick={() => runAction("startSession", () => adminApi.startSession(session.id))}
                >
                    Indítás
                </button>

                <button
                    disabled={!!busyAction}
                    onClick={() =>
                        runAction("closeCurrentQuestion", () => adminApi.closeCurrentQuestion(session.id))
                    }
                >
                    Kérdés lezárása
                </button>

                <button
                    disabled={!!busyAction}
                    onClick={() => runAction("advance", () => adminApi.advance(session.id))}
                >
                    Következő kérdés
                </button>

                <button
                    disabled={!!busyAction}
                    onClick={() => runAction("finish", () => adminApi.finish(session.id))}
                >
                    Befejezés
                </button>

                <button disabled={!!busyAction} onClick={loadSession}>
                    Frissítés
                </button>
            </div>

            {busyAction && <p>Futó művelet: {busyAction}</p>}
            {error && <p style={{ color: "crimson" }}>{error}</p>}
        </div>
    );
}
