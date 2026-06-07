# 🤖 Inter-Agent Communication Protocol

> **IMPORTANT**: This file contains instructions for you as an AI agent. Follow this protocol to collaborate with other AI agents (and the human) working on the **FYDNE recovery** project.

---

## Overview

You are one of several AI agents that may work on this project across devices/sessions.
To coordinate work and avoid conflicts, all agents communicate through `SHARED_LOG.md`
in this `agent-exchange/` folder.

**Before starting any work**, read `SHARED_LOG.md` to understand:
- What other agents have already done
- What problems / blockers exist
- What decisions have been made
- What is in progress right now

**After completing work**, update `SHARED_LOG.md` with your results, then `git add/commit/push`.

---

## Your Identity

When writing to the log, identify yourself with:
- **Agent Name**: the app/tool you run in (`Claude Code`, `Cursor`, `Antigravity`, `Copilot`, `Cline`, etc.)
- **Session ID**: a short unique id (e.g. start timestamp `2026-06-07_21:40`)

---

## How to Read the Log

1. **Always read `SHARED_LOG.md` at the start of every session** and whenever the user says "sync".
2. Pay attention to:
   - `## 🔴 ACTIVE BLOCKERS` — critical issues that may affect your work
   - `## 📋 CURRENT STATUS` — what each agent is currently doing
   - `## 📝 LOG ENTRIES` — chronological history
3. If another agent flagged a problem in your area, address it.
4. If another agent asked a `❓ QUESTION`, answer if you can.

---

## How to Write to the Log

### When to Write
- ✅ When you **complete** a task or subtask
- 🐛 When you **encounter a problem** you cannot solve
- ❓ When you have a **question** for other agents / the user
- ⚠️ When you make a **decision** affecting architecture
- 🔄 When you **start** something (to prevent conflicts)
- 📌 When you discover important **context**

### Entry Format

Add entries to `## 📝 LOG ENTRIES`. **Always add new entries at the TOP** (reverse chronological).

```markdown
### [TIMESTAMP] [CATEGORY] — Agent: [YOUR_NAME]

**Status**: DONE | IN_PROGRESS | BLOCKED | QUESTION
**Files Changed**: list of files (if applicable)
**Related To**: feature/component

Description of what you did, found, or need.

---
```

### Categories

| Tag | When to Use |
|-----|-------------|
| `✅ DONE` | Task completed |
| `🔄 IN_PROGRESS` | Starting work (claim it) |
| `🐛 BUG` | Found a bug |
| `⚠️ DECISION` | Architectural/design decision |
| `❓ QUESTION` | Need input |
| `📌 INFO` | Useful context/discovery |
| `🔴 BLOCKER` | Something blocks progress |
| `💡 SUGGESTION` | Proposed improvement |
| `🔧 FIX` | Fixed a reported problem |

---

## Rules

1. **Don't overwrite others' work** — check for `🔄 IN_PROGRESS` before editing a file.
2. **Claim before you work** — add an `IN_PROGRESS` entry for the component you take.
3. **Report results** — change your entry to `DONE` and list files changed.
4. **Flag problems immediately** — write `BUG`/`BLOCKER` as soon as found.
5. **Keep ACTIVE BLOCKERS updated** — add/remove as they appear/resolve.
6. **Keep CURRENT STATUS updated** — on session start and end.
7. **Be concise but complete** — file paths + specifics, bullet points, no novels.

---

## Project-specific notes (FYDNE)

- Build harness lives in `scripts/` (`build-shim.ps1`, `build-plugin.ps1`).
- Game DLLs + LabApi are in `dependencies/` (gitignored — regenerate via `gather-dependencies.ps1`).
- The plugin source is in `plugin/Loli` (MIT). The Qurre→LabAPI shim is in `plugin/QurreShim`.
- Read `docs/02_RECOVERY_ROADMAP.md` and `TODO.md` for the task list.
- **Never commit secrets, DLLs, or DB dumps** (`.gitignore` blocks them).

---

## Quick Reference

```
START SESSION:  1. Read SHARED_LOG.md  2. Add yourself to CURRENT STATUS  3. Check BLOCKERS  4. Claim work (IN_PROGRESS)
DURING WORK:    5. Log problems (BUG/BLOCKER)  6. Log decisions (DECISION)  7. Share discoveries (INFO)
END SESSION:    8. Log completed work (DONE)  9. Update CURRENT STATUS  10. Update BLOCKERS  11. git commit + push
```
