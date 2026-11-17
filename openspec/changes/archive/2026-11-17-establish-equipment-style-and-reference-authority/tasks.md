# Tasks

## 1. Introduce asset-kind-scoped style authority

- [x] 1.1 Evolve `ReferenceStyleManifest` to version 2 with optional ordered `common` PNG references plus required non-empty `avatar` and `equipment` scopes for the corresponding authored kinds; verify focused contract tests accept valid scoped manifests and reject missing matching scopes, duplicate applicable paths, unknown/unsupported kind resolution, non-PNG paths, and missing files.
- [x] 1.2 Migrate `Art/Style/low-resolution-voxel.v1/style.yaml` so the three existing approved character PNGs move unchanged into the `avatar` scope while the style id and shared traits remain unchanged; verify the three character file hashes are unchanged and avatar style resolution returns exactly the intended applicable references.
- [x] 1.3 Make style resolution asset-kind aware so an asset receives optional `common` plus its matching kind scope and never inherits unrelated kind references; verify fixture tests resolve avatar and equipment scopes independently.
- [x] 1.4 Replace whole-manifest style hashing with a deterministic semantic applicable-style projection containing style id, shared traits, selected asset kind, applicable path ordering, and applicable PNG hashes; verify equipment-only scope edits do not stale avatar evidence, avatar-only edits do not stale equipment evidence, and shared trait/common edits stale both.
- [x] 1.5 Update generated reference audit evidence for the scoped authority digest while preserving existing Hero.Voxel workflow provenance; verify old flat-style evidence fails closed after schema migration and resealed scoped evidence passes without changing unrelated proof-audit payloads.

## 2. Establish equipment-specific visual style evidence

- [x] 2.1 Create approved generic equipment style PNG references under `Art/Style/low-resolution-voxel.v1/` covering an elongated hand-held form, a broad hand-held form, and a back-mounted volume in the existing low-resolution voxel language; verify the images demonstrate silhouette/thickness, proportion language, readable grip or attachment regions, restrained detail density, and color blocking without gameplay labels or runtime construction metadata.
- [x] 2.2 Add those exact equipment PNGs to the manifest's `equipment` scope without moving them into `common`; verify equipment style resolution uses them while avatar resolution continues to use the existing three character references only.
- [x] 2.3 Update `.agents/skills/voxel-store-maintain/SKILL.md` so equipment jobs inspect only applicable common/equipment style authority, accept reference sheets before source authoring, and keep compatibility downstream; verify the skill still delegates Composition/Part Sheet semantics to the pinned Hero.Voxel workflow rather than copying its checklist.

## 3. Re-establish avatar evidence under scoped provenance

- [x] 3.1 Reseal `foundation.avatar.humanoid.guard-01` against the new avatar-scoped style authority without regenerating its accepted reference images or changing VoxelSpec/source; verify the avatar returns healthy and its reference evidence contains no equipment-only style hashes.
- [x] 3.2 Mutate one equipment-scoped style image in a controlled test and verify FoundationGuard01 remains reference-healthy, then restore the exact bytes; separately mutate one avatar-scoped image and verify only the avatar reference gate becomes stale.

## 4. Re-author RightBlade01 style-first

- [x] 4.1 Establish RightBlade01 migration intent and target bounds from its stable identity plus current model/proof as recovery evidence, then generate/audit only a new 3:2 Composition Sheet using scoped equipment style authority; verify the accepted sheet freezes the correct construction-significant `RequiredParts` without encoding exact pivot, assembly origin, socket, or mount transform.
- [x] 4.2 Generate/audit exactly one Part Sheet per frozen RightBlade01 Part ID using the canonical Hero.Voxel workflow; verify coverage is exact, local geometry is neutral, visual geometry subparts may describe grip/guard/blade form without creating unnecessary rigid parts, and no compatibility implementation leaks into the sheet.
- [x] 4.3 Compare the accepted RightBlade01 reference package against the existing VoxelSpec/proof; repair source only if materially required by the accepted visual authority, then regenerate/seal proof and run compatibility/publication checks; verify the asset is healthy with the same stable id, UnitRef, `Hand.R` mount contract, and no pair-specific correction.

## 5. Re-author LeftGuard01 style-first

- [x] 5.1 Establish LeftGuard01 migration intent and target bounds from its stable identity plus current model/proof as recovery evidence, then generate/audit only a new 3:2 Composition Sheet using scoped equipment style authority; verify the sheet freezes construction-significant parts and expresses visual hand-contact intent without runtime mount semantics.
- [x] 5.2 Generate/audit exactly one Part Sheet per frozen LeftGuard01 Part ID; verify the accepted local views preserve one coherent broad hand-held form, exact part coverage, and implementation-neutral visual geometry labels.
- [x] 5.3 Compare the accepted LeftGuard01 reference package against current VoxelSpec/proof; repair source only when the visual authority requires it, regenerate/seal proof and compatibility, and verify healthy publication with unchanged stable id, UnitRef, `Hand.L` mount contract, and no corrective transform data.

## 6. Re-author BackPack01 style-first

- [x] 6.1 Establish BackPack01 migration intent and target bounds from its stable identity plus current model/proof as recovery evidence, then generate/audit only a new 3:2 Composition Sheet using scoped equipment style authority; verify it communicates the back-facing attachment region visually without defining an exact socket, pivot, assembly origin, or mount transform.
- [x] 6.2 Generate/audit exactly one Part Sheet per frozen BackPack01 Part ID; verify the accepted local geometry preserves a coherent back-mounted volume, exact part coverage, and implementation-neutral visual geometry labels.
- [x] 6.3 Compare the accepted BackPack01 reference package against current VoxelSpec/proof; repair source only when required by accepted reference authority, regenerate/seal proof and compatibility, and verify healthy publication with unchanged stable id, UnitRef, `Back` mount contract, and no pair correction.

## 7. Integration and regression proof

- [x] 7.1 Run the full Tool regression suite and regenerate catalog/health; verify all four V1 assets publish healthy, compatibility remains 6/6 PASS, coverage gaps are 0, and `NextJob` is null.
- [x] 7.2 Prove scoped staleness on the real store: equipment-only style mutation stales exactly the three equipment assets, avatar-only mutation stales only FoundationGuard01, and a shared trait or declared common-reference mutation stales both kinds; restore/reseal and verify health after each controlled mutation.
- [x] 7.3 Run maintenance planning twice while hashing authoritative style/reference/source inputs; verify both runs return no creative work and mutate no authoritative content.
- [x] 7.4 Audit final equipment references for forbidden implementation authority, gameplay semantics, precomposed avatar/equipment variants, and pair-specific corrections; verify Hero.Voxel remains the only construction workflow/compiler authority and compatibility remains a downstream proof layer.
- [x] 7.5 Run `openspec validate establish-equipment-style-and-reference-authority --strict`, both Tool formatter checks, the full regression suite, repository authority/path audits, and `git diff --check`; verify all checks pass before workflow finalization.

## Workflow follow-up

- Sync the validated deltas into `reference-style-authority`, `asset-publication`, and `store-maintenance`, then archive the change after implementation acceptance.
- Finalize Git history through the repository workflow and push normally only after post-archive verification succeeds.
