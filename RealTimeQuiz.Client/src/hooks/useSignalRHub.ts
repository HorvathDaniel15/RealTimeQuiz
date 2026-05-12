import { useCallback, useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { API_BASE_URL } from "../api/httpClient";
import { getAccessToken } from "../state/authStorage";
import type {
  JoinSessionResultDto,
  SessionLifecycleResultDto,
  SubmitAnswerResultDto,
} from "../types/signalr.types";
import { type LeaderboardEntry } from "../types/participant";

export type UseSignalRHubOptions = {
  sessionId?: number;
  onParticipantJoined?: (data: JoinSessionResultDto) => void;
  onQuestionStarted?: (data: SessionLifecycleResultDto) => void;
  onQuestionClosed?: () => void;
  onAnswerSubmitted?: (data: SubmitAnswerResultDto) => void;
  onSessionFinished?: () => void;
  onLeaderboardUpdated?: (leaderboard: LeaderboardEntry[]) => void;
};

export function useSignalRHub(options: UseSignalRHubOptions) {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  const optionsRef = useRef(options);

  useEffect(() => {
    optionsRef.current = options;
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
      optionsRef.current.onParticipantJoined?.(data);
    });

    newConnection.on("QuestionStarted", (data: SessionLifecycleResultDto) => {
      optionsRef.current.onQuestionStarted?.(data);
    });

    newConnection.on("QuestionClosed", () => {
      optionsRef.current.onQuestionClosed?.();
    });

    newConnection.on("SessionFinished", () => {
      optionsRef.current.onSessionFinished?.();
    });

    newConnection.on("AnswerSubmitted", (data: SubmitAnswerResultDto) => {
      optionsRef.current.onAnswerSubmitted?.(data);
    });

    newConnection.on("LeaderboardUpdated", (leaderboard: LeaderboardEntry[]) => {
      optionsRef.current.onLeaderboardUpdated?.(leaderboard);
    });

    const startConnection = async () => {
      try {
        await newConnection.start();
        if (!isMounted) return;
        setIsConnected(true);

        if (options.sessionId) {
          await newConnection.invoke("JoinSessionGroup", options.sessionId);
        }
      } catch (e) {
        console.error("SignalR Connection Error: ", e);
      }
    };

    startConnection();

    return () => {
      isMounted = false;
      if (options.sessionId && newConnection.state === signalR.HubConnectionState.Connected) {
        newConnection.invoke("LeaveSessionGroup", options.sessionId).catch(console.error);
      }
      newConnection.stop();
    };
  }, [options.sessionId]);

  return { connection, isConnected };
}
