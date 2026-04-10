export type ProblemDetails = {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    traceId?: string;
    errors?: Record<string, string[]>;
};

function flattenValidationErrors(errors?: Record<string, string[]>): string[] {
    if (!errors) return [];

    return Object.entries(errors).flatMap(([field, messages]) =>
        messages.map((message) => `${field}: ${message}`)
    );
}

export function toErrorMessage(err: unknown): string {
    if (!err) return "Unknown error";
    if (err instanceof Error) return err.message || "Unexpected error";
    if (typeof err !== "object") return String(err);

    const p = err as ProblemDetails;
    const validationLines = flattenValidationErrors(p.errors);

    const header = [
        p.status ? `[${p.status}]` : null,
        p.title,
        p.detail,
    ]
        .filter((part): part is string => Boolean(part && part.trim()))
        .join(" ");

    if (header && validationLines.length > 0) {
        return `${header} | ${validationLines.join(" | ")}`;
    }

    if (header) return header;
    if (validationLines.length > 0) return validationLines.join(" | ");

    return "Unexpected error";
}
