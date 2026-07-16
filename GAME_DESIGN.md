# Game Design

## Document purpose

This file defines what the game is intended to become. It describes player experience, rules, systems, scope, and design intent rather than technical implementation.

Statements labelled **Confirmed** are supported by the current game, repository history, or the existing project vision. Statements labelled **Provisional** are the clearest direction implied by those sources but still require approval. Unresolved ideas remain in **Open design questions** and are not committed scope.

---

## Game summary

**Working title:**  
Clash of Pantheons

**Genre:**  
**Provisional:** 2D mythological tug-of-war strategy autobattler with light RTS economy management.

**Target platform(s):**  
Whatever unity can build out to, preferably phones / steam store

**Target audience:**  
Any age. Likely people with interest in strategy games, mythology or ancient warfare.

**Current production stage:**  
- [ ] Concept
- [x] Prototype
- [ ] Vertical slice
- [ ] Production
- [ ] Content complete
- [ ] Stabilization
- [ ] Release preparation

The combat and resource-gathering mechanics exist, but the player-facing economy and army-selection loop is not yet integrated. The project is therefore an active, partially integrated prototype rather than an unstarted one.

**One-sentence pitch:**  
**Provisional:** Build a mythological war economy, shape an autonomous army, and push along a contested lane to destroy the rival stronghold.

---

## Player fantasy

**Provisional:** The player commands a mythological faction indirectly. They grow its economy, decide when to invest in workers or military forces, shape the composition of an autonomous army, and watch that army turn their strategic choices into an escalating push toward the enemy stronghold.

The satisfying moments should come from making a timely investment, fielding an effective counter or composition, breaking an enemy formation, and seeing a modest force grow into a decisive advance.

The intended degree of player control is not yet confirmed. The current prototype runs both armies automatically; direct control of units or an on-field avatar is not established.

---

## Design pillars

1. **Strategic economy** — **Confirmed direction**
   - What this means: Building the economy should create long-term strength at the cost of immediate battlefield pressure. Timing and opportunity cost should matter.
   - What this excludes: Passive resource accumulation with no meaningful decisions, and economy systems disconnected from the battle.

2. **Army composition and counterplay** — **Confirmed direction**
   - What this means: Unit roles should have understandable strengths, weaknesses, costs, and battlefield purposes. Adapting the army should change the outcome.
   - What this excludes: One universally dominant lineup or roles that differ only cosmetically.

3. **Mythological faction identity** — **Confirmed direction**
   - What this means: Civilizations and legendary creatures should give each faction a recognizable visual and mechanical identity.

4. **Readable, escalating battles** — **Provisional**
   - What this means: Matches should develop from small clashes into larger confrontations while keeping the battle line, unit roles, damage, and momentum understandable.
   - What this excludes: Opaque combat, uncontrolled visual clutter, or spectacle that hides strategic consequences.

---

## Core loop

### Intended player loop — provisional

1. Workers gather gold for the player's faction.
2. The player chooses between improving future income and committing resources to military units.
3. Recruited units deploy, advance, acquire targets, and fight autonomously.
4. The player reads the battle and adapts future recruitment or investment.
5. A successful army pushes through opposing forces and destroys the enemy stronghold.

### Current prototype loop — confirmed

1. Both sides automatically spawn units at intervals, using a fixed cycle or weighted random selection.
2. Units march toward the opposing base and automatically fight enemies in range.
3. Workers independently mine and deposit gold for each side.
4. Destroying a base ends the match and declares the opposing team the winner.

Gold does not currently pay for military spawning, and the player cannot yet select an army composition through the runtime UI.

**Short-loop duration:**  
The recurring rhythm is worker trips and timed unit spawns, players battle for control of the field, and aim to destroy the enemy base

**Session structure:**  
**Provisional:** One self-contained battle between two strongholds. Match should be about 5 minutes. Potentially an endless game mode. If time runs out before a base is destroyed, the base with the least damage to it wins, if they've taken the same damage then the player who lost the least units value wins. 

**Primary reward:**  
**Provisional:** Gaining battlefield momentum and ultimately destroying the enemy stronghold. No persistent or meta-progression reward is established.

---

## Player actions

The current runtime has no functional strategic player inputs. Units and workers move and act autonomously.

### Intended actions — provisional

- **Buy a worker:** Spend gold to increase future income, limited by worker cost and capacity.
- **Recruit or queue a unit:** Spend resources to influence upcoming army composition; units then spawn every x seconds, depending on the units config and upgrades. Each unit will spawn independantly and not influence each other.
- **Read and adapt:** Observe resources, base health, unit roles, and battlefield momentum to choose the next investment.
- **Control battle speed:** Shown in the rough UI, but not yet confirmed as a player feature or implemented.

Direct unit movement is not an established player action.

---

## Game rules

### Movement

**Confirmed:** The battlefield is a horizontal lane. Units spawned on the left advance right; units spawned on the right advance left. Movement and combat are autonomous. Friendly units make small vertical adjustments while fighting to reduce overlap, constrained within a narrow lane.

### Combat

**Confirmed:** Units search within their attack range, prioritize the nearest enemy unit, and target the enemy base when no enemy unit is in range. Attacks use role-specific health, damage, range, speed, and attack-rate values. Some units apply damage directly; ranged units can use travel-time projectiles. Damage can be modified according to whether the target is a unit or building.

Five role slots currently exist:

- Melee
- Archer
- Cavalry
- Siege
- Mythic

The detailed rock-paper-scissors counter table in the original project vision is **not yet implemented or approved as the final counter model**. Current role differences are primarily statistical, with range, projectiles, speed, and building-damage modifiers.

### Health, damage, and failure

**Confirmed:** Units and bases have health. Units are removed at zero health. When a base reaches zero health, the opposing team wins and autonomous units and workers stop acting.

**Incomplete:** A player loss presentation, restart/rematch flow, and full game-over UI behavior are not established.

### Interaction

**Confirmed:** There is no direct interaction with battlefield units in the current runtime.

**Provisional:** The principal interaction should be strategic purchasing and recruitment through the battle UI, not direct micro-control. This requires confirmation.

### Progression

No in-match unlocking, upgrades, technology tree, campaign progression, or persistent progression is currently implemented. The earlier vision mentions stronger units and upgrades, and this will manifest in currently 3 tiers of upgrade for each unit, 1 star 2 stars and 3 stars.

### Resources and economy

**Confirmed:** Each side can have workers that repeatedly travel to a gold vein, mine for a duration, return to a drop-off point, and deposit gold. Workers have a configurable starting count and maximum capacity. The underlying system can purchase an additional worker for a supplied gold cost.

**Incomplete:** Gold is not connected to unit spawning or the visible HUD. The rough UI displays favour and essence, but no runtime systems implement those resources; they are not committed design.

### Saving and persistence

No saving or persistence is established. Whether the game needs persistent progress beyond individual matches is an open decision.

---

## World and level structure

**World structure:**  
No campaign, map selection, or connected world is established. This can be expanded upon later. Currently no plans for a campaign, map may have differing tiles or field lengths / widths / potential room for allies.

**Level structure:**  
**Confirmed prototype:** One 2D battlefield with a stronghold and resource area for each side and a shared combat lane.

**Checkpoints:**  
Not applicable to the current self-contained battle; no future need for it unless battle length increases dramatically and saving is required.

**Scene transitions:**  
No player-facing scene-transition flow is established. The prototype uses a single battle scene.

**Replay structure:**  
**Provisional:** Restart or rematch the battle after a win or loss. No implementation currently exists.

Whether the single lane is a permanent design constraint or only a prototype constraint remains open.

---

## Enemies and challenges

### Enemy principles

**Confirmed:** The opposing force uses the same autonomous movement, targeting, combat, faction, and economy framework as the player's side.

**Provisional:** Challenge should arise from economic pressure, army composition, timing, and faction capabilities rather than demanding direct unit control. Initiall the oponent will be AI, with the option for online players eventually. AI player will always be supported for offline play.

### Unit roster

These are role slots rather than a final faction roster.

| Role | Current behavior | Intended counterplay | Status |
|---|---|---|---|
| Melee | Short-ranged frontline combatant | Should anchor or screen an army; strong against cavalry, weak against archer | Implemented prototype role |
| Archer | Ranged projectile attacker | Should pressure suitable targets from behind a frontline; strong against melee, weak against cavalry | Implemented prototype role |
| Cavalry | Fast combatant | Intended to create composition counterplay; strong against archer, weak against melee | Implemented prototype role |
| Siege | Ranged projectile unit with increased building damage in current data | Strong against structures and weak to everything else | Implemented prototype role |
| Mythic | Expensive high-stat role with faction-specific creature visuals | Intended as a powerful situational unit; no counterplay | Implemented prototype role |

Faction data can map each role to a different prefab. Existing default and humorous placeholder factions demonstrate the system but do not establish the final civilization roster or tone.

### Bosses and heroes

No boss or hero system exists. Mythic units are a role, not confirmed bosses. Heroes appeared in an earlier example counter table but are not committed scope.

---

## Progression and rewards

**Confirmed current match progression:** Workers accumulate gold while repeated unit waves create changing battlefield pressure.

**Provisional intended progression:** Economic investment should increase the player's future options, while military investment should create immediate pressure. Stronger or more specialized units may become available during a match, but unlock conditions and upgrade rules are unresolved.

No campaign, account, unlock, or other persistent reward structure is established.

---

## Difficulty

**Default difficulty intent:**  
**Provisional:** Strategic decisions should be understandable, with depth coming from timing, investment, composition, and adaptation rather than high input speed.

**Difficulty progression:**  
Room for AI difficulty, likely speed of procurement of units / upgrades. Potentially room for AI types, i.e. economy focussed, cavalry rush etc. Matchmaking will be entirely randomised

**Accessibility or assist options:**  
Battle-speed controls are represented only in the rough UI and are not functional. Eventually a tutorial battle will bring the user through the controls.

---

## UI and player feedback

### HUD

**Confirmed functional feedback:** Units and bases show health bars after taking damage, and a text message announces the winning team.

**Rough, non-functional concept:** The battle scene contains a temporary HUD showing base health, a timer, gold/favour/essence, workers, unit cards and costs, a production queue, battle information, category tabs, and speed controls. Most values are static and its buttons are not connected to gameplay. These elements must not be treated as approved systems solely because they appear in the mock-up.

### Menus

No main menu, settings, pause flow, match setup, results screen, or restart flow is established.

### Tutorials and onboarding

No tutorial or onboarding is established. The prototype target should eventually explain the economy-versus-army decision, autonomous deployment, role differences, and victory condition.

### Feedback

**Confirmed:** The prototype uses animated movement and attacks where assets support them, fallback attack motion, projectile arcs, color-changing health bars, base hit shake, team colors, and victory text.

There will be audio feedback eventually. No controller feedback. There should be base damage feedback, with the outline of the screen getting a red hue. 

---

## Audio and visual direction

### Visual direction

**Confirmed prototype language:** Colorful 2D pixel-art medieval/fantasy units, buildings, terrain, and effects, with readable left-versus-right team presentation.

Tiny swords stlye should be used as the primary art direction until better assets are found, or supplimentary material is provided. It should keep the same tone and humour though and not lean into historical accuracy, though there may be elements taken from certain factions that are iconic and used, like a greek hoplites plume

### Audio direction

No project-specific audio direction or implemented audio system is established.

---

## Accessibility goals

These are open design commitments, not implemented features.

- Input remapping: Should be touch controls for a phone / mouse clicks. Potentially keyboard shortcuts down the line.
- Controller support: Initially none, can implement proper support alongside the shortcuts.
- Keyboard and mouse support: yes
- Text readability: Required in principle
- Color-independent feedback: Needed because team identity currently relies partly on red/blue; solution - the tiny swords pack comes with 5 faction colours, these should be utilised.
- Reduced motion: No
- Screen shake controls: No
- Flashing controls: Some, just for edge of screen currently
- Subtitle requirements: Dependent on future voiced or essential audio content, but likely no.
- Audio controls: Yes, for music / sound effects.
- Difficulty assists: None

---

## Scope

### Must have — proposed prototype target

- One complete, playable single-lane match between two strongholds.
- A functional gold economy with workers and a meaningful economy-versus-military choice.
- Player-directed recruitment or composition selection tied to unit costs.
- A small set of mechanically distinct autonomous unit roles.
- Readable autonomous movement, targeting, combat, damage, and base destruction.
- A clear win/loss result and restart or rematch flow.
- A minimal live HUD showing the information required to make decisions.

### Should have — requires approval

- At least one coherent mythological faction presentation rather than placeholder identity.
- An opposing force capable of testing the player's economic and composition decisions.
- Basic onboarding for the full prototype loop.
- Basic input, readability, and color-independent feedback support for the chosen target platform.

### Could have — uncommitted

- Additional factions or battlefields after the core loop is validated.
- In-match upgrades or additional unit options after recruitment and counterplay work.
- Additional resources only if gold alone cannot support the intended decisions.
- Battle-speed controls for solo play or testing.

### Explicit non-goals for the current prototype

- Ranked matchmaking or a live-service structure.
- Broad faction or map production before one complete loop is validated.
- A production-ready save or meta-progression system.
- Final art, audio, balance, or visual polish.
- Heroes, bosses, neutral objectives, buildings, powers, or multiple currencies unless separately approved.
- Direct control or micro-management of individual combat units unless the intended player role changes.

Multiplayer appears in an earlier aspirational plan but is not accepted scope here; its long-term status requires a separate decision.

---

## Success criteria

### Proposed prototype success criteria

The game is considered successful for the current target when:

- A player can understand and complete one economy → recruitment → autonomous combat → base-destruction loop.
- At least one real economy-versus-military decision materially changes the battle outcome.
- Unit roles and their battlefield consequences are readable enough to support adaptation.
- The battle clearly communicates resources, base health, unit deployment, and win or loss.
- A completed battle can be restarted or replayed without manual Editor intervention.

These criteria describe the recommended prototype target; they do not claim the current build already satisfies it.

---

## Open design questions

- [x] Single player vs AI. Online multiplayer is a future feature.
- [x] Players purchase units which enter a spawn queue. All military units cost gold.
- [x] Gold is the only gameplay currency for the prototype.
- [x] Target platforms: PC (Steam) and Mobile.
- [x] Tone: colourful, humorous mythology.
- [ ] Exact combat counter model still to be designed.
- [x] Single lane is the core gameplay.
- [x] 5 minute matches with tie-break rules.
- [x] No persistent progression in prototype.
- [x] Multiplayer is a long-term goal.

---

## Related documents

- `README.md`
- `ROADMAP.md`
- `TODO.md`
- `DECISIONS.md`
- `CHANGELOG.md`
