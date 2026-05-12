import { apiFetch } from "./httpClient";

export type QuizListItem = {
    id: number;
    title: string;
    description?: string;
    isPublished: boolean;
    createdUtc: string;
};

export type QuizOptionDto = {
    id: number;
    text: string;
    orderIndex: number;
};

export type QuizQuestionDto = {
    id: number;
    text: string;
    orderIndex: number;
    timeLimitSeconds: number;
    options: QuizOptionDto[];
};

export type QuizDetailsDto = {
    id: number;
    ownerId: string;
    title: string;
    description?: string;
    isPublished: boolean;
    createdUtc: string;
    questions: QuizQuestionDto[];
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

export type CreateSessionRequest = {
    quizId: number;
};

export type SessionDto = {
    id: number;
    quizId: number;
    joinPin: string;
    state: number; // enum backendből jön intként
    currentQuestionId: number | null;
    startedAtUtc: string | null;
    questionOpenedAtUtc: string | null;
    questionClosedAtUtc: string | null;
    finishedAtUtc: string | null;
    currentQuestionOrderIndex: number | null;
    currentQuestionText: string | null;
    currentQuestionTimeLimitSeconds: number | null;
    currentQuestion?: {
        id: number;
        orderIndex: number;
        text: string;
        timeLimitSeconds: number;
        imageUrl: string | null;
    } | null;
};

export const adminApi = {
    getQuizzes: () => apiFetch<QuizListItem[]>("/api/admin/quizzes", undefined, true),

    getQuizById: (quizId: number) =>
        apiFetch<QuizDetailsDto>(`/api/admin/quizzes/${quizId}`, undefined, true),

    createQuiz: (body: CreateQuizRequest) =>
        apiFetch<{ id: number }>(
            "/api/admin/quizzes",
            { method: "POST", body: JSON.stringify(body) },
            true
        ),

    createSession: (body: CreateSessionRequest) =>
        apiFetch<SessionDto>(
            "/api/admin/sessions",
            { method: "POST", body: JSON.stringify(body) },
            true
        ),

    getSessionById: (sessionId: number) =>
        apiFetch<SessionDto>(`/api/admin/sessions/${sessionId}`, undefined, true),

    openLobby: (sessionId: number) =>
        apiFetch<SessionDto>(
            `/api/admin/sessions/${sessionId}/open-lobby`,
            { method: "POST" },
            true
        ),

    startSession: (sessionId: number) =>
        apiFetch<SessionDto>(
            `/api/admin/sessions/${sessionId}/start`,
            { method: "POST" },
            true
        ),

    closeCurrentQuestion: (sessionId: number) =>
        apiFetch<SessionDto>(
            `/api/admin/sessions/${sessionId}/close-current-question`,
            { method: "POST" },
            true
        ),

    advance: (sessionId: number) =>
        apiFetch<SessionDto>(
            `/api/admin/sessions/${sessionId}/advance`,
            { method: "POST" },
            true
        ),

    finish: (sessionId: number) =>
        apiFetch<SessionDto>(
            `/api/admin/sessions/${sessionId}/finish`,
            { method: "POST" },
            true
        ),
};
