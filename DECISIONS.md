# Project Decisions

## Purpose

This file records important design, architecture, compatibility, and production decisions.

Use the following statuses:

- **Proposed** — under consideration and not yet approved.
- **Accepted** — approved and currently active.
- **Superseded** — replaced by a later decision.
- **Rejected** — considered and intentionally declined.

Do not record trivial implementation details.

---

## Decision index

| ID | Title | Status | Date |
|---|---|---|---|
| DEC-001 | Project decision-record format | Accepted | YYYY-MM-DD |

---

## DEC-001 — Project decision-record format

**Date:** YYYY-MM-DD  
**Status:** Accepted

### Decision

Important project decisions will be recorded in this file using the structure shown below.

### Context

The project needs durable context that can be understood across separate development sessions.

### Rationale

Git history records implementation changes, but it does not always preserve design intent, alternatives, or production tradeoffs.

### Consequences

- Material design and architecture choices should be documented.
- Proposed decisions must not be treated as accepted.
- Superseded decisions must reference their replacement.

### Alternatives considered

- Rely only on Git history.
- Store decisions in task descriptions.
- Keep decisions only in conversation history.

### Related items

- `AGENTS.md`
- `GAME_DESIGN.md`
- `ROADMAP.md`

---

# Decision template

Copy this section for new decisions.

## DEC-XXX — Decision title

**Date:** YYYY-MM-DD  
**Status:** Proposed

### Decision

Describe the proposed or accepted decision.

### Context

Describe the problem, constraint, or situation that requires a decision.

### Rationale

Explain why this option is preferred.

### Consequences

List important positive and negative consequences.

### Alternatives considered

- Alternative:
  - Benefits:
  - Costs:
  - Reason not selected:

### Related items

- Relevant roadmap milestone
- Relevant TODO
- Relevant systems or files
