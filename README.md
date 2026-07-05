# Godus

A 2D pixel-art roguelite with fast, precise, momentum-based combat.  
**Engine:** Unity 6000 (URP 2D) | **Target:** Steam + itch.io (PC)

## Development Status

- [x] Project scaffold
- [ ] Milestone 1 — Core feel (Knight gray-box)
- [ ] Milestone 2 — Vertical slice
- [ ] Milestone 3 — Class expansion
- [ ] Milestone 4 — Content & polish
- [ ] Milestone 5 — Release prep

## Quick Start

1. Open `E:\OpenCode Projects\VG\Godus` in Unity Hub
2. Unity 6000.5.2f1 is required
3. Open the project — Unity will import packages and generate remaining ProjectSettings

## Architecture

- **FSM-based:** Player controller and enemy AI use explicit state machines (no boolean flags)
- **Event-driven:** Systems communicate via `EventBus` (static C# events), no direct references
- **Object pooled:** All frequently-spawned objects use `ObjectPool<T>`
- **Data-driven:** Weapons, armor, runes, enemies defined as ScriptableObjects
- **Save-versioned:** Save schema includes `saveVersion` from day one

## Folder Structure

```
Assets/
├── _Scripts/Core/         # FSM, EventBus, ObjectPool, GameManager, Save
├── _Scripts/Player/       # PlayerController, states
├── _Scripts/Combat/       # Dash, Attack, Hitbox/Hurtbox
├── _Scripts/Enemies/      # EnemyAI states
├── _Scripts/Systems/      # Inventory, ProcGen, Currency
├── _Scripts/UI/           # HUD, Menus, Hub UI
├── _ScriptableObjects/    # Weapon, Armor, Rune, Enemy data
├── _Scenes/               # Core (bootstrap), Hub, Runs, Testing
├── _Prefabs/
├── _Art/                  # Sprites, Animations, Tilemaps, UI
├── _Audio/                # Music, SFX
└── _Resources/            # URP assets, runtime-loaded content
```

## Key References

- `HERMES_GAME_GUIDELINES.md` — full design document (single source of truth)

## Git Conventions

- Feature branches per system: `inventory`, `combat`, `procgen`, `save`, `ui`
- Small, frequent, clearly-described commits
- Git LFS for binary assets (sprites, audio)
