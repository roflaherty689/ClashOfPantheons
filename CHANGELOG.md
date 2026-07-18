# Changelog

This file records meaningful project changes in a human-readable format.

Git history remains the exact implementation record.

## Unreleased

### Added

- A guarded Unity Editor migration for removing the isolated legacy battle `Canvas/Canvas/VictoryText` hierarchy and normalizing the active HUD root scale.
- A guarded Unity Editor migration for converting the mirrored battle gold nodes to a shared prefab while retaining side transforms, internal mine points, and worker-economy references.
- User-verified both worker loops after the battle gold nodes were converted to shared prefab instances.
- Role-aware unit damage matchups, with a 1.2x first-pass counter triangle: melee beats cavalry, archers beat melee, and cavalry beats archers.

- A three-difficulty enemy AI that buys workers, production, upgrades, and a random mythic through the same economy and production rules as the player.
- A post-faction difficulty screen defaulting to Easy, with enemy starting-gold bonuses of +0/+50/+150 for Easy/Medium/Hard.
- Random opponent-faction selection that excludes the player's chosen faction to prevent colour conflicts.

- A pre-purchase mythic picker in the right-side role details pane, backed by a build-safe roster containing all five monks, Minotaur, and all 15 qualifying Enemy Pack units.
- Fifteen animation-complete Enemy Pack mythic prefabs with independent unit data, plus dedicated bone, harpoon, and shaman projectile prefabs for the ranged roster.
- Phase 1 monk-healer runtime support for lowest-health allied combat targeting, tier-scaled healing on a fixed cadence, clamped health restoration, and recipient-positioned heal effects, plus an Editor builder for all five colour variants.
- A decorative title-screen background layer with Tiny Swords green terrain, buildings, independently animated five-colour melee/archer units, expanded six-unit patrol groups in each side margin, and a smaller number of untinted minotaurs behind an inset wood panel.
- Phase 1 title-menu runtime behavior and an Editor builder for a responsive Play/Exit title scene, Phase 2 placeholder, Input System UI, and deterministic Build Settings ordering.
- A build-safe faction catalog, generated faction-selection buttons, and a scene-transition handoff that applies the chosen player faction before battle presentation initializes.
- Initial project-memory documents for design, roadmap, tasks, decisions, and changelog maintenance.
- Accepted decision records for the core player role, prototype match rules, gold economy, independent role production, three-star upgrades, scope boundary, and Tiny Swords visual direction.
- An accepted purchase-to-recurring-production contract for independent role tracks and three star tiers.
- An accepted PC-first, mobile-second platform and input priority decision.
- A game-specific ordered Prototype task sequence with explicit acceptance and verification criteria.
- Per-role production cadence fields on `UnitData`, with initial melee, archer, cavalry, siege, and mythic timing values.
- A selectable `GameManager` spawn pattern for legacy global spawning or independent per-role timers.
- Live player-economy HUD bindings for gold, aggregate gold per trip, worker count, worker purchasing, and worker-cap button state.
- Live player and enemy stronghold-health HUD bindings for current/maximum text and proportional fill.
- A configurable five-minute match countdown with stronghold-health, lost-unit-value, and exact-draw timeout resolution.
- Per-team and per-role unit-death counters, with destroyed production-slot costs accumulated for timeout tiebreaks.
- Live result reasons and active-scene restart through the battle HUD.
- Team-scoped unit-production purchasing with locked initial states, gold-funded unlocks, independent recurring timers, and three star tiers.
- Live unlock, upgrade, maximum-tier, affordability, and locked-icon presentation for all five player production cards and the selected-role panel.
- Black, blue, purple, red, and yellow prototype faction assets with matching animated melee and archer prefabs; cavalry, siege, and mythic remain shared.
- Matching Castle and House3 presentation references for all five prototype colour factions, applied to world buildings and stronghold HUD icons from the selected faction.
- Black, blue, purple, red, and yellow animated worker prefabs owned by their matching faction assets.

### Changed

- Reconciled project tracking with the source-integrated critical loop, AI, selectable mythic roster, HUD, timer/results, and restart implementation while preserving outstanding verification work.
- Audited scene and prefab structure and recorded focused follow-up work for the duplicate Canvas hierarchy, reusable production-card/panel prefabs, mirrored gold-vein presentation, and third-party asset boundaries.

- Expanded the animated title background to include every selectable mythic at least once: all 16 qualifying Enemy Pack creatures and all five colour monks, independently of the melee/archer decoration mix.

- Mythic choices in the production picker are now ordered by ascending gold cost, with alphabetical ordering for equal-cost options.

- Differentiated the mythic test roster by creature identity, cost, production cadence, health, damage, and movement. Light creatures now arrive faster at lower power and cost, while Minotaur and Bear are premium bruisers and Troll is the strongest, slowest-producing, uniquely highest-cost option; all monk colours remain mechanically identical.

- Reduced star-tier stat scaling from 1x/1.5x/2x to 1x/1.25x/1.5x, lowering compounded effective DPS from 1x/2.25x/4x to 1x/1.5625x/2.25x while preserving future-spawn-only upgrades.

- Rebalanced the full prototype roster around a 100-health melee baseline, differentiated siege and mythic roles, and a 1,000-health stronghold; values remain subject to Play Mode tuning.

- Medium and Hard AI now gate early worker purchases behind military production, with Hard requiring a broader opening composition to resist rushes.

- Completed and accepted the Phase 2 animated Enemy Pack mythic roster and Phase 3 pre-purchase mythic picker after combined Play Mode review.
- Mythic-picker choices now show their configured avatar beside the unit name and cost, making roster-art review explicit.
- Standard Tiny Swords archers now fire the pack's matching arrow artwork; mythic slots begin with crossed swords and switch both slot and details imagery to the selected unit's matching Enemy Avatar or colour-specific Human Avatar.
- Simplified the mythic picker by removing its redundant heading and back button; leaving the picker remains available by selecting another production role.
- Mythic selection and the initial unlock purchase now occur atomically; later purchases upgrade the locked-in unit, whose own cost, cadence, prefab, and data remain authoritative until the match resets.
- Normalized the new Enemy Pack mythic Animator Controllers around Idle, Run/Walk, and one-shot Attack states; Troll uses its Attack clip while Boat, guard, windup, recovery, and breaking-club animations remain excluded.
- Added optional projectile trajectory rotation so animated projectiles such as the shaman orb can retain their authored orientation without changing existing projectile defaults.
- Restyled the title and faction-selection screens with supplied Tiny Swords wood-table, paper, banner, slot-panel, and blue-button UI sprites.
- Moved the Default and two meme-team faction assets into `Factions/NonMenu`; title-menu catalog generation now exposes only factions stored directly in the main Factions folder.
- The selected-role details now follow the last production card hovered over or interacted with, remain pinned after the pointer leaves, show that role's unit artwork at twice the previous icon size instead of a generic sword icon, and omit the redundant production/independent row.
- Redesigned the editor-generated battle HUD around Tiny Swords panels, bars, buttons, icons, workers, strongholds, and human/enemy avatar portraits; replaced conflicting multi-currency and shared-queue mockups with binding-ready gold, worker, five-role independent-production, match-status, and result presentation.
- Reconciled `GAME_DESIGN.md` with the current Unity repository and the user-approved single-player autobattler direction.
- Rewrote the Prototype milestone around one economy → production → autonomous combat → result → restart loop against AI.
- Corrected project tracking to distinguish existing source/asset foundations from unimplemented integration and unverified runtime behavior.
- Deferred multiplayer, campaign and persistent progression, multiple currencies, controller support, final art, and broad content beyond the core prototype.
- Renamed the animated default faction and its black melee/archer prefabs to explicit colour-based names while preserving their Unity GUID references.
- Accepted the independent production contract: first purchase unlocks continuous one-star production, while later purchases upgrade future spawns to two and three stars.
- Recorded initial 1.2× counter and 1×/1.5×/2× star-scaling defaults, timeout loss valuation, and future-spawn-only upgrade behavior.
- Clarified that counters modify damage, star tiers scale every configured unit stat except cost, and exact timeout equality produces a draw.
- Implemented per-role cadence ownership in `UnitData` while retaining the global interval as an explicit prototype/debug option.
- Renamed the misleading per-team unit-cap field while preserving the scene's configured 60-unit value and serialized compatibility.
- Refactored global spawn selection to share one role list, derive weighted-random spawning from `UnitData` costs, advance fixed cycles only after successful spawns, and validate faction/team configuration once at startup.
- Encapsulated serialized faction and unit configuration behind read-only runtime properties while preserving existing Unity field names and asset data.
- Consolidated ranged recoil feedback in `BaseUnit`, preserving authored visual scale and preventing animation code from stopping unrelated combat coroutines.
- Changed match-result reporting from colour strings to typed left/right team values and moved result presentation into `GameManager`.
- Moved worker gold-rate ownership to `WorkerManager`, with configurable base income and upgrade multiplier, and changed the active scene to start the player with one worker.
- Set the current prototype starting gold to 200 to support economy-flow testing.
- Changed both teams to begin with every unit role locked; enemy production remains inactive until the planned AI purchasing layer is implemented.
- Applied star upgrades as per-unit stat snapshots so future spawns receive 1×/1.5×/2× stats without changing fielded units or shared `UnitData` assets.
- Consolidated each team's worker hand-in visual into the shared Base prefab and made the active scene default to Black versus Red.
- Made the selected faction's worker prefab authoritative before initial workers spawn while retaining scene-authored fallbacks for legacy factions.

### Fixed

- The battle-HUD builder now explicitly initializes its root RectTransform to unit scale, preventing regenerated scene UI from retaining a serialized zero scale.
- Aligned both battle economies and the shared Base prefab to 200 starting gold, so AI difficulty bonuses now produce enemy starts of 200, 250, and 350.

- Restored parchment behind the locked crossed-swords mythic art, removed the empty-image grey fill, and mirrored that locked presentation in the hovered role-details panel.
- Corrected the Minotaur, Lizard, Shaman, Thief, Troll, and Turtle portrait mapping, and stopped shared monk `UnitData` from collapsing every colour variant onto one avatar.
- Corrected mythic portrait and crossed-swords sprite serialization so the HUD no longer retains one authored Enemy Avatar for every selection or while the slot is locked.
- Replaced the faction selector's clipped Scroll View with a directly clickable two-column grid that displays all configured faction options, with each castle-and-label pair centered inside its button.
- Matched the blue, purple, red, and yellow Warrior/Archer clip behavior to black: Idle and Run loop continuously while attacks remain one-shot.
- Production purchase buttons now repair disabled target-graphic raycasts at runtime, allowing the EventSystem to receive clicks from the existing generated scene.
- Production tier counters now resolve from their card structure instead of fragile initial text-object names, keeping the displayed star count synchronized with successful upgrades.
- Generated HUD buttons now accept pointer input by enabling raycasts on their target graphics; the live worker binding also repairs the already-generated Buy Worker button at runtime.
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
- Removed duplicate world-space health bars from bases and their obsolete serialized prefab/scene configuration; unit health bars and their shared assets remain in use.

### Technical

- Replaced runtime production-card hierarchy-name lookups with serialized `ProductionCardView` bindings, updated the HUD builder to author them, and added a guarded migration for the five existing scene cards.
- Extracted global and per-role spawn selection, capacity checks, prefab resolution, weighted choice, and unit instantiation from `GameManager` into `UnitSpawnController`; idle production frames now avoid scene-wide capacity scans until at least one cadence is ready.
- Extracted economy, worker, stronghold, countdown, result-overlay, restart, and listener lifecycle presentation from `BattleEconomyUI` into `BattleHudReadoutPresenter`.
- Extracted title-menu faction-option generation and binding into a focused presenter, and changed enemy AI startup to receive its manager/economy dependencies from the initialized faction boundary while retaining a legacy-scene fallback.
- User-verified the final runtime responsibility batch in Play Mode, covering the extracted spawning, battle readouts, menu faction options, and injected AI initialization behavior.
- Extracted session faction overrides, base discovery, faction-driven base/worker presentation, and castle HUD icons from `GameManager` into `FactionTeamInitializer`; the user verified authored defaults, menu selections, both teams' presentation/economy, result handling, restart, and fallback diagnostics in Play Mode.
- Extracted the match clock, terminal result state, per-team/per-role death counts, and total lost-unit values from `GameManager` into `MatchStateController`; the user verified base destruction, all timeout outcomes, post-game loss immutability, system stopping, result presentation, and restart reset in Play Mode.
- Moved locked-timer resets, cadence advancement, interval clamping, ready-role scanning, slot consumption, spawn-timer consumption, and fairness rotation into `ProductionStateController`; the user verified player/AI cadence, simultaneous readiness, cap-resume, failure handling, match-end, and restart behavior in Play Mode.
- Extracted both teams' production tiers, selected mythics, per-role timers, ready-role indices, and role mapping from `GameManager` into `ProductionStateController`; the user verified unlock, upgrade, cadence, mythic, AI, cap-resume, match-end, and restart behavior in Play Mode.
- Consolidated mythic avatars, fallback sprites, display names, parchment, crossed-swords construction, visibility, and tinting into one shared artwork presenter; the user verified card, selected-role, picker, and restart presentation in Play Mode.
- Extracted selected-role icon, title, status, tier, description, affordability, action text, and action-button ownership from `BattleEconomyUI`; the user verified the complete selected-role interaction in Play Mode.
- Extracted the five production cards' bindings, hover/tap routing, affordability, tier/action text, mythic artwork, purchase listeners, and cleanup from `BattleEconomyUI`; the user verified the complete card behavior in Play Mode.
- Extracted runtime mythic-picker construction, ordering, affordability, purchase routing, and cleanup from `BattleEconomyUI` into a focused controller; the user verified the complete picker interaction in Play Mode.
- Moved battle countdown formatting into the tested Core presentation rules; all 38 Core Edit Mode cases passed in Unity, including clamping and minute-boundary behavior.
- Centralized the accepted 1x/1.25x/1.5x star-tier stat curve in the Core production rules; all 32 Core Edit Mode cases passed in Unity.
- Extracted production-tier transition rules into the Core assembly; all 26 Core Edit Mode cases passed in Unity, including normal progression, the tier cap, and selection-based mythic unlock behavior.
- Extracted player-relative match-result title and reason formatting from the battle HUD into the Core assembly; all 15 Core Edit Mode cases passed in Unity.
- Began the maintainability refactor by extracting deterministic timeout comparisons into an engine-independent Core assembly; all five Edit Mode characterization cases for health, lost-unit-value, and exact-draw outcomes passed in Unity.
- Corrected the battle-HUD builder completion message so it reflects the implemented runtime bindings and required Play Mode layout verification.

- User-verified the three difficulty selections, distinct enemy faction assignment, corrected starting resources, AI purchasing behavior, and rush-resistant Medium/Hard opening priorities in Play Mode.

- Added persistent Codex project instructions through `AGENTS.md`.
- Documented that no project-owned automated tests or test assemblies were found during the 2026-07-16 static review.
- User-verified the selectable global and independent per-role spawn patterns in Play Mode.
- Completed a coordinated audit of all project-owned runtime and Editor C#; removed the wholly commented, unreferenced `ProductionSlot` script and its metadata.
- Updated deprecated Unity/TMP API usage found during the pass.
- Compiled both runtime and Editor assemblies externally with zero warnings and errors after the cleanup.
- User-verified the cleanup pass in Unity and confirmed the current gameplay behavior is working correctly.
- Resaved the active scene after removing the stray `GoldVein` component from `Tilemap_Water`.
- User-verified live worker deposits, worker purchasing, gold and worker HUD updates, and five-worker capacity behavior in Play Mode.
- User-verified live stronghold-health text and proportional HUD fills, fixed UI fill colours, and removal of duplicate world-space base health bars in Play Mode.
- Compiled runtime and Editor assemblies externally with zero warnings and errors after the match-flow integration.
- User-verified countdown expiry and correct lost-gold timeout resolution in a shortened 20-second Play Mode match.
- User-verified clickable production controls, melee unlock/upgrades through the three-purchase cap, and live tier-counter updates in Play Mode.
- User-verified the selected-role panel's sticky behavior, role artwork, simplified details, and enlarged icon in Play Mode.
- Configured the previously empty non-black Tiny Swords melee and archer Animator Controllers with the working `isMoving` and `Attack` state-machine behavior and their colour-matched clips.
- Configured all non-black pawn Animator Controllers to mirror black's worker state machine and enabled black-matching loops across all pawn clips.
- User-verified matching idle, run, and attack behavior for the black, blue, purple, red, and yellow melee/archer variants in Play Mode after their loop and transition settings were aligned.
- Updated the battle-HUD builder to create faction-neutral left/right castle-icon bindings and reconnect regenerated icons to `GameManager`.
- Compiled the faction-building runtime and Editor changes externally with zero warnings and errors.
- Compiled the faction-owned worker runtime and Editor changes externally with zero warnings and errors.
- User-verified that the faction-owned worker variants import and work correctly in Play Mode.

### Known Issues

- Recurring production cadence across all five roles, insufficient-funds behavior, revised tier scaling/counters, and restart reset require broader Play Mode verification. Representative AI purchasing, all three difficulty selections and bonuses, and opening behavior were already user-verified.
- Health-tiebreak, exact-draw, base-destruction result-overlay, and restart-reset paths still require targeted Play Mode verification.
- Exact role values and matchups remain subject to implementation and playtest tuning.
- Targeted projectile/worker edge cases and stale serialized fields on the non-animated archer prefab remain to be verified or cleaned.
- Shared cavalry, siege, and mythic spawning and both-team faction switching still require Play Mode verification across the five team-colour faction assets.
- Faction-driven Castle, House3 hand-in, HUD icon, worker-deposit, base-damage, and restart behavior still require complete Play Mode verification across both team sides.

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
