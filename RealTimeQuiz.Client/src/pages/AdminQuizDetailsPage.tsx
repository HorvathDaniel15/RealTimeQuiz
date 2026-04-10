import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { adminApi, type QuizDetailsDto } from "../api/adminApi";
import { toErrorMessage } from "../types/problemDetails";

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
        <div style={{ padding: 16 }}>
            <Link to="/admin/quizzes">← Vissza a listához</Link>
            <h1>{quiz.title}</h1>
            <p>{quiz.description || "Nincs leírás."}</p>

            <button onClick={handleCreateSession} disabled={busy}>
                {busy ? "Létrehozás..." : "Session indítása"}
            </button>

            {error && <p style={{ color: "crimson" }}>{error}</p>}

            <h2>Kérdések</h2>
            {quiz.questions.length === 0 && <p>Még nincs kérdés.</p>}

            {quiz.questions.map((q) => (
                <div key={q.id} style={{ border: "1px solid #ddd", margin: "12px 0", padding: 12 }}>
                    <strong>
                        {q.orderIndex + 1}. {q.text}
                    </strong>
                    <div>Időlimit: {q.timeLimitSeconds} sec</div>
                    <ul>
                        {q.options.map((o) => (
                            <li key={o.id}>
                                {o.orderIndex + 1}. {o.text}
                            </li>
                        ))}
                    </ul>
                </div>
            ))}
        </div>
    );
}
