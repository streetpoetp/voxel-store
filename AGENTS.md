# Voxel Store Agent Contract

`voxel-store` is a source-authoritative library for reusable voxel visual assets. It is not a game project and it does not own a voxel runtime.

## Authority

- `policy/**`, `toolchain/**`, `Art/**/asset.yaml`, accepted `Art/**/Reference/**`, editable `*.voxel.yaml`, optional editable `*.vox`, and optional editable `*.animation.yaml` are authoritative.
- `*.generated.vox` and `*.animation.generated.yaml` are machine-owned reproducible Hero.Voxel siblings. Never edit them manually and never move them to a separate generated-art tree.
- `Art/**/Proof/**` is evidence. Visual PASS must be explicitly sealed against exact input hashes; compilation alone is not visual acceptance.
- `catalog/*.generated.json` is derived state. Never use it as the sole authority for identity, compatibility, coverage, or publication.

## Hero.Voxel boundary

- Hero Studio/Hero.Voxel is the only VoxelSpec parser/compiler, preview authority, generated VOX writer, animation-manifest writer, and downstream Unity bake/runtime technology.
- Resolve Hero.Voxel through `toolchain/hero-voxel.lock.yaml` plus a portable machine-local provider root such as `HERO_STUDIO_ROOT`. Never check in a developer-local absolute path.
- Before agent-driven reference or source authoring, resolve and read the pinned canonical `src/client/unity/docs/voxel-art-workflow.md`. Do not copy that document's semantic rules into store skills.

## Content boundary

- Store metadata may own visual identity, style family, palette, bounds, physical compatibility family, mount sockets, and authoring evidence.
- Do not add damage, rarity, class, inventory rules, AI, progression, economy, combat balance, or other game-specific semantics.
- Avatars and equipment are independent assemblies. Do not create precomposed avatar/equipment variants or per-pair correction records.

## Maintenance

- Begin from deterministic `Tool` scan/plan output.
- Repair broken published foundation assets before filling coverage gaps.
- One maintenance invocation owns at most one asset-scoped creative/repair job.
- A failed gate stays with the same asset and earliest owning stage; do not silently skip to the next asset.
- A healthy/no-gap store is a valid no-op.
