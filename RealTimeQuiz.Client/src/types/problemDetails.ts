export type ProblemDetails = {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    traceId?: string;
    errors?: Record<string, string[]>;
};

export function toErrorMessage(err: unknown): string {
    if (!err || typeof err !== "object") return "Unknown error";

    const p = err as ProblemDetails;
    if (p.errors) {
        return Object.entries(p.errors)
            .map(([k, v]) => `${k}: ${v.join(", ")}`)
            .join(" | ");
    }

    return p.detail || p.title || "Unexpected error";
}
