import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { adminApi, type QuizDetailsDto } from "../api/adminApi";
import { toErrorMessage } from "../types/problemDetails";
import "./admin.css";

export default function AdminQuizDetailsPage() {
    const { quizId } = useParams<{ quizId: string }>();
    const navigate = useNavigate();

    const [quiz, setQuiz] = useState<QuizDetailsDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function load() {
            if (!quizId) return;
            setLoading(true);
            setError(null);

            try {
                const data = await adminApi.getQuizById(Number(quizId));
                setQuiz(data);
            } catch (e: unknown) {
                setError(toErrorMessage(e));
            } finally {
                setLoading(false);
            }
        }

        load();
    }, [quizId]);

    async function handleCreateSession() {
        if (!quiz) return;
        setBusy(true);
        setError(null);

        try {
            const session = await adminApi.createSession({ quizId: quiz.id });
            navigate(`/admin/sessions/${session.id}`);
        } catch (e: unknown) {
            setError(toErrorMessage(e));
        } finally {
            setBusy(false);
        }
    }

    if (loading) return <div>Betöltés...</div>;
    if (!quiz) return <div>Nincs ilyen kvíz.</div>;

    return (
        <div className="admin-page">
            <div className="admin-shell">
                <section className="admin-card">
                    <Link className="admin-link" to="/admin/quizzes">← Vissza a listához</Link>
                    <h1 className="admin-title">{quiz.title}</h1>
                    <p className="admin-subtitle">{quiz.description || "Nincs leiras."}</p>
                    <div className="admin-actions admin-actions-top">
                        <button className="admin-button" onClick={handleCreateSession} disabled={busy}>
                            {busy ? "Letrehozas..." : "Session inditasa"}
                        </button>
                    </div>
                </section>

                {error && <p className="admin-error">{error}</p>}

                <section className="admin-card">
                    <h2>Kerdesek</h2>
                    {quiz.questions.length === 0 && <p className="admin-muted">Meg nincs kerdes.</p>}

                    {quiz.questions.map((q) => (
                        <div key={q.id} className="admin-question-card">
                            <strong>
                                {q.orderIndex + 1}. {q.text}
                            </strong>
                            <div className="admin-muted">Idolimit: {q.timeLimitSeconds} sec</div>
                            <ul className="admin-list">
                                {q.options.map((o) => (
                                    <li key={o.id}>
                                        {o.orderIndex + 1}. {o.text}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    ))}
                </section>
            </div>
        </div>
    );
}
