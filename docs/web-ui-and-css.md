# Web UI and custom CSS

Tenancy Hub’s web front end uses **Microsoft Fluent UI Blazor v5** (`TenancyHub.Web`). The component library should own look, layout primitives, spacing, and theme behavior. Custom CSS is the exception, not the default.

For Fluent component usage and theming APIs, see `.agents/skills/fluentui-blazor-usage/` (especially `references/THEMING.md` and `references/SETUP.md`).

## Default rule

**Prefer Fluent components and their built-in parameters** (for example `Appearance`, `Margin`, `Padding`, `FluentStack`, `FluentSpacer`, `FluentLayout`) instead of writing CSS to replicate or override library styling.

Do **not**:

- Target Fluent internal class names or shadow DOM internals to change component appearance.
- Add new global rules in `wwwroot/app.css` to “fix” a component when a Fluent API or layout pattern exists.
- Scatter hex colors, pixel spacing, or font sizes in `.razor` files, inline `style` attributes, or many small CSS files.

If something looks wrong, first check whether Fluent already exposes the behavior (parameters, layout areas, design tokens). Only then consider custom CSS—and document why.

## When custom CSS is acceptable

| Category | Examples | Guidance |
|----------|----------|----------|
| **Platform / Blazor chrome** | `#blazor-error-ui`, `.blazor-error-boundary`, `.loading-progress*`, reload bar in `MainLayout` | Not Fluent components; template-provided UI. Styles may live in `wwwroot/app.css` (or a dedicated `wwwroot/blazor-chrome.css` if we split later). Keep grouped and minimal. |
| **App shell layout hooks** | `#main-menu`, `.content` padding/width | Prefer `FluentLayout` / item parameters first. Existing shell rules should shrink over time, not grow. Avoid new `!important` rules. |
| **Branding** | Logo treatment, brand palette overrides, marketing surfaces | **Not yet implemented.** When added, must live in one owned place (see below)—not ad hoc overrides. |
| **Page-specific layout** | Rare one-off layout that Fluent stacks cannot express | Prefer component-scoped `.razor.css` with **Fluent design tokens** (`var(--spacingVerticalM)`, etc.), not raw literals. |

## Branding (future)

When product branding requires overrides beyond Fluent’s theme:

1. **Single ownership** — e.g. `wwwroot/branding.css` (or `wwwroot/css/tenancy-hub-brand.css`) imported once from `App.razor`, after Fluent/reboot styles.
2. **Scoped root** — apply a root class on the app shell (e.g. `tenancy-hub-brand` on `body` or the top-level layout) so brand rules do not leak globally.
3. **Tokens, not literals** — define brand values as CSS custom properties on that root, and map surfaces to Fluent tokens where possible (`--colorBrandBackground`, `--colorBrandForeground1`, spacing/typography variables). Pages and components reference `var(--tenancy-hub-…)` or Fluent `var(--color…)`, not `#1b6ec2` in Razor.
4. **No component surgery** — branding adjusts tokens and approved wrapper classes; it does not restyle `fluent-button` internals.

Until `branding.css` exists, do not introduce one-off brand colors in the codebase.

## Theming

Fluent v5 uses **CSS custom properties** and `body[data-theme]` (no `FluentDesignTheme` component). Use:

- JS interop: `Blazor.theme.setLightTheme` / `setDarkTheme` / `switchTheme`
- C# helpers: `SystemColors`, `StylesVariables`, `CommonStyles` for inline styles when absolutely needed
- Theme utilities: `hidden-if-light`, `hidden-if-dark`

Custom CSS that must respect light/dark should use Fluent token variables so it tracks theme automatically—not hardcoded `lightyellow` / `#b32121` except in Blazor chrome where we intentionally match template behavior (and may refactor later to tokens).

## Inline styles and scoped CSS

- **Avoid `style="..."` on markup** unless prototyping; replace with Fluent layout components or scoped CSS backed by design tokens.
- **`*.razor.css` scoped styles** follow the same rules: no library overrides, tokens over literals, minimal surface area.

## Current `wwwroot/app.css` (inventory)

As of initial standards, `app.css` contains:

1. Fluent **reboot** import (required baseline).
2. **Shell**: `#main-menu`, `.content`, responsive `#main-menu` rule — layout helpers; treat as legacy shell, not a pattern to copy.
3. **Blazor chrome**: error UI, error boundary, loading progress — acceptable special case.
4. **`code` element color** — generic prose styling; prefer Fluent typography/tokens if we extend markdown/code display.

New work should not expand sections 2–4 without an explicit exception documented in a PR or task.

## Review checklist

Before adding CSS or inline styles:

1. Can Fluent components or `Margin`/`Padding` parameters do this?
2. If color/spacing is needed, is it a **token** or **brand variable**, not a literal?
3. Is the rule **global** (needs strong justification) or **scoped** (`.razor.css` / branding file)?
4. Will it break in **dark mode**?
5. Are we overriding the library, or composing it?
