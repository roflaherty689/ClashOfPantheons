# Changelog

This file records meaningful project changes in a human-readable format.

Git history remains the exact implementation record.

## Unreleased

### Added

- Initial project-memory documents for design, roadmap, tasks, decisions, and changelog maintenance.
- Accepted decision records for the core player role, prototype match rules, gold economy, independent role production, three-star upgrades, scope boundary, and Tiny Swords visual direction.
- An accepted purchase-to-recurring-production contract for independent role tracks and three star tiers.
- An accepted PC-first, mobile-second platform and input priority decision.
- A game-specific ordered Prototype task sequence with explicit acceptance and verification criteria.
- Per-role production cadence fields on `UnitData`, with initial melee, archer, cavalry, siege, and mythic timing values.
- A selectable `GameManager` spawn pattern for legacy global spawning or independent per-role timers.

### Changed

- Reconciled `GAME_DESIGN.md` with the current Unity repository and the user-approved single-player autobattler direction.
- Rewrote the Prototype milestone around one economy → production → autonomous combat → result → restart loop against AI.
- Corrected project tracking to distinguish existing source/asset foundations from unimplemented integration and unverified runtime behavior.
- Deferred multiplayer, campaign and persistent progression, multiple currencies, controller support, final art, and broad content beyond the core prototype.
- Accepted the independent production contract: first purchase unlocks continuous one-star production, while later purchases upgrade future spawns to two and three stars.
- Recorded initial 1.2× counter and 1×/1.5×/2× star-scaling defaults, timeout loss valuation, and future-spawn-only upgrade behavior.
- Clarified that counters modify damage, star tiers scale every configured unit stat except cost, and exact timeout equality produces a draw.
- Implemented per-role cadence ownership in `UnitData` while retaining the global interval as an explicit prototype/debug option.
- Renamed the misleading per-team unit-cap field while preserving the scene's configured 60-unit value and serialized compatibility.

### Fixed

- _None._

### Removed

- _None._

### Technical

- Added persistent Codex project instructions through `AGENTS.md`.
- Documented that no project-owned automated tests or test assemblies were found during the 2026-07-16 static review.
- User-verified the selectable global and independent per-role spawn patterns in Play Mode.

### Known Issues

- Player gold, worker purchasing, independent unit production, upgrades, strategic AI, timer/tiebreak results, restart, and live HUD integration are not yet complete.
- Independent cadence spawning currently starts all five role timers immediately; purchase-slot activation remains future work.
- Exact role values and matchups remain subject to implementation and playtest tuning.
- Compilation and Play Mode behavior were not verified during the documentation-only reconciliation pass.

---

## Version template

Copy this section when creating a release entry.

## [Version] — YYYY-MM-DD

### Added

- _None._

### Changed

- _None._

### Fixed

- _None._

### Removed

- _None._

### Technical

- _None._

### Known Issues

- _None._
