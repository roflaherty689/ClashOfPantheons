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
- Refactored global spawn selection to share one role list, derive weighted-random spawning from `UnitData` costs, advance fixed cycles only after successful spawns, and validate faction/team configuration once at startup.
- Encapsulated serialized faction and unit configuration behind read-only runtime properties while preserving existing Unity field names and asset data.
- Consolidated ranged recoil feedback in `BaseUnit`, preserving authored visual scale and preventing animation code from stopping unrelated combat coroutines.
- Changed match-result reporting from colour strings to typed left/right team values and moved result presentation into `GameManager`.

### Fixed

- Worker purchases no longer spend gold or report success when worker creation fails, and negative purchase costs are rejected.
- Mining slots now have exclusive reservations; waiting workers retry when a slot becomes available, and disable/game-over cleanup updates reservation and active-miner state exactly once.
- Workers unregister when destroyed, preventing stale worker counts from blocking later purchases.
- Destroyed projectile targets are now checked with Unity object semantics before dereferencing or applying damage.
- Projectile cleanup now respects configured travel times longer than two seconds.
- Lethal base/unit damage is clamped and idempotent, preventing repeated death or match-result handling.
- Health bars tolerate missing presentation references and invalid maximum-health input without division errors.
- The rough battle UI builder now creates the project's configured Input System UI module rather than the legacy standalone module.

### Removed

- Removed the wholly commented, unreferenced `ProductionSlot` script and its metadata.

### Technical

- Added persistent Codex project instructions through `AGENTS.md`.
- Documented that no project-owned automated tests or test assemblies were found during the 2026-07-16 static review.
- User-verified the selectable global and independent per-role spawn patterns in Play Mode.
- Completed a coordinated audit of all project-owned runtime and Editor C#; removed the wholly commented, unreferenced `ProductionSlot` script and its metadata.
- Updated deprecated Unity/TMP API usage found during the pass.
- Compiled both runtime and Editor assemblies externally with zero warnings and errors after the cleanup.
- User-verified the cleanup pass in Unity and confirmed the current gameplay behavior is working correctly.
- Resaved the active scene after removing the stray `GoldVein` component from `Tilemap_Water`.

### Known Issues

- Player gold, worker purchasing, independent unit production, upgrades, strategic AI, timer/tiebreak results, restart, and live HUD integration are not yet complete.
- Independent cadence spawning currently starts all five role timers immediately; purchase-slot activation remains future work.
- Exact role values and matchups remain subject to implementation and playtest tuning.
- Targeted projectile/worker edge cases and stale serialized fields on the non-animated archer prefab remain to be verified or cleaned.

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
