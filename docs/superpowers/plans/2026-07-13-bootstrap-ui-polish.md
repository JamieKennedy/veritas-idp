# Bootstrap UI Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make validation feedback readable, replace composed Veritas branding with the shared lockup, and clarify the setup journey's current stage.

**Architecture:** Reuse semantic CSS tokens and existing shared layout components. A shared brand-lockup component selects the supplied lockup in light mode and a reversed wordmark plus original blue mark in dark mode; the progress component retains its state model and receives visual-only styling changes.

**Tech Stack:** React 19, TypeScript, Tailwind CSS 4, Vitest, Testing Library.

## Global Constraints

- Do not change setup, authentication, SMTP, or routing behavior.
- Use `assets/brand/veritas-lockup.svg`; do not alter the SVG asset.
- Maintain accessible image names and existing progress announcements.
- Preserve light and dark mode support.

---

## File structure

- `src/frontend/admin-ui/src/styles.css`: theme-specific lockup treatment, deeper dark palette, destructive token contrast, and current-progress styles.
- `src/frontend/admin-ui/src/app/brand/brand-lockup.tsx`: accessible shared light/dark lockup presentation.
- `src/frontend/admin-ui/src/app/layouts/guest-shell.tsx`: login-shell shared lockup.
- `src/frontend/admin-ui/src/app/layouts/admin-shell.tsx`: sidebar shared lockup.
- `src/frontend/admin-ui/src/features/setup/journey/components/setup-journey-shell.tsx`: setup-header shared lockup.
- `src/frontend/admin-ui/src/styles.test.ts`: semantic destructive-token and current-stage CSS assertions.
- `src/frontend/admin-ui/src/features/setup/journey/components/setup-journey-shell.test.tsx`: accessible lockup assertion.
- `src/frontend/admin-ui/src/features/setup/journey/components/setup-progress.test.tsx`: distinct current-stage assertion.

### Task 1: Replace composed branding with the shared lockup

**Files:**
- Modify: `src/frontend/admin-ui/src/app/layouts/guest-shell.tsx`
- Modify: `src/frontend/admin-ui/src/app/layouts/admin-shell.tsx`
- Modify: `src/frontend/admin-ui/src/features/setup/journey/components/setup-journey-shell.tsx`
- Modify: `src/frontend/admin-ui/src/features/setup/journey/components/setup-journey-shell.test.tsx`
- Modify: `src/frontend/admin-ui/src/styles.css`

**Interfaces:**
- Consumes: `@brand/veritas-lockup.svg?url`.
- Produces: every brand composition renders `<img alt="Veritas">` without adjacent authored brand text.

- [ ] **Step 1: Write the failing setup-header test**

```tsx
const lockup = screen.getByRole('img', { name: 'Veritas' })
expect(lockup.getAttribute('src')).toContain('veritas-lockup.svg')
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `pnpm --filter admin-ui test -- setup-journey-shell`

Expected: FAIL because the setup shell still renders `veritas-mark.svg`.

- [ ] **Step 3: Implement the lockup rendering**

```tsx
import veritasLockupUrl from '@brand/veritas-lockup.svg?url'

<img src={veritasLockupUrl} alt="Veritas" className="setup-journey__lockup" />
```

Apply the same import and accessible image pattern to the guest shell and admin sidebar. Remove each separate `Veritas` text node. Add a compact light neutral brand plate and explicit lockup sizing in `styles.css` so the fixed dark wordmark remains readable in dark mode and the dark sidebar.

- [ ] **Step 4: Run the test to verify it passes**

Run: `pnpm --filter admin-ui test -- setup-journey-shell`

Expected: PASS.

- [ ] **Step 5: Commit the task**

```powershell
git add src/frontend/admin-ui/src/app/layouts/guest-shell.tsx src/frontend/admin-ui/src/app/layouts/admin-shell.tsx src/frontend/admin-ui/src/features/setup/journey/components/setup-journey-shell.tsx src/frontend/admin-ui/src/features/setup/journey/components/setup-journey-shell.test.tsx; git commit -m "fix(admin-ui): use shared veritas lockup"
```

### Task 2: Improve validation and current-stage contrast

**Files:**
- Modify: `src/frontend/admin-ui/src/styles.css`
- Create: `src/frontend/admin-ui/src/styles.test.ts`

**Interfaces:**
- Consumes: `text-destructive` in `FieldError` and `.setup-progress__circle.is-current`.
- Produces: readable validation text in both themes and a current stage distinct from complete/upcoming stages.

- [ ] **Step 1: Write failing assertions**

```ts
import styles from './styles.css?raw'

expect(styles).toContain('--destructive: oklch(0.46 0.19 27)')
expect(styles).toContain('background: var(--surface-strong)')
expect(styles).toContain('color: var(--lagoon-deep)')
```

- [ ] **Step 2: Run tests to verify the new assertions fail before implementation**

Run: `pnpm --filter admin-ui test -- styles`

Expected: FAIL because the current destructive token and distinct current-stage styles do not yet exist.

- [ ] **Step 3: Implement shared token and state styles**

```css
:root { --destructive: oklch(0.46 0.19 27); }
.dark { --destructive: oklch(0.76 0.18 27); }
.setup-progress__circle.is-current {
    border-color: var(--lagoon);
    background: var(--surface-strong);
    color: var(--lagoon-deep);
    box-shadow: 0 0 0 5px color-mix(in oklab, var(--lagoon) 28%, transparent);
}
```

Keep `.is-complete` blue with a white checkmark and set the current label to `var(--sea-ink)`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `pnpm --filter admin-ui test -- styles`

Expected: PASS.

- [ ] **Step 5: Commit the task**

```powershell
git add src/frontend/admin-ui/src/styles.css src/frontend/admin-ui/src/styles.test.ts; git commit -m "fix(admin-ui): improve setup progress contrast"
```

### Task 3: Verify the integrated polish

**Files:**
- Modify: `docs/superpowers/specs/2026-07-13-bootstrap-ui-polish-design.md`
- Create: `docs/superpowers/plans/2026-07-13-bootstrap-ui-polish.md`

**Interfaces:**
- Consumes: Tasks 1 and 2.
- Produces: a clean, verified feature branch ready for review and merge.

- [ ] **Step 1: Format the workspace**

Run: `pnpm format`

Expected: formatting completes without unrelated modifications.

- [ ] **Step 2: Run full verification**

Run: `pnpm check`

Expected: formatting, lint, typecheck, builds, backend tests, and Admin UI tests PASS.

- [ ] **Step 3: Inspect the merge-ready diff**

Run: `git diff --check dev..HEAD` and `git status --short`

Expected: no whitespace errors and only planned files changed.

- [ ] **Step 4: Commit the planning documents**

```powershell
git add docs/superpowers/specs/2026-07-13-bootstrap-ui-polish-design.md docs/superpowers/plans/2026-07-13-bootstrap-ui-polish.md; git commit -m "docs: add bootstrap UI polish plan"
```
