# Proposal

## Why

New production asset creation currently couples Product Target validity to image-model generation metadata and a provider-specific exact-byte handoff path. This makes the production workflow depend on ChatGPT/image-generation infrastructure even though the real authority is the accepted target image, its provenance, and its visual fidelity.

## What Changes

- Make Product Target acquisition provider-neutral: a target may come from any valid acquisition path that can establish repository-owned bytes and current provenance.
- Keep Product Target mandatory for new production authoring; if no acceptable target can be acquired, the production asset remains blocked and downstream construction/source work does not begin.
- Remove image-model-specific validity requirements such as mandatory `generator.kind=image-model`, provider/model identity, and mandatory generation-request evidence from the canonical new-target contract.
- Preserve optional generator/source metadata as traceability evidence when available, without making one producer technology authoritative.
- Keep the production flow single-lane and target-first: no Samples fallback, no model-first continuation, and no alternate publication path when Product Target acquisition is blocked.
- Preserve backward compatibility for existing accepted Product Target provenance while introducing the provider-neutral contract for newly accepted targets.
- Update workflow/skills so they say acquire/accept Product Target rather than requiring ChatGPT or any particular image-generation provider.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `reference-target-fidelity`: Product Target provenance and acceptance become provider-neutral while remaining hash-bound, style-aware, visually audited, and fail-closed.
- `asset-publication`: new target-driven publication requires an accepted Product Target regardless of acquisition origin, not image-model provenance specifically.
- `store-maintenance`: new production creation acquires and accepts Product Target before downstream work and remains blocked when that authority is unavailable, with no Samples/model-first fallback.

## Impact

- `Tool/VoxelStore.Tool/Contracts.cs` and `StoreEngine.cs` Product Target provenance validation.
- Product Target provenance tests in `Tool/VoxelStore.Tool.Tests/Program.cs`.
- `.agents/skills/voxel-store-product-target/SKILL.md`, `voxel-store-create-model/SKILL.md`, and workflow guidance.
- Existing accepted v1/v2 provenance must remain valid; newly accepted targets use the provider-neutral provenance contract.
- No Hero.Voxel semantic change, VPU algorithm change, asset identity change, or Samples authoring lane is introduced.
