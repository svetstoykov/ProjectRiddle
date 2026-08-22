import { request } from "../../../shared/api/client";
import type { PublicRiddleList, PublicRiddlePlay, PublicRiddleWeek } from "../models/publicRiddle";
import type {
    AccountRiddleProgressList,
    AnonymousRiddleProgressRequest,
    RiddlePlayState,
} from "../models/riddleProgress";
import type { RiddleRangeKind } from "../models/riddleRange";

interface PlayCommandBody {
    readonly progress?: AnonymousRiddleProgressRequest;
}

interface AnswerCommandBody extends PlayCommandBody {
    readonly answer: string;
}

interface HintCommandBody extends PlayCommandBody {
    readonly kind: RiddleRangeKind;
}

export const riddlesApi = {
    getWeek(): Promise<PublicRiddleWeek> {
        return request<PublicRiddleWeek>({ method: "GET", url: "/api/riddles/week" });
    },

    getToday(): Promise<PublicRiddlePlay> {
        return request<PublicRiddlePlay>({ method: "GET", url: "/api/riddles/today" });
    },

    getPlay(riddleId: string): Promise<PublicRiddlePlay> {
        return request<PublicRiddlePlay>({ method: "GET", url: `/api/riddles/${riddleId}` });
    },

    listArchive(page: number, pageSize: number): Promise<PublicRiddleList> {
        return request<PublicRiddleList>({ method: "GET", url: "/api/riddles", params: { page, pageSize } });
    },

    resume(riddleId: string, progress: AnonymousRiddleProgressRequest | undefined): Promise<RiddlePlayState> {
        const body: PlayCommandBody = { progress };
        return request<RiddlePlayState>({ method: "POST", url: `/api/riddles/${riddleId}/resume`, data: body });
    },

    submitAnswer(
        riddleId: string,
        answer: string,
        progress: AnonymousRiddleProgressRequest | undefined,
    ): Promise<RiddlePlayState> {
        const body: AnswerCommandBody = { answer, progress };
        return request<RiddlePlayState>({ method: "POST", url: `/api/riddles/${riddleId}/answer`, data: body });
    },

    useHint(
        riddleId: string,
        kind: RiddleRangeKind,
        progress: AnonymousRiddleProgressRequest | undefined,
    ): Promise<RiddlePlayState> {
        const body: HintCommandBody = { kind, progress };
        return request<RiddlePlayState>({ method: "POST", url: `/api/riddles/${riddleId}/hint`, data: body });
    },

    revealLetter(riddleId: string, progress: AnonymousRiddleProgressRequest | undefined): Promise<RiddlePlayState> {
        const body: PlayCommandBody = { progress };
        return request<RiddlePlayState>({ method: "POST", url: `/api/riddles/${riddleId}/reveal`, data: body });
    },

    listProgress(fromDate: string, toDate: string): Promise<AccountRiddleProgressList> {
        return request<AccountRiddleProgressList>({
            method: "GET",
            url: "/api/riddles/progress",
            params: { fromDate, toDate },
        });
    },
};
