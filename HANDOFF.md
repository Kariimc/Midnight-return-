# HANDOFF - Kariimc/Midnight-return-

> Continuity doc. Any agent must resume cold from this file with zero briefing.
> Update it in the same commit as any code change.

**Seeded:** 2026-07-15 from verified repo state. Sections marked UNVERIFIED were not
provable from the repo alone - fill them in, do not guess.

## Verified facts

| | |
|---|---|
| Repo | `Kariimc/Midnight-return-` |
| Namespace | user `Kariimc` |
| Default branch | `claude/confident-fermi-amdht6` |
| Visibility | public |
| Language | C# |
| Files | 147 |
| Last commit | 2026-06-21 - feat: production sprite sheet generator — 13 animation sections + enem |
| Branches | 3 |
| Open PRs | 1 |

**Top-level dirs:** `Assets`, `Packages`, `web-prototype`

**Root files:** `.gitignore`, `CLAUDE.md`, `SETUP.md`, `package-lock.json`

**Existing docs:** `CLAUDE.md`, `SETUP.md`

## Open PRs

- #1 docs+test+ci: README, EditMode tests, and Unity CI  `chore/readme-and-tests`

## Current state

**UNVERIFIED.** Percent-complete and working/broken status cannot be derived from
the repo alone. Do not write a number here you have not proven. Read the code, run
the build, then record what you observed and how you observed it.

## Exact next steps

**UNVERIFIED.** Fill in on first real session in this repo.

## Open decisions

**UNVERIFIED.**

## Rules

- Repos span TWO namespaces: user `Kariimc` AND org `shift9-studio`. Enumerate with
  `gh api '/user/repos?affiliation=owner,collaborator,organization_member'`, never
  `gh repo list Kariimc` alone. See `Kariimc/my-skills` `rules/10-repo-topology.md`.
- Never assert an absence, status, or completion without proving your scope was exhaustive.
- Update this file in the same commit as any code change. A global pre-commit hook enforces it.
