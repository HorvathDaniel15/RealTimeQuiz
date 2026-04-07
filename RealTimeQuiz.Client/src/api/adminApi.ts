import { apiFetch } from "./httpClient";

export type QuizListItem = {
    id: number;
    title: string;
    description?: string;
    isPublished: boolean;
    createdUtc: string;
};

export type CreateQuizRequest = {
    title: string;
    description?: string;
    questions: {
        text: string;
        timeLimitSeconds: number;
        options: { text: string; isCorrect: boolean }[];
    }[];
};

export const adminApi = {
    getQuizzes: () => apiFetch<QuizListItem[]>("/api/admin/quizzes", undefined, true),

    createQuiz: (body: CreateQuizRequest) =>
        apiFetch<{ id: number }>(
            "/api/admin/quizzes",
            { method: "POST", body: JSON.stringify(body) },
            true
        ),
};
