# Hades Gameplay Production Pipeline

## Goal
Build the Mythology/Hades gameplay screen as a premium portrait mobile scene without breaking gameplay mechanics.

## Production Rule
- Mechanics stay in Unity.
- Figma/Stitch provides layout, frames, and sliced panel art.
- Rotator, pin, launcher, hit feedback, glow, and motion stay in Unity.
- Do not ship a single flattened full-screen mockup.

## What To Produce In Figma or Stitch
- Top bar frame
- Heart vial holder frame
- Question panel frame
- Answer slot frame
- Keyboard frame and key surround
- Bottom CTA frame
- Background matte
- Side-statue decorative panels
- Lava framing pieces

## Export Rules
- Export UI frames as `PNG`.
- Use `SVG` only for very simple icons or line ornaments.
- Keep elements separated by role.
- Prefer 9-slice friendly borders for panels.
- Do not export the whole screen as one image.

## What To Build In Unity
- `RotatorVisual`
  - outer stone disk
  - rune band
  - inner ring
  - center emblem
  - glow overlays
- `AnchorRoot`
  - invisible or low-visibility mechanic attach points only
- `Pin`
  - base mechanic root
  - theme visual child
  - letter payload child
- `Launcher`
  - altar/nozzle base
  - charge glow
  - fire flash
  - ember burst
- World composition
  - parallax background
  - lava glow strips
  - side statues
  - ambient glow

## Runtime Integration Order
1. Lock gameplay tuning with `GameplaySceneTuner`.
2. Replace placeholder panel surfaces with exported UI slices.
3. Replace placeholder background matte.
4. Rebuild `RotatorVisual` as layered Unity sprite composition.
5. Skin `Pin` and `Launcher`.
6. Add hit feedback and launcher blast.
7. Add placeholder audio.
8. Swap placeholder audio with premium Hades audio.

## Hades Color Direction
- Primary: obsidian brown / dark basalt
- Accent: ember orange / molten amber
- Text contrast: bone / warm stone
- Feedback highlight: pale gold to hot orange
- Fail highlight: cracked ember red

## Hit Feedback Targets
- Perfect
  - bright rune flare
  - pale gold text
  - fast radial flash
- Tolerated
  - amber flash
  - softer text
- Near miss
  - short spark or weak flare
- Wrong slot / miss
  - red crack flash
  - brief shake or pulse offset

## Audio Mapping
- Load
- Fire
- Perfect
- Tolerated
- Miss
- Question complete
- Level complete

Start with placeholder procedural or simple imported sounds. Replace later using the theme resource paths in `themes.json`.

## Safety Rules
- Do not move mechanic anchors to match art by eye; tune them with `GameplaySceneTuner`.
- Do not scale `AnchorRoot` with `RotatorVisual`.
- Keep gameplay text in TMP, not baked into exported art.
- Test every art pass in portrait safe area after import.
