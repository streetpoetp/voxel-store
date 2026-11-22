# Proposal: Establish production voxel model creation skill

## Why

The repository has separate maintenance and Product Target skills, but no single production skill for the common user intent `design and create <asset>`. This leaves agents to reconstruct the end-to-end creation sequence and can encourage accidental reuse of old model/template assets.

## What Changes

- Add `voxel-store-create-model` as the production authoring skill for one new asset.
- Make the canonical flow target-first: intent -> Product Target -> construction -> density -> VoxelSpec -> proof -> compatibility -> publish.
- Make production creation independent of template/experimental assets and existing asset visual identity.
- Remove the deleted Samples root from active repository/workflow contracts.
- Keep specialized Product Target generation in `voxel-store-product-target` and deterministic validation in Tool/OpenSpec/Hero.Voxel.

## Non-goals

- No asset schema change.
- No Hero.Voxel semantic change.
- No VPU algorithm change.
- No changes to existing published asset identities.
