# Design: Production voxel model creation skill

## Ownership

`voxel-store-create-model` owns orchestration of one new production asset. It does not duplicate detailed semantics owned by Product Target, Hero.Voxel, density, compatibility, or publication contracts.

## Canonical flow

```text
collection/user form
  -> distinct asset identity
  -> Product Target
  -> Composition/Parts
  -> density
  -> VoxelSpec/palette
  -> generated proof + visual iteration
  -> compatibility
  -> publication
```

There is no template-asset branch. Existing assets may inform repository conventions only.

## Fail-closed behavior

Any unresolved upstream authority blocks downstream work. In particular, a missing or unaccepted Product Target blocks VoxelSpec authoring for a new production asset.

## Prompt surface

Once installed, a user can request simply `design and create an axe`; the skill carries the production workflow without requiring the user to restate it.
