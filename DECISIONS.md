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
- Exact costs, cadences, and matchup values remain tuning inputs rather than production-contract ambiguity.
- `UnitData` currently stores cost but must gain per-role production cadence; the global `GameManager` interval is stale relative to this decision.

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
