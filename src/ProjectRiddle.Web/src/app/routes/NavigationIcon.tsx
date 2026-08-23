import type { ReactElement } from "react";

export type NavigationIconName = "home" | "archive" | "administration" | "register" | "signIn" | "signOut" | "busy";

/*
 * Drawn glyphs rather than font symbols. The navigation bar carries no labels, so each shape has to sit exactly in
 * the middle of its round control at every size, which a text glyph never does: it lands wherever its font's side
 * bearings put it. Every path is stroked with the control's own colour, so one icon serves the resting, hovered,
 * and current states.
 */
const iconPaths: Record<NavigationIconName, readonly string[]> = {
    home: ["M3.5 9.8 12 3.2l8.5 6.6", "M5.8 8.2V20.3h12.4V8.2", "M9.8 20.3v-5.2h4.4v5.2"],
    archive: ["M4.6 7.4h14.8v12.4H4.6z", "M4.6 11.6h14.8", "M9 4.8v4", "M15 4.8v4"],
    administration: [
        "M4 8.4h9.2",
        "M17.6 8.4H20",
        "M4 15.6h3.2",
        "M11.6 15.6H20",
        "M15.4 6.2a2.2 2.2 0 1 1 0 4.4 2.2 2.2 0 0 1 0-4.4z",
        "M9.4 13.4a2.2 2.2 0 1 1 0 4.4 2.2 2.2 0 0 1 0-4.4z",
    ],
    register: [
        "M10 5.2a3.4 3.4 0 1 1 0 6.8 3.4 3.4 0 0 1 0-6.8z",
        "M3.6 20.2v-1.2a4.6 4.6 0 0 1 4.6-4.6h3.6a4.6 4.6 0 0 1 4.6 4.6v1.2",
        "M19.4 6.4v5",
        "M21.9 8.9h-5",
    ],
    signIn: [
        "M13.8 3.6h3.6A1.6 1.6 0 0 1 19 5.2v13.6a1.6 1.6 0 0 1-1.6 1.6h-3.6",
        "M9.4 8.4 13 12l-3.6 3.6",
        "M13 12H3.8",
    ],
    signOut: [
        "M10.2 20.4H6.6A1.6 1.6 0 0 1 5 18.8V5.2a1.6 1.6 0 0 1 1.6-1.6h3.6",
        "M15.2 8.4 18.8 12l-3.6 3.6",
        "M18.8 12H9.6",
    ],
    busy: ["M12 4.4A7.6 7.6 0 1 0 19.6 12"],
};

export interface NavigationIconProps {
    readonly name: NavigationIconName;
    readonly className?: string;
}

export function NavigationIcon({ name, className }: NavigationIconProps): ReactElement {
    return (
        <svg className={className} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
            {iconPaths[name].map((path) => (
                <path key={path} d={path} />
            ))}
        </svg>
    );
}
