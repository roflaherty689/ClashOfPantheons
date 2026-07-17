# Roadmap

## Current milestone

**Milestone:** Prototype  
**Status:** In progress — foundations present, player loop not integrated

**Primary outcome:** Deliver one complete economy → production → autonomous combat → result → restart match against AI.

### Entry conditions

- [x] Unity 2D project and one battle scene exist.
- [x] Autonomous lane combat, strongholds, five unit roles, factions, and worker mining have source and asset foundations.
- [x] Core design and scope are documented in `GAME_DESIGN.md` and `DECISIONS.md`.

Checked entry conditions are based on repository inspection, not a new Unity launch or Play Mode run.

### Exit criteria

- [ ] A player can use gold to buy workers, establish independent unit-type production, and buy one-, two-, and three-star upgrades.
- [ ] An AI opponent makes economy, production, composition, and upgrade decisions under the same match rules.
- [ ] Units autonomously resolve combat on one shared lane and can destroy either stronghold.
- [ ] An approximately five-minute timer resolves matches by stronghold health, then lower total value of units lost when health is equal.
- [ ] The result is clearly presented and a new match can start without Editor intervention.
- [ ] The functional HUD accurately communicates gold, workers, production, upgrades, time, stronghold health, and result state.
- [ ] Exact equality after both timeout comparisons produces a draw with no winner or loser.
- [ ] The project compiles without known blocking errors and the complete critical path is verified in Play Mode.
- [ ] Critical economy, production, and result rules have proportionate automated coverage or documented manual verification where automation is impractical.
- [ ] Material assumptions and the next milestone's entry conditions are documented.

### Current blockers

- No project-owned automated tests or test assemblies were found during the 2026-07-16 static review.

---

## Milestone 1 — Prototype

**Goal:** Prove that short single-player matches create meaningful economy, production, composition, and upgrade decisions without direct unit control.

### Existing foundation

- [x] Autonomous horizontal movement, target acquisition, melee/ranged combat, projectiles, health, and unit death are represented in source.
- [x] Two strongholds and base-destruction victory handling are represented in source and the battle scene.
- [x] Melee, archer, cavalry, siege, and mythic role mappings and prototype assets exist.
- [x] Workers can mine and deposit gold; the player HUD displays live worker economy state and can buy workers up to capacity.
- [x] Faction data can map roles to faction-specific prefabs.
- [x] Black, blue, purple, red, and yellow prototype faction assets map colour-specific animated worker, melee, and archer prefabs while sharing cavalry, siege, and mythic.
- [x] The five prototype colour factions own matching Castle and House3 presentation references consumed by one shared Base prefab and the stronghold HUD; integrated Play Mode verification remains pending.
- [x] Prototype health, hit, animation, projectile, and victory feedback exists.

These checks confirm repository presence only. Their integrated runtime behavior remains subject to compilation and Play Mode verification.

### Planned outcomes

- [x] Accept `DEC-007`: first purchase unlocks continuous role production; the next two purchases upgrade future spawns to two and three stars.
- [ ] Connect the player's gold and worker systems to independent production for all five roles.
- [ ] Implement an AI decision layer that participates in the same economy and production game.
- [ ] Implement the five-minute timer, stronghold-health comparison, unit-loss-value tiebreaker, result states, and restart flow.
- [ ] Implement the three star tiers with clear costs, effects, and production/UI feedback.
- [x] Move recurring production cadence ownership from the global spawn interval into each role's `UnitData`; retain per-role cost ownership there. Initial values and selectable spawn patterns were user-verified in Play Mode on 2026-07-16.
- [ ] Replace static mock HUD values and shared-queue presentation with functional prototype state.
- [ ] Validate role readability and at least one meaningful economy-versus-pressure decision through playtesting.
- [ ] Add focused tests for deterministic economy, production, and result rules, plus a documented Play Mode critical-path check.

### Dependencies

- Accepted design constraints in `DEC-002` through `DEC-006`.
- Accepted production semantics in `DEC-007`.
- Existing `GameManager`, `WorkerManager`, `WorkerUnit`, `GoldVein`, `FactionData`, `UnitData`, `BaseUnit`, and `Base` foundations.
- Functional UI bindings and scene wiring after the underlying rules are defined.

### Deferred from this milestone

- Online multiplayer, ranked matchmaking, and networking architecture.
- Campaign and persistent progression.
- Multiple currencies beyond gold.
- Controller support and broad platform certification.
- Final art, final audio, and broad designed-faction or map production beyond the five mechanically identical prototype team-colour variants.
- Heroes, bosses, neutral objectives, buildings, and powers unless separately approved.
- Production-ready saving and deep optimization without measured need.

---

## Milestone 2 — Vertical Slice

**Goal:** Produce one polished, representative section of the intended game.

### Planned outcomes

- [ ] Representative gameplay loop
- [ ] Representative visual direction
- [ ] Representative audio and feedback
- [ ] Stable UI flow
- [ ] Initial accessibility support
- [ ] Core systems integrated
- [ ] Basic save or progression continuity
- [ ] Meaningful automated and manual test coverage

### Entry conditions

- Prototype core loop is validated.
- Major design risks have been reduced.

### Exit criteria

- [ ] A new player can understand and complete the slice.
- [ ] The slice represents expected final quality and structure.
- [ ] Core architecture is stable enough for content production.
- [ ] Major performance risks are understood.

---

## Milestone 3 — Production

**Goal:** Build the planned systems and content using the validated foundation.

### Planned outcomes

- [ ] Complete core systems
- [ ] Produce planned gameplay content
- [ ] Expand enemies, levels, abilities, or progression
- [ ] Maintain tests and regression checks
- [ ] Maintain performance targets
- [ ] Keep design and project documents current

### Entry conditions

- Vertical slice is accepted.
- Scope is defined.
- Production pipeline is repeatable.

### Exit criteria

- [ ] Planned feature scope is implemented.
- [ ] Planned content scope is substantially complete.
- [ ] No known architectural blocker remains.

---

## Milestone 4 — Content Complete

**Goal:** Finish planned content and stop adding major features.

### Planned outcomes

- [ ] All planned content is present
- [ ] Feature scope is locked
- [ ] Save compatibility is stable
- [ ] Major progression and balance pass completed
- [ ] Full-game completion is possible

### Exit criteria

- [ ] The full game can be played from start to finish.
- [ ] Remaining work is stabilization, tuning, accessibility, and polish.

---

## Milestone 5 — Stabilization

**Goal:** Improve reliability, performance, clarity, and balance.

### Planned outcomes

- [ ] Fix high-priority defects
- [ ] Address performance bottlenecks
- [ ] Complete accessibility pass
- [ ] Complete controller and input checks
- [ ] Complete save/load regression checks
- [ ] Complete balance and pacing passes
- [ ] Test supported resolutions and platforms

### Exit criteria

- [ ] No known blocker or critical defect remains.
- [ ] High-priority regression scenarios pass.
- [ ] Release candidate criteria are defined.

---

## Milestone 6 — Release Preparation

**Goal:** Produce and verify the release candidate.

### Planned outcomes

- [ ] Final builds
- [ ] Platform-specific validation
- [ ] Credits and legal requirements
- [ ] Store or distribution requirements
- [ ] Final settings defaults
- [ ] Final known-issues review
- [ ] Version and release notes

### Exit criteria

- [ ] Release candidate accepted.
- [ ] Known issues are documented.
- [ ] Distribution checklist is complete.

---

## Deferred ideas

Ideas listed here are not committed scope.

- Online multiplayer and ranked matchmaking.
- Alternative maps, lane structures, or an endless mode after the five-minute match is validated.
- Campaign and persistent progression.
- Additional currencies, buildings, powers, heroes, bosses, and neutral objectives.
- Controller support.

---

## Removed or rejected scope

Record intentionally removed work and reference the relevant decision.

- Shared FIFO unit production for the prototype — conflicts with `DEC-004`.
- Direct control of combat units — conflicts with `DEC-002`.

---

## Roadmap update notes

Use this section only for major sequencing or scope changes.

- 2026-07-16: Reconciled the Prototype milestone with the implemented foundations and accepted design direction. Replaced generic outcomes with the game-specific critical path and deferred non-prototype scope.
