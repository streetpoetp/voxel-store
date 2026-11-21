# Voxel Store Agent Contract

`voxel-store` is a source-authoritative library for reusable voxel visual assets. It is not a game project and it does not own a voxel runtime.

## Authority

- `Store/**`, `Collections/**`, `Profiles/**`, `Styles/**`, `toolchain/**`, `Assets/**/asset.yaml`, accepted `Assets/**/Reference/**`, editable `*.voxel.yaml`, optional editable `*.vox`, and optional editable `*.animation.yaml` are authoritative.
- `*.generated.vox` and `*.animation.generated.yaml` are machine-owned reproducible Hero.Voxel siblings. Never edit them manually and never move them to a separate generated-art tree.
- `Assets/**/Proof/**` is evidence. Visual PASS must be explicitly sealed against exact input hashes; compilation alone is not visual acceptance.
- `Catalog/*.generated.json` is derived state. Never use it as the sole authority for identity, compatibility, collection coverage, or publication.

## Domain ownership

- `Assets/**` owns reusable product content.
- `Styles/**` owns reusable visual grammar; a collection is not a style family.
- `Profiles/**` owns reusable physical compatibility contracts.
- `Collections/**` owns desired reusable asset coverage; collections select assets but do not contain or own their published identity.
- `Store/**` owns global store configuration, numeric authoring policy, and retired identity history only.

## Production authoring

- New production models are authored from current `Collections/**`, `Styles/**`, `Profiles/**`, explicit asset intent, and a newly accepted asset-specific Product Target.
- Do not use another asset or an experimental/template asset as upstream visual/model authority for a new production asset.
- Product Target precedes construction, density, VoxelSpec, proof, compatibility, and publication. Do not model first and create or simplify the target afterward to justify the model.

## Hero.Voxel boundary

- Hero Studio/Hero.Voxel is the only VoxelSpec parser/compiler, preview authority, generated VOX writer, animation-manifest writer, and downstream Unity bake/runtime technology.
- Resolve Hero Studio/Hero.Voxel from the repository-owned `Dependencies/hero-studio` Git submodule and verify it against `toolchain/hero-voxel.lock.yaml`. Initialize it with `git submodule update --init --recursive`; do not use a developer-local Hero Studio checkout as production dependency authority.
- Before agent-driven reference or source authoring, read the canonical `Dependencies/hero-studio/src/client/unity/docs/voxel-art-workflow.md` from that pinned submodule. Do not copy that document's semantic rules into store skills.
## Content boundary

- Store metadata may own visual identity, style family, palette, bounds, physical compatibility family, mount sockets, and authoring evidence.
- Do not add damage, rarity, class, inventory rules, AI, progression, economy, combat balance, or other game-specific semantics.
- Avatars and equipment are independent assemblies. Do not create precomposed avatar/equipment variants or per-pair correction records.

## Maintenance

- Begin from deterministic `Tool` scan/plan output.
- Repair broken published assets before filling selected-collection gaps.
- One maintenance invocation owns at most one asset-scoped creative/repair job.
- A failed gate stays with the same asset and earliest owning stage; do not silently skip to the next asset.
- A healthy/no-gap store is a valid no-op.
