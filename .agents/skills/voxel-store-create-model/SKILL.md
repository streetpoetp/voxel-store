---
name: voxel-store-create-model
description: Design and create one new production voxel asset from scratch through Product Target, construction, density, VoxelSpec, proof, compatibility, and publication. Use for prompts such as "create an axe", "design a weapon", or "create voxel model".
---

# Voxel Store Create Model

Use this skill only for creating one new production asset in `voxel-store`.

## Contract

- Create exactly one asset per invocation.
- Do not use another asset as a design template. Existing assets may be inspected only for repository conventions, never as palette, geometry, proportion, or motif authority.
- Shared visual authority comes from `Styles/**`; physical compatibility comes from model-owned avatar sockets plus equipment capability/side/`Mount` metadata; demand/form comes from `Collections/**` or the explicit user request.
- The new asset's Product Target is its first asset-specific visual authority. Never model first and invent/adjust the target afterward to match an easier model.
- Do not add gameplay semantics such as damage, rarity, class, progression, economy, AI, or balance.
- Do not publish an asset while any required gate is unresolved.

## Flow

1. **Bootstrap**
   - Read `AGENTS.md` and `WORKFLOW.md`.
   - Resolve/read the pinned Hero.Voxel `voxel-art-workflow.md` before source authoring.
   - Determine one requested asset form from the user or the current deterministic collection gap.

2. **Define identity**
   - Establish silhouette goal, material hierarchy, palette direction, distinctive construction, and restrained accents.
   - Separate shared style grammar from asset-specific identity.
   - Do not mechanically inherit another asset's colors, wraps, material recipe, proportions, or decorative motifs.

3. **Acquire and accept Product Target**
   - Invoke `voxel-store-product-target`.
   - Acquire or author a new asset-specific Product Target grounded in the active `Styles/**` authority without requiring a particular generator/provider.
   - Accept/freeze it only after visual inspection, repository-byte ownership, and provenance validation pass.
   - If valid Product Target authority cannot be established, stop before Composition, density, or VoxelSpec. There is no alternate authoring lane.

4. **Create construction authority**
   - Follow the pinned Hero.Voxel workflow: Product Target -> Composition -> one Part Sheet per required part.
   - Prefer the smallest meaningful rigid-part decomposition. Do not create parts merely for visual regions that do not need independent transforms.
   - Audit each construction sheet against the frozen Product Target before advancing.

5. **Select density**
   - For equipment using reference-driven density, run the Tool density gate and use `Store/vpu-ruler.yaml`.
   - Choose the lowest sufficient VPU that preserves meaningful silhouette/construction/detail cadence.
   - Never choose VPU from a hard-coded weapon default.

6. **Author source**
   - Create the asset directly under the appropriate `Assets/<Kind>/<Asset>/` root.
   - Author an asset-local palette and editable VoxelSpec source.
   - Make VoxelSpec converge to the accepted Product Target/construction package, not vice versa.

7. **Generate and inspect proof**
   - Compile/generate through the pinned Hero.Voxel provider.
   - Generate canonical multi-view standalone/part proof.
   - Visually inspect silhouette, proportions, materials/colors, meaningful construction detail, and readability against Product Target.
   - If proof materially misses the target, repair VoxelSpec/palette and regenerate. Compilation alone is not acceptance.

8. **Compatibility and publication**
   - Run applicable profile compatibility checks.
   - Seal accepted reference/proof evidence.
   - Run scan/verify and ensure the asset is published with no failed gate.
   - Regenerate derived Catalog state only from authoritative inputs.

## Completion

Success means the requested asset exists under `Assets/**`, its Product Target/reference authority is current, its editable source and generated proof match that authority, compatibility passes where applicable, and deterministic store state reports it published.

If any required authority or gate cannot be completed, report the exact blocking gate and stop with the asset unpublished rather than fabricating substitute evidence.
