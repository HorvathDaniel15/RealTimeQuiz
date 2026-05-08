export type SessionState = 0 | 1 | 2 | 3 | 4 | 5;

export type SubmissionStatus = 0 | 1 | 2;

export interface SessionLifecycleResultDto {
  id: number;
  quizId: number;
  joinPin: string;
  state: SessionState;
  currentQuestionId: number | null;
  startedAtUtc: string | null;
  questionOpenedAtUtc: string | null;
  questionClosedAtUtc: string | null;
  finishedAtUtc: string | null;
}

export interface JoinSessionResultDto {
  participantId: number;
  sessionId: number;
  joinPin: string;
  displayName: string;
  sessionState: SessionState;
  joinedAtUtc: string;
}

export interface SubmitAnswerResultDto {
  sessionId: number;
  participantId: number;
  questionId: number;
  optionId: number;
  status: SubmissionStatus;
  isCorrect: boolean | null;
  awardedPoints: number;
  submittedAtUtc: string;
}
