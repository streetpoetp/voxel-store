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
    dotnet run --project Tool/VoxelStore.Tool -- generate <asset-id>
    dotnet run --project Tool/VoxelStore.Tool -- seal <asset-id>

Set `HERO_STUDIO_ROOT` to a compatible Hero Studio checkout before building/running because the Tool references Hero.Voxel's standalone .NET project, not copied source.

## Agent maintenance

Use `.agents/skills/voxel-store-maintain/SKILL.md`. The skill consumes deterministic plan output, claims one job, reads the pinned Hero.Voxel authoring workflow, and owns visual/creative decisions only for that job.

## Finalization

- Run `dotnet run --project Tool/VoxelStore.Tool.Tests`.
- Regenerate and verify catalog/health.
- Run strict OpenSpec validation and `git diff --check`.
- Commit only verified repository-owned work.
- Push only when a real upstream remote exists; never invent or force a push target.
