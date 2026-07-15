# Unity Codex agent pack

Copy the `.codex` directory into the root of the Unity project.

Included agents:

- `game_systems_architect` — read-only architecture and planning
- `unity_gameplay_engineer` — gameplay implementation
- `unity_debugger` — evidence-first diagnosis and focused fixes
- `unity_code_reviewer` — read-only Unity-specific review
- `unity_test_engineer` — automated and manual verification
- `game_design_balance` — read-only mechanics and tuning
- `unity_ui_ux_accessibility` — UI implementation and UX/accessibility

The included `.codex/config.toml` allows up to six concurrent agent threads and
keeps nesting at one level.

Suggested first prompt:

Use subagents to inspect this Unity project. Have game_systems_architect map the
architecture, unity_test_engineer assess the current test setup, and
unity_code_reviewer identify the highest-risk Unity-specific issues. Keep this
read-only, wait for all agents, then consolidate their findings.

For implementation, avoid assigning multiple write-enabled agents to the same
files at the same time.
