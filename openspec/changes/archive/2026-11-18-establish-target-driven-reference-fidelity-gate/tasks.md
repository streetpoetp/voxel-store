# Tasks

## 1. Target-style authority without legacy regression

- [x] 1.1 Extend the reference-style contract/resolver to support style-manifest v3 target-specific `targetReferences` while continuing to resolve legacy `references` exactly as today; add focused tests proving manifest v2 remains accepted, manifest v3 resolves target common + matching-kind inputs, and the existing construction-style authority hash is unchanged when only target-specific fields are added.
- [x] 1.2 Upgrade `Art/Style/low-resolution-voxel.v1/style.yaml` to the compatible target-aware form with polished `reference-02.png` in target `common` authority and the existing equipment images as target equipment guidance; verify equipment Product Target resolution includes the polished common image while unchanged v2 asset reference audits remain current.

## 2. Target-driven reference-contract v3

- [x] 2.1 Extend `ReferenceContract` to support legacy v2 and target-driven v3 with declared Product Target image, target provenance, Composition, RequiredParts, Part Sheets, and fidelity evidence; add contract tests covering valid v2/v3 packages plus missing/invalid target, fidelity, part mapping, and PNG inputs.
- [x] 2.2 Add deterministic `target.provenance.json` contracts/validation for imported and generated Product Targets, including source hash/derivative relation when imported and current target-style authority hashes when generated; verify target/source/style changes stale provenance while unchanged inputs validate identically.
- [x] 2.3 Add `fidelity.audit.json` contracts/validation requiring exact Product Target/sheet hashes, exact audited-sheet coverage, and PASS for silhouette, proportions, construction richness, material/color hierarchy, meaningful detail density, visual-evidence dominance, and implementation neutrality; verify missing criteria, failed criteria, unknown/missing sheets, and stale hashes fail closed.

## 3. Reference sealing and publication behavior

- [x] 3.1 Update reference enumeration/sealing so v3 includes target image, target provenance, Composition, every Part Sheet, and fidelity evidence before writing `audit.generated.json`, while v2 enumeration/sealing remains behaviorally identical; verify deterministic resealing tests and byte-stable legacy v2 audit evidence.
- [x] 3.2 Update reference validation/publication so v3 fails before source authority on missing/stale Product Target provenance or fidelity evidence and passes only when the full target-driven package is current; verify unchanged v2 assets continue through the existing reference gate without forced migration.
- [x] 3.3 Keep implementation-density authority downstream by making `implementationNeutral` a mandatory fidelity criterion and by rejecting reference metadata that attempts to own VPU/voxel dimensions/budgets; verify a target-driven fixture with failed implementation-neutral evidence is blocked before density/source while existing reference-driven VPU selection continues unchanged after a valid v3 reference passes.

## 4. Autonomous maintenance workflow guidance

- [x] 4.1 Update `.agents/skills/voxel-store-maintain/SKILL.md` so new/design-redesign jobs resolve target-style authority, establish/accept Product Target, audit Composition against it, freeze RequiredParts, audit one Part Sheet at a time, then proceed to density/source; verify the skill explicitly keeps a failed target/sheet on the same asset and forbids exact VPU/voxel-bound decisions in visual-reference authority.
- [x] 4.2 Update `WORKFLOW.md` to document the three authority layers—shared style -> Product Target -> Hero.Voxel construction sheets—and the v2 legacy/v3 target-driven migration boundary; verify the documented sequence agrees with the tool gate order and the existing reference-driven density stage.

## 5. Bounded visual-fidelity proof

- [x] 5.1 Add a non-published `Art/Samples/TargetDrivenReferenceProof/` package using an existing high-quality repository-owned weapon/style image as Product Target authority, then build a visual-first Composition example with large readable weapon evidence and minimal annotation; visually verify it preserves target silhouette, proportions, material/color hierarchy, and meaningful construction richness and contains no exact VPU/voxel-bound declaration.
- [x] 5.2 Add representative Part construction evidence plus current target provenance/fidelity audit for the sample and verify the v3 validation path accepts the package deterministically; keep Hero.Voxel construction semantics authoritative and do not publish the sample into catalog/coverage.
- [x] 5.3 Record the current Axe01 Composition/Part sheets as a negative visual-fidelity example without modifying published Axe01: document which required criteria fail (tiny visual evidence, simplified construction richness, annotation substituting for visible detail, premature `6 x 3 x 10` voxel bounds) and verify an equivalent failing fidelity fixture is rejected by the tool.
- [x] 5.4 After the sample v3 reference package is accepted, author a non-published Bow VoxelSpec/palette, compile it with the pinned Hero.Voxel Tool, generate individual-part and assembled preview evidence, and record a target-to-model visual audit; verify the generated VOX is deterministic, the model preserves the target silhouette/proportions/material hierarchy/meaningful construction detail, and no published asset/catalog authority changes.

## 6. Integration convergence

- [x] 6.1 Run the full VoxelStore build/test suite and authoritative scan; verify the existing catalog remains healthy with 11 published assets / 10 equipment / 40 passing compatibility rows / 0 gaps / `NextJob=null`, and verify all unchanged v2 reference audit hashes and published asset source hashes remain unchanged by this workflow-only migration.
- [x] 6.2 Run `openspec validate establish-target-driven-reference-fidelity-gate --strict`, `git diff --check`, target/fidelity deterministic validation, semantic-boundary audits, and workspace hygiene review; verify only target-style/v3-reference/fidelity workflow infrastructure plus the bounded non-published proof were added.

## Workflow follow-up

- Archive the change only after visual review of the target-driven sample and implementation review are accepted.
