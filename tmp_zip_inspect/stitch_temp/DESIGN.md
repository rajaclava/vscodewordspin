# Design System Document: Casual Adventure Mobile UI

## 1. Overview & Creative North Star
**Creative North Star: "The Ethereal Journey"**
This design system moves away from the rigid, flat grids typical of mobile apps to create a world that feels immersive, tactile, and alive. By blending **vibrant playfulness** with **sophisticated glassmorphism**, we achieve a "High-End Editorial" game experience. 

The aesthetic is driven by intentional asymmetry—mimicking the organic path of a map—and high-contrast typography scales that command attention. We reject "standard" UI by layering surfaces like sheets of colored crystal, using depth and soft luminosity rather than harsh borders to define our interactive world.

---

## 2. Colors & Surface Philosophy
Our palette balances deep, oceanic blues with energetic, sun-kissed oranges and ethereal purples. 

### The "No-Line" Rule
**Strict Mandate:** Prohibit the use of 1px solid borders for sectioning or containment. Boundaries are defined exclusively through background color shifts. A `surface-container-low` panel sitting on a `surface` background creates all the definition required. 

### Surface Hierarchy & Nesting
Treat the UI as a physical stack of semi-transparent materials.
*   **Base:** `surface` (#e8faf9) serves as the world floor.
*   **Layer 1:** `surface-container-low` (#e1f5f4) for large underlying sections (e.g., the map background).
*   **Layer 2 (The Float):** `surface-container-highest` (#c8e2e1) or `secondary-container` (#d5cbff) for active level cards.

### The "Glass & Gradient" Rule
To achieve a premium feel, floating panels (like the "Level Selection" modal) must use **Glassmorphism**. Apply a semi-transparent `surface-container-lowest` (#ffffff at 60-80% opacity) with a background blur (16px–32px). 

**Signature Texture:** Use a linear gradient for primary actions, transitioning from `primary` (#8c4a00) to `primary-container` (#fd8b00). This adds "soul" and a tactile glow that flat colors lack.

---

## 3. Typography: The Editorial Voice
We use a high-contrast pairing to balance game-world charm with extreme readability.

*   **Display & Headlines (Plus Jakarta Sans):** Our "Voice." Used for level titles (`display-md`) and major section headers (`headline-lg`). The bold weight and generous tracking make names like "Macera Merkezi" feel authoritative yet inviting.
*   **Titles & Body (Be Vietnam Pro):** Our "Guide." Used for navigation, descriptions, and buttons. It provides a clean, modern contrast to the expressive headlines.

**Hierarchy Tip:** Use `on-secondary-container` (#4824be) for text on purple surfaces to maintain sophisticated tonal harmony rather than defaulting to pure black or white.

---

## 4. Elevation & Depth
In this system, depth is a mechanic, not an afterthought.

*   **Tonal Layering:** Achieve "lift" by stacking. Place a `surface-container-lowest` card on a `surface-container-low` section. The subtle shift in hex code creates a soft, natural edge.
*   **Ambient Shadows:** For elements that truly "float" (like the level badges), use a shadow with a blur radius of 24pt–40pt at 6% opacity. The shadow color must be a tinted version of `on-surface` (#223131), never pure grey.
*   **The "Ghost Border" Fallback:** If a UI element (like an inactive input) risks disappearing, use the `outline-variant` (#9fb1b0) at **15% opacity**. This creates a suggestion of a container without breaking the organic flow.
*   **Soft Corners:** Follow the Roundedness Scale religiously.
    *   **Level Cards:** `lg` (2rem) for a friendly, approachable feel.
    *   **Floating Modals:** `xl` (3rem) to emphasize the "bubbled" glass effect.

---

## 5. Components

### Primary Buttons (Action CTAs)
*   **Style:** Gradient from `primary` to `primary-container`. 
*   **Rounding:** `full` (9999px) for a "pill" shape that invites tapping.
*   **Shadow:** 8% opacity shadow tinted with `primary_dim`.
*   **Text:** `title-md` in `on_primary` (#fff0e7).

### Level Selection Cards
*   **Structure:** No dividers. Use `secondary_container` (#d5cbff) for inactive or upcoming levels. Use `error_container` (#fb5151) for the "Active" or "Hot" level.
*   **Asymmetry:** Tilt cards slightly (1–2 degrees) or stagger them along a path to break the rigid grid.

### Glassmorphism Floating Panels
*   **Background:** `surface_container_lowest` at 70% opacity + 20px Background Blur.
*   **Content:** Keep text high-contrast using `on_surface` (#223131).

### Navigation Bar
*   **Style:** A single "slab" of `surface_container_highest` without a top border.
*   **Active State:** Use a `secondary` (#5d3fd3) pill background behind the icon/text.

---

## 6. Do's and Don'ts

### Do
*   **DO** use varying opacities of `secondary` and `tertiary` to create "glow" effects behind level nodes.
*   **DO** leave significant white space (using the `xl` corner radius as a guide for margin size) to allow the "vibrant" colors to breathe.
*   **DO** use `surface_bright` for highlight moments where you want a container to feel "illuminated" from within.

### Don't
*   **DON'T** use 1px solid black or grey lines to separate list items. Use a 4px gap and a subtle background shift.
*   **DON'T** use sharp 90-degree corners. Everything in this world should feel safe, soft, and "polished" like a river stone.
*   **DON'T** use high-contrast drop shadows. If the shadow is the first thing you notice, it is too dark. It should feel like an ambient atmospheric effect.