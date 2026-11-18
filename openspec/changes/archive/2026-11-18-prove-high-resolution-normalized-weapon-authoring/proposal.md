# Proposal

## Why

The current product-quality weapon experiment shows that authoring equipment at the avatar's low voxel density cannot reach the visual fidelity of the approved reference target. Hero.Voxel already normalizes authored voxel scale through `voxelsPerUnit` and uses greedy meshing, but voxel-store currently blocks higher-density equipment through a profile-VPU equality check and a 4096 occupied-voxel publication budget.

## What Changes

- Prove high-resolution normalized equipment authoring on exactly one dedicated staff sample derived from the current Staff01 proportions and the user-provided product-quality staff reference.
- Allow equipment to use a different positive `voxelsPerUnit` than its avatar compatibility profile when the equipment still has a canonical assembly origin and passes the same physical mount/frame evidence.
- Raise the store's occupied-source-voxel budget only enough to permit future publication of the high-resolution proof while preserving an explicit safety cap.
- Author the proof staff at substantially higher density, targeting VPU 64 and the same final world-space scale as Staff01, without mutating the current published Staff01 until visual acceptance is demonstrated.
- Measure the resulting Hero.Voxel output and greedy-mesh runtime complexity as proof that higher source density does not imply one runtime cube per source voxel.
- Keep every published weapon unchanged in this proof change. A Staff01 replacement or catalog-wide migration is explicitly out of scope until the sample visually passes.
## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-compatibility`: equipment physical compatibility no longer requires equipment VPU to equal avatar/profile VPU; compatibility is based on canonical mount/frame behavior in normalized world space.
- `asset-publication`: publication budget rules permit bounded high-resolution source voxel authoring when the asset remains deterministic, reference-driven, and within the declared store budget.

## Impact

- `Tool/VoxelStore.Tool/StoreEngine.cs` compatibility validation and tests.
- `policy/store.yaml` occupied-voxel publication budget.
- A dedicated high-resolution staff proof package containing the uploaded visual authority, VPU64 VoxelSpec/palette, generated VOX/preview evidence, and measured complexity evidence.
- No published asset id, UnitRef, form, profile id, Hand capability, avatar source, Staff01 source, or other weapon source changes are intended.
- Hero.Voxel itself is not modified; the proof relies on its existing VPU normalization, 256-voxel-per-axis compiler limit, and greedy-mesh runtime path.