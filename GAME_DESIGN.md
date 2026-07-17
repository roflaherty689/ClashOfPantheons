# Game Design

## Document purpose

This file defines the intended player experience, rules, systems, and scope. **Implemented** means repository evidence exists, though Play Mode validation may still be pending. **Accepted direction** means user-approved intent that may not yet be implemented. **Proposed** items await approval.

---

## Game summary

**Working title:** Clash of Pantheons

**Genre:** 2D mythological tug-of-war strategy autobattler

**Target platforms:** PC storefronts first, with mobile as the second platform target. Mouse input has prototype priority; touch adaptation follows for mobile. Controller support remains deferred.

**Target audience:** Broad-age strategy players interested in mythology or ancient warfare; exact age rating and audience positioning remain unconfirmed.

**Current stage:** Prototype — mechanics exist but the player-facing loop is not integrated.

**Pitch:** Build a mythological war economy, shape an autonomous army, and push along a contested lane to destroy the rival stronghold before time runs out.

## Player fantasy

The player indirectly commands a mythological faction. They invest in workers, unit production, composition, and upgrades, then watch autonomous forces turn those strategic choices into an escalating push.

There is no direct unit control. The prototype is primarily single-player against AI. The enemy now has a source-implemented strategic purchasing policy; full Play Mode validation remains pending.

## Design pillars

1. **Strategic economy**
   - Economic investment creates long-term strength at the cost of immediate pressure.
   - Excludes passive income with no meaningful tradeoffs.

2. **Army composition and counterplay**
   - Five readable roles have distinct purposes, costs, strengths, and weaknesses.
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
3. Each purchased unit type produces independently; spawned units march and fight autonomously.
4. The player reads the battle and adapts investment, composition, and upgrades.
5. A stronghold is destroyed or the approximately five-minute timer expires.
6. The result is shown and the player can restart.

### Current prototype loop — implemented, not Play Mode verified in this pass

1. Both sides begin with every production role locked. The player and AI spend their own gold to unlock independent per-role `UnitData` cadences. A legacy free-spawn pattern remains available only for prototype/debug testing.
2. Units march, acquire targets, and fight automatically.
3. Workers independently mine and deposit gold.
4. Destroying a base or reaching the time limit ends the match and displays the result.

The player HUD now presents live economy and stronghold state, a five-minute countdown, timeout results, restart, and production purchasing in source. Both teams start with locked roles and spend their own gold to unlock or upgrade independent tracks. The enemy AI purchasing policy is implemented in source but still needs Play Mode verification and tuning. Production purchasing remains to be broadly Play Mode verified. Countdown expiry and the lost-unit-value timeout resolver were Play Mode verified in a shortened 20-second match on 2026-07-17; the remaining result branches still need targeted verification.

**Short-loop rhythm:** Worker trips and independent recurring production cadences. Buying a locked role unlocks its continuous production; later purchases advance that role to two and three stars.

**Session:** One approximately five-minute match on one shared horizontal lane.

**Primary reward:** Battlefield momentum and match victory. Campaign and persistent rewards are deferred.

## Player actions

- **Buy a worker:** Spend gold to increase future income, limited by cost and worker capacity.
- **Establish or improve production:** Spend gold on melee, archer, cavalry, siege, or mythic production. Each type operates independently rather than through one shared FIFO queue.
- **Upgrade a role:** Spend gold to advance that role through one-, two-, and three-star tiers.
- **Read and adapt:** Observe gold, production, upgrades, time, base health, composition, and momentum.

The first purchase unlocks one continuously producing track for that role. The second and third purchases advance future spawns to two and three stars respectively. Purchases do not enter a shared queue and upgrades do not alter units already on the field. Direct movement and combat commands are not player actions.

## Game rules

### Movement and combat

**Implemented:** The battlefield is a horizontal lane. Left units advance right and right units advance left. Units prioritize the nearest enemy unit in range, then the enemy base. Attacks use configured health, damage, range, speed, attack rate, projectiles, and unit/building damage modifiers. Friendly units make limited vertical separation adjustments.

### Unit roles and counterplay

The five accepted roles are melee, archer, cavalry, siege, and mythic. All five role mappings and prototype prefabs exist. A favourable role matchup applies a 1.2× damage multiplier. Exact matchups beyond those listed below remain balance work.

| Role | Intended identity | Implementation status |
|---|---|---|
| Melee | Frontline; intended strong versus cavalry and weak versus archers | Prototype role exists |
| Archer | Ranged support; intended strong versus melee and weak versus cavalry | Projectile role exists |
| Cavalry | Fast pressure; intended strong versus archers and weak versus melee | Prototype role exists |
| Siege | Structure specialist, vulnerable without support | Projectile role and building modifier exist |
| Mythic | Expensive selectable power unit; initial roster supports testing and counter-selection | Shared Minotaur prototype exists; monk healing and the broader selectable roster are accepted but not implemented |

Heroes and bosses are not prototype scope.

### Health, failure, and match result

**Implemented:** Units die at zero health. Destroying a stronghold immediately ends the match and stops autonomous units and workers.

**Implemented in source, partially Play Mode verified:** A match also ends when the configurable timer expires. The healthier stronghold wins. If health is equal, the side with the lower total value of units lost wins. Lost-unit value is accumulated from the purchase cost of each destroyed unit's production slot, with per-role death counts retained for inspection. If both comparisons are equal, the match is a draw with no winner or loser. Countdown expiry and the lost-value resolver were user-verified in a shortened match on 2026-07-17.

The result overlay identifies victory, defeat, or draw and its resolution condition. Its restart action reloads the active battle scene to reset current runtime state; this path still requires Play Mode verification.

### Economy and production

**Implemented:** Workers repeatedly travel to a gold vein, mine, return, and deposit gold. Starting and maximum worker counts, worker cost, base gold per trip, and a future-facing income-upgrade multiplier are configurable on `WorkerManager`. The player starts with one worker in the active scene, and the live HUD can buy workers up to the five-worker capacity while displaying current gold, worker count, and aggregate income per trip.

**Accepted but not implemented:** Gold is the only prototype currency. It funds workers, five independent unit-production types, and three star tiers of in-match upgrades. Favour and essence shown in the rough HUD are not prototype systems.

### Progression and persistence

Each role begins locked. Its first purchase unlocks continuous one-star production; its next two purchases upgrade future spawns to two and three stars. Star tiers multiply all configured unit stats except purchase cost by 1×, 1.5×, and 2×. Existing fielded units do not change when a role is upgraded.

Each role's purchase cost and recurring production cadence are configured in its `UnitData` asset. `GameManager` can run either the legacy global spawn pattern or independent per-role timers for prototype testing. Purchase-slot activation gates the intended per-role mode, and a newly unlocked track begins a fresh cadence rather than inheriting locked time. Initial values remain balance tuning rather than an unresolved ownership decision.

Campaign and persistent progression are deferred beyond the core prototype. No save system is required for the core loop.

## World and match structure

- **Prototype battlefield:** One shared horizontal lane with a stronghold and resource area for each side.
- **Opponent:** Single-player against AI.
- **Target length:** Approximately five minutes.
- **Transitions:** Implemented in source: title, player-faction selection, difficulty selection, then battle. Difficulty defaults to Easy; the opponent randomly receives a different configured faction.
- **Replay:** Restart or rematch after the result; not implemented.
- **Later possibilities:** Alternative maps, lane structures, or endless mode may be reconsidered after prototype validation.

## AI and difficulty

The AI uses the same worker, gold-spending, production, tier, mythic-selection, spawn, and match rules as the player. Easy makes slower, partly idle/random decisions and receives no starting bonus. Medium establishes production before each early worker investment, then balances workers with varied production at a moderate cadence, and receives +50 starting gold. Hard makes faster decisions, requires a broader military opening before expanding its economy, and receives +150 starting gold. The bonus applies once to the enemy only. The AI randomly chooses a valid mythic and a configured faction other than the player's faction. This system is implemented in source and awaits full Play Mode tuning and verification. Online matchmaking remains deferred.

## UI and feedback

**Implemented:** Units display world-space damage health bars. Stronghold health is shown as live current/maximum text and proportional bars in the battle HUD, without duplicate world-space bars above the bases. Base hits shake the base visual; animated attacks and projectile arcs provide combat feedback; victory text identifies the winning team.

**Partially implemented:** The redesigned scene HUD presents the accepted single-gold economy and independent role cards. Its player gold, aggregate worker income, worker count, buy-worker button, five production purchase controls, locked/producing and tier states, affordability, greyed locked art, both stronghold-health displays, match timer, result overlay, resolution reason, and restart button are live in source. Production purchasing requires Play Mode verification. The shortened countdown and lost-value result were Play Mode verified on 2026-07-17; the other result and restart paths remain targeted verification work.

The functional prototype HUD must communicate gold, worker purchase state, independent production, star tiers, remaining time, base health, and the final result. A results/restart flow is required. The accepted initial front-end flow is a title screen with Play and Exit, then a faction-selection screen whose clickable options are generated from configured `FactionData` assets. The chosen player faction must drive the existing faction-owned battle presentation and unit mappings. Basic onboarding and most feedback remain unimplemented.

## Audio and visual direction

Use the colourful, humorous Tiny Swords pixel-art style as the prototype direction. Replacement or supplementary art should preserve that tone while using iconic mythological or cultural details where useful; strict historical accuracy is not the goal. Five mechanically identical Tiny Swords team-colour variants (black, blue, purple, red, and yellow) are available for prototype presentation. Each variant owns matching Castle and House3 hand-in sprites, an animated worker prefab, and animated melee/archer prefabs; cavalry, siege, and mythic remain shared. The selected faction drives its world presentation, HUD stronghold icon, and spawned worker colour. These palette variants are not five designed mythological factions. Final art and broad faction production remain deferred.

No project-specific audio system or direction is implemented. Music/SFX controls and audio content are later work.

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
- Gold-funded workers, five independent unit-production types, and three star tiers.
- Meaningful economy-versus-pressure and composition decisions.
- Approximately five-minute matches with base-destruction and timeout results.
- Minimal functional HUD and clear feedback.
- Play Mode verification of the critical path.

### Should have

- At least one coherent mythological faction presentation.
- Basic onboarding for the complete loop.
- Basic readability and colour-independent state feedback.

### Could have after core validation

- Additional factions or maps.
- Battle-speed controls for solo play or testing.
- Additional unit options if the five-role model proves insufficient.

### Accepted mythic-roster test expansion

Unlocking mythic production opens a choice menu before gold is spent. Selecting an option atomically purchases the unlock; later purchases upgrade that chosen option for the rest of the match. Each option's `UnitData` owns its cost and production cadence. The initial menu includes every combat-capable Tiny Swords Enemy Pack unit with usable idle, movement, and attack-equivalent animation, plus the five colour monks, so the roster can be reduced after review and used to test counter-selection when the opponent reveals a mythic choice first.

Most first-pass enemy-pack options share the Minotaur's melee behavior and balance baseline, while retaining independent data assets for later tuning. Gnoll, Harpoon Fish, and Shaman are ranged mythics and use the bone, harpoon, and shaman projectile art supplied beside their source animations rather than the standard archer arrow. Troll uses its Windup clip as its attack; Troll Recovery and club-breaking clips, Skull and Turtle guard clips, and Boat are excluded. Monks are the support exception: they stop to heal the most-injured valid allied combat unit within range 2, excluding themselves, bases, dead units, and full-health units. They heal 5 health every 3 seconds at one star; heal amount follows the 1x/1.5x/2x tier curve while cadence stays fixed. Multiple monks may heal the same target, without overhealing, and resume movement when no valid ally in combat is in range.

### Deferred beyond the core prototype

- Online multiplayer and ranked matchmaking.
- Campaign and persistent progression.
- Multiple currencies.
- Controller support.
- Final art, broad designed-faction production beyond the accepted test-oriented mythic roster and five prototype team-colour variants, production-ready saves, and deep optimization.
- Heroes, bosses, neutral objectives, buildings, and powers unless separately approved.

## Prototype success criteria

- A player can understand and complete the full loop against AI.
- At least one economy-versus-military decision materially changes the battle.
- Independent production and three star tiers create readable strategic choices.
- The HUD accurately communicates all decision-critical state.
- Base destruction and both timeout comparisons produce the correct result.
- The battle can restart without Editor intervention.
- The project compiles and the critical loop is verified in Play Mode.

These criteria define the target; they do not claim the current build satisfies it.

## Open design questions

No material design questions from this reconciliation remain open. Exact per-role values and matchups require implementation and playtest tuning.

## Related documents

- `README.md`
- `ROADMAP.md`
- `TODO.md`
- `DECISIONS.md`
- `CHANGELOG.md`
