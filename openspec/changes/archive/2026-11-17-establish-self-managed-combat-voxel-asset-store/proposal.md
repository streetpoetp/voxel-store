# Proposal

## Why

Reusable combat voxel art currently has no independent source repository that can continuously design, verify, catalog, and repair a shared avatar-and-equipment foundation. Establishing `voxel-store` as a source-authoritative, self-managed asset store lets agents grow reusable visual content while preserving Hero.Voxel as the single compiler/bake/runtime technology rather than creating a parallel voxel pipeline.

## What Changes

- Establish a repository-owned voxel asset publication model for reusable avatars and equipment, with immutable semantic asset identity, canonical Hero.Voxel source ownership, reference-package evidence, generated-source verification, and derived publication health.
- Establish reusable physical compatibility families so avatars expose canonical visual mount sockets and equipment declares compatible mount profiles, allowing independent avatar and equipment assets to compose without prebuilding every avatar/equipment combination.
- Establish a self-management workflow that scans repository state, derives catalog/health/coverage, prioritizes broken or missing foundation assets, and drives one bounded design/repair job through reference, VoxelSpec, proof, compatibility, and publication gates.
- Keep generated Hero.Voxel siblings beside their source units, treat them as machine-owned reproducible artifacts, and consume Hero.Voxel through an explicitly resolved/pinned tool contract instead of copying its compiler/runtime into this repository or hard-coding a local checkout path.
- Seed the design around a narrow `CombatFoundation.v1`: one reusable humanoid compatibility family plus representative independently authored equipment, with expansion driven by repository policy after the foundation proves composition and self-management.
- Keep gameplay semantics such as damage, rarity, class, balance, AI, and progression outside the store contract; the store owns reusable visual assets and physical compatibility only.

## Capabilities

### New Capabilities

- `asset-publication`: Defines source-authoritative avatar/equipment asset records, immutable identity, Hero.Voxel-compatible source/reference/proof ownership, evidence-derived publication status, and generated catalog entries.
- `asset-compatibility`: Defines reusable avatar compatibility families, canonical mount sockets, equipment mount declarations, composition rules, and compatibility proof without per-pair corrective authoring.
- `store-maintenance`: Defines policy-driven repository scanning, derived health/coverage, bounded autonomous design/repair selection, failure-first maintenance, and regeneration of machine-owned catalog/health outputs.

### Modified Capabilities

None. This repository has no existing OpenSpec capabilities.

## Impact

- Adds the first repository-level contracts and implementation surface for `H:\projects\voxel-store`.
- Introduces store policy, asset manifests, compatibility-family definitions, agent workflow/skills, canonical `Art/...` source layout, proof/evidence layout, and generated catalog/health artifacts.
- Depends on Hero Studio's canonical Hero.Voxel reference-package and source/tool contracts while keeping Hero Studio external to this repository; Hero.Voxel remains responsible for VoxelSpec compilation, generated `.vox`/animation siblings, Unity baking, baked runtime assets, socket/pose semantics, and rendering integration.
- Does not add a Unity runtime, duplicate voxel compiler, game-specific balance/config data, or a combined avatar-with-equipment asset format.
