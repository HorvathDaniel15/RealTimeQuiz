import type { LoginResult, RefreshResult } from "../types/auth";

const ACCESS_TOKEN_KEY = "accessToken";
const REFRESH_TOKEN_KEY = "refreshToken";
const ACCESS_EXPIRES_AT_KEY = "accessTokenExpiresAtUtc";

export function saveAuthSession(payload: LoginResult | RefreshResult): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, payload.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, payload.refreshToken);
    localStorage.setItem(ACCESS_EXPIRES_AT_KEY, payload.accessTokenExpiresAtUtc);
}

export function clearAuthSession(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(ACCESS_EXPIRES_AT_KEY);
}

export function getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function hasAccessToken(): boolean {
    return Boolean(getAccessToken());
}

