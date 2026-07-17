# Project Decisions

## Purpose

This file records important design, architecture, compatibility, and production decisions.

Use the following statuses:

- **Proposed** — under consideration and not yet approved.
- **Accepted** — approved and currently active.
- **Superseded** — replaced by a later decision.
- **Rejected** — considered and intentionally declined.

Do not record trivial implementation details.

---

## Decision index

| ID | Title | Status | Date |
|---|---|---|---|
| DEC-001 | Project decision-record format | Accepted | 2026-07-16 |
| DEC-002 | Core player role and opponent | Accepted | 2026-07-16 |
| DEC-003 | Prototype battlefield and match resolution | Accepted | 2026-07-16 |
| DEC-004 | Prototype economy, roles, and upgrades | Accepted | 2026-07-16 |
| DEC-005 | Prototype scope boundary | Accepted | 2026-07-16 |
| DEC-006 | Prototype visual direction | Accepted | 2026-07-16 |
| DEC-007 | Unit purchase and recurring-production contract | Accepted | 2026-07-16 |
| DEC-008 | Prototype platform priority | Accepted | 2026-07-16 |
| DEC-009 | Prototype team-colour faction variants | Accepted | 2026-07-17 |
| DEC-010 | Faction-owned building presentation with shared gameplay prefabs | Accepted | 2026-07-17 |
| DEC-011 | Faction-owned animated worker prefabs | Accepted | 2026-07-17 |
| DEC-012 | Initial title and faction-selection flow | Accepted | 2026-07-17 |

---

## DEC-001 — Project decision-record format

**Date:** 2026-07-16
**Status:** Accepted

### Decision

Important project decisions will be recorded in this file using the structure shown below.

### Context

The project needs durable context that can be understood across separate development sessions.

### Rationale

Git history records implementation changes, but it does not always preserve design intent, alternatives, or production tradeoffs.

### Consequences

- Material design and architecture choices should be documented.
- Proposed decisions must not be treated as accepted.
- Superseded decisions must reference their replacement.

### Alternatives considered

- Rely only on Git history.
- Store decisions in task descriptions.
- Keep decisions only in conversation history.

### Related items

- `AGENTS.md`
- `GAME_DESIGN.md`
- `ROADMAP.md`

---

## DEC-002 — Core player role and opponent

**Date:** 2026-07-16
**Status:** Accepted

### Decision

Clash of Pantheons is a 2D mythological tug-of-war strategy autobattler. The prototype is primarily single-player against AI. Units move and fight autonomously; the player acts indirectly through workers, economy, unit production, composition, and upgrades.

### Context and rationale

The existing lane combat is autonomous, while the intended player value comes from strategic choices rather than unit micro-control. A single-player AI target provides one coherent loop without taking on multiplayer infrastructure.

### Consequences

- Direct unit control is outside prototype scope.
- The AI must use or respect the same strategic rules as the player.
- Online multiplayer remains possible later but cannot block prototype completion.

### Alternatives considered

- Directly controlled units: rejected for the prototype because it changes the core player role.
- AI-versus-AI sandbox only: useful for testing, but insufficient as the primary player experience.
- Online-first play: deferred due to scope and dependency cost.

### Related items

- `GAME_DESIGN.md` — Player fantasy and core loop
- `ROADMAP.md` — Prototype

---

## DEC-003 — Prototype battlefield and match resolution

**Date:** 2026-07-16
**Status:** Accepted

### Decision

The prototype uses one shared horizontal lane and targets matches of approximately five minutes. A destroyed stronghold loses immediately. On timeout, the healthier stronghold wins; if health is equal, the side with the lower total value of units lost wins.

### Context and rationale

The repository already contains a one-lane, two-stronghold battle. Adding a bounded match duration and deterministic timeout rules supports short sessions and resolves stalemates.

### Consequences

- Match flow must track time, stronghold health, and the value of destroyed units.
- Results must identify the resolution reason and support restart.
- Lost-unit value is the purchase cost of that unit's production slot multiplied by the number of units of that type destroyed.
- Exact equality after both timeout comparisons produces a draw with no winner or loser.

### Alternatives considered

- Base destruction only: rejected because matches need a bounded target length.
- Draw on equal base health: rejected in favour of a second strategic tiebreaker.
- Multiple lanes in the prototype: deferred until the single-lane loop is validated.

### Related items

- `GAME_DESIGN.md` — Health, failure, and match result
- `TODO.md` — Match result and restart task

---

## DEC-004 — Prototype economy, roles, and upgrades

**Date:** 2026-07-16
**Status:** Accepted

### Decision

Gold is the only prototype currency. It supports workers, military production, and upgrades. Military production is independent for melee, archer, cavalry, siege, and mythic roles rather than using one shared FIFO queue. Every role has one-, two-, and three-star in-match upgrade tiers. A favourable matchup applies a 1.2× damage multiplier. Star tiers multiply all configured unit stats except purchase cost by 1×, 1.5×, and 2×.

### Context and rationale

The repository already has gold workers, five role mappings, costs, and automated spawning, but these systems are disconnected. A single currency and independent role production focus the prototype on economy, composition, and timing.

### Consequences

- Favour and essence in the rough HUD are not prototype systems.
- The shared queue shown in the rough HUD must not define runtime behavior.
- Each role's purchase cost and recurring production cadence are owned by its `UnitData` asset. Exact values and remaining matchup details require tuning.
- The purchase-to-recurring-production contract is accepted in `DEC-007`.

### Alternatives considered

- Multiple prototype currencies: deferred as unnecessary complexity.
- One shared FIFO army queue: rejected because unit types must produce independently.
- No upgrades: rejected because three star tiers are part of the approved strategic layer.

### Related items

- `GAME_DESIGN.md` — Economy and production
- `TODO.md` — Production semantics and integration tasks

---

## DEC-005 — Prototype scope boundary

**Date:** 2026-07-16
**Status:** Accepted

### Decision

The immediate goal is one complete economy → production → autonomous combat → result → restart loop. Online multiplayer, final art, broad faction production, campaign progression, persistent progression, multiple currencies, controller support, and production-ready saving are deferred beyond this core prototype unless separately approved.

### Context and rationale

The combat and worker foundations exist, but the player decision loop is not connected. Completing and validating that critical path has higher value than expanding content or platforms.

### Consequences

- Deferred systems must not become dependencies of the Prototype milestone.
- Documentation and UI should not imply deferred mock-up features are implemented commitments.
- Prototype work prioritizes integration, AI, match flow, restart, feedback, and verification.

### Alternatives considered

- Begin multiplayer or broad content now: rejected because neither proves the incomplete core loop.
- Add persistence before match flow: deferred until persistence has a validated player purpose.

### Related items

- `ROADMAP.md` — Prototype deferred scope
- `TODO.md` — P1 tasks

---

## DEC-006 — Prototype visual direction

**Date:** 2026-07-16
**Status:** Accepted

### Decision

The prototype uses the colourful, humorous Tiny Swords pixel-art style. Supplementary or replacement art should preserve that tone while using recognizable mythological and cultural details; strict historical accuracy is not the goal.

### Context and rationale

Tiny Swords assets already define the current battlefield and provide an appropriate readable, light-hearted prototype language.

### Consequences

- Final art production remains deferred.
- Faction identity should remain readable and mythological without requiring historical simulation.
- Existing humorous placeholder factions do not by themselves define final faction content.

### Alternatives considered

- Historically realistic presentation: rejected for the current direction.
- Replace the asset style during the prototype: deferred unless current assets block validation.

### Related items

- `GAME_DESIGN.md` — Audio and visual direction

---

## DEC-007 — Unit purchase and recurring-production contract

**Date:** 2026-07-16
**Status:** Accepted

### Decision

Each role starts locked. Its first purchase unlocks one independent track that continuously produces that unit type. The second and third purchases upgrade future spawns from that track to two and three stars. Purchases never enter a shared FIFO queue, and upgrades do not change units already on the field.

### Context

The approved direction requires independent production by unit type. The current `GameManager` instead spawns both teams for free from one global timer, so the player-facing contract must be explicit before integration.

### Rationale

One recurring track per role is easy to read, fits short matches, preserves composition decisions, and avoids hidden queue interactions. Using later purchases for star tiers gives every production control a consistent three-step lifecycle.

### Consequences

- Production tracks must retain independent timers and state.
- The UI must show locked, one-star, two-star, and three-star states without presenting a shared queue.
- The AI must purchase and upgrade through the same contract.
- The locked initial state applies to both teams. Until the separate AI purchasing task is implemented, enemy roles remain locked and produce no units in the active per-role mode; there is no temporary automatic enemy bootstrap.
- Exact costs, cadences, and matchup values remain tuning inputs rather than production-contract ambiguity.
- `UnitData` stores both cost and per-role production cadence.
- `GameManager` may retain the legacy global interval as a selectable prototype/debug pattern, but independent `UnitData` timers remain the intended production behavior.
- The independent-timer mode gates every role by its purchased tier and does not accumulate production time while locked. The legacy global pattern may continue free spawning only as an explicit prototype/debug mode.

### Alternatives considered

- Each purchase produces exactly one unit: simpler economy, but does not naturally express recurring independent production.
- Purchases add parallel recurring slots: creates scaling depth, but may snowball and clutter the HUD.
- One shared FIFO queue: conflicts with accepted direction.

### Related items

- `GAME_DESIGN.md` — Open design questions
- `TODO.md` — Resolve independent production contract

---

## DEC-008 — Prototype platform priority

**Date:** 2026-07-16
**Status:** Accepted

### Decision

PC storefronts and mouse input are the first prototype and release priority. Mobile and touch adaptation are second. Controller support remains deferred beyond the core prototype.

### Context

Both PC and mobile are intended targets, but UI, input, resolution, and validation work require an explicit sequencing priority.

### Rationale

PC-first development matches the current Editor-driven workflow and avoids requiring simultaneous touch adaptation while the core loop remains incomplete.

### Consequences

- Prototype UI and verification target mouse input first while preserving a layout that can later adapt to touch.
- Mobile-specific input and platform validation follow core-loop validation.
- Controller support is not a Prototype milestone dependency.

### Alternatives considered

- Mobile first: viable later, but adds touch and device-layout work before the loop is proven.
- Simultaneous PC/mobile implementation: deferred to protect the critical path.

### Related items

- `GAME_DESIGN.md` — Accessibility and input
- `ROADMAP.md` — Prototype deferred scope

---

## DEC-009 — Prototype team-colour faction variants

**Date:** 2026-07-17
**Status:** Accepted

### Decision

Support all five Tiny Swords unit colours—black, blue, purple, red, and yellow—as mechanically identical prototype faction assets. Melee and archer receive colour-specific animated prefabs and controllers. Cavalry, siege, and mythic remain shared across the five variants for now.

### Context

The original animated prototype faction used black Tiny Swords melee and archer assets. The user requested the other four available colour options and explicitly limited this pass to melee and archer.

### Rationale

Team-colour variants provide readable visual alternatives using already imported art without introducing five mechanically or thematically distinct factions before the core loop is complete.

### Consequences

- Faction assets use the consistent `BlackFaction`, `BlueFaction`, `PurpleFaction`, `RedFaction`, and `YellowFaction` naming convention.
- Each variant maps its matching animated melee and archer prefabs, with identical `UnitData` and gameplay configuration.
- Non-black animator controllers must expose the same `isMoving` and `Attack` behavior as the working black controllers while using their own colour clips.
- These assets are presentation variants, not distinct mythological factions, and do not satisfy or expand the deferred broad faction-production scope.
- Unity import and Play Mode verification across all colours remains required before the asset task is considered fully validated.

### Alternatives considered

- Four total variants: rejected because the user confirmed all five available colours.
- Colour-specific versions of all five gameplay roles: deferred because the user limited this pass to melee and archer.
- Treat each colour as a separate designed faction: rejected because palette alone does not provide the thematic or mechanical identity required by the game-design pillars.

### Related items

- `GAME_DESIGN.md` — Audio and visual direction
- `ROADMAP.md` — Prototype existing foundation and deferred scope
- `TODO.md` — Five-colour Unity verification
- `Assets/ScriptableObjects/Factions`

---

## DEC-010 — Faction-owned building presentation with shared gameplay prefabs

**Date:** 2026-07-17
**Status:** Accepted

### Decision

Each prototype colour faction owns references to its matching Tiny Swords Castle and House3 sprites. The selected faction drives the team's world castle, worker hand-in, and HUD stronghold icon. Both teams continue to use one shared Base gameplay prefab; colour-specific Base prefab variants are not created.

### Context

The scene independently hardcoded a black left castle/hand-in and red right castle/hand-in while both `GameManager` faction fields selected `BlackFaction`. This allowed unit selection, world buildings, and HUD icons to disagree. The user approved Black versus Red as the default, faction-driven HUD icons, House3 for every colour, and the proposed Base prefab refactor.

### Rationale

Presentation sprites are authored faction configuration, while base health, collision, worker economy, and deposit behavior are shared gameplay. Keeping those responsibilities separate makes `FactionData` the single presentation source without duplicating gameplay configuration across five prefabs.

### Consequences

- `FactionData` exposes Castle and House3 presentation sprites in addition to unit mappings.
- A focused Base presentation component consumes faction art; `WorkerManager` remains responsible for economy and uses the prefab-owned hand-in transform.
- The shared Base prefab owns its castle and hand-in children, while scene instances retain only team, placement, spawn-point, and gold-vein differences.
- `GameManager` applies presentation at match startup and updates the corresponding HUD icons.
- The legacy Default and meme faction assets may omit these sprites; authored prefab fallbacks remain visible and configuration diagnostics are emitted.
- "Selected faction" currently means the serialized left/right `GameManager` references before a match; live mid-match faction switching is not introduced.

### Alternatives considered

- Five colour-specific Base prefabs: rejected because identical health, collider, worker, and placement configuration would be duplicated and could drift.
- Keep scene-owned sprite overrides: rejected because it caused the confirmed mismatch between faction selection and presentation.
- Put art selection in `WorkerManager`: rejected because the economy component should not own faction presentation.

### Related items

- `GAME_DESIGN.md` — Audio and visual direction
- `ROADMAP.md` — Prototype existing foundation
- `TODO.md` — Faction-driven prototype building presentation
- `Assets/Scripts/Factions/FactionData.cs`
- `Assets/Prefabs/Buildings/Base.prefab`

---

## DEC-011 — Faction-owned animated worker prefabs

**Date:** 2026-07-17
**Status:** Accepted

### Decision

Each prototype colour faction owns a matching animated `WorkerUnit` prefab in addition to its military and building presentation references. At match initialization, `GameManager` applies the selected faction's worker prefab to that team's shared `WorkerManager` before initial workers spawn.

### Context

The five colour factions already selected their melee, archer, Castle, and House3 presentation, but both teams still spawned the same black worker through scene-owned `WorkerManager` overrides. The user requested colour-matched workers with the same animation-controller behavior and looping as black.

### Rationale

Worker colour is faction presentation, while worker costs, capacity, movement, mining, deposits, and lifecycle remain shared economy behavior. Storing only the prefab reference in `FactionData` keeps visual selection faction-owned without duplicating `WorkerManager` configuration.

### Consequences

- `FactionData` exposes a colour-specific worker prefab alongside its role mappings and building sprites.
- Black, blue, purple, red, and yellow each have an animated worker prefab; the existing black prefab retains its GUID under an explicit name.
- Non-black pawn controllers mirror black's three parameters, 20 states, eight transitions, transition settings, and state settings while referencing their own colour clips.
- All 20 pawn clips per colour inherit black's enabled loop behavior.
- `GameManager` applies faction presentation during `Awake` so `WorkerManager.Start` cannot spawn the serialized fallback before faction selection is applied.
- Legacy factions without a worker prefab retain the serialized `WorkerManager` fallback and emit an actionable warning.

### Alternatives considered

- Tint one shared worker sprite: rejected because it would not use the authored colour-specific animation frames.
- Store colour selection directly in `WorkerManager`: rejected because it would duplicate faction presentation ownership.
- Create five economy-manager or Base prefabs: rejected because identical gameplay configuration would drift between colours.

### Related items

- `DEC-009` — Prototype team-colour faction variants
- `DEC-010` — Faction-owned building presentation with shared gameplay prefabs
- `TODO.md` — Faction-driven prototype presentation
- `Assets/Scripts/Factions/FactionData.cs`
- `Assets/Scripts/Workers & Resources/WorkerManager.cs`

---

## DEC-012 — Initial title and faction-selection flow

**Date:** 2026-07-17
**Status:** Accepted

### Decision

The initial front end is delivered in three phases: (1) a dedicated title screen with Play and Exit as its only primary actions; (2) a faction-selection screen whose clickable choices are generated from a build-safe serialized list/catalog of `FactionData` assets and whose selection is applied to the player team before entering battle; and (3) a decorative animated title background using buildings and non-interactive moving units.

### Context

The project currently starts directly in its only Build Settings scene, `SampleScene`, and `GameManager` receives both factions from scene-serialized fields. The user requested a player-facing start flow and specifically required faction choices to be populated from faction ScriptableObjects.

### Rationale

Separating functional navigation and selection from animated presentation keeps each phase independently testable. A serialized catalog is available in player builds, unlike Editor-only `AssetDatabase` discovery, while still allowing button instances to be generated rather than manually duplicated for each faction. Applying the selection before battle initialization preserves `FactionData` as the source of unit, worker, castle, hand-in, and HUD presentation.

### Consequences

- The title scene becomes the first enabled Build Settings scene; the existing battle remains a separate scene.
- Menu UI generates options from configured `FactionData` references and reports invalid or duplicate configuration clearly.
- A small scene-boundary selection/session mechanism must make the chosen player `FactionData` available before `GameManager.Awake` applies faction presentation. It must not create duplicate persistent managers or retain stale state unintentionally.
- The opponent faction remains an explicit battle configuration until a separate opponent-selection rule is approved.
- Exit must work in a built player and have a safe Editor test path.
- Phase 3 is decorative only: it stays behind the UI, does not intercept input, and does not instantiate or drive combat, economy, or match systems.
- Scene wiring, Build Settings order, responsive Canvas layout, and end-to-end transitions require Unity Editor and player-build verification.

### Alternatives considered

- Hand-author one button per faction: rejected because the menu would drift when configured faction content changes.
- Discover arbitrary assets at runtime with `AssetDatabase`: rejected because Editor APIs are unavailable in player builds.
- Reuse the battle scene behind the menu: rejected because it would initialize simulation systems before the player starts a match and would couple decorative presentation to gameplay state.
- Implement the animated background before navigation: rejected as sequencing because it does not validate the required player flow.

### Related items

- `GAME_DESIGN.md` — World and match structure; UI and feedback
- `ROADMAP.md` — Prototype and Vertical Slice
- `TODO.md` — Initial title and faction-selection flow
- `Assets/Scripts/Factions/FactionData.cs`
- `Assets/Scripts/Managers/GameManager.cs`
- `ProjectSettings/EditorBuildSettings.asset`

---

## DEC-013 - Selectable test roster for mythic production

**Date:** 2026-07-17
**Status:** Accepted

### Decision

Mythic production becomes a per-match choice rather than one fixed faction prefab. The locked mythic card opens a picker before payment; selecting an option atomically purchases its configured unlock, and later purchases upgrade that choice without reopening the picker. Each option's `UnitData` owns cost and cadence. The initial test picker contains all qualifying Enemy Pack combatants plus five colour monks. Most first-pass enemy units share Minotaur melee behavior; Gnoll, Harpoon Fish, and Shaman are ranged mythics using their packaged bone, harpoon, and shaman projectile art rather than the standard archer arrow.

Monks heal the most-injured allied combat unit within range 2, excluding themselves, bases, dead units, and full-health units. They stop while healing and otherwise move normally. Healing starts at 5 every 3 seconds; amount uses existing tier multipliers, cadence does not, multiple monks may share a target, and healing cannot exceed maximum health.

### Context

The current model maps one prefab to each faction role. This broad roster is intentionally for animation/gameplay testing and possible counter-selection after an opponent reveals its mythic choice. It may be reduced after review.

### Rationale

A data-driven picker permits rapid comparison without prematurely designing distinct mechanics for every imported character. Monk healing proves support behavior separately from the reusable animated melee-prefab pipeline.

### Consequences

- `FactionData`, `GameManager`, and the HUD need a build-safe option model and per-team match selection while preserving serialized assets.
- Troll uses Windup as its attack. Troll Recovery and club-breaking animations, Skull and Turtle guard animations, and Boat are excluded.
- Opponent choice remains explicit until AI selection is implemented; the model must allow later enemy-first reveal and player counter-selection.
- This expansion remains subordinate to the prototype's incomplete AI and full-match critical path.

### Alternatives considered

- Keep one faction-owned mythic: rejected for this testing phase because it blocks roster and counter-selection experiments.
- Give every unit distinct mechanics immediately: deferred due to combat, balance, VFX, and validation scope.
- Spend before opening the picker: rejected because cancellation would create an unclear transaction.

### Related items

- `GAME_DESIGN.md` - Accepted mythic-roster test expansion
- `ROADMAP.md` - Prototype planned outcomes
- `TODO.md` - Selectable mythic roster
- `Assets/Scripts/Factions/FactionData.cs`
- `Assets/Scripts/Managers/GameManager.cs`
- `Assets/Scripts/UI/BattleEconomyUI.cs`
- `Assets/Scripts/Units/BaseUnit.cs`

---

# Decision template

Copy this section for new decisions.

## DEC-XXX — Decision title

**Date:** YYYY-MM-DD
**Status:** Proposed

### Decision

Describe the proposed or accepted decision.

### Context

Describe the problem, constraint, or situation that requires a decision.

### Rationale

Explain why this option is preferred.

### Consequences

List important positive and negative consequences.

### Alternatives considered

- Alternative:
  - Benefits:
  - Costs:
  - Reason not selected:

### Related items

- Relevant roadmap milestone
- Relevant TODO
- Relevant systems or files
