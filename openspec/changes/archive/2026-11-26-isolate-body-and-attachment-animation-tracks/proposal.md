# Proposal

## Why

The current composed-actor animation contract gives body and equipment different clip identities but still couples them through one actor-wide plan, one body-owned clock, one normalized phase, and driver-side body reactions. This prevents truly independent weapon animation and causes attachment topology changes or weapon playback to affect unrelated body/weapon animation state.

## What Changes

- **BREAKING**: Remove actor-wide synchronized body/equipment playback as the default animation model.
- **BREAKING**: Remove weapon-driven body reaction from the shared voxel actor contract. Playing or changing a weapon animation SHALL NOT select, restart, or otherwise modify body animation.
- **BREAKING**: Remove the requirement that attached models share one semantic motion, duration, loop state, variant, or normalized phase.
- Give the body and every attached model its own ordinary animation playback state: independently selected clip, clock/phase, loop state, speed, start/stop/reset/sample lifecycle.
- Keep physical composition independent from animation ownership: each equipment root continues to follow the current animated body socket through socket × inverse bind-Mount while the equipment model samples its own animation.
- Keep coordination consumer-owned and generic. A consumer MAY issue ordinary commands to multiple model tracks in the same update (for example body `Death` plus weapon `Death`), but Hero.Voxel/voxel-store SHALL NOT add hard-coded `Die`, `PlayDeath`, synchronization-group, weapon-drives-body, or equivalent semantic coordination APIs.
- Attachment add/remove/rebind/replace SHALL affect only the relevant attachment track/topology; body playback and unrelated attachment playback SHALL continue unless explicitly commanded by the consumer.
- Update the preview/evaluation surface so body and each equipment instance can be inspected and controlled independently, with clip choices discovered from that model's own baked animation asset.
- Update graphical proof to cover independent clocks/motions and continued attachment lock rather than shared-phase synchronization.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-compatibility`: replace driver-side body reaction and shared-phase body/equipment animation requirements with model-local independent animation ownership while retaining current socket/Mount attachment authority.
- `unity-baked-preview`: replace actor-wide synchronized preview playback with independent body/equipment model tracks, independent Inspector/control surfaces, and proof that attachment remains correct while tracks diverge.

## Impact

- Hero Studio provider foundation: `hero.voxel.actor` currently owns one `VoxelActorAnimationPlan`, one clock, and actor-wide invalidation. A provider revision is required before voxel-store can adopt the new contract.
- Voxel-store pin: after the provider change is published, `Dependencies/hero-studio` and `toolchain/hero-voxel.lock.yaml` must advance together.
- Preview runtime: `AvatarPresentation`, `AvatarMotionPlayer`, `EquipmentPresentation`, motion candidate/control code, and capture scenarios will change from one coordinated actor motion to per-model playback controls.
- Tests/proof: provider actor tests must prove independent clocks and topology isolation; voxel-store preview tests/capture must prove mixed body/weapon motions, different speeds/phases, consumer-issued simultaneous Death commands, and continued socket/Mount lock.
- No gameplay semantics move into voxel-store. Consumers remain responsible for deciding when body and equipment should coincidentally play related clips.
