export type JoinSessionRequest = {
    joinPin: string;
    displayName: string;
};

export type JoinSessionResult = {
    participantId: number;
    sessionId: number;
    joinPin: string;
    displayName: string;
    sessionState: number;
    joinedAtUtc: string;
};

export type ParticipantQuestionOption = {
    id: number;
    text: string;
    orderIndex: number;
};

export type ParticipantCurrentQuestion = {
    sessionId: number;
    questionId: number;
    orderIndex: number;
    text: string;
    imageUrl: string | null;
    timeLimitSeconds: number | null;
    openedAtUtc: string;
    options: ParticipantQuestionOption[];
};

export type SubmitAnswerRequest = {
    participantId: number;
    questionId: number;
    optionId: number;
};

export type SubmitAnswerResult = {
    sessionId: number;
    participantId: number;
    questionId: number;
    optionId: number;
    status: number;
    isCorrect: boolean | null;
    awardedPoints: number;
    submittedAtUtc: string;
};

export type ParticipantSessionContext = {
    participantId: number;
    sessionId: number;
    joinPin: string;
    displayName: string;
};

const PARTICIPANT_CONTEXT_KEY = "participantContext";

export function saveParticipantContext(context: ParticipantSessionContext): void {
    localStorage.setItem(PARTICIPANT_CONTEXT_KEY, JSON.stringify(context));
}

export function getParticipantContext(): ParticipantSessionContext | null {
    const raw = localStorage.getItem(PARTICIPANT_CONTEXT_KEY);
    if (!raw) return null;

    try {
        return JSON.parse(raw) as ParticipantSessionContext;
    } catch {
        return null;
    }
}

export function clearParticipantContext(): void {
    localStorage.removeItem(PARTICIPANT_CONTEXT_KEY);
}

export function submissionStatusLabel(status: number): string {
    switch (status) {
        case 0:
            return "Accepted";
        case 1:
            return "Late";
        case 2:
            return "Rejected";
        default:
            return `Unknown(${status})`;
    }
}

