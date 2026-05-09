import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { participantApi } from "../api/participantApi";
import ProblemAlert from "../components/ProblemAlert";
import { toErrorMessage, type ProblemDetails } from "../types/problemDetails";
import {
    clearParticipantContext,
    getParticipantContext,
    submissionStatusLabel,
    type ParticipantCurrentQuestion,
    type SubmitAnswerResult,
} from "../types/participant";
import { useSignalRHub } from "../hooks/useSignalRHub";
import "./participant.css";

function getProblemStatus(err: unknown): number | null {
    if (!err || typeof err !== "object") return null;
    const problem = err as ProblemDetails;
    return typeof problem.status === "number" ? problem.status : null;
}

function ParticipantSessionPage() {
    const { participantId } = useParams<{ participantId: string }>();
    const navigate = useNavigate();

    const numericParticipantId = Number(participantId);
    const storedContext = useMemo(() => getParticipantContext(), []);

    const [currentQuestion, setCurrentQuestion] = useState<ParticipantCurrentQuestion | null>(null);
    const [selectedOptionId, setSelectedOptionId] = useState<number | null>(null);
    const [submitResult, setSubmitResult] = useState<SubmitAnswerResult | null>(null);
    const [initialLoading, setInitialLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [info, setInfo] = useState<string | null>(null);
    const [timeLeft, setTimeLeft] = useState<number | null>(null);

    const submittedQuestionId = submitResult?.questionId ?? null;

    const { isConnected } = useSignalRHub({
        sessionId: storedContext?.sessionId,
        onQuestionStarted: () => {
            loadCurrentQuestion(true);
        },
        onQuestionClosed: () => {
            if (submittedQuestionId) {
                loadResult(submittedQuestionId, true);
            }
            else if (currentQuestion) {
                setSubmitResult({
                    sessionId: currentQuestion.sessionId,
                    participantId: numericParticipantId,
                    questionId: currentQuestion.questionId,
                    optionId: 0,
                    status: 1,
                    isCorrect: false,
                    awardedPoints: 0,
                    submittedAtUtc: new Date().toISOString()
                })
            }
        },
        onSessionFinished: () => {
            setInfo("A játék véget ért.");
            setCurrentQuestion(null);
            setSubmitResult(null);
        },
    });

    const loadCurrentQuestion = useCallback(
        async (silent = false) => {
            if (!numericParticipantId) return;

            try {
                const data = await participantApi.getCurrentQuestion(numericParticipantId);
                setCurrentQuestion(data);
                setSelectedOptionId(null);
                setSubmitResult(null);
                setInfo(null);
                if (!silent) setError(null);
            } catch (e: unknown) {
                const status = getProblemStatus(e);

                if (status === 400 || status === 403) {
                    setCurrentQuestion(null);
                    setInfo("Nincs aktiv kerdes. Varakozas az admin kovetkezo lepesere...");
                    if (!silent) setError(null);
                    return;
                }

                if (!silent) {
                    setError(toErrorMessage(e));
                }
            } finally {
                setInitialLoading(false);
            }
        },
        [numericParticipantId]
    );

    const loadResult = useCallback(
        async (questionId: number, silent = false) => {
            if (!numericParticipantId) return;

            try {
                const result = await participantApi.getAnswerResult(numericParticipantId, questionId);
                setSubmitResult(result);
                if (result.isCorrect === null) {
                    setInfo("A kerdes meg nincs lezarva. Eredmenyre varakozas...");
                } else {
                    setInfo(null);
                }
                if (!silent) setError(null);
            } catch (e: unknown) {
                if (!silent) {
                    setError(toErrorMessage(e));
                }
            }
        },
        [numericParticipantId]
    );

    useEffect(() => {
        loadCurrentQuestion();
    }, [loadCurrentQuestion]);

    useEffect(() => {
        if (!currentQuestion || !currentQuestion.timeLimitSeconds) {
            setTimeLeft(null);
            return;
        }

        const limit = currentQuestion.timeLimitSeconds;
        const openedTimeStr = currentQuestion.openedAtUtc.endsWith("Z") ? currentQuestion.openedAtUtc : currentQuestion.openedAtUtc + "Z";
        const openedAt = new Date(openedTimeStr).getTime();
        const expiresAt = openedAt + limit * 1000;

        const updateTimer = () => {
            const now = new Date().getTime();
            const remaining = Math.max(0, Math.floor((expiresAt - now) / 1000));
            if (remaining === 0) {
                setSubmitResult({
                   sessionId: currentQuestion.sessionId,
                    participantId: numericParticipantId,
                    questionId: currentQuestion.questionId,
                    optionId: 0,
                    status: 1,
                    isCorrect: false,
                    awardedPoints: 0,
                    submittedAtUtc: new Date().toISOString()
                });
                setInfo("Time's up! Waiting for the admin's next move...");
                clearInterval(intervalId);
            }
            setTimeLeft(remaining);
        };

        updateTimer();
        const intervalId = setInterval(updateTimer, 1000);

        return () => clearInterval(intervalId);
    }, [currentQuestion]);

    async function submitAnswer() {
        if (!currentQuestion || !selectedOptionId) {
            setError("Valassz egy opciot bekuldes elott.");
            return;
        }

        setBusy(true);
        setError(null);

        try {
            const result = await participantApi.submitAnswer({
                participantId: numericParticipantId,
                questionId: currentQuestion.questionId,
                optionId: selectedOptionId,
            });
            setSubmitResult(result);

            if (result.isCorrect === null) {
                setInfo("Valasz elmentve. Eredmeny akkor lesz lathato, ha az admin lezarja a kerdest.");
            } else {
                setInfo(null);
            }
        } catch (e: unknown) {
            setError(toErrorMessage(e));
        } finally {
            setBusy(false);
        }
    }


    if (!numericParticipantId) {
        return <div className="participant-page">Hibas participant azonosito az URL-ben.</div>;
    }

    if (initialLoading) {
        return <div className="participant-page">Betoltes...</div>;
    }

    return (
        <div className="participant-page">
            <div className="participant-shell">
                <section className="participant-card">
                    <h1 className="participant-title">Participant Session</h1>
                    <div className="participant-meta">
                        <span><strong>Participant ID:</strong> {numericParticipantId}</span>
                        <span><strong>SignalR:</strong> <span style={{ color: isConnected ? "green" : "red" }}>{isConnected ? "Connected" : "Disconnected"}</span></span>
                        {storedContext && (
                            <>
                                <span><strong>Display name:</strong> {storedContext.displayName}</span>
                                <span><strong>PIN:</strong> {storedContext.joinPin}</span>
                            </>
                        )}
                    </div>
                </section>

                {currentQuestion && !submitResult && (
                    <section className="participant-card">
                        <h2 className="participant-question-title">
                            {currentQuestion.orderIndex + 1}. {currentQuestion.text}
                        </h2>
                        <p style={{ color: timeLeft !== null && timeLeft <= 5 ? "red" : "inherit", fontWeight: timeLeft !== null && timeLeft <= 5 ? "bold" : "normal" }}>
                            <strong>Hátralévő idő:</strong> {timeLeft !== null ? `${timeLeft} másodperc` : (currentQuestion.timeLimitSeconds ? `${currentQuestion.timeLimitSeconds} sec` : "-")}
                        </p>

                        <div className="participant-options">
                            {currentQuestion.options.map((option) => (
                                <label key={option.id} className="participant-option">
                                    <input
                                        type="radio"
                                        checked={selectedOptionId === option.id}
                                        onChange={() => setSelectedOptionId(option.id)}
                                        disabled={busy || timeLeft === 0}
                                    />
                                    <span>{option.text}</span>
                                </label>
                            ))}
                        </div>

                        <div className="participant-actions">
                            <button className="participant-button" onClick={submitAnswer} disabled={busy || timeLeft === 0}>
                                {timeLeft === 0 ? "Az idő lejárt!" : (busy ? "Sending..." : "Submit answer")}
                            </button>
                        </div>
                    </section>
                )}

                {submitResult && (
                    <section className="participant-card">
                        <h2>Answer status</h2>
                        <div className="participant-status-grid">
                            <p><strong>Status:</strong> {submissionStatusLabel(submitResult.status)}</p>
                            <p>
                                <strong>Is correct:</strong>{" "}
                                {submitResult.isCorrect === null
                                    ? "N/A (waiting)"
                                    : submitResult.isCorrect
                                      ? "Yes"
                                      : "No"}
                            </p>
                            <p><strong>Awarded points:</strong> {submitResult.awardedPoints}</p>
                        </div>
                    </section>
                )}

                {!currentQuestion && !submitResult && <section className="participant-card">Nincs aktiv kerdes.</section>}

                {info && <section className="participant-card">{info}</section>}
                <ProblemAlert message={error} />

                <section className="participant-actions">
                    <button
                        className="participant-button participant-button-secondary"
                        onClick={() => {
                            clearParticipantContext();
                            navigate("/participant/join");
                        }}
                    >
                        Leave session
                    </button>
                    <Link className="participant-button participant-button-secondary" to="/participant/join">
                        Back to join
                    </Link>
                </section>
            </div>
        </div>
    );
}

export default ParticipantSessionPage;
