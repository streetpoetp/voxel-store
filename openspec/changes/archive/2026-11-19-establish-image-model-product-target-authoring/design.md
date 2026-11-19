# Design

## Context

See proposal.md for motivation. The current v3 authority chain is already correct, but its Product Target generation step is underspecified. The repository proves the loophole directly: Sword02 and Dagger02 were able to create `target.png` with `System.Drawing` while still producing structurally valid generated provenance. Existing published targets must remain valid; new authoring must no longer use that path.

## Goals / Non-Goals

**Goals:**

- Make image-model generation the only valid creative path for newly generated Product Targets.
- Preserve exact accepted generated image bytes as repository authority.
- Record enough generator/request provenance to distinguish image-model authoring from procedural rendering and to detect stale requests.
- Keep provider choice outside the store contract.
- Preserve backward compatibility for existing accepted generated provenance v1 and imported targets.
- Keep deterministic/scripted tools available downstream for construction sheets, annotations, diagnostics, density candidates, and proof.

**Non-Goals:**

- Bundling or authenticating a particular image-generation provider in the repository.
- Making image generation deterministic or reproducible byte-for-byte.
- Treating image-model output as Hero.Voxel construction authority.
- Migrating accepted Sword02/Dagger02 target PNGs merely to fabricate image-model provenance they never had.
- Allowing a generated image visible only in chat/UI to be approximated locally when exact transfer is unavailable.

## Decisions

### 1. Generated Product Target is a creative-model boundary

The Product Target authoring skill must use an actual image-generation capability for newly generated targets. `System.Drawing`, SVG, PIL/canvas scripts, hand-authored rectangles, VoxelSpec previews, or other procedural renderers may support Composition/Part sheets or diagnostics but may not invent generated Product Target authority.

This boundary is behavioral rather than provider-specific. The repository does not name one vendor or API.

Alternative considered: keep “generate” generic and rely only on human visual review. Rejected because the current workflow already demonstrated that agents can accept materially weaker procedural targets despite understanding a richer style.

### 2. Add generated provenance v2 while retaining v1

`target.provenance.json` supports:

- **v1**: existing generated/imported provenance, retained for backward compatibility.
- **v2 generated**: all current generated style evidence plus an image-model generator record and a hash-bound repository generation request.

The v2 generator record contains:

`kind = image-model`, `provider`, `model`, and `request { path, sha256 }`.

The tool validates structure and staleness, not whether a provider actually executed a model call. Agent workflow plus visual audit own that creative act; deterministic Tool validation owns provenance integrity.

Alternative considered: invalidate all v1 generated targets immediately. Rejected because that would force false provenance or unnecessary visual redesign of already accepted published assets.

### 3. Store the generation brief as repository evidence

Each newly generated Product Target keeps a repository-owned request artifact (recommended `Reference/target.request.json`). It captures the creative brief used for the accepted generation: asset/form intent, visual goals, negative constraints, and any provider-facing prompt text needed to understand the accepted result.

The provenance file hashes this request. Style-image hashes remain independently bound by existing target-style provenance.

Alternative considered: store only provider/model strings. Rejected because that cannot explain what visual request produced the accepted target or detect a changed creative brief.

### 4. Exact-byte project delivery is mandatory

The accepted image-model result must become `target.png` without redrawing. Valid delivery paths include:

- generator writes directly into the project;
- generator returns a public/downloadable result that the Runner stores byte-for-byte;
- trusted attachment/file import copies exact bytes into the project.

If the result is only visible through a host-internal URL that the project cannot import, the Product Target stage is blocked. The agent must not recreate it with a local drawing script.

Alternative considered: redraw the visible image procedurally. Rejected because this is exactly the fidelity loss the change is intended to eliminate.

### 5. Candidate iteration happens before authority freeze

The Product Target skill may generate and visually review multiple candidates. Only the accepted candidate is required as repository authority. Once accepted, its exact hash plus request/style provenance are frozen before Composition is authored.

Visual acceptance checks at minimum: silhouette, proportions, material/color hierarchy, meaningful construction richness, chunky voxel readability, and adherence to the active high-fidelity style language.

### 6. Construction extraction remains downstream and may be deterministic

Composition and Part sheets are still interpretations of the accepted target under pinned Hero.Voxel rules. Scripts may lay out views, labels, callouts, or derived diagrams provided they do not redesign the visual target and still pass target-fidelity audit.

This preserves deterministic tooling where it is useful without allowing it to replace creative visual authority.

### 7. Model iteration converges to the target, not the reverse

After density/source generation, standalone and isolated-part Hero.Voxel renders are compared against the accepted Product Target. A visual mismatch repairs the VoxelSpec/model or construction interpretation. The Product Target changes only through an explicit visual redesign, which stales downstream evidence normally.

### 8. Remove procedural Product Target generators from published examples

Tracked `Sword02/Reference/generate-target.ps1` and `Dagger02/Reference/generate-target.ps1` are deleted because their presence teaches the wrong authoring pattern. Their accepted `target.png`, v1 provenance, reference sheets, source, and proof remain unchanged and valid.

### 9. Same-desktop exact-byte bridge is a valid delivery implementation

When ChatGPT image generation and the WebCodex Runner share the same Windows desktop/filesystem, the accepted generated image may arrive through the user's/browser's normal download path. `Tool/import-product-target.ps1` is the deterministic boundary: it accepts a PNG source, copies it to the requested repository `target.png`, and requires source/destination SHA-256 equality.

This avoids a paid provider dependency and avoids the host-internal URL problem without weakening provenance. It does not turn the download path into visual authority; the image-model bytes remain authority. The helper is transport-only and contains no generation logic.

The AxeStyleTest proof demonstrates this path with `Voxel Ranger Axe Design Sheet.png`: the downloaded source and repository target are byte-identical at SHA-256 `252cfca605f95e3701669fa9841cce314c5e12c8607d646d173b1edc414603d8`.

### 10. Prove target-to-model convergence with an isolated Axe sample

`Art/Samples/AxeStyleTest` becomes the end-to-end proof for this change rather than an ad-hoc artifact. The accepted image-model target is imported exactly, target-derived Composition/Part evidence is produced without redrawing the target, and a fresh VPU80 Axe VoxelSpec is iterated against Hero.Voxel renders.

The proof intentionally records rejected model iterations. The first model misread the target as a double-bit axe; the second corrected the semantic form but retained an undersized rectangular blade; the final model converges to the target's single broad stepped blade, dark poll/head, small gold inset, wrapped handle, gray collar, and pommel. The target hash never changes during those repairs.

### 11. Prove the workflow on a second independent generated target

A second `Art/Samples/KnifeStyleTest` proof guards against an Axe-specific success path. Its accepted OpenAI image-model target is delivered through the same exact-byte desktop bridge, then frozen while target-derived construction evidence, normalized VPU candidates, and Hero.Voxel model iterations converge to the knife.

The density comparison is generated from one normalized construction definition at VPU64, VPU80, and VPU96. The bright cutting-edge band is a meaningful target feature: a candidate must preserve it as a distinct readable material region rather than merging it into the dark blade body. The lowest sufficient candidate is selected and the target hash must remain unchanged through all model repair.

## Risks / Trade-offs

- **[Risk] An execution environment has no image-generation provider.** -> Fail closed at Product Target authoring and report the missing capability; do not degrade visual authority.
- **[Risk] Provider output is visible but cannot be copied into the Runner.** -> Require exact-byte delivery/import and block if transport is unavailable.
- **[Risk] v1 backward compatibility leaves a theoretical loophole for a malicious/new agent.** -> The maintenance/Product Target skills explicitly forbid creating new v1 generated provenance; v2 is the current generated workflow. Future migration can remove v1 once legacy targets are intentionally regenerated.
- **[Risk] Provider/model identifiers are not enough to reproduce nondeterministic generation.** -> The contract treats generation as creative provenance, not deterministic build output; exact accepted target bytes remain authority.
- **[Risk] A generated target is attractive but impossible to voxelize within budgets.** -> Existing downstream density/budget gates remain authoritative; the target may require intentional redesign rather than silent simplification.

## Migration Plan

1. Add provenance-v2 contracts and validation while keeping v1 behavior unchanged.
2. Add regression tests for valid v2, missing/non-image generator, stale request, and v1 compatibility.
3. Add a dedicated Product Target authoring skill and update the maintenance workflow to invoke it for generated targets.
4. Update WORKFLOW.md with exact-byte/fail-closed semantics.
5. Remove procedural Product Target generator scripts from Sword02/Dagger02 without changing their accepted target bytes or provenance.
6. Run full Tool tests, repository scan/plan, strict OpenSpec validation, and diff hygiene.

Rollback restores the previous Tool/skill/docs behavior and deleted helper scripts; accepted target PNGs and existing publication evidence are otherwise unchanged.
