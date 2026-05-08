import { clearAuthSession, getAccessToken } from "../state/authStorage";

export const API_BASE_URL = "https://localhost:7154";

export async function apiFetch<T>(
    path: string,
    init?: RequestInit,
    requireAuth = false
): Promise<T> {
    const headers = new Headers(init?.headers ?? {});

    if (init?.body && !headers.has("Content-Type")) {
        headers.set("Content-Type", "application/json");
    }

    if (requireAuth) {
        const accessToken = getAccessToken();
        if (!accessToken) {
            throw { title: "Unauthorized", status: 401, detail: "Please sign in first." };
        }

        headers.set("Authorization", `Bearer ${accessToken}`);
    }

    const res = await fetch(`${API_BASE_URL}${path}`, {
        ...init,
        headers,
    });

    if (!res.ok) {
        const body = await res.json().catch(() => null);

        if (res.status === 401 && requireAuth) {
            clearAuthSession();
        }

        throw body ?? { title: "Request failed", status: res.status };
    }

    if (res.status === 204) return undefined as T;
    return (await res.json()) as T;
}
