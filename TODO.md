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

### 1. Integrate player gold, workers, and independent unit production

**Outcome:** The player can make the prototype's core economy-versus-pressure decision with gold.

**Acceptance criteria:**

- [x] Player gold is the authoritative currency for worker purchases, all five production types, and upgrades.
- [x] Buying a worker uses the existing worker limit and updates visible gold/worker state.
- [x] Every unit type follows the approved independent recurring-production contract without a shared FIFO queue in source; all-role Play Mode verification remains tracked below.
- [x] A role begins locked; its first purchase unlocks continuous one-star production and its next two purchases upgrade only future spawns to two and three stars in source.
- [x] Favourable matchups apply a 1.2× damage multiplier.
- [x] Star tiers multiply every configured unit stat except purchase cost by 1×/1.25×/1.5× and affect future spawns only.
- [x] Each `UnitData` asset owns that role's purchase cost and recurring production cadence; the global spawn interval no longer defines all roles.
- [ ] Production stops correctly when the match ends and resets correctly for a new match.
- [ ] Costs, failed purchases, and production state are visibly communicated.
- [ ] Deterministic economy and production rules have focused automated tests where practical.
- [ ] Compilation and relevant Play Mode paths are verified.

**Relevant systems:** `WorkerManager`, `GameManager`, `UnitData`, faction data, battle UI, scene wiring

**Dependencies:** `DEC-004` and `DEC-007`; initial values require balance tuning during implementation

**Progress:** `UnitData` owns each role's cost and cadence. Player production purchasing is now implemented in source: all roles begin locked, a successful purchase atomically spends player gold, the first purchase starts that role's fresh recurring timer, the next two purchases snapshot 1.25×/1.5× stats onto future units, and a fourth purchase is rejected. The implemented counter triangle applies 1.2× favourable damage for melee against cavalry, archers against melee, and cavalry against archers. The five production cards show live locked/producing state, tier, unlock/upgrade/max actions, affordability, and greyed locked art; the selected-role panel mirrors the chosen role. The user Play Mode verified clickable production controls, successful melee unlock/upgrade processing through the three-purchase cap, and live 0/3 → 1/3 → 2/3 → 3/3 tier-counter updates on 2026-07-17, and accepted the revised curve and counter implementation on 2026-07-17. The completed AI task now purchases and runs enemy production through the same APIs. Runtime and Editor assemblies compile externally with zero warnings and errors. Recurring spawn cadence, revised stat scaling, matchup behavior, insufficient-funds behavior, all five roles, and restart reset still require targeted Play Mode verification; focused automated tests remain absent.

**Status:** Partially implemented

### 2. Implement the single-player AI economy and production opponent

**Outcome:** The opposing side makes strategic worker, production, composition, and upgrade decisions under the same rules as the player.

**Acceptance criteria:**

- [x] AI purchases consume its gold and respect the same costs, limits, and production rules.
- [x] AI can buy workers, use all required roles, and buy star upgrades.
- [x] A simple documented strategy creates credible pressure within a five-minute match.
- [x] AI stops on match end and resets on restart.
- [x] AI decisions are observable enough to debug and tune.
- [x] Compilation and representative Play Mode matches are verified.

**Relevant systems:** future AI policy, economy/production interfaces, match state

**Dependencies:** Task 1

**Progress:** Complete. `EnemyAIController` uses the shared worker, production, tier, and atomic mythic-purchase APIs. Easy/Medium/Hard change cadence and policy quality; enemy-only bonuses produce 200/250/350 initial totals. Medium and Hard gate early workers behind military production to resist rushes. The user verified the integrated AI and difficulty flow in Play Mode on 2026-07-17 and accepted current balance for later fine-tuning.

**Status:** Complete

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
- [ ] Restart resets time, gold, workers, production, upgrades, units, AI, strongholds, and UI.
- [ ] Base-destruction, health-tiebreak, value-tiebreak, and exact-equality behavior have tests or explicit verification cases.
- [ ] Compilation and full Play Mode result/restart paths are verified.

**Relevant systems:** `GameManager`, `Base`, `BaseUnit`, economy/production, result UI

**Dependencies:** Tasks 1–2

**Progress:** `GameManager` now owns a configurable 300-second countdown, typed match-end reasons, per-team/per-role death counts, total purchase-value losses, and active-scene reload restart. Deterministic timeout comparison has been extracted into the engine-independent `MatchResultResolver`, with Edit Mode characterization tests for both health winners, both lost-value winners, and exact draws. All five Edit Mode cases passed in Unity on 2026-07-18. Lethal unit damage records the destroyed unit's production-slot cost exactly once. The battle HUD shows live remaining time, player-relative victory/defeat/draw state, the resolution reason, loss totals for value tiebreaks, and a functional restart action in source. Runtime and Editor assemblies previously compiled externally with zero warnings and errors. The user Play Mode verified countdown expiry and correct lost-gold resolution with a shortened 20-second match on 2026-07-17. Targeted Play Mode coverage of base destruction, the health-tiebreak presentation, exact-draw presentation, overlay input, and complete reset behavior remains required, so this task is not complete.

**Status:** Partially implemented

### 4. Replace the static HUD with a functional prototype HUD and validate the loop

**Outcome:** A new player can read and complete the entire prototype loop against AI.

**Acceptance criteria:**

- [x] HUD shows live gold, workers, five independent production states, star tiers, timer, both stronghold health values, and results in source; complete-loop and multi-resolution verification remain below.
- [x] Favour, essence, and shared FIFO queue presentation are removed or clearly excluded from the functional prototype UI.
- [ ] Player purchase and upgrade controls provide success, failure, affordability, and cooldown/cadence feedback.
- [ ] Critical state does not rely on red/blue colour alone and text is readable at the chosen prototype resolution.
- [ ] At least one complete economy → production → combat → result → restart match is verified in Play Mode.
- [ ] Playtesting confirms at least one meaningful economy-versus-pressure choice and identifies required balance follow-up separately.

**Relevant systems:** battle HUD, UI bindings, safe area, all critical-loop systems

**Dependencies:** Tasks 1–3

**Progress:** The editor-authored battle HUD has been redesigned around the accepted Tiny Swords direction. Gold, workers, both strongholds, timer, results, restart, and all five independent production cards are now bound in source. Production cards display live locked/producing state, tier, affordability, greyed locked art, and unlock/upgrade/max actions; the selected-role panel follows the last production card hovered over, clicked, tapped, or purchased from and remains pinned after the pointer leaves. Its icon mirrors the selected card's unit artwork, the redundant production/independent row has been removed, and the role-icon container was doubled from 54×54 to 108×108. The user verified the complete selected-role interaction and visual update in Play Mode on 2026-07-17. Worker, stronghold, countdown-expiry, and lost-gold paths were previously user-verified. Broader production purchasing and the remaining result/restart paths still need targeted Play Mode verification. The generated HUD requires multi-resolution Unity inspection and complete-loop Play Mode verification. The attempted three-part Tiny Swords stronghold health-bar frame did not produce an acceptable result; its stretched middle segment is temporarily commented out pending a later UI pass.

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
- [ ] Menu scripts compile, both scenes are present and ordered correctly in Build Settings, and the title -> selection -> battle path is verified in Play Mode and a player build where practical.

**Relevant systems:** new menu scene/UI and controller, `FactionData`, a serialized faction catalog/menu configuration, scene-loading/session-selection boundary, `GameManager`, Build Settings

**Dependencies:** Existing faction assets and faction-driven presentation. Phase 3 depends on Phases 1–2 but the functional menu flow does not depend on the animated background.

**Risks and manual Unity work:** Runtime builds cannot use `AssetDatabase` to discover ScriptableObjects, so available factions must be explicitly serialized or supplied through another build-safe content mechanism. Scene creation, Canvas layout, button references, Build Settings ordering, faction catalog contents, multi-resolution inspection, and Play Mode/player-build verification require Unity Editor validation.

**Progress:** Coordinated and accepted in `DEC-012` and extended by `DEC-014`. The user verified the title, faction selection, Tiny Swords animated presentation, difficulty selection, distinct opponent faction, and transition into battle in Play Mode on 2026-07-17. Difficulty choices launch battle directly from a parchment-contained Tiny Swords menu. The title-background builder now includes at least one runner for every selectable mythic: all 16 qualifying Enemy Pack creatures and all five colour monks. Their count and placement are independent of the standard melee/archer mix; regenerating and visually reviewing the scene remains required. Player-build verification remains pending, so the parent task remains partially implemented.

**Status:** Partially implemented

---

## P2 — Important after the critical loop

- [ ] Playtest and refine the first roster-wide combat balance pass. Source and assets now implement the 1.2x melee > cavalry > archer > melee counter triangle, differentiated siege/building damage, a creature-identity-driven 140-300 gold mythic roster with 8-16 second cadences, mechanically identical monk colours, a 1,000-health stronghold, and a reduced 1x/1.25x/1.5x tier curve (1x/1.5625x/2.25x effective DPS). Validate equal-tier/equal-gold matchups, light-versus-heavy mythic throughput, ranged safety, monk stacking, Troll value, base time-to-kill, economy/cadence interaction, and tier value before treating the values as final.
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

**Dependencies/risks:** Existing faction and production data assume one prefab/data pair per role. Healing requires friendly targeting and current/max-health access without costly broad scans. This test expansion must not displace the incomplete AI/core-loop work.

**Progress:** Phases 1-3 are complete. Runtime source provides ally-combat-state tracking, clamped healing, lowest-health ally selection, fixed-cadence tier-scaled monk healing, movement suppression, recipient-owned heal VFX, moving/healing friendly separation, enemy-proximity stopping, and a target-point stopping distance equal to heal range. The Editor builder normalized all five controllers and clip-loop settings, generated the shared `MonkUnitData`, one-shot heal-effect prefab, and five colour monk prefabs, and assigned each colour faction's mythic entry to its matching monk. Phase 2 provides 15 independent Enemy Pack mythic prefabs and `UnitData` assets, normalized Idle/Run-or-Walk/Attack controllers, and dedicated bone, harpoon, and shaman projectiles for the three ranged units. Phase 3 adds a build-safe 21-unit roster and a scrollable right-side picker whose selection and initial gold spend are atomic; the selected prefab's data controls upgrades and spawning for the remainder of the match, while cancellation and unaffordable options spend nothing. Combat mythics now have accepted creature-identity-driven health, damage, speed, cost, and cadence profiles; all monk colours share one profile, Troll is uniquely strongest and highest-cost, and picker options sort by ascending gold cost with alphabetical ties. Presentation maps all 16 Enemy Pack choices to their configured Enemy Avatars and all five monks to their colour-specific Human Avatars, shows portraits beside picker labels, switches the mythic slot and details portrait after selection, starts with greyed crossed swords on parchment, removes redundant picker chrome, and gives all standard faction archers the Tiny Swords arrow projectile. Static serialized-reference checks and both C# assembly builds pass; the user accepted the combined Phase 2-3 Play Mode review and the subsequent balance and sorting pass on 2026-07-17.

**Status:** Complete; Phases 1-3 implemented and accepted (`DEC-013`).

### Faction-driven prototype presentation

**Outcome:** The faction selected for either team is the single source of truth for its world Castle, House3 worker hand-in, HUD stronghold icon, and animated worker prefab without duplicating economy or building gameplay configuration.

**Acceptance criteria:**

- [x] Black, blue, purple, red, and yellow `FactionData` assets reference their matching Tiny Swords Castle and House3 sprites.
- [x] Each colour faction references a matching animated worker prefab; non-black worker controllers replicate black's parameters, states, transitions, durations, and colour-specific clips, with all worker clips using black's loop settings.
- [x] `GameManager` applies each selected faction to the corresponding world presentation and HUD icon, with Black versus Red as the active scene default.
- [x] `GameManager` applies the selected faction worker to `WorkerManager` during `Awake`, before `WorkerManager.Start` creates initial workers.
- [x] One shared Base prefab owns the castle, hand-in child, worker economy component, and presentation bindings; loose scene hand-in objects and per-instance sprite/tint overrides are removed.
- [x] Missing presentation data retains authored fallback sprites and produces actionable diagnostics instead of a null exception.
- [x] Unity imports the prefab, scene, and new component with no missing scripts or broken serialized references.
- [ ] In Play Mode, every colour is verified on both Left and Right; Castle, House3, HUD icon, and spawned colour-specific units agree with the selected faction.
- [ ] Both teams' workers still spawn, mine, return to the inward hand-in point, and increase gold; base damage shake, targeting, destruction, and restart remain correct.

**Progress:** Source, faction assets, five worker prefabs, worker controllers/clips, shared-prefab structure, scene migration, HUD bindings, and the HUD regeneration tool are implemented. Runtime and Editor assemblies compile externally with zero warnings and errors, and the worker prefab/controller/faction reference graph passes static validation. The user confirmed the faction worker variants import and work correctly in Play Mode on 2026-07-17. Full both-side faction presentation, worker economy, base-damage, and restart coverage remains pending.

**Status:** Partially implemented

---

## P3 — Later

_Deferred scope is tracked in `ROADMAP.md` and `DEC-005`._

---

## Blocked

_No items currently blocked by external state._

---

## Discovered follow-up work

- After critical-loop verification, normalize the battle-scene UI hierarchy in Unity: remove or rename the duplicate `Canvas` wrapper, correct the actual Canvas RectTransform's serialized zero scale, and confirm whether the legacy victory Canvas can be consolidated with the battle HUD without changing sorting or input. Verify scene reopen, pointer input, result overlay, and representative aspect ratios.
- Reduce scene/UI duplication after the critical path is stable: create a reusable production-card prefab and shared bordered-panel treatment, and prefab the mirrored gold-vein presentation while preserving side-specific mine points and worker references. Keep the builder deterministic and verify all five cards plus both worker loops after migration.
- Continue behavior-preserving tests while splitting the oversized `BattleEconomyUI` and `GameManager` responsibilities. Timeout resolution, player-relative result/countdown text, production-tier transitions, and the accepted star-stat curve now live in the Core assembly, with all 38 Edit Mode cases passing in Unity on 2026-07-18. Runtime battle presentation is separated and verified. Production state/scheduling and match state/loss accounting now live in focused controllers; base destruction, all timeout outcomes, post-game loss immutability, system stopping, and restart reset were verified in Play Mode. Extract faction and team presentation initialization next while preserving session selection and the `GameManager.Awake` before `WorkerManager.Start` ordering.
- Inventory project-owned modifications inside third-party Tiny Swords folders. Treat vendor content as read-mostly and migrate future project-owned Animator Controllers or overrides into a project-owned animation folder when safe; do not move `Resources/MythicUnitRoster.asset` while `GameManager` relies on its fallback `Resources.Load` contract.
- Revisit the editor-generated stronghold health-bar frame in `ClashBattleUIBuilder.CreateHealthBarFrame`. Determine the correct use of the Tiny Swords bar assets or replace the frame treatment; the final frame must fully contain the health fill without distortion or overflow for both teams at QHD 2560×1440 and representative narrower/wider aspect ratios. The current stretched middle segment is intentionally commented out until this is addressed.
- Tune initial purchase costs, recurring production cadences, role matchups, and stat values through representative playtests.
- Resolve projectile targeting semantics before changing combat behavior: current projectiles aim at the launch position and may miss moving targets; choose deliberate misses, target leading, or live-target tracking, then document and verify the approved rule.
- Profile a representative 60-versus-60 battle before changing target-query or unit-count architecture; if measurements confirm pressure, replace allocating physics queries and scene-wide unit scans with proportionate non-allocating queries and lifecycle-owned counts.

---

## Completed

### Resolve the independent production contract

**Completed:** 2026-07-16

**Evidence:** User-approved answers are recorded in accepted `DEC-007`: first purchase unlocks one continuously producing role track; later purchases advance future spawns to two and three stars; fielded units are unchanged. `DEC-003`, `DEC-004`, and `DEC-008` also record the resolved timeout valuation, initial multipliers, and PC-first priority.

This completed the design dependency only. Production and upgrades were not implemented when this decision task closed; their current implementation and remaining verification are tracked in P1 Task 1.

### Define and reconcile the current game direction

**Completed:** 2026-07-16

**Evidence:** `GAME_DESIGN.md` defines the pitch, fantasy, core loop, pillars, scope, and success criteria; `DEC-002` through `DEC-006` record direct user-approved constraints; the Prototype milestone is game-specific.

This documentation completion does not claim that the gameplay loop is implemented or validated.
