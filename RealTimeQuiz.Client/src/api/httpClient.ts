const API_BASE_URL = "https://localhost:7154";

export async function apiFetch<T>(
    path: string,
    init?: RequestInit,
    requireOwner = false
): Promise<T> {
    const headers = new Headers(init?.headers ?? {});
    headers.set("Content-Type", "application/json");

    if (requireOwner) {
        const ownerId = localStorage.getItem("ownerId");
        if (!ownerId) {
            throw new Error("Owner ID is missing. Please set it first.");
        }
        headers.set("X-Owner-Id", ownerId);
    }

    const res = await fetch(`${API_BASE_URL}${path}`, {
        ...init,
        headers,
    });

    if (!res.ok) {
        const body = await res.json().catch(() => null);
        throw body ?? { title: "Request failed", status: res.status };
    }

    // 204 eseten ne parse-oljon
    if (res.status === 204) return undefined as T;
    return (await res.json()) as T;
}
