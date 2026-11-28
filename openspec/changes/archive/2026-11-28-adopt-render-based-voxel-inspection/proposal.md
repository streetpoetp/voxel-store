## Why

Hero Studio now discovers actual render instances and delegates preview playback explicitly. The old hierarchy registration API was removed.

## What Changes

- Upgrade the pinned provider and align its lock identity.
- Remove per-model selection hosts and registration; delegate one preview scope for the existing world.
- Update the runtime grid proof and review documentation; retain existing content/capture semantics.

## Capabilities

### Modified Capabilities
- `unity-baked-preview`: direct provider selection and scoped playback.

## Impact

Preview Editor orchestration, proof, provider pin, and documentation only. No content or authoring evidence migration.
