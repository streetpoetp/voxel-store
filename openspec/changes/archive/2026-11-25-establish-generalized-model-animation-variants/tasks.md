# Tasks

## 1. Animation Variant Grammar and Publication

- [x] 1.1 Add a single voxel-store parser/formatter for `<Motion>`, `<Motion>.<Side>`, `<Motion>.V<n>`, and `<Motion>.<Side>.V<n>` and verify focused Tool tests accept canonical names while rejecting malformed, zero/negative, ambiguous, or unsupported qualifier forms.
- [x] 1.2 Extend reusable-model animation validation so optional variants are allowed for Idle, WalkForward, Attack, Hit, and Death and inherit the base motion loop policy; verify StoreEngine tests cover valid looping/non-looping variants, side+variant clips, missing baseline clips, and invalid loop settings.
- [x] 1.3 Extend generated catalog/health projection with structured base/side/variant/side+variant coverage while retaining legacy raw clip projection during migration; verify catalog tests project `Attack`, `Attack.Left`, `Attack.V2`, and `Attack.Left.V2` as one semantic motion with structured coverage.
- [x] 1.4 Update publication/workflow documentation to define model-local `V<n>` ownership, optional per-motion counts, loop inheritance, and gameplay-neutral semantics; verify the documented examples match the accepted parser grammar.

## 2. Compatibility Resolution and Representative Assets

- [x] 2.1 Implement one deterministic candidate builder for motion + optional side + optional variant with order `M.S.V -> M.V -> M.S -> M`; verify compatibility tests cover every fallback branch and preserve the existing no-variant side/base behavior.
- [x] 2.2 Extend compatibility request/proof paths so the root and each attachment can receive independent optional variant keys while all resolved clips remain on the existing shared normalized phase; verify tests cover a body on V2, equipment on V1, and an attachment falling back to side/base independently.
- [x] 2.3 Author representative current avatar/equipment variants spanning Idle, WalkForward, Attack, Hit, and Death, including at least one side-qualified Attack variant, then regenerate derived animation/proof artifacts and verify store scan/publication checks pass without changing asset identity or canonical model frame.
- [x] 2.4 Verify no Hero Studio/`hero.voxel.actor` API change is required by running the voxel-store compatibility tests against the pinned submodule; if a genuinely generic actor-candidate limitation appears, stop this project change at that boundary rather than adding voxel-store semantics to Hero Studio.

## 3. Unity Preview and Interactive Selection

- [x] 3.1 Extend preview motion state and `AvatarMotionPlayer`/presentation APIs with optional root and per-equipment variant selection, preserving existing motion, driver side, loop, speed, and default behavior; verify runtime/editor tests show an omitted variant never auto-selects an authored take.
- [x] 3.2 Route preview plan construction through the shared variant-aware candidate rule and verify Unity preview tests resolve fully qualified, generic-variant, side-only, and base fallbacks for body and attachments independently.
- [x] 3.3 Extend `AvatarMotionPreviewWindow` and inspector-driven preview controls to select/clear variants during Play Mode without becoming verification authority; verify editor tests exercise selection changes through the normal reset/play path and no manual menu/button action is required for automated proof.
- [x] 3.4 Update runtime-gallery documentation for variant inspection and verify the generated gallery continues to compose all discovered avatars/equipment through the existing socket/actor architecture with no special variant-only scene.

## 4. Command-Driven Variant Proof and Integration

- [x] 4.1 Extend the registered `avatar-motion-gallery` capture runner with a bounded deterministic checkpoint matrix covering representative Idle.V*, WalkForward.V*, Attack side+variant, Hit.V*, and Death.V* samples; verify the repository capture command remains the only required automation entry point.
- [x] 4.2 Run `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` and verify current provider-attested Game View PNGs visibly distinguish the selected variants, preserve side-aware attachment lock, and show correct looping/non-looping or terminal presentation behavior.
- [x] 4.3 Run focused Tool tests, Unity preview/editor tests through the repository-supported command path, store validation/scan checks, and `openspec validate establish-generalized-model-animation-variants --strict`; verify all pass with the pre-existing unrelated dirty worktree preserved.
- [x] 4.4 Fix manual runtime-gallery refresh so the generated scene restores transient voxel provider instances after save/reopen and frames the Scene View to the generated gallery bounds; verify an EditMode save→reopen regression test finds populated provider instances without entering Play Mode.
- [x] 4.5 Mirror every published model, including equipment, into `Assets/__VoxelStorePreview/Baked/<Catalog>/<Model>` derived from canonical `Assets/<Catalog>/<Model>/Source/<Model>` rather than asset ID; recursively remove stale legacy ID-based bake directories, make manual Runtime Gallery Refresh perform the full bake before scene rebuild, and verify gallery discovery resolves model-folder identities.



## Workflow follow-up

- Archive the change only after implementation and visual proof are accepted.
- Sync the archived deltas into the durable specs and verify the resulting main specs preserve the generalized variant contract.
