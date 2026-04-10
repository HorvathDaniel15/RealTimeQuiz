import { useEffect, useState, type SubmitEvent } from "react";
import { Link } from "react-router-dom";
import { adminApi, type QuizListItem } from "../api/adminApi";
import { toErrorMessage } from "../types/problemDetails";

type DraftOption = {
    text: string;
};

type DraftQuestion = {
    text: string;
    timeLimitSeconds: number;
    options: DraftOption[];
    correctOptionIndex: number;
};

function createEmptyQuestion(): DraftQuestion {
    return {
        text: "",
        timeLimitSeconds: 20,
        options: [{ text: "" }, { text: "" }],
        correctOptionIndex: 0,
    };
}

export default function AdminQuizListPage() {
    const [items, setItems] = useState<QuizListItem[]>([]);
    const [error, setError] = useState("");
    const [createBusy, setCreateBusy] = useState(false);
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [questions, setQuestions] = useState<DraftQuestion[]>([createEmptyQuestion()]);

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

    function updateQuestion(questionIndex: number, patch: Partial<DraftQuestion>) {
        setQuestions((prev) => prev.map((q, i) => (i === questionIndex ? { ...q, ...patch } : q)));
    }

    function updateOption(questionIndex: number, optionIndex: number, text: string) {
        setQuestions((prev) =>
            prev.map((q, i) => {
                if (i !== questionIndex) return q;

                const updatedOptions = q.options.map((opt, oi) =>
                    oi === optionIndex ? { ...opt, text } : opt
                );

                return { ...q, options: updatedOptions };
            })
        );
    }

    function addQuestion() {
        setQuestions((prev) => [...prev, createEmptyQuestion()]);
    }

    function removeQuestion(questionIndex: number) {
        setQuestions((prev) => {
            if (prev.length <= 1) return prev;
            return prev.filter((_, i) => i !== questionIndex);
        });
    }

    function addOption(questionIndex: number) {
        setQuestions((prev) =>
            prev.map((q, i) =>
                i === questionIndex ? { ...q, options: [...q.options, { text: "" }] } : q
            )
        );
    }

    function removeOption(questionIndex: number, optionIndex: number) {
        setQuestions((prev) =>
            prev.map((q, i) => {
                if (i !== questionIndex || q.options.length <= 2) return q;

                const nextOptions = q.options.filter((_, oi) => oi !== optionIndex);
                let nextCorrect = q.correctOptionIndex;

                if (optionIndex === q.correctOptionIndex) nextCorrect = 0;
                if (optionIndex < q.correctOptionIndex) nextCorrect -= 1;

                return {
                    ...q,
                    options: nextOptions,
                    correctOptionIndex: nextCorrect,
                };
            })
        );
    }

    function validateForm(): string | null {
        if (!title.trim()) return "A kvíz cím kötelező.";
        if (questions.length === 0) return "Legalább egy kérdés kell.";

        for (let qi = 0; qi < questions.length; qi++) {
            const q = questions[qi];
            if (!q.text.trim()) return `${qi + 1}. kérdés szövege kötelező.`;
            if (!Number.isInteger(q.timeLimitSeconds) || q.timeLimitSeconds < 5 || q.timeLimitSeconds > 600) {
                return `${qi + 1}. kérdés időkorlátja 5 és 600 másodperc között legyen.`;
            }
            if (q.options.length < 2) return `${qi + 1}. kérdéshez legalább 2 opció kell.`;
            if (q.correctOptionIndex < 0 || q.correctOptionIndex >= q.options.length) {
                return `${qi + 1}. kérdésnél válassz helyes opciót.`;
            }

            for (let oi = 0; oi < q.options.length; oi++) {
                if (!q.options[oi].text.trim()) {
                    return `${qi + 1}. kérdés ${oi + 1}. opció szövege kötelező.`;
                }
            }
        }

        return null;
    }

    async function createQuiz(e: SubmitEvent<HTMLFormElement>) {
        e.preventDefault();

        const trimmedTitle = title.trim();
        const validationError = validateForm();
        if (validationError) {
            setError(validationError);
            return;
        }

        setCreateBusy(true);
        setError("");

        try {
            await adminApi.createQuiz({
                title: trimmedTitle,
                description: description.trim() || undefined,
                questions: questions.map((q) => ({
                    text: q.text.trim(),
                    timeLimitSeconds: q.timeLimitSeconds,
                    options: q.options.map((opt, index) => ({
                        text: opt.text.trim(),
                        isCorrect: index === q.correctOptionIndex,
                    })),
                })),
            });

            setTitle("");
            setDescription("");
            setQuestions([createEmptyQuestion()]);
            await load();
        } catch (e) {
            setError(toErrorMessage(e));
        } finally {
            setCreateBusy(false);
        }
    }

    return (
        <div>
            <h2>My Quizzes</h2>
            <button onClick={load}>Refresh</button>

            <form onSubmit={createQuiz} style={{ margin: "12px 0", display: "grid", gap: 8, maxWidth: 480 }}>
                <input
                    placeholder="Quiz title"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                />
                <textarea
                    placeholder="Description (optional)"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                />

                <h3>Questions</h3>
                {questions.map((q, qi) => (
                    <div key={qi} style={{ border: "1px solid #ddd", padding: 10, borderRadius: 4 }}>
                        <div style={{ display: "grid", gap: 6 }}>
                            <strong>{qi + 1}. kérdés</strong>
                            <input
                                placeholder="Question text"
                                value={q.text}
                                onChange={(e) => updateQuestion(qi, { text: e.target.value })}
                            />
                            <input
                                type="number"
                                min={5}
                                max={600}
                                value={q.timeLimitSeconds}
                                onChange={(e) =>
                                    updateQuestion(qi, {
                                        timeLimitSeconds: Number(e.target.value || 0),
                                    })
                                }
                            />

                            <strong>Opciók (jelöld a helyeset):</strong>
                            {q.options.map((opt, oi) => (
                                <div key={oi} style={{ display: "flex", gap: 8, alignItems: "center" }}>
                                    <input
                                        type="radio"
                                        name={`correct-${qi}`}
                                        checked={q.correctOptionIndex === oi}
                                        onChange={() => updateQuestion(qi, { correctOptionIndex: oi })}
                                    />
                                    <input
                                        placeholder={`Option ${oi + 1}`}
                                        value={opt.text}
                                        onChange={(e) => updateOption(qi, oi, e.target.value)}
                                        style={{ flex: 1 }}
                                    />
                                    <button
                                        type="button"
                                        onClick={() => removeOption(qi, oi)}
                                        disabled={q.options.length <= 2}
                                    >
                                        Remove option
                                    </button>
                                </div>
                            ))}

                            <div style={{ display: "flex", gap: 8 }}>
                                <button type="button" onClick={() => addOption(qi)}>
                                    Add option
                                </button>
                                <button
                                    type="button"
                                    onClick={() => removeQuestion(qi)}
                                    disabled={questions.length <= 1}
                                >
                                    Remove question
                                </button>
                            </div>
                        </div>
                    </div>
                ))}

                <button type="button" onClick={addQuestion}>
                    Add question
                </button>

                <button type="submit" disabled={createBusy}>
                    {createBusy ? "Creating..." : "Create Quiz"}
                </button>
            </form>

            {error && <p style={{ color: "crimson" }}>{error}</p>}
            <ul>
                {items.map((q) => (
                    <li key={q.id}>
                        <Link to={`/admin/quizzes/${q.id}`}>
                            #{q.id} - {q.title}
                        </Link>
                    </li>
                ))}
            </ul>
        </div>
    );
}
