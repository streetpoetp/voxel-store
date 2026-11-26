# Proposal

## Why

Voxel Store still enforces the superseded rule that an equipped model's animated Mount must remain coincident with the current body socket, which forbids authored position motion on one-part equipment and makes equipment variants differ mostly by rotation. Hero Studio `61bf50c1` now separates logical attachment/reference identity from each model track's final visual world transform, so the store can allow authored equipment takes to produce distinct spatial motion without introducing gameplay-specific placement modes.

## What Changes

- **BREAKING** Upgrade the pinned Hero Studio provider from `d94c4818` to published revision `61bf50c135d509d4b59e051143475f708dd78aa1`, including the matching workflow fingerprint in `toolchain/hero-voxel.lock.yaml`.
- **BREAKING** Replace the store-wide animated-Mount lock rule with a reference-plus-model-transform contract: logical attachment stays valid while authored equipment animation may move the visible Mount-bearing part away from the socket reference.
- Permit position animation on current one-part equipment while retaining the existing single `Mount`, rigid one-part assembly, canonical frame, and bind-Mount requirements.
- Update compatibility proof to validate bind/reference correctness and finite authored animation evaluation rather than requiring animated Mount world origin equality at every sampled phase.
- Migrate the Unity preview from removed `RefreshBindings`/`Reset` behavior to the new Hero.Voxel attachment-reference and model-track transform flow. Socket-relative presentation explicitly consumes the current attachment reference; authored part animation remains free to create visual displacement.
- Update capture and PlayMode proof so attachment identity/reference validity is enforced while visual Mount drift is allowed during authored takes.
- Redesign `Assets/Equipment/Axe03` baseline `Attack` (V0) and `Attack.V1` with distinct authored position+rotation trajectories, while `Idle` remains a stable socket-relative hold, proving animation-authored spatial variants without a clip-name placement state machine.
- Do not add `SocketAttached`, detach/reattach gameplay state, target semantics, damage timing, replay/network requirements, or a new voxel/source metadata schema.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-compatibility`: equipment compatibility no longer requires evaluated animated Mount coincidence with the body socket; logical binding/reference validity remains required while authored visual motion may drift.
- `asset-publication`: current one-part equipment may publish semantic position tracks on the Mount-bearing part, with publication validating authored finite motion rather than rejecting translation categorically.
- `unity-baked-preview`: preview/capture uses Hero Studio's attachment-reference plus model-track transform ownership and proves independent authored equipment spatial motion instead of continuous socket/Mount lock.

## Impact

- Provider dependency: `Dependencies/hero-studio` and `toolchain/hero-voxel.lock.yaml`.
- Store policy/tooling: `Tool/VoxelStore.Tool/StoreEngine.cs` and focused Tool tests around equipment composition and compatibility.
- Preview runtime/proof: `AvatarPresentation`, `EquipmentPresentation`, `VoxelModelMotionPlayer`, capture validation, RuntimeGallery PlayMode tests, and any direct `RefreshBindings` callers.
- Product content: `Assets/Equipment/Axe03/Source/Axe03.voxel.yaml` plus regenerated machine-owned outputs/proof required by the normal store pipeline.
- No Player/APK build is required; Unity Editor verification and command-driven Game View capture are sufficient acceptance surfaces.
