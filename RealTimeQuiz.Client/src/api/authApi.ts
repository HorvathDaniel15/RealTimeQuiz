import { apiFetch } from "./httpClient";
import type {
    AuthMeResult,
    LoginRequest,
    LoginResult,
    LogoutRequest,
    RefreshRequest,
    RefreshResult,
    RegisterRequest,
    RegisterResult,
} from "../types/auth";

export const authApi = {
    register: (body: RegisterRequest) =>
        apiFetch<RegisterResult>("/api/auth/register", {
            method: "POST",
            body: JSON.stringify(body),
        }),

    login: (body: LoginRequest) =>
        apiFetch<LoginResult>("/api/auth/login", {
            method: "POST",
            body: JSON.stringify(body),
        }),

    refresh: (body: RefreshRequest) =>
        apiFetch<RefreshResult>("/api/auth/refresh", {
            method: "POST",
            body: JSON.stringify(body),
        }),

    logout: (body: LogoutRequest) =>
        apiFetch<void>(
            "/api/auth/logout",
            {
                method: "POST",
                body: JSON.stringify(body),
            },
            true
        ),

    me: () => apiFetch<AuthMeResult>("/api/auth/me", undefined, true),
};

