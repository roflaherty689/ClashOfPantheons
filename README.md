# Clash of Pantheons

A mythological tug-of-war autobattler where ancient civilizations and legendary creatures clash for dominance.

Players build their economy, recruit armies, unlock powerful units, and adapt their strategy to overcome opposing factions. Victory is achieved by destroying the enemy stronghold.

---

## Vision

Clash of Pantheons combines elements of:

- Tug-of-War strategy games
- Autobattlers
- RTS economy management
- Mythological faction warfare

The goal is to create a game where:

- Economic decisions matter
- Unit composition matters
- Counterplay matters
- Mythological factions feel unique
- Matches evolve from small skirmishes into massive battles

---

## Core Gameplay Loop

1. Generate resources
2. Invest in economy or military
3. Train units automatically
4. Units march toward the enemy
5. Armies clash
6. Gain map control
7. Unlock stronger units and upgrades
8. Destroy the enemy base

---

## Design Pillars

### Strategic Economy

Players must balance:

- Expansion
- Upgrades
- Unit production
- Technology progression

A stronger economy creates long-term advantages but leaves players vulnerable early.

### Army Composition

Different units counter each other.

Examples:

| Unit Type    | Strong Against | Weak Against |
|--------------|----------------|--------------|
| Spearmen     | Cavalry        | Archers      |
| Cavalry      | Archers        | Spearmen     |
| Archers      | Infantry       | Cavalry      |
| Siege        | Buildings      | Fast units   |
| Mythic Units | Situational    | Situational  |
| Heroes       | Mythic Units   | Nothing      |

### Mythological Identity

Every faction should feel distinct.

Example Greek faction:

- Hoplite
- Archer
- Hippeis
- Helepolis
- Minotaur

Future factions may include:

- Norse
- Egyptian
- Celtic
- Roman
- Chinese
- Japanese

---

## Planned Features

### Phase 1 - Prototype

- Single lane battlefield
- Two bases
- Basic economy
- Melee units
- Ranged units
- Unit combat
- Win/Loss conditions

### Phase 2 - Alpha

- Multiple factions
- Upgrades
- Mythological units
- Neutral objectives
- Save system

### Phase 3 - Beta

- Multiplayer
- Ranked matchmaking
- Additional maps
- Visual polish
- Audio and music

---

## Build a shareable Windows version

Requirements:

- Windows
- Unity `6000.4.11f1` with Windows Build Support installed
- The project must be closed in the Unity Editor while the command runs

From the repository root, run:

```powershell
.\Build-Windows.cmd
```

From a WSL Ubuntu terminal, the equivalent command is:

```bash
make build
```

To build the runnable folder without creating the shareable ZIP:

```bash
make build-no-archive
```

The command builds the enabled scenes in their required playable order (title menu, then battle) and creates:

- `Builds\Windows\ClashOfPantheons\` — the complete runnable player folder
- `Builds\Windows\ClashOfPantheons-Windows-x64.zip` — the archive to share
- `Logs\Builds\Windows-x64-<timestamp>.log` — the retained Unity build log

Friends should extract the ZIP and run `ClashOfPantheons.exe` without removing it from the accompanying data and runtime files.

Optional arguments:

```powershell
.\Build-Windows.cmd -SkipArchive
.\Build-Windows.cmd -OutputPath "Builds\Playtest\ClashOfPantheons"
.\Build-Windows.cmd -UnityPath "C:\path\to\Unity.exe"
```

For safety, custom output paths must remain beneath this repository's ignored `Builds` directory. A failed rebuild preserves the last successfully promoted player folder.

---
