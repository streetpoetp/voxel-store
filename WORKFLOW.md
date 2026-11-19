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
    dotnet run --project Tool/VoxelStore.Tool -- seal-reference <asset-id>
    dotnet run --project Tool/VoxelStore.Tool -- seal <asset-id>

Set `HERO_STUDIO_ROOT` to a compatible Hero Studio checkout before building/running because the Tool references Hero.Voxel's standalone .NET project, not copied source.

## Visual and construction authority

The project has three distinct visual-authority layers:

    shared style authority
        -> Product Target
        -> Hero.Voxel construction sheets
        -> density (when reference-driven)
        -> VoxelSpec source

- **Shared style authority** defines reusable visual grammar and the family quality bar. Construction-style `references` keep legacy sheet/style behavior; target-specific `targetReferences` provide the polished common + matching-kind inputs used to create Product Targets. Exemplar-specific palette, material hierarchy, accent colors, wrap/decorative motifs, or other identity choices are **not** automatically global style rules; they become family-wide only when active style authority explicitly declares them as such.
- **Product Target** is per-asset visual-fidelity authority for target-driven work. It answers “what should this asset look like?” and preserves silhouette, proportions, material/color hierarchy, and meaningful construction richness.
- **Composition/Part Sheets** are Hero.Voxel construction authority. They answer “how is this visual target constructed?” while retaining Hero.Voxel view/orientation/part semantics.

No layer silently substitutes for another. A Product Target cannot replace required orthographic construction sheets, and a structurally valid sheet cannot pass merely because its labels describe detail that is not visibly present.

### Generated Product Target authoring

For a new target-driven asset with no approved supplied Product Target, creative target generation is an **image-model stage**. Use `.agents/skills/voxel-store-product-target/SKILL.md`.

- The image-generation model consumes the applicable polished `targetReferences` plus asset/form intent.
- Treat style references as grammar/quality evidence rather than copy templates. Preserve explicitly global style invariants, but let asset intent establish a distinct silhouette, palette, material hierarchy, accent family, and motif where those choices are not globally constrained. Do not mechanically repeat another asset's colors/material recipe merely because it appears in the style exemplars.
- `System.Drawing`, SVG/canvas/PIL scripts, procedural pixel/voxel rendering, and VoxelSpec previews are not valid substitutes for newly generated Product Target authority. Those tools remain valid downstream for construction layout, diagnostics, or proof where appropriate.
- The exact visually accepted image-model output must become the repository `target.png`. If the result is visible but cannot be transferred into the project byte-for-byte, stop at Product Target authoring; do not redraw it locally.
- For the same-desktop ChatGPT/WebCodex path, an accepted image downloaded to the Runner-visible filesystem may be imported with `Tool/import-product-target.ps1`; the importer copies the PNG and verifies source/destination SHA-256 equality. This is a transport bridge only—the downloaded image-model output remains the creative authority.
- Newly generated targets use provenance v2 with `generator.kind: image-model`, provider/model identity, and a hash-bound repository `target.request.json` (or equivalent request artifact). Existing accepted generated provenance v1 may remain unchanged until an intentional redesign.
- Generate/review/iterate candidates before freezing authority. After acceptance, downstream construction and voxel geometry converge to the target; the target is not simplified merely to make modeling easier.

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
        -> imported approved target OR image-model Product Target generation
        -> exact-byte project delivery + current target provenance
        -> target visual acceptance
        -> Composition
        -> target-fidelity audit
        -> freeze RequiredParts
        -> one Part Sheet at a time
        -> target-fidelity + Hero.Voxel audit
        -> density when applicable
        -> source
        -> generation/proof
        -> model-to-Product-Target visual convergence
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

### Per-unit VPU ruler authority

VPU is a **per-unit implementation property**, not a global project or equipment-class density.

- `openspec/specs/reference-density-selection/spec.md` owns density-selection behavior.
- `policy/vpu-ruler.yaml` owns the shared numeric candidate ladder and broad search priors. Its ranges guide candidate search only; they are not hard bounds and never directly select VPU.
- Each unit's accepted Part Sheets plus intended world extent provide apparent voxel-cadence measurements.
- Each unit's `Density/input.yaml` owns those measurements; `Density/evidence.generated.json` owns evaluated candidates and the selected VPU for that unit.
- Intentional one-voxel highlights, bevels, edge bands, straps, gaps, and silhouette steps are valid when readable and faithful to the accepted reference. Density must not be raised merely to make every feature multiple voxels thick.
- `policy/coverage.yaml` remains catalog coverage authority and must not contain VPU policy.
- Product Target, Composition, and Part Sheets remain implementation-neutral and must not declare the selected VPU.

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
