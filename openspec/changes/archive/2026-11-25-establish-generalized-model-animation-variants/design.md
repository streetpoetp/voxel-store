# Design

## Context

See `proposal.md` for motivation. Today voxel-store validates five required semantic clips plus side-reactive `Attack.Left` / `Attack.Right`, projects compiled clip names into generated catalog/health data, and builds preview candidate lists from semantic motion plus physical side. `AvatarPresentation` currently emits side-first candidates and passes them into Hero.Voxel.Actor.

Hero.Voxel.Actor already provides the generic foundation needed here: root and attachment clip requests each accept an ordered list of opaque exact clip names, resolution selects the first available candidate, and all resolved clips are sampled on the same root-owned normalized phase. Gameplay semantics and motion vocabulary remain caller-owned.

## Goals / Non-Goals

**Goals:**
- Add one reusable clip-name grammar for optional model-local variants across every semantic motion.
- Keep semantic motion, physical side, and authored variant as independent dimensions.
- Preserve existing assets and requests that do not use variants.
- Let the root and each attachment resolve an explicitly requested variant independently.
- Expose machine-readable catalog coverage without forcing consumers to parse clip names.
- Reuse Hero.Voxel.Actor candidate resolution and shared-phase playback unchanged.

**Non-Goals:**
- Random, weighted, combo, mood, injury, AI, or gameplay-driven selection inside voxel-store.
- Global meaning for `V1`, `V2`, or any take key across models or motions.
- Equal variant counts across motions or assets.
- Animation blending, concurrent root motion authorities, or a state machine.
- Hero Studio changes unless implementation proves a missing generic primitive.

## Decisions

### 1. Use `V<n>` as the canonical authored take suffix

Supported forms are:

```text
<Motion>
<Motion>.<Side>
<Motion>.V<n>
<Motion>.<Side>.V<n>
```

`n` is a positive integer. This keeps parsing deterministic and prevents collisions with `Left`, `Right`, and `Center`.

**Alternative considered:** arbitrary names such as `Heavy`, `Tired`, or `Combo3`. Rejected because they invite gameplay semantics into reusable art contracts and make structured parsing ambiguous.

### 2. Keep baseline clips mandatory and variants optional

`Idle`, `WalkForward`, `Attack`, `Hit`, and `Death` remain mandatory. Variants never replace the base clip. Loop policy derives from the semantic motion: Idle and WalkForward variants loop; Attack, Hit, and Death variants do not.

**Alternative considered:** variants without a base clip. Rejected because fallback would become model-specific and old consumers would lose a stable contract.

### 3. Use one deterministic candidate order

For motion `M`, side `S`, and requested variant `V`, build:

```text
M.S.V
M.V
M.S
M
```

Absent dimensions are omitted. Validation, compatibility proof, and Unity preview should share this exact rule.

**Alternative considered:** `M.S.V -> M.S -> M.V -> M`. Rejected because an explicit V request would be ignored whenever a side-only clip exists.

### 4. Variant selection is per model instance

The root model and each attachment may receive independent optional variant keys. Each model resolves candidates against its own animation catalog while Hero.Voxel.Actor keeps the resolved clips phase-locked.

```text
motion = Attack
side = Left
body variant = V2
sword variant = V1
shield variant = none

body   -> Attack.Left.V2
sword  -> Attack.V1
shield -> Attack.Left
```

**Alternative considered:** one actor-wide V2. Rejected because variant ids are model-local and do not imply aligned choreography across unrelated assets.

### 5. Keep selection policy outside voxel-store

Voxel-store accepts a requested variant and resolves it. Consumers decide whether gameplay state, deterministic sequence, or randomness maps to a take. Preview controls may select a take for inspection but do not define production policy.

**Alternative considered:** automatic rotation/random selection. Rejected because it introduces hidden nondeterminism and gameplay-adjacent policy.

### 6. Add structured catalog coverage additively

Generated catalog/health data gains structured coverage per semantic motion with base presence, sides, variants, and side+variant coverage. Existing raw clip-name projection may remain during migration for compatibility/diagnostics, but structured coverage becomes the semantic view for variant-aware consumers and tests.

**Alternative considered:** keep only raw clip names. Rejected because every consumer would need to duplicate the grammar parser.

### 7. Extend preview state, not Hero.Voxel.Actor

`AvatarMotionPlayer` and presentation-facing preview state gain optional root and attachment variant selection. `AvatarPresentation` converts motion, side, and variant into ordered opaque candidates and calls the existing actor plan API. No `MotionVariant` concept is added to Hero Studio.

### 8. Prove representative variants through the existing capture path

At least one representative avatar/equipment composition should publish variants spanning Idle, WalkForward, Attack, Hit, and Death, including side-qualified Attack and independent body/equipment fallback. Canonical visual verification remains `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`.

## Risks / Trade-offs

- [Stricter clip grammar] -> Centralize parsing/formatting and reject malformed suffixes during publication.
- [Catalog consumers rely on flat motion names] -> Add structured coverage additively and migrate internal consumers before removing legacy projection.
- [Per-model selection increases preview state] -> Keep only optional root and per-binding keys; add no policy/state machine.
- [Fallback can hide missing exact takes] -> Project exact coverage and test each fallback branch.
- [Visual proof can grow combinatorially] -> Test grammar/fallback exhaustively in code but capture a bounded representative matrix.
- [V numbers may be mistaken for semantics] -> Document syntax only; keep keys model-local.

## Migration Plan

1. Add shared voxel-store parsing/formatting and validation for generalized clip names while preserving current assets.
2. Add representative authored variants and regenerate derived animation/catalog evidence.
3. Add structured catalog/health coverage while retaining legacy raw projection during migration.
4. Extend compatibility tests and preview candidate construction for explicit per-model variants and every fallback branch.
5. Extend interactive preview state and command-driven captures for Idle, WalkForward, Attack, Hit, and Death variants.
6. Run strict OpenSpec/tool validation and provider-attested graphical capture.
7. Rollback removes variant clips and variant-aware projection/request fields; baseline and side-only contracts remain valid.
