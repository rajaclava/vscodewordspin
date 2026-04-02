# Scene Setup Guide

Create these scenes inside Unity:

- `Boot`
- `MainMenu`
- `Gameplay`
- `Store`

Recommended scene wiring:

1. Add `SceneBootstrap` to the first loaded scene.
2. Add `GameManager`, `LevelFlowController`, `HitEvaluator`, `SessionManager`, `PinLauncher`, `FireGate`, `SlotManager`, and `TargetRotator` to `Gameplay`.
3. Add `ThemeRuntimeController`, `KeyboardPresenter`, `InfoCardPresenter`, and `ResultPresenter` to gameplay UI.
4. Add `MainMenuPresenter` to the main menu canvas.
5. Add `StorePresenter` and `MembershipPresenter` to the store canvas.
6. Place `Slot` colliders under the rotator object and assign them into `SlotManager.slots`.
7. Use a `Pin` prefab with `Rigidbody2D`, `Collider2D`, and a visible sprite child.
8. Use world-space placement for the rotator, launcher, and impact path; keep text-heavy UI in screen-space canvas.

Suggested alpha visual composition:

- top bar, hearts, question panel, answer row
- rotator centered in upper-middle playfield
- long swipe flight lane
- launcher attached just above the keyboard
- keyboard anchored at the bottom safe area
- hint/store buttons in the bottom strip
