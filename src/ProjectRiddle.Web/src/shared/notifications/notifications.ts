import { toast } from "sonner";

export const notifications = {
    success(message: string): void {
        toast.success(message);
    },

    error(message: string): void {
        toast.error(message);
    },
};
