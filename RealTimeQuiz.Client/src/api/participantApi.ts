import { apiFetch } from "./httpClient";
import type {
    JoinSessionRequest,
    JoinSessionResult,
    ParticipantCurrentQuestion,
    SubmitAnswerRequest,
    SubmitAnswerResult,
} from "../types/participant";

export const participantApi = {
    joinSession: (body: JoinSessionRequest) =>
        apiFetch<JoinSessionResult>("/api/participant-sessions/join", {
            method: "POST",
            body: JSON.stringify(body),
        }),

    getCurrentQuestion: (participantId: number) =>
        apiFetch<ParticipantCurrentQuestion>(
            `/api/participant-sessions/${participantId}/current-question`
        ),

    submitAnswer: (body: SubmitAnswerRequest) =>
        apiFetch<SubmitAnswerResult>("/api/participant-sessions/submit-answer", {
            method: "POST",
            body: JSON.stringify(body),
        }),

    getAnswerResult: (participantId: number, questionId: number) =>
        apiFetch<SubmitAnswerResult>(
            `/api/participant-sessions/${participantId}/results/${questionId}`
        ),
};

