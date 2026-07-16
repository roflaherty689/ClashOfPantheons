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

There is no direct unit control. The prototype is primarily single-player against AI. The current build instead drives both armies through automatic spawn logic and has no strategic AI.

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

1. Both sides automatically spawn free units at a shared interval using a fixed cycle or weighted random selection.
2. Units march, acquire targets, and fight automatically.
3. Workers independently mine and deposit gold.
4. Destroying a base ends the match and displays the winner.

Gold is not connected to military spawning, the runtime UI does not select production, and no timer, AI strategy, timeout tiebreaker, upgrade, or restart flow exists.

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
| Mythic | Expensive faction-defining power unit | High-stat prototype role exists; final counterplay unresolved |

Heroes and bosses are not prototype scope.

### Health, failure, and match result

**Implemented:** Units die at zero health. Destroying a stronghold immediately ends the match and stops autonomous units and workers.

**Accepted but not implemented:** A match also ends when the timer expires. The healthier stronghold wins. If health is equal, the side with the lower total value of units lost wins. Lost-unit value is the purchase cost of that unit's production slot multiplied by the number of units of that type destroyed. If both comparisons are equal, the match is a draw with no winner or loser.

The result must support a restart or rematch without Editor intervention.

### Economy and production

**Implemented:** Workers repeatedly travel to a gold vein, mine, return, and deposit gold. Starting and maximum worker counts are configurable, and the underlying manager can spend a supplied gold cost to buy a worker.

**Accepted but not implemented:** Gold is the only prototype currency. It funds workers, five independent unit-production types, and three star tiers of in-match upgrades. Favour and essence shown in the rough HUD are not prototype systems.

### Progression and persistence

Each role begins locked. Its first purchase unlocks continuous one-star production; its next two purchases upgrade future spawns to two and three stars. Star tiers multiply all configured unit stats except purchase cost by 1×, 1.5×, and 2×. Existing fielded units do not change when a role is upgraded.

Each role's purchase cost and recurring production cadence are configured in its `UnitData` asset. `UnitData` currently contains `cost`, but cadence is not yet represented there; the prototype currently uses a global spawn interval. Initial values remain balance tuning rather than an unresolved ownership decision.

Campaign and persistent progression are deferred beyond the core prototype. No save system is required for the core loop.

## World and match structure

- **Prototype battlefield:** One shared horizontal lane with a stronghold and resource area for each side.
- **Opponent:** Single-player against AI.
- **Target length:** Approximately five minutes.
- **Transitions:** No player-facing menu or transition flow is implemented.
- **Replay:** Restart or rematch after the result; not implemented.
- **Later possibilities:** Alternative maps, lane structures, or endless mode may be reconsidered after prototype validation.

## AI and difficulty

The AI should use the same economy, production, upgrade, and match rules as the player. Its decision model and difficulty controls are unresolved. Possible strategic profiles such as economy-focused or early pressure are ideas, not accepted prototype requirements. Online matchmaking is deferred with multiplayer.

## UI and feedback

**Implemented:** Units and bases display damage health bars; base hits shake the base visual; animated attacks and projectile arcs provide combat feedback; victory text identifies the winning team.

**Rough concept only:** The scene HUD includes static base health, timer, gold/favour/essence, workers, unit cards, a shared queue, category tabs, battle information, and speed controls. Its values and buttons are mostly disconnected. Favour, essence, and the shared FIFO queue conflict with accepted prototype direction.

The functional prototype HUD must communicate gold, worker purchase state, independent production, star tiers, remaining time, base health, and the final result. A results/restart flow is required. Menus, onboarding, and most feedback remain unimplemented.

## Audio and visual direction

Use the colourful, humorous Tiny Swords pixel-art style as the prototype direction. Replacement or supplementary art should preserve that tone while using iconic mythological or cultural details where useful; strict historical accuracy is not the goal. Final art and broad faction production are deferred.

No project-specific audio system or direction is implemented. Music/SFX controls and audio content are later work.

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

### Deferred beyond the core prototype

- Online multiplayer and ranked matchmaking.
- Campaign and persistent progression.
- Multiple currencies.
- Controller support.
- Final art, broad faction production, production-ready saves, and deep optimization.
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
