# Codex Project Instructions

## Project context

This repository contains a Unity 2D game.

Before starting substantial work, read the following files when they exist:

- `GAME_DESIGN.md`
- `ROADMAP.md`
- `TODO.md`
- `DECISIONS.md`
- `CHANGELOG.md`

Also inspect relevant recent Git history and any uncommitted changes when this helps establish current project state.

Do not assume the codebase alone fully describes design intent.

## Sources of truth

Use these files for different kinds of project knowledge:

- `GAME_DESIGN.md` defines what the game is intended to become.
- `ROADMAP.md` defines milestones and planned outcomes.
- `TODO.md` defines immediate actionable work.
- `DECISIONS.md` records important approved or proposed decisions and their rationale.
- `CHANGELOG.md` records meaningful completed changes in human-readable form.
- Git history records the exact implementation history.
- The repository contents record the current implementation.

When these sources conflict, do not silently choose one. Identify the conflict and explain which source appears stale.

## Start-of-session behavior

For a project status or planning request:

1. Read the project-memory files listed above.
2. Inspect `git status`.
3. Inspect recent Git history relevant to the current milestone.
4. Compare the documented state with the actual repository.
5. Identify:
   - the current milestone;
   - recently completed work;
   - incomplete or partially implemented work;
   - blockers;
   - technical risks;
   - the highest-value next tasks.
6. Do not modify files during a read-only review.
7. Do not mark work complete during a review-only task.

For project planning, feature requests, scope changes, milestone questions, or
requests that may affect multiple systems, use `producer_technical_lead` as the
entry point.

For narrow implementation, testing, review, UI, balance, or debugging tasks,
the relevant specialist may be used directly when project direction is already
clear.

## Specialist handoff rules

Specialist agents should report:

- findings;
- files and systems affected;
- assumptions;
- risks;
- recommended follow-up work;
- proposed decision changes;
- validation performed;
- manual Unity steps still required.

When specialist work affects project direction, scope, priorities, milestones,
or accepted conventions, the result should be handed back to
`producer_technical_lead` for reconciliation into the project-memory files.

Specialists should not leave important discoveries only in chat output when
they belong in project tracking.

## Task selection

Do not choose work solely because it is technically interesting.

Prefer work that:

- advances the current milestone;
- unblocks other work;
- improves the core gameplay loop;
- removes a significant technical or production risk;
- can be validated clearly;
- fits the current project scope.

When recommending next work, provide a small ordered set of options rather than a large undifferentiated backlog.

## Before implementation

Before implementing a feature or bug fix:

1. Read the relevant project-memory files.
2. Inspect the existing implementation and applicable instructions.
3. Inspect recent related Git history when useful.
4. Confirm the intended behavior and acceptance criteria.
5. Identify dependencies, scene or prefab impact, and manual Unity Editor steps.
6. Use `game_systems_architect` first when system ownership or architecture is unclear.
7. Avoid multiple write-enabled agents editing the same files concurrently.

## Project coordination and document ownership

`producer_technical_lead` is the primary coordinator for project direction,
scope, priorities, milestones, and project-memory documents.

It owns coordination of:

- `GAME_DESIGN.md`
- `ROADMAP.md`
- `TODO.md`
- `DECISIONS.md`
- `CHANGELOG.md`

When the user requests a material gameplay, scope, architecture, production, or
priority change, use `producer_technical_lead` first unless the task is a narrow
implementation or debugging request with no project-level consequences.

The producer technical lead should:

1. Interpret the user's requested outcome.
2. Inspect the relevant project state and documentation.
3. Identify which project-memory files are affected.
4. Record approved decisions without requiring the user to manually edit files.
5. Update roadmap and TODO priorities where the requested change materially
   affects sequencing or scope.
6. Delegate specialist investigation or implementation where appropriate.
7. Reconcile specialist findings back into the project-memory documents.
8. Avoid treating specialist recommendations as approved decisions unless the
   user has delegated that authority or explicitly approves them.

Specialist agents may recommend documentation changes, but should not
independently redefine project scope, priorities, or milestone direction unless
the producer technical lead has delegated that responsibility for the task.

## Project tracking

After completing a feature, bug fix, refactor, or meaningful production task:

1. Compare the completed work against `TODO.md` and `ROADMAP.md`.
2. Mark an item complete only when its stated acceptance criteria are satisfied.
3. Do not mark an item complete merely because code was written.
4. If manual Unity setup or Play Mode verification remains, keep the item incomplete or mark it partially complete.
5. Add newly discovered follow-up work as separate TODO items.
6. Preserve incomplete portions instead of deleting the entire parent item.
7. Summarize all project-tracking changes in the final response.
8. Do not change project priorities without explaining why.
9. Do not silently add large new features to the roadmap.

Prefer completed checkboxes over deleting work immediately. Completed items may later be archived during roadmap cleanup.

## Roadmap maintenance

Update `ROADMAP.md` when:

- a milestone outcome is completed;
- milestone scope is intentionally changed;
- a new dependency materially affects sequencing;
- a feature is explicitly deferred or removed;
- acceptance criteria need clarification.

Do not rebuild the roadmap after every task.

Do not move work between milestones without explaining the production reason.

When updating a milestone, preserve:

- its intended outcome;
- entry conditions;
- exit criteria;
- dependencies;
- incomplete items.

## TODO maintenance

Update `TODO.md` when:

- a task is completed;
- a task is partially completed;
- new required follow-up work is discovered;
- a blocker is identified;
- priorities are intentionally changed.

Each actionable TODO should include enough context to be understood in a later session.

Prefer entries that state:

- the desired outcome;
- relevant system or file;
- acceptance criteria;
- blockers or dependencies;
- priority.

Avoid vague tasks such as “fix combat” or “improve UI.”

## Decision records

Record a decision in `DECISIONS.md` when it:

- changes architecture or system ownership;
- establishes a project-wide convention;
- selects one substantial approach over another;
- affects saved data or compatibility;
- changes game-design intent;
- creates a constraint future work must respect;
- changes production scope or milestone direction.

Do not record:

- trivial implementation details;
- temporary debugging changes;
- obvious consequences of existing conventions;
- speculative ideas that have not been accepted;
- personal preferences without project impact.

Before recording a decision:

1. Distinguish user-approved decisions from agent-made implementation choices.
2. Clearly label assumptions.
3. Do not treat an unresolved recommendation as settled.
4. Use `Proposed` for material decisions awaiting user approval.
5. Use `Accepted` only after approval or when the user has explicitly delegated that authority.
6. Use `Superseded` when a later decision replaces it.
7. Use `Rejected` when a considered proposal is intentionally declined.

Each decision should include:

- identifier;
- date;
- status;
- decision;
- context;
- rationale;
- consequences;
- alternatives considered;
- links or references to related systems or tasks.

Material changes to game design, scope, architecture, save compatibility, or player experience must not be silently settled.

When such a decision arises, present:

- the recommended option;
- alternatives;
- tradeoffs;
- a recommended default.

## Delegated authority

The user delegates routine project-maintenance authority to
`producer_technical_lead`.

The producer technical lead may:

- update TODO status and acceptance criteria based on verified repository state;
- correct stale roadmap wording;
- add discovered technical follow-up work;
- update changelog entries after completed work;
- record direct user instructions as Accepted decisions;
- record agent recommendations as Proposed decisions;
- resolve minor sequencing and task-breakdown choices.

The producer technical lead must ask for approval before accepting decisions
that materially change:

- the core player experience;
- monetization;
- supported platforms;
- multiplayer scope;
- save compatibility;
- major architecture;
- milestone scope;
- release commitments;
- substantial new features or removal of approved features.

## Changelog maintenance

Update `CHANGELOG.md` after meaningful completed work.

Record:

- player-visible features;
- meaningful bug fixes;
- important technical changes;
- test or tooling improvements that affect project quality;
- notable content or UI changes;
- breaking changes or migration requirements.

Do not record:

- every tiny edit;
- formatting-only changes;
- temporary debugging;
- failed experiments;
- internal implementation details with no lasting relevance.

Use the current development version or the `Unreleased` section unless the user specifies a release version.

Group entries under:

- Added
- Changed
- Fixed
- Removed
- Technical
- Known Issues

Keep entries concise but specific.

Git history remains the exact implementation record. The changelog is the human-readable summary.

## Completion rules

A task is not complete until all applicable conditions are met:

- implementation is present;
- C# compilation has been checked where possible;
- relevant tests have been run or limitations stated;
- required manual Unity Editor setup is documented;
- Play Mode verification steps are provided;
- acceptance criteria are satisfied;
- project tracking is updated;
- important decisions are recorded;
- meaningful completed work is added to the changelog.

Do not claim Unity, tests, builds, scenes, prefabs, or gameplay were verified unless they were actually run or inspected.

## Unity repository safety

- Do not edit `Library/`, `Temp/`, `Logs/`, `obj/`, or generated solution/project files.
- Preserve `.meta` files.
- Consider Unity asset GUID references before moving or renaming assets.
- Do not hand-edit binary Unity assets.
- Do not change package versions without explaining why.
- Do not silently change Project Settings.
- Do not claim an Inspector reference, scene, prefab, Animator Controller, Input Action Asset, or Project Setting was updated unless verified.
- Clearly identify manual Unity Editor work.

## Git behavior

Do not create commits unless explicitly asked.

Before changing files, inspect uncommitted work when relevant and avoid overwriting unrelated user changes.

Use Git history to understand recent implementation, but do not treat commit history as a replacement for game-design intent, project priorities, or decision rationale.

## Final response after implementation

Report:

- what was implemented;
- files changed;
- validation performed;
- manual Unity verification required;
- TODO and roadmap updates;
- decisions added or changed;
- changelog entries added;
- remaining risks or follow-up work.
