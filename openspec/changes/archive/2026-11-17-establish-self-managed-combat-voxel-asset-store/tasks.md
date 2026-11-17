# Tasks

## 1. Repository and toolchain foundation

- [x] 1.1 Add `AGENTS.md` and `WORKFLOW.md` defining source/evidence/derived ownership, Hero.Voxel as the external canonical compiler/reference authority, the one-job maintenance boundary, and the ban on gameplay semantics; verify a fresh repository bootstrap surfaces these rules and no rule requires a developer-local absolute path.
- [x] 1.2 Create the standalone `Tool/` .NET project and test project with repository-root discovery and deterministic command exit/report conventions; verify the tool builds and its empty-store smoke test runs without Unity.
- [x] 1.3 Add `toolchain/hero-voxel.lock.yaml` plus portable local resolution of the pinned Hero Studio/Hero.Voxel contract; verify tests cover successful resolution, missing dependency, revision/fingerprint mismatch, and rejection of a checked-in developer-local absolute Hero Studio dependency.
- [x] 1.4 Add a Hero.Voxel adapter that consumes the resolved provider's public compile/preview APIs rather than copying VoxelSpec parsing; verify a fixture can compile, preview, expose compiled sockets, and read deterministic `preview-poses.json` samples through the adapter.

## 2. Asset publication contracts and deterministic scanning

- [x] 2.1 Define and parse `policy/store.yaml`, `policy/coverage.yaml`, compatibility-family YAML, and per-asset `asset.yaml` contracts with strict field validation; verify tests accept avatar/equipment examples and reject duplicate identities, unsupported kinds, malformed source paths, and gameplay-authority fields such as damage or rarity.
- [x] 2.2 Implement repository asset discovery and stable `id`/`UnitRef` uniqueness plus published-identity history checks; verify fixtures detect collisions and reject an incompatible identity change while allowing a new asset with a new semantic identity.
- [x] 2.3 Implement canonical Hero.Voxel source-layout and generated-sibling validation, including deterministic regeneration/byte comparison for spec-driven assets; verify generated artifacts remain beside their source base, stale generated output fails health, and regeneration restores the gate without treating generated files as authority.
- [x] 2.4 Implement the ordered publication gate model for manifest/identity, reference coverage, authoritative source, deterministic generation, standalone proof, budget, and applicable compatibility evidence; verify tests report the earliest failing stable gate id and do not allow later evidence to mask an earlier failure.
- [x] 2.5 Implement hash-bound visual/reference audit evidence so an agent's accepted Composition/Part/assembled proof is valid only for the exact authoritative inputs it audited; verify changing a reference, palette, VoxelSpec, or required proof invalidates stale audit evidence and removes derived published health.

## 3. Derived catalog, health, coverage, and maintenance planning

- [x] 3.1 Generate `catalog/catalog.generated.json` and `catalog/health.generated.json` exclusively from authoritative inputs plus current verification evidence; verify deleting both files and rescanning reconstructs equivalent semantic results.
- [x] 3.2 Implement coverage evaluation independent of current catalog state and emit machine-readable uncovered required slots; verify a missing required avatar/equipment slot is derived from `coverage.yaml` and disappears only when a healthy published asset satisfies it.
- [x] 3.3 Implement deterministic maintenance candidate ordering: invalid authority, regressed published evidence, compatibility failure, stale reproducible/derived output, required coverage gap, then explicitly allowed optional expansion; verify fixtures containing both failures and gaps always select the failure first.
- [x] 3.4 Add CLI commands for scan/verify/plan (or equivalent single-purpose surfaces) that emit machine-readable state and select at most one asset-scoped maintenance job; verify repeated runs over unchanged healthy inputs are idempotent and return no creative work.

## 4. Compatibility-family and composition proof

- [x] 4.1 Add the versioned `humanoid.medium.v1` physical profile with required `Hand.R`, `Hand.L`, and `Back` mounts plus explicit axis, scale/VPU, envelope, and representative pose-sample conventions; verify profile tests reject missing required mount semantics and incompatible in-place redefinition of a published profile version.
- [x] 4.2 Implement avatar conformance validation against compiled Hero.Voxel sockets and sampled pose evidence; verify fixtures pass only when all required sockets exist with valid bind/animated frames and fail with the responsible profile/socket when one is missing or incompatible.
- [x] 4.3 Implement equipment mount validation using one declared profile/socket and one canonical equipment local mount frame, with no per-pair correction records; verify schema/tests reject pair-specific correction data and confirm a canonical equipment fixture composes directly at the declared socket frame.
- [x] 4.4 Derive animated socket world frames by combining Hero.Voxel compiled socket metadata with provider-generated part pose samples, then generate family composition proof records for every current avatar/equipment pair in the profile; verify deterministic fixtures cover neutral and animated samples without reimplementing VoxelSpec parsing or Hero.Voxel pose sampling.
- [x] 4.5 Add composition preview/audit evidence for family pairs and fold current all-pairs results into compatibility health; verify a failed pair names the avatar, equipment, profile, mount, pose sample, and evidence path, and a new nonconforming avatar cannot become a healthy member of the family.

## 5. Self-managing agent workflow

- [x] 5.1 Add `.agents/skills/voxel-store-maintain/SKILL.md` that begins with the deterministic plan, claims only the first eligible job, resolves/reads the pinned canonical Hero.Voxel workflow, and never copies its semantic reference-package rules; verify a dry-run/simulated agent trace cannot select a second asset after claiming one job.
- [x] 5.2 Implement the agent sequence for repair/design jobs: resolve art policy, produce/audit only the Composition Sheet, freeze `RequiredParts`, produce/audit one Part Sheet at a time, author source, run generation/proofs, audit visuals, run compatibility, and regenerate health; verify a failed gate keeps ownership on the same asset and emits concrete blocking evidence instead of advancing coverage.
- [x] 5.3 Add maintenance guidance for healthy-store no-op behavior and explicit stop conditions for unresolved Hero.Voxel toolchain, ambiguous visual construction intent, or failed visual audit; verify simulations leave authoritative assets unchanged in each fail-closed/no-op case.

## 6. CombatFoundation.v1 proof content

- [x] 6.1 Add `CombatFoundation.v1` art/store policy, shared palette, bounded coverage policy, and the `humanoid.medium.v1` profile without gameplay stats; verify the deterministic scan reports exactly the intended initial avatar and three mount-coverage gaps and no optional expansion work.
- [x] 6.2 Use the maintenance workflow to design and publish one general medium humanoid avatar with canonical `Hand.R`, `Hand.L`, and `Back` sockets plus representative rigid-part animation needed by the profile; verify its reference package has 100% `RequiredParts` coverage, Hero.Voxel generation/proofs pass, and the asset becomes healthy published content.
- [x] 6.3 Use separate maintenance runs to design and publish one independent equipment asset for each required V1 mount (`Hand.R`, `Hand.L`, `Back`), keeping each equipment asset gameplay-neutral and source-authoritative; verify every asset passes its own reference/source/proof/budget gates and no precomposed avatar-equipment source is created.
- [x] 6.4 Run all-pairs family composition proof across the V1 avatar and equipment under required neutral/animated samples, repairing the owning asset/profile when necessary; verify `humanoid.medium.v1` ends with complete compatibility health and no pair-specific corrective transform data.

## 7. Integration acceptance

- [x] 7.1 Run the full store Tool test suite and a clean repository scan from regenerated outputs; verify all deterministic tests pass, catalog/health regenerate from authority, and the generated catalog contains only healthy publication state supported by current evidence.
- [x] 7.2 Run the maintenance planner twice on the completed `CombatFoundation.v1`; verify both runs select no unexpected creative work, do not mutate published identities/assets, and report only policy-declared future coverage if any exists.
- [x] 7.3 Audit the final repository for forbidden duplicate voxel technology, checked-in developer-local Hero Studio paths, gameplay semantics in store contracts, precomposed avatar/equipment variants, and per-pair correction records; verify the audit is clean and the canonical generated siblings remain beside their VoxelSpec sources.
- [x] 7.4 Run `openspec validate establish-self-managed-combat-voxel-asset-store --strict` and repository formatting/diff checks; verify the change and all implementation-owned files are valid before workflow finalization.

## Workflow follow-up

- Sync the validated delta specs into the canonical specs and archive the change after implementation acceptance.
- Finalize repository history according to the established `WORKFLOW.md`; if no remote is configured, preserve the verified local commit without inventing a push target.
