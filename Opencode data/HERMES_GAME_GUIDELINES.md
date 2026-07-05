# Project Guidelines — [Working Title] 2D Roguelite
**Engine:** Unity (2D, URP recommended)
**Target platforms:** PC, targeting Steam release, itch.io as secondary/early distribution
**Primary references for feel:** Hollow Knight (animation smoothness, precision platforming), Wizard of Legend (combat speed, dash-based mobility), Dead Cells (roguelite structure, procedural room stitching)

This document is the single source of truth for the project. Every subagent and every task should be scoped against this file. If a decision isn't covered here, escalate to the Orchestrator rather than inventing a new convention.

---

## 1. Core Concept

A 2D pixel-art roguelite with fast, precise, momentum-based combat. Players choose one of 5 classes at the start of a run. Each run is procedurally assembled from hand-authored room chunks. Death ends the run and returns the player to a persistent hub, but meta-progression (currency, unlocks) is retained. Light story is delivered through the environment and hub NPCs — no cutscenes.

### Pillars
- **Movement/combat feel first.** If dashing and attacking doesn't feel great in a gray-box room, no amount of content will fix it. Validate feel before building content on top of it.
- **Progress must feel palpable.** Both meta-progression (unlocks, currency) and narrative progression (hub changes, NPC dialogue evolving) should visibly respond to player progress.
- **Anti-grind economy.** Cheap unlocks (new items enter the drop pool), expensive power (stat/ability upgrades). Players should feel like build variety, not repetition, is the path forward.

---

## 2. Game Loop & World Structure

- **Hub (persistent, hand-crafted):** the only non-procedural space. Contains merchants, the upgrade tree interface, and NPCs who deliver story/flavor over time.
- **Runs (procedural):** built by stitching hand-authored room "chunks" together algorithmically (see Section 6). Each chunk is tagged with entry/exit points and difficulty/biome metadata.
- **Death:** ends the run immediately, player returns to hub. Currency and unlocks earned during the run are kept; run-local state (health, temporary buffs, position) is discarded.
- **Progression per run:** currency earned → spent in hub on (a) new items/weapons/runes added to the drop pool, and (b) permanent stat/ability upgrades via the upgrade tree.

---

## 3. Classes

Five classes, chosen once at the start of a run (unlocking additional classes may itself be a meta-progression goal — flag this as an open design question if not already decided).

Each class has:
- A **class-locked weapon type** — only variations/tiers of this weapon type will appear for that class.
- **Class-locked armor** — reinforces visual and mechanical identity; not shared across classes.
- Access to the **universal rune pool** — the build-customization layer, usable by any class.
- A unique **dash variant** (same underlying system — free dash, cooldown-based, i-frames during the dash window — but with a distinct implementation per class).

| Class | Weapon Type | Identity | Dash Variant |
|---|---|---|---|
| **Knight** | Heavy melee (swords, hammers) | Tanky, slow, high poise | Short heavy dash-tackle; consider brief poise/armor instead of full i-frames |
| **Royalty** | Light, swift melee | Fast, evasive, fragile | Quick double-dash or blink |
| **Magus** | Staff/spells | Ranged caster, positioning-dependent | Teleport-style blink; consider a decoy or burst effect on arrival |
| **Hunter** | Bow/ranged | Kiting, sustained ranged damage | Backstep/disengage dash, tuned for kiting |
| **Prisoner (Deprived)** | Stun weapons (clubs, gauntlets) | Extremely hard early game, highest ceiling late game | Risky dash — longer cooldown or resource cost, fitting the "no safety net" identity |

### Prisoner (Deprived) — special notes
- **Early game:** quick but low DPS, low defense, low HP. Weapon kit is built around stagger/stun rather than raw damage. This class should be genuinely brutal to start — near-perfect play required.
- **Late game:** once enough currency is invested, this class has access to a **hidden branch** of the shared upgrade tree, inaccessible to any other class (see Section 4). This branch contains rule-breaking upgrades — e.g. lifesteal, a stamina-free parry — that let Prisoner become the most powerful class in the game, as a reward for surviving its brutal start.

---

## 4. Inventory & Progression Systems

### Inventory categories
- **Weapon** — class-locked, tiers/variants only.
- **Armor** — class-locked, reinforces class identity.
- **Runes** — universal, usable by any class; primary build-customization layer.

### Upgrade Tree
- All 5 classes share the **same upgrade tree structure**, for consistency and balance.
- One branch of the tree is **hidden/blacked-out by default**, and is only accessible when playing Prisoner. This branch contains the class's signature rule-breaking upgrades (lifesteal, free-cost parry, etc).
- Implementation note for the Data Agent: model this as a single tree data structure with an optional `classRestricted` field per node, rather than building a separate tree system for Prisoner.

### Economy design (anti-grind, important constraint)
- New drop-pool unlocks (weapons, runes) should be **relatively cheap** — the goal is build variety, not gatekeeping content.
- Stat/power upgrades should be **deliberately expensive**, scaling so that pure grinding is an inefficient strategy compared to improving play and choosing better builds.
- This applies to every class's upgrade tree, not just Prisoner's hidden branch.

---

## 5. Combat

- **Dash:** free (no resource cost by default, except where a class's identity calls for a cost — e.g. Prisoner), on a cooldown, grants i-frames during the active window. Each class has a distinct dash implementation (see Section 3 table) built on the same underlying dash system/component.
- Combat should be validated in a gray-box room (no art, placeholder capsules/sprites) before any content is built on top of it — see Section 8, Milestone 1.

---

## 6. Procedural Generation

- **Approach:** hand-authored room chunks + algorithmic stitching (the Dead Cells / Enter the Gungeon / Binding of Isaac approach) — **not** raw tile-by-tile procedural generation. This produces far fewer bugs and lets hand-crafted pixel art read as intentional.
- Each room chunk should be tagged with:
  - Entry/exit points (and their directions)
  - Biome/theme
  - Difficulty tier
  - Any special flags (contains merchant, contains boss, etc.)
- The stitching algorithm selects and connects chunks at runtime based on these tags.
- This is one of the highest-risk systems in the project — it has its own dedicated subagent (see Section 9) and should be stress-tested early with placeholder chunks before final art is applied.

---

## 7. Story & Narrative Delivery

- **No cutscenes.** Story is delivered through:
  - Environmental storytelling (level design, background details, item flavor text)
  - Hub NPC dialogue, which should evolve as the player progresses (new dialogue unlocked by meta-progression milestones) — this is part of what makes progress "feel palpable"

---

## 8. Development Workflow & Build Order

### Scope sequencing
Build a **vertical slice around one class first — Knight** — before expanding to the other 4. Knight is the simplest to validate (no projectile physics, no ranged AI, no teleport edge cases) and establishes the baseline moveset the other classes' dashes/kits are judged against.

### Milestone-based development (hard gates, not parallel partial features)
1. **Milestone 1 — Core feel:** gray-box player controller, Knight's dash + basic attack, one placeholder room. Validate movement/combat feel before anything else.
2. **Milestone 2 — Vertical slice:** Knight fully playable — hub (basic), 1-2 real biomes via procedural stitching, one boss, currency + a minimal upgrade tree, save/load working.
3. **Milestone 3 — Class expansion:** add Royalty, Magus, Hunter, Prisoner using the established patterns (weapon data, dash variant, armor).
4. **Milestone 4 — Content & polish:** additional biomes, full rune pool, full upgrade trees including Prisoner's hidden branch, hub NPC dialogue progression.
5. **Milestone 5 — Release prep:** Steam/itch.io packaging, platform-specific testing, final QA pass.

Do not begin a milestone until the previous one is fully functional and committed — this is what makes it possible to know whether a later change introduced a regression.

### Technical architecture (non-negotiable conventions)

**Data-driven design via ScriptableObjects**
All weapons, armor, runes, enemies, and upgrade tree nodes are defined as ScriptableObject assets, not hardcoded classes. Balancing is done by editing data assets in the Inspector, not by touching gameplay code.

**Explicit finite state machines**
Player controller, enemy AI, and UI flow all use defined states (Idle, Attack, Dash, Hurt, Dead, etc.), not boolean flags. Build one lightweight, reusable FSM system early — this becomes the backbone for the player controller and all enemy AI.

**Event-driven communication between systems**
Systems (inventory, hub UI, currency, save, combat) communicate via C# events or ScriptableObject-based Game Events (Ryan Hipple / Unite pattern) rather than direct references to each other. This is what prevents "changing the shop breaks the save system" style failures.

**Object pooling**
Any frequently-spawned object (enemies, projectiles, hit effects, pickups) is pooled from the start. Never raw `Instantiate`/`Destroy` in gameplay code.

**Save system**
- Meta-progression save data is fully separate from in-run state.
- Save schema includes a `saveVersion` field from the very first commit of the save system, to allow safe migration as fields are added later.

**Version control**
- Proper Unity `.gitignore` (`Library/`, `Temp/`, `obj/`, `Logs/`, etc.) from the first commit.
- Git LFS for sprite/audio binaries.
- Small, frequent, clearly-described commits — this is the rollback safety net for AI-generated changes.
- Feature branches per system (inventory, combat, procgen, etc.) so an experimental change never risks the stable base.

**Testing**
Unity Test Framework tests are required for save/load, currency transactions, and inventory math specifically — these are the systems where a silent bug (corrupted saves, duplicated items) is both likely to go unnoticed and expensive to have shipped. Not every system needs automated tests; these do.

---

## 9. Subagent Workflow

Goal: reduce token usage per task and, above all, protect a consistent, clean code structure across the whole project.

### Structure

**Orchestrator**
- The only agent that holds the "whole picture." Reads this document plus a living `ARCHITECTURE.md` (which it maintains as the project grows — naming conventions, current folder structure, established patterns).
- Breaks each milestone into discrete tasks and assigns them to the correct specialist below.
- Reviews each completed task via `git diff` (not full file re-reads) to confirm it follows established patterns before merging.
- Should rarely write gameplay code directly — its job is decomposition and quality gating, which keeps its own token usage low.

**Specialists** (each receives only the files relevant to its task, plus this document — never the full repo):
1. **Systems/Core Programmer** — player controller, shared FSM framework, the dash/combat framework used by all classes.
2. **Data/Content Agent** — ScriptableObject definitions and actual data entries (weapon stats, armor stats, rune effects, enemy stats). Deliberately separate from Systems: balancing a weapon should never require touching the code that makes weapons function.
3. **Procedural Generation Agent** — room chunk pool, tagging, and the stitching algorithm. Isolated due to being the highest-complexity, highest-bug-risk system.
4. **Enemy AI Agent** — enemy behavior and state machines, built on the shared FSM system but kept separate from the player Systems agent.
5. **UI/Inventory/Hub Agent** — inventory screens, hub interactions, merchant UI, upgrade tree UI.
6. **Save/Meta-Progression Agent** — save schema, currency, persistence, save versioning. Treated carefully — bugs here are the most damaging class of bug in the project.
7. **QA/Test Agent** — writes and runs tests for save/currency/inventory math; performs regression passes after major merges. Its job is to try to break what the others built.

### Token-optimization rules
- Specialists get only the files relevant to their assigned task, plus this guidelines document — never the whole repository.
- Specialists return a **summary + diff** to the Orchestrator, not full pasted files.
- Git commits/diffs are the checkpoint system — the Orchestrator verifies work via `git diff`/`git log`, not by re-reading full source files.
- Shared conventions (naming, the event pattern, folder structure, the FSM pattern) are written once in `ARCHITECTURE.md` and referenced by path — never re-explained inline in every task prompt.

---

## 10. Open Assumptions / Flagged for Decision

These were not yet specified and have been given a sensible default. Revisit before they become load-bearing:

- **Pixel art resolution / animation pipeline:** no resolution (e.g. 32x32, 48x48) or animation tool (Aseprite import, Unity native sprite animation, Spine2D) has been chosen yet. Recommend deciding this before Milestone 1 art pass, since it affects import pipeline setup.
- **Class unlocking:** whether all 5 classes are available from the start, or unlocked via meta-progression, is not yet decided.
- **Number of biomes and run length target** (how many rooms/biomes per run, expected run duration) is not yet decided — relevant for procedural generation scope in Milestone 3-4.

---

*This document should be updated as decisions evolve. Any subagent or contributor encountering a gap not covered here should escalate to the Orchestrator rather than assuming a convention.*
