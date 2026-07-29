# Roadmap

## Current milestone

**Milestone:** Prototype  
**Status:** In progress — representative critical loop verified in Play Mode; in-game menu, balance correction, player-build validation, and regression coverage remain

**Primary outcome:** Deliver one complete economy → production → autonomous combat → result → restart match against AI.

### Entry conditions

- [x] Unity 2D project and one battle scene exist.
- [x] Autonomous lane combat, strongholds, five unit roles, factions, and worker mining have source and asset foundations.
- [x] Core design and scope are documented in `GAME_DESIGN.md` and `DECISIONS.md`.

Checked entry conditions are based on repository inspection, not a new Unity launch or Play Mode run.

### Exit criteria

- [ ] A player can use gold to buy workers, establish four independent ordered standard-production slots plus the separate mythic track, and buy one-, two-, and three-star upgrades on each track. The earlier role-keyed flow was verified; the ordered-slot migration remains to be verified.
- [x] An AI opponent makes economy, production, composition, and upgrade decisions under the same match rules.
- [x] Units autonomously resolve combat on one shared lane and can destroy either stronghold.
- [x] An approximately five-minute timer resolves matches by stronghold health, then lower total value of units lost when health is equal.
- [x] The result is clearly presented and a new match can start without Editor intervention.
- [x] The functional HUD accurately communicates gold, workers, production, upgrades, time, stronghold health, and result state in representative matches.
- [x] Exact equality after both timeout comparisons produces a draw with no winner or loser.
- [ ] The project compiles without known blocking errors and the complete slot-keyed critical path is verified in Play Mode. The earlier role-keyed path was verified; the ordered-slot migration is pending validation.
- [ ] Critical economy, production, and result rules have proportionate automated coverage or documented manual verification where automation is impractical.
- [ ] Material assumptions and the next milestone's entry conditions are documented.

### Current blockers

_No confirmed implementation blocker._

### Current risks and coverage gaps

- The existing 38 Core Edit Mode cases cover timeout resolution, result/countdown presentation rules, and tier transitions/scaling, but slot-keyed economy transactions, duplicate-role production scheduling, match integration, and scene flow need proportionate coverage.
- Several complete Play Mode matches and representative aspect ratios are user-verified, but a packaged player build and targeted edge-case matrix remain.
- Three-star melee is a reported near-deterministic strategy across all difficulties, while mythics appear too weak for their cost.

---

## Milestone 1 — Prototype

**Goal:** Prove that short single-player matches create meaningful economy, production, composition, and upgrade decisions without direct unit control.

### Existing foundation

- [x] Autonomous horizontal movement, target acquisition, melee/ranged combat, projectiles, health, and unit death are represented in source.
- [x] Two strongholds and base-destruction victory handling are represented in source and the battle scene.
- [x] Melee, archer, cavalry, siege, and mythic combat classifications and prototype assets exist; the supported factions currently map their four standard entries in melee/archer/cavalry/siege order and use the separate mythic roster.
- [x] Workers can mine and deposit gold; the player HUD displays live worker economy state and can buy workers up to capacity.
- [x] Faction data can map roles to faction-specific prefabs.
- [x] Black, blue, purple, red, and yellow prototype faction assets map colour-specific animated worker, melee, and archer prefabs while sharing cavalry, siege, and mythic.
- [x] The five prototype colour factions own matching Castle and House3 presentation references consumed by one shared Base prefab and the stronghold HUD; the user exhaustively verified the faction-colour visual combinations in Play Mode.
- [x] Prototype health, hit, animation, projectile, and victory feedback exists.

These checks confirm repository presence only. Their integrated runtime behavior remains subject to compilation and Play Mode verification.

### Planned outcomes

- [x] Accept the recurring three-purchase lifecycle originally recorded in `DEC-007` and carried forward per standard slot and separate mythic track by `DEC-018`.
- [ ] Complete representative Play Mode verification of the source-integrated player gold, worker, four ordered standard-slot, duplicate-role, and separate mythic production paths. The earlier unique-role path is verified, but the accepted slot-keyed migration remains.
- [x] Implement an AI decision layer that participates in the same economy and production game, with three Play Mode-verified difficulties, same-rule purchasing, and random mythic/opponent selection.
- [x] Complete targeted verification of the implemented five-minute timer, stronghold-health comparison, unit-loss-value tiebreaker, result states, and restart/reset flow.
- [ ] Correct the dominant three-star melee strategy and establish credible mythic value through controlled balance diagnosis and tuning.
- [ ] Preserve cost and recurring cadence ownership in each configured unit's `UnitData` while moving runtime unlock, tier, timer, and purchase identity from combat role to ordered standard slot. The earlier role-keyed values and spawn patterns were user-verified on 2026-07-16; duplicate-role slots and the global debug pattern require regression.
- [x] Validate the functional HUD at representative resolutions and across the complete critical path in Play Mode.
- [ ] Add a minimal in-game menu with confirmed pause and navigation behavior.
- [x] Add a player-facing title -> faction selection -> difficulty selection -> battle flow, verified in Play Mode; player-build coverage remains tracked separately.
- [x] Validate role readability and at least one meaningful economy-versus-pressure decision through playtesting; balance defects remain tracked separately.
- [ ] Diagnose and retune the implemented role counters, mythic roster, star curve, and roster/base values across equal-gold, equal-time, mixed-composition, and tiered matchups.
- [ ] Add focused tests for deterministic economy, production, and result rules; representative Play Mode critical-path verification is complete.
- [x] Deliver the accepted selectable mythic test roster in sequence: five colour monk healers, reusable animated Enemy Pack melee/ranged prefabs, then a pre-purchase details-pane picker with per-match selection and data-owned balance.
- [ ] Review the broad initial mythic roster after animation, balance, and counter-selection testing; do not let this expansion displace the AI and complete-match critical path.

### Dependencies

- Accepted design constraints in `DEC-002` through `DEC-006`.
- Recurring three-purchase semantics inherited from superseded `DEC-007`.
- Accepted ordered standard-roster semantics in `DEC-018`, which supersede `DEC-007` where production identity was keyed by combat role.
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
- [x] Add animated title-screen background presentation using buildings and non-interactive units; this was pulled forward and completed during the Prototype milestone.
- [ ] Representative audio and feedback
- [x] Pull forward an initial prototype SFX layer for UI, economy, production, combat, healing, strongholds, and match results; representative final mixing, controls, and Play Mode review remain part of this milestone's audio outcome.
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
- 2026-07-17: Accepted a test-oriented selectable mythic roster. Sequence monk healing and reusable animated prefabs before the HUD picker; AI and the complete match loop remain the milestone's primary blockers.
