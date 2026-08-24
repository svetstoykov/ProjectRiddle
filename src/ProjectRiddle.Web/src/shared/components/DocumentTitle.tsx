import { useEffect } from "react";

export interface DocumentTitleProps {
    readonly title: string;
}

export function DocumentTitle({ title }: DocumentTitleProps): null {
    useEffect(() => {
        document.title = `${title} · Криптики`;
    }, [title]);

    return null;
}
