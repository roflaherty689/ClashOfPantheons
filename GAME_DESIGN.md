# Game Design

## Document purpose

This file defines the intended player experience, rules, systems, and scope. **Implemented** means repository evidence exists, though Play Mode validation may still be pending. **Accepted direction** means user-approved intent that may not yet be implemented. **Proposed** items await approval.

---

## Game summary

**Working title:** Clash of Pantheons

**Genre:** 2D mythological tug-of-war strategy autobattler

**Target platforms:** PC storefronts first, with mobile as the second platform target. Mouse input has prototype priority; touch adaptation follows for mobile. Controller support remains deferred.

**Target audience:** Broad-age strategy players interested in mythology or ancient warfare; exact age rating and audience positioning remain unconfirmed.

**Current stage:** Late Prototype — the player-facing loop is integrated and the user has completed several clean end-to-end Play Mode matches through result and restart. Player-build validation, targeted regression coverage, an in-game menu, and balance tuning remain.

**Pitch:** Build a mythological war economy, shape an autonomous army, and push along a contested lane to destroy the rival stronghold before time runs out.

## Player fantasy

The player indirectly commands a mythological faction. They invest in workers, unit production, composition, and upgrades, then watch autonomous forces turn those strategic choices into an escalating push.

There is no direct unit control. The prototype is primarily single-player against AI. The enemy uses a Play Mode-verified strategic purchasing policy; its balance and resistance to dominant strategies remain under review.

## Design pillars

1. **Strategic economy**
   - Economic investment creates long-term strength at the cost of immediate pressure.
   - Excludes passive income with no meaningful tradeoffs.

2. **Army composition and counterplay**
   - A non-empty ordered standard-production roster of up to five slots defines a faction's composition, may repeat combat roles, and is supplemented by one separate mythic track. The five combat roles remain readable classifications with distinct strengths and weaknesses.
   - Excludes one dominant composition or cosmetic-only role differences.

3. **Mythological faction identity**
   - Factions use legendary creatures and iconic cultural details to feel visually and mechanically distinct.
   - Excludes strict historical simulation and palette-swap factions.

4. **Readable, escalating battles**
   - Matches grow from small clashes into larger confrontations without hiding momentum or strategic consequences.
   - Excludes opaque combat and uncontrolled visual clutter.

## Core loop

### Intended player loop — accepted direction

1. Workers gather gold.
2. The player chooses between improving income, establishing unit production, and buying upgrades.
3. Each purchased standard-production slot, plus the separate mythic track, produces independently; spawned units march and fight autonomously.
4. The player reads the battle and adapts investment, composition, and upgrades.
5. A stronghold is destroyed or the approximately five-minute timer expires.
6. The result is shown and the player can restart.

### Current prototype loop — implemented, not Play Mode verified in this pass

1. Both sides begin with every available production track locked. The player and AI spend their own gold to unlock independent cadences. Each faction supplies a non-empty ordered standard roster of up to five tracks, plus one separate picker-backed mythic track. The five-slot colour-faction/six-card path and three-slot Fishman/Goblin source/assets are implemented and statically inspected, and external solution compilation passes with zero warnings or errors; Unity import, scene binding, and complete Play Mode regression remain pending. A legacy free-spawn pattern remains available only for prototype/debug testing.
2. Units march, acquire targets, and fight automatically.
3. Workers independently mine and deposit gold.
4. Destroying a base or reaching the time limit ends the match and displays the result.

The player HUD presents live economy and stronghold state, a five-minute countdown, timeout results, restart, and production purchasing. Both teams start with locked tracks and spend their own gold to unlock or upgrade them. The user completed several clean end-to-end Play Mode matches through result and restart by 2026-07-28 under the earlier role-keyed standard roster. Earlier focused verification also covered AI purchasing, production scheduling, timeout branches, match stopping, and reset behavior. The five-standard-slot/six-card migration for the established factions and the variable-roster/Fishman/Goblin changes are implemented in source and statically checked, but Unity import, scene rebinding, shorter-roster card hiding in Play Mode, Fishman/Goblin selection and battles, complete regression, targeted automation, and a packaged player build remain incomplete.

**Short-loop rhythm:** Worker trips and independent recurring production cadences. Buying a locked standard slot or the mythic track unlocks its continuous production; later purchases advance that same track to two and three stars.

**Session:** One approximately five-minute match on one shared horizontal lane.

**Primary reward:** Battlefield momentum and match victory. Campaign and persistent rewards are deferred.

## Player actions

- **Buy a worker:** Spend gold to increase future income, limited by cost and worker capacity.
- **Establish or improve production:** Spend gold on one of the selected faction's configured ordered standard-production slots or the separate mythic track. Each track operates independently rather than through one shared FIFO queue.
- **Upgrade a production track:** Spend gold to advance that standard slot or mythic track through one-, two-, and three-star tiers.
- **Read and adapt:** Observe gold, production, upgrades, time, base health, composition, and momentum.

The first purchase unlocks one continuously producing track. The second and third purchases advance future spawns from that same track to two and three stars respectively. Purchases do not enter a shared queue and upgrades do not alter units already on the field. Direct movement and combat commands are not player actions.

## Game rules

### Movement and combat

**Implemented:** The battlefield is a horizontal lane. Left units advance right and right units advance left. Units prioritize the nearest enemy unit in range, then the enemy base. Attacks use configured health, damage, range, speed, attack rate, projectiles, and unit/building damage modifiers. Friendly units make limited vertical separation adjustments.

### Unit roles and counterplay

The five accepted combat classifications are melee, archer, cavalry, siege, and mythic. `UnitRole` classifies combat behavior and counter relationships; it is not the identity, uniqueness key, or required-count template for a faction's standard-production roster. A faction owns a non-empty ordered roster of one to five standard slots, and multiple slots may use the same combat role. Each configured slot selects its own prefab and `UnitData` and retains independent unlock tier, timer, purchase cost, and production cadence. The separate picker-backed mythic track sits outside standard faction-roster composition, so a faction currently exposes two to six total production tracks.

The five supported colour factions use melee, archer, cavalry, siege, and their matching colour monk in `Standard0` through `Standard4`. The monk remains classified as `UnitRole.Mythic` for combat behavior and reporting, but its faction slot is purchased, upgraded, timed, and spawned as normal standard production. The legacy `Default` faction also retains five ordered standard entries. The accepted Fishman faction uses exactly Harpoon Fish, Paddle Fish, and Lizard in `Standard0` through `Standard2`; the accepted Goblin faction uses exactly Skull, Lancer, and Shaman. All six creatures retain `UnitRole.Mythic` while functioning as ordinary faction-owned standard slots, and they also remain in the global mythic picker. The implemented first-pass counter triangle applies 1.2× damage for melee against cavalry, archers against melee, and cavalry against archers. Siege remains a structure specialist; mythics use melee, ranged, and support baselines without a role-wide counter bonus. Exact values remain subject to representative Play Mode tuning.

| Faction group | Ordered standard roster | Standard slot count | Prototype presentation |
|---|---|---:|---|
| Black, Blue, Purple, Red, Yellow | Melee, Archer, Cavalry, Siege, matching-colour Monk | 5 | Matching Tiny Swords colour |
| Legacy `Default` | Existing configured five-entry roster | 5 | Existing legacy configuration |
| Fishman | Harpoon Fish, Paddle Fish, Lizard | 3 | Temporarily reuses Black |
| Goblin | Skull, Lancer, Shaman | 3 | Temporarily reuses Red |

The Fishman/Goblin assets, exact prefab order, Black/Red presentation references, and faction-catalog registration are implemented and statically inspected. Unity import, menu exposure, hidden-card behavior, and battles must still be verified before those rows are described as Play Mode verified.

| Role | Intended identity | Implementation status |
|---|---|---|
| Melee | Frontline; intended strong versus cavalry and weak versus archers | Prototype role exists |
| Archer | Ranged support; intended strong versus melee and weak versus cavalry | Projectile role exists |
| Cavalry | Fast pressure; intended strong versus archers and weak versus melee | Prototype role exists |
| Siege | Structure specialist, vulnerable without support | Projectile role and building modifier exist |
| Mythic | Combat classification for exceptional creatures and support units; ownership depends on production track | The separate pre-purchase picker contains 16 Enemy Pack creatures, including Minotaur, and excludes monks. Each colour faction instead owns its matching monk in `Standard4`; monks retain `UnitRole.Mythic` combat behavior while using normal standard-slot purchasing. Creature profiles, ranged projectiles, monk healing, and picker mechanics are implemented; the revised six-track presentation still needs scene rebinding and Play Mode review. |

Heroes and bosses are not prototype scope.

### Health, failure, and match result

**Implemented:** Units die at zero health. Destroying a stronghold immediately ends the match and stops autonomous units and workers.

The first-pass combat scale uses 100 health for one-star melee units and 1,000 health for strongholds. One-star role baselines are tuned together with production cost and cadence. Star tiers scale health, damage, attack rate, range, and movement by 1×/1.25×/1.5×. Because damage and attack rate both scale, effective damage per second becomes 1×/1.5625×/2.25× rather than the previous 1×/2.25×/4×; high-tier pacing still requires targeted playtesting.

**Confirmed balance problem:** The user reports that upgrading a melee production track to three stars wins roughly 99% of matches across Easy, Medium, and Hard, while mythic units feel too weak relative to their cost. This violates the intended composition and counterplay pillar. The next balance pass must separate upgrade-curve strength from AI-policy weakness, compare equal-gold and equal-time throughput, and investigate other dominant or non-viable choices before approving new values.

**Implemented in source, partially Play Mode verified:** A match also ends when the configurable timer expires. The healthier stronghold wins. If health is equal, the side with the lower total value of units lost wins. Lost-unit value is accumulated from the purchase cost of each destroyed unit's production slot, with per-role death counts retained for inspection. If both comparisons are equal, the match is a draw with no winner or loser. Countdown expiry and the lost-value resolver were user-verified in a shortened match on 2026-07-17.

The result overlay identifies victory, defeat, or draw and its resolution condition. Its restart action reloads the active battle scene to reset current runtime state; representative end-to-end restart behavior is user-verified, while targeted reset regression coverage remains.

### Economy and production

**Implemented:** Workers repeatedly travel to a gold vein, mine, return, and deposit gold. Starting and maximum worker counts, worker cost, base gold per trip, and a future-facing income-upgrade multiplier are configurable on `WorkerManager`. The player starts with one worker in the active scene, and the live HUD can buy workers up to the five-worker capacity while displaying current gold, worker count, and aggregate income per trip.

**Implemented in source; variable-roster flow awaits Unity verification:** Gold is the only prototype currency. It funds workers, every configured independent standard-production slot, the separate mythic track, and three star tiers of in-match upgrades for both the player and AI. Favour, essence, and shared-queue presentation are excluded from the functional battle HUD. Faction validation now accepts one to five standard entries, existing purchase/spawn/AI paths reject unconfigured slot data, and the HUD hides unavailable standard-card objects while retaining the mythic card. Unity compilation/import, six-card maximum binding, ordered duplicate-role and three-slot roster Play Mode verification, deterministic transaction tests, exhaustive track coverage, and targeted edge cases remain incomplete.

### Progression and persistence

Each standard slot and the mythic track begins locked. Its first purchase unlocks continuous one-star production; its next two purchases upgrade future spawns to two and three stars. Star tiers multiply all configured unit stats except purchase cost by 1×, 1.25×, and 1.5×. Existing fielded units do not change when a track is upgraded.

Each standard slot resolves purchase cost and recurring cadence from its configured unit's `UnitData`; duplicate-role slots therefore remain economically and temporally independent. The mythic selection's `UnitData` remains authoritative for its separate track. `GameManager` can retain a legacy global spawn pattern for prototype/debug testing, but it must enumerate configured standard slots rather than collapse duplicates by role. A newly unlocked track begins a fresh cadence rather than inheriting locked time. Initial values remain balance tuning rather than an unresolved ownership decision.

Campaign and persistent progression are deferred beyond the core prototype. No save system is required for the core loop.

## World and match structure

- **Prototype battlefield:** One shared horizontal lane with a stronghold and resource area for each side.
- **Opponent:** Single-player against AI.
- **Target length:** Approximately five minutes.
- **Transitions:** Implemented in source: title, player-faction selection, difficulty selection, then battle. Difficulty defaults to Easy; the opponent randomly receives a different configured faction.
- **Replay:** Active-scene restart is implemented, exposed by the result HUD, and representatively verified through complete matches; targeted reset regression coverage remains. A distinct rematch flow is not implemented.
- **Later possibilities:** Alternative maps, lane structures, or endless mode may be reconsidered after prototype validation.

## AI and difficulty

The AI uses the same worker, gold-spending, configured standard-slot production, tier, separate mythic-selection, spawn, and match rules as the player. It must address and choose only available standard slots so repeated combat roles—including colour-faction monks and the three-slot creature rosters—remain independently purchasable and upgradeable without attempting hidden slots. Easy makes slower, partly idle/random decisions and receives no starting bonus. Medium establishes production before each early worker investment, then balances workers with varied production at a moderate cadence, and receives +50 starting gold. Hard makes faster decisions, requires a broader military opening before expanding its economy, and receives +150 starting gold. The bonus applies once to the enemy only. The AI randomly chooses one of the 16 Enemy Pack picker mythics and a configured faction other than the player's faction. The earlier role-keyed system was representatively Play Mode verified; the revised slot-keyed, variable-roster flow still requires verification. The three-star melee dominance report also shows that difficulty and purchasing policy require balance diagnosis. Online matchmaking remains deferred.

## UI and feedback

**Implemented:** Units display world-space damage health bars. Stronghold health is shown as live current/maximum text and proportional bars in the battle HUD, without duplicate world-space bars above the bases. Base hits shake the base visual; animated attacks and projectile arcs provide combat feedback; victory text identifies the winning team.

**Implemented in source and representatively Play Mode verified only before the latest roster migrations:** The redesigned HUD supports the accepted single-gold economy and a maximum of six production cards. Its player gold, aggregate worker income, worker count, buy-worker button, production controls, locked/producing and tier states, affordability, greyed locked art, both stronghold-health displays, match timer, result overlay, resolution reason, and restart button are live. Up to five cards represent ordered faction-owned standard slots and the final card is the separate mythic picker. Standard-card labels and interactions derive from slot configuration rather than assuming unique roles; source now hides unavailable standard-card objects for shorter rosters while retaining the mythic card and shows prefab-derived names for standard-slot mythics. `SampleScene` still requires **Tools > Clash of Pantheons > Bind Production Card Views** after Unity compiles, followed by Play Mode regression of the five-slot/six-card layout and the three-slot/four-card Fishman/Goblin layout.

The functional prototype HUD must communicate gold, worker purchase state, independent production, star tiers, remaining time, base health, and the final result. A results/restart flow is required. The accepted initial front-end flow is a title screen with Play and Exit, then a faction-selection screen whose clickable options are generated from configured `FactionData` assets. The chosen player faction must drive the existing faction-owned battle presentation and unit mappings. An in-game menu is now accepted scope; its exact actions and pause semantics remain to be finalized before implementation. Basic onboarding and most feedback remain unimplemented.

The implemented front-end menu comprises title, faction selection, and difficulty selection. The accepted active-match in-game menu is a separate, not-yet-implemented feature.

Unit health bars are faction-neutral: a black backing contains a health-state fill that moves from red through yellow to green and is hidden at full health. Stronghold HUD bars use fixed player-side blue and opponent-side red fills, supplemented by current/maximum text and proportional fill length. No five-faction health-bar permutation is currently intended.

## Audio and visual direction

Use the colourful, humorous Tiny Swords pixel-art style as the prototype direction. Replacement or supplementary art should preserve that tone while using iconic mythological or cultural details where useful; strict historical accuracy is not the goal. Five mechanically identical Tiny Swords team-colour variants (black, blue, purple, red, and yellow) are available for prototype presentation. Each variant owns matching Castle and House3 hand-in sprites, an animated worker prefab, animated melee/archer prefabs, and a matching colour monk in its fifth standard slot; cavalry, siege, and the separate Enemy Pack mythic picker remain shared. Fishman and Goblin are accepted as composition-focused prototype factions built from existing Enemy Pack units: Fishman temporarily borrows Black presentation and Goblin temporarily borrows Red presentation. This reuse validates roster identity before committing to new workers, buildings, or final faction art. The selected faction drives its configured world presentation, HUD stronghold icon, and spawned worker. The five palette variants are not five designed mythological factions, and borrowed presentation is not final Fishman/Goblin art direction. Final art and broad faction production remain deferred.

**Implemented and accepted as a prototype first pass:** A persistent project-owned SFX service provides an initial feedback layer using the imported 400 Sounds Pack. It covers menu/UI input, purchases and rejections, worker deposits, unit spawning, role-aware attacks, deaths, monk healing, stronghold damage/destruction, and victory/defeat/draw. Repeated battlefield cues use category cooldowns, limited simultaneous voices, modest pitch variation, and partial spatial blending to preserve readability during large clashes. Sound-level and cue-selection refinements remain for a later audio pass.

The supplied pack does not provide a suitable continuous music direction, so background music remains unimplemented. Player-facing music/SFX volume controls, mixer groups, final clip selection, and a full Play Mode mix/accessibility review remain later work.

The title screen should eventually use the same colourful Tiny Swords presentation, with decorative buildings and units moving across its background. This animated background is presentation polish after the functional title and faction-selection flow; it must not own or mutate battle simulation state.

## Accessibility and input

- PC and mouse input are first priority. Mobile and touch adaptation are second.
- Controller support is deferred beyond the core prototype.
- Keyboard shortcuts may be considered later.
- Text must remain readable and important state must not rely on red/blue colour alone.
- Reduced-motion, screen-shake, flashing, subtitle, and difficulty-assist requirements remain unresolved rather than assumed absent.

## Prototype scope

### Must have

- One complete economy → production → autonomous combat → result → restart loop against AI.
- One shared horizontal lane and two strongholds.
- Gold-funded workers, a non-empty ordered roster of up to five independent standard-production slots per faction, one separate mythic track, and three star tiers per available track.
- Meaningful economy-versus-pressure and composition decisions.
- Approximately five-minute matches with base-destruction and timeout results.
- Minimal functional HUD and clear feedback.
- A minimal in-game menu for leaving or restarting a match and returning to play.
- Play Mode verification of the critical path.

### Should have

- At least one coherent mythological faction presentation.
- Basic onboarding for the complete loop.
- Basic readability and colour-independent state feedback.

### Could have after core validation

- Additional factions beyond the accepted Fishman/Goblin prototype compositions, or additional maps.
- Battle-speed controls for solo play or testing.
- Additional slot identities beyond `Standard4` if the variable one-to-five standard-roster maximum plus separate mythic track proves insufficient.

### Accepted mythic-roster test expansion

Unlocking the separate mythic production track opens a choice menu before gold is spent. Selecting an option atomically purchases the unlock; later purchases upgrade that chosen option for the rest of the match. Each option's `UnitData` owns its cost and production cadence. Following the initial 21-unit test, the current picker contains the 16 qualifying combat-capable Tiny Swords Enemy Pack creatures and excludes all monks. This preserves counter-selection testing without duplicating faction-owned monk production.

The combat mythics use differentiated first-pass profiles rather than one shared Minotaur baseline. Small, agile creatures such as Gnome, Thief, Spider, and Snake are cheaper, weaker, and produced more frequently; the middle roster trades cadence for increasing durability and damage; Bear and Minotaur are premium bruisers; and Troll is the strongest, slowest-producing, uniquely highest-cost option. Turtle emphasizes durability over damage and speed. Gnoll, Harpoon Fish, and Shaman are ranged mythics with increasing cost and power, reduced building damage, and their supplied bone, harpoon, and shaman projectile art. Troll uses its Windup clip as its attack; Troll Recovery and club-breaking clips, Skull and Turtle guard clips, and Boat are excluded.

All five monk colours reference the same `MonkUnitData` and are mechanically identical. Each supported colour faction owns its matching monk in `Standard4`; it retains `UnitRole.Mythic` but follows the standard-slot purchase lifecycle rather than the picker lifecycle. Monks stop to heal the most-injured valid allied combat unit within range 2, excluding themselves, bases, dead units, and full-health units. They heal 5 health every 3 seconds at one star; heal amount follows the 1x/1.25x/1.5x tier curve while cadence stays fixed. Multiple monks may heal the same target, without overhealing, and resume movement when no valid ally in combat is in range.

### Deferred beyond the core prototype

- Online multiplayer and ranked matchmaking.
- Campaign and persistent progression.
- Multiple currencies.
- Controller support.
- Final art, broad designed-faction production beyond the accepted Fishman/Goblin compositions and five prototype team-colour variants, production-ready saves, and deep optimization.
- Heroes, bosses, neutral objectives, buildings, and powers unless separately approved.

## Prototype success criteria

- A player can understand and complete the full loop against AI.
- At least one economy-versus-military decision materially changes the battle.
- Independent standard-slot and mythic production with three star tiers creates readable strategic choices.
- The HUD accurately communicates all decision-critical state.
- Base destruction and both timeout comparisons produce the correct result.
- The battle can restart without Editor intervention.
- The project compiles and the critical loop is verified in Play Mode.

These criteria define the target; they do not claim the current build satisfies it.

## Open design questions

No material design questions from this reconciliation remain open. Exact per-unit-profile values, slot compositions, and matchups require implementation and playtest tuning.

## Related documents

- `README.md`
- `ROADMAP.md`
- `TODO.md`
- `DECISIONS.md`
- `CHANGELOG.md`
