# Tasks

## 1. Publication Coverage and Alias Rejection

- [x] 1.1 Add one kind-aware minimum V1 coverage validator for published reusable models: avatars require `Idle.V1`, `WalkForward.V1`, `Attack.Left.V1`, `Attack.Right.V1`, `Hit.V1`, and `Death.V1`; equipment requires `Idle.V1`, `WalkForward.V1`, `Attack.V1`, `Hit.V1`, and `Death.V1`. Verify focused Tool tests report exact missing clips for each model kind and preserve the existing baseline/loop/side validation.
- [x] 1.2 Add compiled authored-motion fingerprint comparison for each required V1 against its correct fallback clip, excluding clip identity from the fingerprint; verify Tool tests reject exact aliases while accepting differences in duration, track coverage, interpolation, key times, or keyed transform values.
- [x] 1.3 Extend generated catalog/health projection and workflow documentation with required-V1 completeness for every published reusable model; verify Tool tests project complete/incomplete coverage from compiled source without making generated files authoritative.

## 2. Avatar V1 Authoring Migration

- [x] 2.1 Author the required six V1 takes for `CultistAcolyte01`, `FoundationGuard01`, and `Gargoyle01`; run per-asset generation/validation, regenerate proof/audit output, and visually inspect each model's animation proof to confirm the V1 takes are semantically readable and not aliases.
- [x] 2.2 Author the required six V1 takes for `Ghoul01`, `LichLord01`, and `Necromancer01`; run per-asset generation/validation, regenerate proof/audit output, and visually inspect each model's animation proof to confirm the V1 takes preserve canonical frame and side-reactive Attack semantics.
- [x] 2.3 Author the required six V1 takes for `SkeletonArcher01`, `SkeletonGrunt01`, and `SkeletonKnight01`; run per-asset generation/validation, regenerate proof/audit output, and visually inspect each model's animation proof to confirm the V1 takes are distinct and attachment sockets remain valid.
- [x] 2.4 Author the required six V1 takes for `Wraith01` and add the missing `Attack.Right.V1` to `ZombieBrute01`; run per-asset generation/validation, regenerate proof/audit output, and verify all 11 published avatars pass the complete V1 coverage and alias-rejection gates.

## 3. Equipment V1 Authoring Migration

- [x] 3.1 Author `Idle.V1`, `WalkForward.V1`, `Attack.V1`, `Hit.V1`, and `Death.V1` for `Axe03`, `Bow02`, `Crossbow02`, and `Dagger02`; regenerate/validate each asset and inspect animation proof to confirm the alternate take preserves each model's existing action semantics and Mount authority.
- [x] 3.2 Author the required five V1 takes for `Halberd01`, `Hammer02`, `Kukri01`, and `Mace02`; regenerate/validate each asset and inspect animation proof to confirm no Mount-bearing-part position track or attachment correction is introduced.
- [x] 3.3 Author the required five V1 takes for `Rifle02`, `Scythe01`, `Spear02`, `Staff02`, and `Sword02`; regenerate/validate each asset and inspect animation proof to confirm ranged/melee/caster action direction remains model-appropriate; then verify all 14 published equipment models, including existing `WarPick01`, pass the complete V1 coverage and alias-rejection gates.

## 4. Model-Driven Unity Inspector and Baked Coverage

- [x] 4.1 Make the `AvatarMotionPlayer` component Inspector enumerate exact clip names from the selected avatar's bound baked `VoxelAnimationAsset`, parse a selected exact clip back to semantic motion + optional side + optional variant, and keep no hard-coded clip/variant option table; verify Editor tests cover models with different clip inventories and `Attack.Left.V1`, `Death.V1`, and baseline selection.
- [x] 4.2 Add baked-preview coverage validation that checks every discovered published avatar/equipment animation asset contains its kind-specific required V1 set; verify the Unity EditMode suite fails with model + missing-clip evidence for an incomplete fixture and passes on the fully rebaked store.
- [x] 4.3 Run the full preview bake and verify all 25 published models resolve under `Assets/__VoxelStorePreview/Baked/<Catalog>/<Model>` with current animation assets whose exact clip inventories match source publication coverage; do not build a Player or APK.

## 5. Store-Wide Integration Proof

- [x] 5.1 Regenerate catalog/health state for the fully migrated store, run Tool tests plus `scan --write` and `verify`, and verify the published baseline reports 25/25 models complete with no required-V1 or alias failures.
- [x] 5.2 Run the full Unity preview/EditMode test suite and verify Inspector discovery, variant parsing, runtime-gallery composition, socket/Mount compatibility, and baked-coverage checks all pass without manual Editor clicks.
- [x] 5.3 Run `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`, verify current provider attestation, and visually inspect the bounded V1 checkpoints for distinct authored presentation, side-aware attachment lock, shared normalized phase, and correct loop/non-loop behavior.
- [x] 5.4 Run `openspec validate author-multi-animation-variants-across-all-models --strict` and `git diff --check`; verify the unrelated dirty worktree remains preserved and no Player/APK artifact was produced.

## Workflow follow-up

- Archive `establish-generalized-model-animation-variants` before or together with finalizing this dependent publication-floor change so the durable main specs retain the generalized variant grammar this change assumes.
- Archive this change only after every model's generated proof has been visually accepted and the store-wide integration gates are green.
- Commit/publish only when explicitly requested.
