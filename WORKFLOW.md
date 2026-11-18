# Voxel Store Workflow

## Bootstrap

1. Read `AGENTS.md`.
2. Resolve `HERO_STUDIO_ROOT` (or pass `-p:HeroStudioRoot=...`) and validate `toolchain/hero-voxel.lock.yaml`.
3. Run the store tool tests before changing deterministic contracts.
4. Use OpenSpec for architecture, ownership, schema, workflow, or cross-cutting changes.

## Deterministic commands

From the repository root:

    dotnet run --project Tool/VoxelStore.Tool -- scan --write
    dotnet run --project Tool/VoxelStore.Tool -- verify
    dotnet run --project Tool/VoxelStore.Tool -- plan
    dotnet run --project Tool/VoxelStore.Tool -- density <asset-id> [--write]
    dotnet run --project Tool/VoxelStore.Tool -- generate <asset-id>
    dotnet run --project Tool/VoxelStore.Tool -- seal <asset-id>

Set `HERO_STUDIO_ROOT` to a compatible Hero Studio checkout before building/running because the Tool references Hero.Voxel's standalone .NET project, not copied source.

## Visual and construction authority

The project has three distinct visual-authority layers:

    shared style authority
        -> Product Target
        -> Hero.Voxel construction sheets
        -> density (when reference-driven)
        -> VoxelSpec source

- **Shared style authority** defines reusable visual language. Construction-style `references` keep legacy sheet/style behavior; target-specific `targetReferences` provide the polished common + matching-kind inputs used to create Product Targets.
- **Product Target** is per-asset visual-fidelity authority for target-driven work. It answers “what should this asset look like?” and preserves silhouette, proportions, material/color hierarchy, and meaningful construction richness.
- **Composition/Part Sheets** are Hero.Voxel construction authority. They answer “how is this visual target constructed?” while retaining Hero.Voxel view/orientation/part semantics.

No layer silently substitutes for another. A Product Target cannot replace required orthographic construction sheets, and a structurally valid sheet cannot pass merely because its labels describe detail that is not visibly present.

## Reference package versions

### Legacy v2

Unchanged published assets may keep the existing v2 package:

    Composition
        -> freeze RequiredParts
        -> Part Sheet(s)
        -> source

A v2 repair does not force target-driven migration.

### Target-driven v3

New asset design and intentional visual redesign use v3:

    applicable target style authority
        -> Product Target
        -> target visual acceptance
        -> Composition
        -> target-fidelity audit
        -> freeze RequiredParts
        -> one Part Sheet at a time
        -> target-fidelity + Hero.Voxel audit
        -> density when applicable
        -> source
        -> generation/proof
        -> compatibility/publication

A v3 package declares `target.png` (or its mapped equivalent), target provenance, Composition, every required Part Sheet, and fidelity evidence. Any changed target/sheet/upstream target authority stales the corresponding evidence.

The fidelity audit must pass:

- silhouette
- proportions
- construction richness
- material/color hierarchy
- meaningful detail density
- visual-evidence dominance
- implementation neutrality

Product Target/Composition/Part authority must not freeze exact VPU, exact voxel W x H x D dimensions, source voxel budgets, pivots, sockets, or runtime transforms. For `density: reference-driven` equipment, VPU is selected only after the v3 reference gate passes.

Publication order for target-driven density equipment is:

    manifest
        -> reference (target/provenance/fidelity included)
        -> density
        -> source
        -> generation
        -> proof
        -> budget
        -> compatibility

## Agent maintenance

Use `.agents/skills/voxel-store-maintain/SKILL.md`. The skill consumes deterministic plan output, claims one job, reads the pinned Hero.Voxel authoring workflow, and owns visual/creative decisions only for that job. A failed Product Target, Composition, Part, density, source, proof, or compatibility gate remains owned by that same asset until repaired or explicitly blocked.

## Finalization

- Run `dotnet run --project Tool/VoxelStore.Tool.Tests`.
- Regenerate and verify catalog/health.
- Run strict OpenSpec validation and `git diff --check`.
- Commit only verified repository-owned work.
- Push only when a real upstream remote exists; never invent or force a push target.