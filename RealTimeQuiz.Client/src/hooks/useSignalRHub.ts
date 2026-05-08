import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { API_BASE_URL } from "../api/httpClient";
import { getAccessToken } from "../state/authStorage";
import type {
  JoinSessionResultDto,
  SessionLifecycleResultDto,
  SubmitAnswerResultDto,
} from "../types/signalr.types";

interface UseSignalRHubProps {
  sessionId?: number;
  onParticipantJoined?: (data: JoinSessionResultDto) => void;
  onQuestionStarted?: (data: SessionLifecycleResultDto) => void;
  onQuestionClosed?: () => void;
  onSessionFinished?: () => void;
  onAnswerSubmitted?: (data: SubmitAnswerResultDto) => void;
}

export function useSignalRHub({
  sessionId,
  onParticipantJoined,
  onQuestionStarted,
  onQuestionClosed,
  onSessionFinished,
  onAnswerSubmitted,
}: UseSignalRHubProps) {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  
  
  const callbacksRef = useRef({
    onParticipantJoined,
    onQuestionStarted,
    onQuestionClosed,
    onSessionFinished,
    onAnswerSubmitted,
  });

  useEffect(() => {
    callbacksRef.current = {
      onParticipantJoined,
      onQuestionStarted,
      onQuestionClosed,
      onSessionFinished,
      onAnswerSubmitted,
    };
  });

  useEffect(() => {
    let isMounted = true;

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/session-hub`, {
        accessTokenFactory: () => getAccessToken() || "",
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);

    newConnection.on("ParticipantJoined", (data: JoinSessionResultDto) => {
      callbacksRef.current.onParticipantJoined?.(data);
    });

    newConnection.on("QuestionStarted", (data: SessionLifecycleResultDto) => {
      callbacksRef.current.onQuestionStarted?.(data);
    });

    newConnection.on("QuestionClosed", () => {
      callbacksRef.current.onQuestionClosed?.();
    });

    newConnection.on("SessionFinished", () => {
      callbacksRef.current.onSessionFinished?.();
    });

    newConnection.on("AnswerSubmitted", (data: SubmitAnswerResultDto) => {
      callbacksRef.current.onAnswerSubmitted?.(data);
    });

    async function startConnection() {
      try {
        await newConnection.start();
        if (!isMounted) return;
        setIsConnected(true);

        if (sessionId) {
          await newConnection.invoke("JoinSessionGroup", sessionId);
        }
      } catch (e) {
        console.error("SignalR Connection Error: ", e);
      }
    }

    startConnection();

    return () => {
      isMounted = false;
      if (sessionId && newConnection.state === signalR.HubConnectionState.Connected) {
        newConnection.invoke("LeaveSessionGroup", sessionId).catch(console.error);
      }
      newConnection.stop();
    };
  }, [sessionId]);

  return { connection, isConnected };
}
