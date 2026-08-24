import type { AnonymousCourseProgress } from "../../features/courses/storage/anonymousCourseProgress";
import type { AnonymousRiddleProgress } from "../../features/riddles/models/riddleProgress";

const progressStorageKey = "project-riddle.progress";

export const currentProgressSchemaVersion = 1 as const;

/**
 * The adapter stores only typed progress data; credentials, session material, answers, and explanations must never
 * be stored in the document. Each capability validates the shape of its own segment before using it.
 */
export interface ProgressDocument {
    readonly schemaVersion: typeof currentProgressSchemaVersion;
    readonly riddle?: AnonymousRiddleProgress;
    readonly courses?: AnonymousCourseProgress;
}

function emptyProgressDocument(): ProgressDocument {
    return { schemaVersion: currentProgressSchemaVersion };
}

function isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null;
}

function isCurrentProgressDocument(value: unknown): value is ProgressDocument {
    return isRecord(value) && value.schemaVersion === currentProgressSchemaVersion;
}

function getStorage(): Storage | undefined {
    try {
        return globalThis.localStorage;
    } catch {
        return undefined;
    }
}

export const progressStorage = {
    read(): ProgressDocument {
        const storage = getStorage();

        if (storage === undefined) {
            return emptyProgressDocument();
        }

        try {
            const rawValue = storage.getItem(progressStorageKey);

            if (rawValue === null) {
                return emptyProgressDocument();
            }

            const parsedValue: unknown = JSON.parse(rawValue);
            return isCurrentProgressDocument(parsedValue) ? parsedValue : emptyProgressDocument();
        } catch {
            return emptyProgressDocument();
        }
    },

    write(document: ProgressDocument): void {
        const storage = getStorage();

        if (storage === undefined) {
            return;
        }

        try {
            storage.setItem(progressStorageKey, JSON.stringify(document));
        } catch {
            // Browser storage is optional; an unavailable or full store must not break the application.
        }
    },

    reset(): void {
        const storage = getStorage();

        if (storage === undefined) {
            return;
        }

        try {
            storage.removeItem(progressStorageKey);
        } catch {
            // Browser storage is optional; a failed reset leaves the in-memory empty document authoritative.
        }
    },
};
