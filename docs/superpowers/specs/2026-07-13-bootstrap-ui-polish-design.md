# Bootstrap UI polish design

## Goal

Improve the readability and visual hierarchy of the Admin UI's setup experience without changing setup behavior or routes.

## Scope

### Brand lockup

Replace every Admin UI composition that renders the Veritas mark beside separately authored `Veritas` text with the shared `veritas-lockup.svg` asset. This applies to the setup journey header, guest/login shell, and admin sidebar. Each image uses `alt="Veritas"`; the redundant text nodes are removed.

### Validation contrast

Keep the existing red validation treatment, but assign `--destructive-foreground` a color that is legible on the page and card backgrounds in both color schemes. Existing field error components and alert markup continue to consume this semantic token, so the correction is shared rather than page-specific.

### Progress hierarchy

Keep the three-stage progression and existing completion semantics. Make the current circle clearly distinct from complete and upcoming states:

- Current: light fill, dark numeral, blue border and outer ring, and emphasized label.
- Complete: blue fill with a white checkmark.
- Upcoming: muted outlined circle and label.

This removes the low-contrast blue-on-dark current state while preserving accessibility labels and reduced-motion behavior.

## Tests

Add or update focused component tests to assert the shared lockup asset is rendered with its accessible name and that the current progress stage has its unique class. Continue running the Admin UI suite and the repository-wide check.

## Non-goals

- No changes to bootstrap, authentication, SMTP, or routing behavior.
- No new image assets or changes to the brand SVG itself.
