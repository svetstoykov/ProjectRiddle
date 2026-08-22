import { request } from "../../../shared/api/client";
import type {
    PublishRiddleRequest,
    Riddle,
    RiddleContentRequest,
    RiddleListResponse,
    ScheduleRiddleRequest,
} from "../models/adminRiddle";

export const adminRiddlesApi = {
    async list(): Promise<Riddle[]> {
        const response = await request<RiddleListResponse>({
            method: "GET",
            url: "/api/admin/riddles",
        });
        return [...response.riddles];
    },

    get(id: string): Promise<Riddle> {
        return request<Riddle>({
            method: "GET",
            url: `/api/admin/riddles/${id}`,
        });
    },

    create(input: RiddleContentRequest): Promise<Riddle> {
        return request<Riddle>({
            method: "POST",
            url: "/api/admin/riddles",
            data: input,
        });
    },

    schedule(id: string, publicationDate: string): Promise<Riddle> {
        const body: ScheduleRiddleRequest = { publicationDate };
        return request<Riddle>({
            method: "POST",
            url: `/api/admin/riddles/${id}/schedule`,
            data: body,
        });
    },

    publish(id: string, publicationDate?: string): Promise<Riddle> {
        const body: PublishRiddleRequest | undefined = publicationDate === undefined ? undefined : { publicationDate };
        return request<Riddle>({
            method: "POST",
            url: `/api/admin/riddles/${id}/publish`,
            data: body,
        });
    },

    unpublish(id: string): Promise<Riddle> {
        return request<Riddle>({
            method: "POST",
            url: `/api/admin/riddles/${id}/unpublish`,
        });
    },

    delete(id: string): Promise<void> {
        return request<void>({
            method: "DELETE",
            url: `/api/admin/riddles/${id}`,
        });
    },
};
