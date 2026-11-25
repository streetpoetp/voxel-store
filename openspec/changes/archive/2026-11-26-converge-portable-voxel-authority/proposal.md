# Proposal

## Why

The current voxel-store is healthy in the existing checkout but three active authorities no longer describe one coherent system: sealed text evidence is sensitive to checkout line endings, maintenance guidance still treats retired `Profiles/**` as authority, and compatibility documentation still retains obsolete Body-ratio composition wording alongside the current socket/Mount contract.

## What Changes

- Make sealed evidence hashing checkout-portable for text inputs by hashing canonical LF text while continuing to hash binary evidence byte-for-byte.
- Add regression proof that equivalent CRLF/LF text produces the same sealed evidence and that substantive text changes still stale the audit.
- Remove retired `Profiles/**` ownership from active agent guidance, model-creation guidance, and maintenance requirements; current reusable compatibility is derived from model-owned sockets/Mount metadata plus asset/collection discriminators.
- Remove obsolete Body-ratio composition wording and duplicated canonical-frame authority from the active compatibility spec.
- Preserve the existing Hero.Voxel actor, socket/Mount attachment, render, streaming, pooling, and benchmark architecture unchanged.
- Do not build a Player/APK or claim new low-device performance evidence in this change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-publication`: sealed text evidence becomes invariant to checkout line-ending representation while binary evidence remains byte-exact.
- `store-maintenance`: active authority and coverage rules no longer depend on retired compatibility profiles.
- `asset-compatibility`: canonical composition wording is converged exclusively on model-owned sockets and equipment Mount frames.

## Impact

- `Tool/VoxelStore.Tool`: evidence hashing/verification behavior.
- `Tool/VoxelStore.Tool.Tests`: portability regression coverage.
- `AGENTS.md` and `.agents/skills/voxel-store-create-model/SKILL.md`: active authoring authority.
- `openspec/specs/{asset-publication,store-maintenance,asset-compatibility}`: durable behavior.
- Existing runtime APIs, baked asset formats, Hero Studio pin, asset identities, and current published catalog remain unchanged.
