import { useEffect, useState } from "react";
import { adminApi, type QuizListItem } from "../api/adminApi";
import { toErrorMessage } from "../types/problemDetails";

export default function AdminQuizListPage() {
    const [items, setItems] = useState<QuizListItem[]>([]);
    const [error, setError] = useState("");

    async function load() {
        try {
            setError("");
            const data = await adminApi.getQuizzes();
            setItems(data);
        } catch (e) {
            setError(toErrorMessage(e));
        }
    }

    useEffect(() => {
        load();
    }, []);

    return (
        <div>
            <h2>My Quizzes</h2>
            <button onClick={load}>Refresh</button>
            {error && <p style={{ color: "crimson" }}>{error}</p>}
            <ul>
                {items.map((q) => (
                    <li key={q.id}>
                        #{q.id} - {q.title}
                    </li>
                ))}
            </ul>
        </div>
    );
}
