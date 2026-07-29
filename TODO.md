# TODO

## How to use this file

- Keep immediate work here and long-term outcomes in `ROADMAP.md`.
- Complete items only after their acceptance criteria and applicable validation are satisfied.
- Preserve manual Unity setup and Play Mode work as real task scope.

---

## P0 — Blockers

_No confirmed implementation blocker._

---

## P1 — Critical next work

Tasks are ordered by dependency and prototype value.

### 0. Validate the project-owned C# correctness and maintainability pass

**Outcome:** The cleaned combat, spawning, worker, resource, health, and Editor-tooling foundations are confirmed safe before further prototype integration builds on them.

**Acceptance criteria:**

- [x] All project-owned C# receives a coordinated architecture, correctness, lifecycle, naming, and maintainability audit.
- [x] Runtime and Editor assemblies compile externally with zero warnings and errors against the current Unity references; the stale generated entry for the deleted script was excluded only for this check.
- [x] Dead `ProductionSlot` source and its unreferenced `.meta` file are removed together.
- [x] Unity imports the changes with no missing-script or serialization errors and refreshes its generated project files.
- [x] The cleanup pass receives a representative Play Mode smoke check covering the active spawning, combat, base, and worker loop.
- [ ] Projectile attacks are verified against destroyed targets and with travel times over two seconds.
- [ ] Worker slot contention, disable-during-mining cleanup, failed purchases, and the worker cap are verified.
- [ ] The duplicate/stale serialized fields on unit prefabs are cleaned by opening and resaving them in Unity; the non-animated archer retains a valid `visualTransform`.
- [x] The stray `GoldVein` component on `Tilemap_Water` is removed in Unity and the scene contains only the intended left and right veins.

**Progress:** The code audit, corrections, refactors, dead-source removal, static reference checks, and external runtime/Editor compilation are complete. The user verified the current behavior in Unity on 2026-07-16, and the scene now contains only the two intended gold veins. Targeted projectile/worker edge cases and stale prefab serialization cleanup remain.

**Status:** Partially implemented

### 1. Integrate player gold, workers, and independent slot production

**Outcome:** The player can make the prototype's core economy-versus-pressure decision with gold.

**Acceptance criteria:**

- [ ] Player gold is the authoritative currency for worker purchases, all four ordered standard-production slots, the separate mythic track, and their upgrades. The earlier role-keyed path is verified; the slot-keyed migration remains.
- [x] Buying a worker uses the existing worker limit and updates visible gold/worker state.
- [ ] Every standard slot follows the approved independent recurring-production contract without a shared FIFO queue; duplicate combat roles do not share unlock, tier, timer, cost, cadence, or purchase state.
- [ ] Each standard slot begins locked; its first purchase unlocks continuous one-star production and its next two purchases upgrade only future spawns from that slot to two and three stars. Mythic follows the same tier lifecycle on a separate picker-backed track.
- [x] Favourable matchups apply a 1.2× damage multiplier.
- [x] Star tiers multiply every configured unit stat except purchase cost by 1×/1.25×/1.5× and affect future spawns only.
- [ ] Each configured unit's `UnitData` remains authoritative for its slot's purchase cost and recurring production cadence; the global spawn interval does not collapse duplicate-role slots.
- [x] Production stops correctly when the match ends and resets correctly for a new match in representative Play Mode verification.
- [x] Costs, failed purchases, and production state are visibly communicated in representative Play Mode verification.
- [ ] Deterministic economy and production rules have focused automated tests where practical.
- [ ] The slot-keyed implementation compiles and representative duplicate-role Play Mode paths are verified. Earlier role-keyed compilation and paths passed.

**Relevant systems:** `WorkerManager`, `GameManager`, `ProductionStateController`, `UnitSpawnController`, `UnitData`, faction data, AI, battle UI, scene wiring

**Dependencies:** `DEC-004` and `DEC-018`; initial values require balance tuning during implementation

**Progress:** The earlier role-keyed implementation uses `UnitData` cost/cadence, atomic spending, fresh unlock timers, future-spawn tier snapshots, and a three-purchase cap. Its production cards, selected-role panel, cadence, simultaneous readiness, cap resume, failures, match-end stopping, restart, and complete matches received representative verification. Migration to four ordered slot identities with duplicate-role support is in progress and is not covered by those earlier checks. Existing Core tests cover tier transitions/scaling, but focused slot-keyed economy transactions and production scheduling tests remain. Balance is not accepted: the user reports three-star melee wins roughly 99% across all difficulties and mythics are too weak for their cost.

**Status:** Partially implemented

### 2. Implement the single-player AI economy and production opponent

**Outcome:** The opposing side makes strategic worker, production, composition, and upgrade decisions under the same rules as the player.

**Acceptance criteria:**

- [x] AI purchases consume its gold and respect the same costs, limits, and production rules.
- [ ] AI can buy workers, purchase and upgrade all four standard slots independently even when roles repeat, and use the separate mythic track.
- [x] A simple documented strategy creates credible pressure within a five-minute match.
- [x] AI stops on match end and resets on restart.
- [x] AI decisions are observable enough to debug and tune.
- [ ] Slot-keyed compilation and representative duplicate-role Play Mode matches are verified. Earlier role-keyed AI matches passed.

**Relevant systems:** future AI policy, economy/production interfaces, match state

**Dependencies:** Task 1

**Progress:** The earlier role-keyed `EnemyAIController` uses the shared worker, production, tier, and atomic mythic-purchase APIs. Easy/Medium/Hard change cadence and policy quality; enemy-only bonuses produce 200/250/350 initial totals. Medium and Hard gate early workers behind military production to resist rushes. The user verified that integrated flow in Play Mode on 2026-07-17. Slot-keyed duplicate-role purchasing and upgrades still require migration and regression.

**Status:** Partially implemented

### 3. Complete match timing, result resolution, and restart

**Outcome:** Every match reaches a deterministic result and can be replayed without Editor intervention.

**Acceptance criteria:**

- [x] Match duration is configured around the five-minute target.
- [x] Stronghold destruction ends the match immediately.
- [x] Timeout compares stronghold health, then lower total value of lost units when health is equal.
- [x] Unit losses and their approved values are tracked per side.
- [x] Lost value uses production-slot purchase cost multiplied by the number of that unit type destroyed.
- [x] Exact equality after both timeout comparisons produces a draw with no winner or loser.
- [x] Result UI identifies the winner and resolution reason.
- [x] Restart resets time, gold, workers, production, upgrades, units, AI, strongholds, and UI in representative Play Mode verification.
- [x] Base-destruction, health-tiebreak, value-tiebreak, and exact-equality behavior have tests or explicit verification cases.
- [x] Compilation and representative full Play Mode result/restart paths are verified.

**Relevant systems:** `GameManager`, `Base`, `BaseUnit`, economy/production, result UI

**Dependencies:** Tasks 1–2

**Progress:** `MatchStateController` owns the configurable countdown, terminal state, death counts, and lost-unit values. Core Edit Mode cases cover both health winners, both lost-value winners, exact draws, and result/countdown presentation. Commit `26db1b4` records user verification of base destruction, every timeout outcome, post-game immutability, system stopping, result presentation, and restart reset. The user subsequently completed several clean end-to-end matches through result and restart. Additional automation for controller integration remains follow-up coverage rather than a blocker to representative loop validation.

**Status:** Complete

### 4. Replace the static HUD with a functional prototype HUD and validate the loop

**Outcome:** A new player can read and complete the entire prototype loop against AI.

**Acceptance criteria:**

- [ ] HUD shows live gold, workers, four ordered standard-slot states plus a separate mythic state, per-track star tiers, timer, both stronghold health values, and results. The earlier five-role presentation is verified; slot-derived duplicate-role presentation remains.
- [x] Favour, essence, and shared FIFO queue presentation are removed or clearly excluded from the functional prototype UI.
- [ ] Player purchase and upgrade controls provide success, failure, affordability, and cooldown/cadence feedback.
- [x] Critical state does not rely on red/blue colour alone and text is readable at the chosen prototype resolutions.
- [x] At least one complete economy → production → combat → result → restart match is verified in Play Mode.
- [x] Playtesting confirms meaningful strategic choices and identifies the dominant three-star melee and weak-mythic balance follow-up separately.

**Relevant systems:** battle HUD, UI bindings, safe area, all critical-loop systems

**Dependencies:** Tasks 1–3

**Progress:** The editor-authored Tiny Swords battle HUD binds gold, workers, both strongholds, timer, results, restart, and five production cards. Focused verification covers the earlier unique-role presenters and interactions, and the user has completed several clean end-to-end Play Mode matches. The accepted model reinterprets the first four cards as ordered standard slots and keeps mythic as the separate fifth track; duplicate-role labels, art, purchase routing, and independent states remain to be verified. The attempted three-part stronghold health-bar frame remains deferred pending a later UI pass.

**Status:** Partially implemented

### 5. Build the initial title and faction-selection flow

**Outcome:** A player can launch the game into a clear title screen, choose a configured faction, and enter the existing battle with that faction applied.

**Phased acceptance criteria:**

- [x] **Phase 1 — Title:** A dedicated title scene is first in Build Settings and presents exactly two primary clickable actions: Play and Exit.
- [x] Play opens the faction-selection view or scene without starting battle simulation.
- [x] Exit calls the platform quit path in a player build and provides a safe, testable Editor behavior.
- [x] **Phase 2 — Faction selection:** Clickable faction options are generated from a serialized, build-safe catalog/list of valid `FactionData` assets; adding a configured faction does not require hand-authoring another button.
- [x] Each option displays at least `FactionData.FactionName`, rejects or clearly diagnoses null/invalid entries, and has an unambiguous selected/clickable state.
- [x] Choosing a faction carries that exact asset into the battle scene and applies it to the player team before faction presentation, workers, or units initialize.
- [x] Match setup selects a different configured opponent faction so the two teams cannot conflict in colour.
- [x] Returning to or restarting a battle does not leave stale duplicate menu/session objects.
- [x] **Phase 3 — Animated background:** The title screen includes decorative Tiny Swords-style buildings and non-interactive units moving across the background without invoking combat, economy, or match systems.
- [x] Background animation is layered behind interactive UI, does not intercept button input, and remains readable and performant at the prototype's representative PC resolutions.
- [x] Menu scripts compile, both scenes are present and ordered correctly in Build Settings, and the title -> selection -> battle path is verified in Play Mode.
- [ ] The same flow, including Exit, is verified in a packaged player build.

**Relevant systems:** new menu scene/UI and controller, `FactionData`, a serialized faction catalog/menu configuration, scene-loading/session-selection boundary, `GameManager`, Build Settings

**Dependencies:** Existing faction assets and faction-driven presentation. Phase 3 depends on Phases 1–2 but the functional menu flow does not depend on the animated background.

**Risks and manual Unity work:** Runtime builds cannot use `AssetDatabase` to discover ScriptableObjects, so available factions must be explicitly serialized or supplied through another build-safe content mechanism. Scene creation, Canvas layout, button references, Build Settings ordering, faction catalog contents, multi-resolution inspection, and Play Mode/player-build verification require Unity Editor validation.

**Progress:** Coordinated and accepted in `DEC-012` and extended by `DEC-014`. The user verified the title, faction selection, Tiny Swords animated presentation, difficulty selection, distinct opponent faction, and transition into battle in Play Mode on 2026-07-17, and later accepted the implemented front-end menus as readable across the tested aspect ratios. Difficulty choices launch battle directly from a parchment-contained Tiny Swords menu. The title background includes all 21 selectable mythic actors. Player-build verification remains pending, so the parent task remains partially implemented.

**Status:** Partially implemented

### 6. Diagnose and retune combat economy

**Outcome:** Upgrades, standard roles, and mythics support meaningful composition choices without one near-deterministic purchase path.

**Acceptance criteria:**

- [ ] Reproduce the reported three-star melee strategy across difficulties while recording faction, purchase order, upgrade timing, composition, duration, outcome, and remaining base health.
- [ ] Separate upgrade-curve strength from AI-policy weakness.
- [ ] Compare equal-gold and equal-time throughput for melee tiers, counter roles, siege, and representative light/heavy/ranged/support mythics.
- [ ] Validate base pressure, counter effectiveness, ranged safety, monk stacking, and mythic opportunity cost before changing values.
- [ ] After tuning, one three-star melee track is no longer near-deterministic across all difficulties and representative mythics offer a credible purchase reason.
- [ ] Approved numeric changes amend or supersede `DEC-015`.

**Progress:** The user reports that three-star melee wins roughly 99% across Easy, Medium, and Hard and that mythics are too weak relative to their cost. No replacement values are approved yet.

**Status:** Not started

### 7. Add a minimal in-game menu

**Outcome:** A player can interrupt and leave an active solo match without closing the application or waiting for the result screen.

**Acceptance criteria:**

- [ ] Confirm the final action set and pause semantics.
- [ ] Provide a battle-HUD button and keyboard access to an overlay that blocks underlying HUD input.
- [ ] Implement the approved navigation actions; the recommended initial set is Resume, Restart Match, and Main Menu.
- [ ] Repeated open/close is idempotent, result presentation takes precedence, and scene transitions do not duplicate persistent services or listeners.
- [ ] Verify compilation, representative resolutions, mouse/keyboard behavior, and Play Mode behavior.
- [ ] Keep settings, volume controls, save/load, and controller navigation outside this task unless separately approved.

**Dependencies:** Accepted scope in `DEC-017`; final UX contract requires confirmation before implementation.

**Progress:** The user's visual approval covers the implemented title, faction-selection, difficulty-selection, and mythic-choice interfaces. It does not verify this accepted but unimplemented active-match menu.

**Status:** Not started

---

## P2 — Important after the critical loop

- Add basic onboarding for workers, production, upgrades, timeout rules, and restart.
- [x] Black, blue, purple, red, and yellow melee/archer variants show their matching idle, run, and attack animations in Play Mode; the user verified every colour on 2026-07-17 after loop and transition settings were matched to black.
- [ ] Verify the shared cavalry, siege, and mythic units still spawn correctly with each team-colour faction. Confirm that switching both team factions does not produce missing references, controller warnings, or unintended mechanical differences.

### Selectable mythic roster

**Outcome:** Mythic production supports an animation-complete test roster and a pre-purchase choice that remains selected for the match.

**Phased acceptance criteria:**

- [x] **Phase 1 - Monk healer:** Create Black, Blue, Purple, Red, and Yellow monk prefabs using their Idle, Run, and Heal clips; loop Idle/Run but not Heal.
- [x] Target the most-injured allied combat unit within range 2, excluding self, bases, dead units, and full-health units; stop to heal and resume moving when none is valid and in combat.
- [x] Heal 5 every 3 seconds; amount scales 1x/1.25x/1.5x by tier, cadence stays fixed, multiple monks may share a target, and healing cannot exceed maximum health.
- [x] Present the monk's heal animation and its effect around the recipient; safely cancel on target death or match end.
- [x] **Phase 2 - Enemy Pack prefabs:** Create Minotaur-structured mythic prefabs for Bear, Gnoll, Gnome, Harpoon Fish, Lancer, Lizard, Paddle Fish, Panda, Shaman, Skull, Snake, Spider, Thief, Troll, and Turtle; retain Minotaur.
- [x] Use Idle, Run/Walk, and Attack clips, treating Gnoll/Harpoon Fish `Throw` as attack equivalents. Loop only Idle and Run/Walk.
- [x] Treat Gnoll, Harpoon Fish, and Shaman as ranged mythics. Use their nearby bone, harpoon, and shaman projectile assets instead of the standard archer arrow, with independently configurable `UnitData` for range, cost, cadence, and balance.
- [x] Treat the remaining Enemy Pack options as Minotaur-style melee mythics initially, with independently configurable `UnitData`.
- [x] Use Troll Attack as its attack. Ignore Troll Windup, Recovery, and breaking-club animations, Skull/Turtle guard animations, and Boat.
- [x] **Phase 3 - Mythic picker:** Open the right-side details picker before spending. Selection atomically purchases the configured unlock and remains locked for the match; cancellation spends nothing.
- [x] Later purchases upgrade the selection without reopening the picker. Its `UnitData` controls cost and cadence, and all future mythic spawns use it.
- [x] Initially populate all Phase 1-2 qualifying units. Preserve explicit opponent configuration until AI selection is implemented, while supporting later enemy-first reveal and counter-selection.
- [x] Differentiate combat mythics by creature identity, with cheaper/weaker options generally producing faster, Troll uniquely strongest and highest-cost, and all monk colours mechanically identical.
- [x] Order the mythic picker by ascending gold cost, then alphabetically when costs match.
- [x] Validate compilation/import, serialized-data migration, both teams/facings, animator references, restart reset, transaction behavior, representative resolutions, and Play Mode.

**Dependencies/risks:** Mythic remains deliberately separate from the four-slot faction roster and continues to use its picker-backed per-match selection. Healing requires friendly targeting and current/max-health access without costly broad scans. This test expansion must not displace the incomplete AI/core-loop work.

**Progress:** Phases 1-3 are complete. Runtime source provides ally-combat-state tracking, clamped healing, lowest-health ally selection, fixed-cadence tier-scaled monk healing, movement suppression, recipient-owned heal VFX, moving/healing friendly separation, enemy-proximity stopping, and a target-point stopping distance equal to heal range. The Editor builder normalized all five controllers and clip-loop settings, generated the shared `MonkUnitData`, one-shot heal-effect prefab, and five colour monk prefabs, and assigned each colour faction's mythic entry to its matching monk. Phase 2 provides 15 independent Enemy Pack mythic prefabs and `UnitData` assets, normalized Idle/Run-or-Walk/Attack controllers, and dedicated bone, harpoon, and shaman projectiles for the three ranged units. Phase 3 adds a build-safe 21-unit roster and a scrollable right-side picker whose selection and initial gold spend are atomic; the selected prefab's data controls upgrades and spawning for the remainder of the match, while cancellation and unaffordable options spend nothing. Combat mythics now have accepted creature-identity-driven health, damage, speed, cost, and cadence profiles; all monk colours share one profile, Troll is uniquely strongest and highest-cost, and picker options sort by ascending gold cost with alphabetical ties. Presentation maps all 16 Enemy Pack choices to their configured Enemy Avatars and all five monks to their colour-specific Human Avatars, shows portraits beside picker labels, switches the mythic slot and details portrait after selection, starts with greyed crossed swords on parchment, removes redundant picker chrome, and gives all standard faction archers the Tiny Swords arrow projectile. Static serialized-reference checks and both C# assembly builds pass; the user accepted the combined Phase 2-3 Play Mode review and the subsequent balance and sorting pass on 2026-07-17.

**Status:** Complete; Phases 1-3 implemented and accepted (`DEC-013`).

### Faction-driven prototype presentation

**Outcome:** The faction selected for either team is the single source of truth for its ordered standard-unit roster, world Castle, House3 worker hand-in, HUD stronghold icon, and animated worker prefab without duplicating economy or building gameplay configuration.

**Acceptance criteria:**

- [x] Black, blue, purple, red, and yellow `FactionData` assets reference their matching Tiny Swords Castle and House3 sprites.
- [x] Each colour faction references a matching animated worker prefab; non-black worker controllers replicate black's parameters, states, transitions, durations, and colour-specific clips, with all worker clips using black's loop settings.
- [x] `GameManager` applies each selected faction to the corresponding world presentation and HUD icon, with Black versus Red as the active scene default.
- [x] `GameManager` applies the selected faction worker to `WorkerManager` during `Awake`, before `WorkerManager.Start` creates initial workers.
- [x] One shared Base prefab owns the castle, hand-in child, worker economy component, and presentation bindings; loose scene hand-in objects and per-instance sprite/tint overrides are removed.
- [x] Missing presentation data retains authored fallback sprites and produces actionable diagnostics instead of a null exception.
- [x] Unity imports the prefab, scene, and new component with no missing scripts or broken serialized references.
- [x] In Play Mode, every colour is verified on both Left and Right; Castle, House3, HUD icon, and spawned colour-specific units agree with the selected faction.
- [x] Both teams' workers still spawn, mine, return to the inward hand-in point, and increase gold; base damage shake, targeting, destruction, and restart remain correct.

**Progress:** Source, faction assets, five worker prefabs, worker controllers/clips, shared-prefab structure, scene migration, HUD bindings, and the HUD regeneration tool are implemented. Runtime and Editor assemblies compile externally with zero warnings and errors, and the worker prefab/controller/faction reference graph passes static validation. The user confirmed the faction worker variants on 2026-07-17, verified both-team presentation/economy and restart behavior during the runtime extraction pass, and later exhaustively verified the faction-colour visual combinations on both sides. The stronghold health fills are fixed team-side presentation rather than faction-colour variants.

**Status:** Complete

---

## P3 — Later

_Deferred scope is tracked in `ROADMAP.md` and `DEC-005`._

---

## Blocked

_No items currently blocked by external state._

---

## Discovered follow-up work
- [ ] Manually verify the ordered-roster migration in Unity Play Mode with a test faction configured as two melee plus two archer standard slots: confirm each card's label/art/data, independent unlock/tier/timer/cost/cadence and spawning, slot-aware AI purchasing/upgrading, match restart reset, and the legacy global debug pattern. Also smoke the five colour factions and legacy `Default` to confirm their preserved melee/archer/cavalry/siege slot order and confirm mythic remains the independent fifth picker-backed track.
- [x] After Unity imported the legacy meme-faction deletions, the user confirmed on 2026-07-28 that the Unity run looked correct, closing the Console and title → faction → difficulty → battle smoke follow-up. No broader gameplay verification is inferred.
- [x] Accept the initial SFX layer as a prototype first pass after several complete user-played matches. Sound-level and cue-selection refinement remains a later audio pass; persistent-service uniqueness should be retained as a scene-transition regression check.
- [x] Normalize the battle-scene UI hierarchy by removing the isolated `Canvas/Canvas/VictoryText` legacy branch and restoring the active `Battle UI` RectTransform scale to one. The user verified the migrated hierarchy and presentation on 2026-07-18.
- [x] Convert the mirrored gold nodes to `Assets/Prefabs/Resources/GoldVein.prefab`, retaining side-specific root transforms, internal `MinePoint` references, and both `WorkerManager` bindings. The user verified the migrated scene and worker loops on 2026-07-18.
- [x] Replace runtime production-card name lookups with serialized `ProductionCardView` bindings and update the HUD builder to author them. The user verified all five migrated cards on 2026-07-18.
- [ ] Convert the five now-self-contained production-card views to a shared prefab with role-specific serialized overrides, keeping the HUD builder deterministic and verifying all card interactions afterward.
- [x] Complete and verify the final behavior-preserving runtime refactor batch. Spawn selection/capacity/prefab resolution now lives in `UnitSpawnController`; battle readouts and listener lifecycle live in `BattleHudReadoutPresenter`; faction-option generation has a focused presenter; and enemy AI receives initialized manager/economy dependencies with a legacy fallback. External runtime and Editor compilation passed with zero warnings, and the user verified the batch in Play Mode on 2026-07-18.
- Inventory project-owned modifications inside third-party Tiny Swords folders. Treat vendor content as read-mostly and migrate future project-owned Animator Controllers or overrides into a project-owned animation folder when safe; do not move `Resources/MythicUnitRoster.asset` while `GameManager` relies on its fallback `Resources.Load` contract.
- Revisit the editor-generated stronghold health-bar frame in `ClashBattleUIBuilder.CreateHealthBarFrame`. The health fills are currently fixed by team side (blue player/left and red enemy/right), not selected faction; decide during the later UI pass whether that convention should remain. Determine the correct use of the Tiny Swords bar assets or replace the frame treatment; the final frame must fully contain the health fill without distortion or overflow. The current stretched middle segment is intentionally commented out until this is addressed.
- Tune initial purchase costs, recurring production cadences, role matchups, and stat values through representative playtests.
- Resolve projectile targeting semantics before changing combat behavior: current projectiles aim at the launch position and may miss moving targets; choose deliberate misses, target leading, or live-target tracking, then document and verify the approved rule.
- [x] Add profiler markers for target acquisition, friendly separation, monk searches, and production capacity scans, then assess representative battle performance before changing architecture. The user judged current performance sufficient for the prototype on 2026-07-18; retain `OverlapCircleAll` and scene-wide capacity scans unless later measured performance regresses.

---

## Completed

### Remove the legacy meme faction assets

**Completed:** 2026-07-28

**Evidence:** A pre-deletion static GUID audit identified the `MemeTeam1` and `MemeTeam2` faction data, unit/projectile prefabs, portrait images, directories, and metadata as one self-contained 62-file set with zero references from outside the set. The complete set was removed; normal Git history retains the deleted content. On 2026-07-28, the user confirmed that the post-deletion Unity run looked correct; no broader gameplay verification is inferred.

### Resolve the independent production contract

**Completed:** 2026-07-16

**Evidence:** User-approved answers were originally recorded in `DEC-007`: first purchase unlocks one continuously producing track; later purchases advance future spawns to two and three stars; fielded units are unchanged. `DEC-018` carries that lifecycle forward per ordered standard slot while superseding role-keyed production identity. `DEC-003`, `DEC-004`, and `DEC-008` also record the resolved timeout valuation, initial multipliers, and PC-first priority.

This completed the design dependency only. Production and upgrades were not implemented when this decision task closed; their current implementation and remaining verification are tracked in P1 Task 1.

### Define and reconcile the current game direction

**Completed:** 2026-07-16

**Evidence:** `GAME_DESIGN.md` defines the pitch, fantasy, core loop, pillars, scope, and success criteria; `DEC-002` through `DEC-006` record direct user-approved constraints; the Prototype milestone is game-specific.

This documentation completion does not claim that the gameplay loop is implemented or validated.
