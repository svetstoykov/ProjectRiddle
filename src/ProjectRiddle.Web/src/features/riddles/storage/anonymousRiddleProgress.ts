import { currentProgressSchemaVersion, progressStorage } from "../../../shared/storage/progressStorage";
import { isLocalDate } from "../models/localDate";
import type {
    AnonymousRiddleProgress,
    AnonymousRiddleProgressRequest,
    RiddleProgressSnapshot,
    RiddleProgressStatus,
} from "../models/riddleProgress";
import type { RiddleRangeKind } from "../models/riddleRange";

const statuses: readonly string[] = ["inProgress", "fullyRevealed", "solved"];
const rangeKinds: readonly string[] = ["definition", "indicator", "fodder"];

function isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null;
}

function isStatus(value: unknown): value is RiddleProgressStatus {
    return typeof value === "string" && statuses.includes(value);
}

function isRangeKindList(value: unknown): value is readonly RiddleRangeKind[] {
    return Array.isArray(value) && value.every((kind) => typeof kind === "string" && rangeKinds.includes(kind));
}

function isPositionList(value: unknown): value is readonly number[] {
    return Array.isArray(value) && value.every((position) => Number.isInteger(position) && position >= 0);
}

function isAnonymousRiddleProgress(value: unknown): value is AnonymousRiddleProgress {
    return (
        isRecord(value) &&
        typeof value.riddleId === "string" &&
        value.riddleId.length > 0 &&
        typeof value.publicationDate === "string" &&
        isLocalDate(value.publicationDate) &&
        isStatus(value.status) &&
        typeof value.answerAttemptCount === "number" &&
        Number.isInteger(value.answerAttemptCount) &&
        value.answerAttemptCount >= 0 &&
        isRangeKindList(value.usedHints) &&
        isPositionList(value.revealedPositions)
    );
}

export function readAnonymousRiddleProgress(): AnonymousRiddleProgress | undefined {
    const segment: unknown = progressStorage.read().riddle;
    return isAnonymousRiddleProgress(segment) ? segment : undefined;
}

/**
 * Returns the stored record only when it describes <code>riddleId</code>, so a stale record can never be posted
 * against a different riddle.
 */
export function anonymousProgressRequestForRiddle(riddleId: string): AnonymousRiddleProgressRequest | undefined {
    const stored = readAnonymousRiddleProgress();

    if (stored === undefined || stored.riddleId !== riddleId) {
        return undefined;
    }

    return { schemaVersion: currentProgressSchemaVersion, ...stored };
}

export function writeAnonymousRiddleProgress(snapshot: RiddleProgressSnapshot): void {
    progressStorage.write({
        ...progressStorage.read(),
        riddle: {
            riddleId: snapshot.riddleId,
            publicationDate: snapshot.publicationDate,
            status: snapshot.status,
            answerAttemptCount: snapshot.answerAttemptCount,
            usedHints: snapshot.usedHints,
            revealedPositions: snapshot.revealedPositions,
        },
    });
}

export function clearAnonymousRiddleProgress(): void {
    progressStorage.write({ ...progressStorage.read(), riddle: undefined });
}

/**
 * Discards the stored record when play starts on a riddle it does not describe. A record for a previous day survives
 * the rollover itself; it is discarded only when the visitor begins a different riddle.
 */
export function evictAnonymousProgressForOtherRiddle(riddleId: string): void {
    const stored = readAnonymousRiddleProgress();

    if (stored === undefined || stored.riddleId === riddleId) {
        return;
    }

    clearAnonymousRiddleProgress();
}
