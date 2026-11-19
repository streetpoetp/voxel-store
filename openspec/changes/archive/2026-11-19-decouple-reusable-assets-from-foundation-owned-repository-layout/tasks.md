# Tasks

## 1. Establish purpose-driven store contracts

- [x] 1.1 Change Store policy to own store identity, selected collection, default style, optional expansion, and budgets under `Store/policy.yaml`.
- [x] 1.2 Replace active coverage policy with `Collections/combat-foundation.v1.yaml` and collection id/requirements semantics.
- [x] 1.3 Move compatibility profiles to `Profiles/**`, VPU/retired identity policy to `Store/**`, and generated catalog/health to `Catalog/**`.
- [x] 1.4 Make asset style declarations resolve reusable `Styles/**` authority and remove the foundation-specific store-wide style equality requirement.

## 2. Migrate product content

- [x] 2.1 Move published avatars/equipment to `Assets/Avatars/**` and `Assets/Equipment/**` while preserving every published asset id and UnitRef.
- [x] 2.2 Move `Art/Style/**` to `Styles/**` and migrate active asset style family to `low-resolution-voxel.v1`.
- [x] 2.3 Make the foundation avatar palette asset-local under its `Source` and update its manifest path.
- [x] 2.4 Move `Art/Samples/**` to top-level `Samples/**`; samples remain outside store discovery.
- [x] 2.5 Remove empty legacy `Art`/`policy` roots after all active authority has moved.

## 3. Update Tool and tests

- [x] 3.1 Locate repository root from `Store/policy.yaml` and discover assets only under `Assets/**`.
- [x] 3.2 Load the selected collection from `Collections`, profiles from `Profiles`, styles from `Styles`, global VPU/retired policy from `Store`, and derived outputs from `Catalog`.
- [x] 3.3 Update style resolution/validation so asset-declared styles are authoritative while `defaultStyle` supports default authoring APIs.
- [x] 3.4 Update all active Tool test fixtures/path assertions to the purpose-driven roots and add proof that `Samples/**` cannot be discovered as published assets.

## 4. Synchronize active documentation/contracts

- [x] 4.1 Update `AGENTS.md` authority/path descriptions to the new domain roots.
- [x] 4.2 Update `WORKFLOW.md` owner pointers (`Store`, `Collections`, `Profiles`, `Styles`, `Assets`, `Samples`, `Catalog`).
- [x] 4.3 Update active OpenSpec requirements from foundation-owned coverage wording to collection-driven reusable asset wording where applicable.

## 5. Regenerate and validate

- [x] 5.1 Verify published ids and UnitRefs are unchanged after the move.
- [x] 5.2 Regenerate/reseal path-sensitive reference/proof/catalog evidence with the pinned Hero.Voxel provider when available.
- [x] 5.3 Run Tool tests, scan/verify/plan, strict OpenSpec validation, and `git diff --check`.
- [x] 5.4 Verify only `Assets/**` is discoverable product content and existing `Samples/**` remains non-published.

Provider-bound validation used a temporary detached worktree from the existing local Hero Studio repository at pinned revision `56d2997250a8d49846a5246b52de5a03bc44b88e`; no external clone or lock change was required. Full Tool tests passed, all three migrated assets publish with 8/8 compatibility evidence, Catalog was regenerated from authority, and `verify` reports only the eight intentionally unfilled collection requirements. Deterministic planning selects `foundation.equipment.axe` as the next required coverage job.
