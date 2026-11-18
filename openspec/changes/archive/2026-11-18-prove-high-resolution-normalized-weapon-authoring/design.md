# Design

## Context

See `proposal.md` for motivation. Hero.Voxel already treats authored voxel size as `1 / voxelsPerUnit`, supports up to 256 voxels per model axis, and bakes runtime geometry through its greedy mesher. The current store adds two unrelated restrictions: equipment compatibility requires equipment VPU to equal the compatibility profile VPU, and publication rejects assets above 4096 occupied source voxels.

The proof must establish visual feasibility before any published weapon is migrated.

## Goals / Non-Goals

**Goals:**
- Prove that a substantially denser voxel source can preserve Staff01's world-space scale while producing product-level stepped detail.
- Prove that equipment compatibility does not need equal avatar/equipment authoring density.
- Keep a bounded source-voxel safety budget while allowing a VPU64 product-quality asset.
- Produce inspectable visual and numeric evidence from one isolated staff proof.

**Non-Goals:**
- Replacing or republishing Staff01 in this change.
- Migrating the other nine weapon forms.
- Modifying Hero.Voxel.
- Treating greedy meshing as permission for unlimited source complexity.

## Decisions

### 1. Use a dedicated non-published proof asset

The sample lives under `Art/Samples/HighResolutionStaffProof/` and has no `asset.yaml`, so normal store scanning does not treat it as catalog content.

The package contains:
- `Reference/source.json`: provenance for the user's uploaded product-quality reference (filename, SHA-256, dimensions); the conversation attachment remains the visual authority because the host-native binary import bridge is unavailable in this session.
- `Source/HighResolutionStaffProof.voxel.yaml`
- `Source/HighResolutionStaffProof.palette.yaml`
- `Generated/HighResolutionStaffProof.generated.vox`
- `Proof/Standalone/`: Hero.Voxel preview output.
- `Proof/metrics.generated.json`: density, bounds, world-size, and greedy-merge surface metrics.

Rationale: the user asked for a sample to test the solution. Keeping published Staff01 untouched makes visual failure cheap to discard and prevents an experimental asset from destabilizing the healthy catalog.

### 2. Author the sample at VPU64 and preserve Staff01 world length

The current Staff01 is 12 voxels long at VPU8, so its authored length is 1.5 world units. The proof targets a 96-voxel long source at VPU64:

`96 / 64 = 12 / 8 = 1.5`

The proof uses approximately `24 x 24 x 96` authored bounds. Its assembly-origin pivot is scaled from Staff01's `[1.5, 1.5, 1.5]` to `[12, 12, 12]`, preserving the same normalized pivot offset.

Rationale: this isolates visual-density improvement from product-scale changes.

Alternative considered: VPU96. Rejected for the first proof because VPU64 already provides 8x linear resolution over VPU8 while keeping source cost materially lower.

### 3. Build product detail from volumetric forms, not many arbitrary boxes

The staff source uses:
- a segmented brown shaft with thickness variation;
- multiple gold collars and end hardware;
- a filled stepped cyan orb/crystal volume;
- a purple retaining cage around the orb;
- gold structural blocks around the cage;
- restrained cyan/purple accents.

Rounded forms are voxelized as deterministic scanline runs rather than a single rectangular primitive, so the orb and cage read as stepped 3D forms from multiple views.

Rationale: the previous change failed because geometry stayed fundamentally rectangular even when primitive count increased.

### 4. Decouple equipment VPU from compatibility profile VPU

Equipment compatibility retains:
- declared profile and capability;
- canonical assembly origin;
- avatar socket/frame validation;
- required pose/socket evidence.

It drops only the requirement that equipment VPU equals the profile/avatar VPU. Avatar conformance still requires the avatar to match its profile VPU.

Rationale: avatar profile VPU describes the avatar's authored scale contract; equipment authoring density is an implementation choice once normalized world-space mounting is preserved.

Alternative considered: create `humanoid.medium.v3` at VPU64. Rejected because that would incorrectly force avatar art to migrate merely to permit denser equipment.

### 5. Raise the store source-voxel safety cap to 65,536

The proof target is expected to exceed 4096 occupied voxels but remain far below 65,536. The cap remains a hard publication safety gate.

Rationale: 65,536 is large enough for a VPU64 product asset while still bounding pathological source density. Runtime greedy mesh complexity is recorded separately and does not replace the source budget.

### 6. Measure greedy-merge complexity without changing Hero.Voxel

The proof report counts exposed colored voxel faces after applying the same axis-by-axis contiguous same-color rectangle merge rule used by Hero.Voxel's `GreedyMesherJob`. It records:
- VPU;
- occupied voxel count;
- occupied integer bounds;
- normalized world bounds;
- naive exposed faces;
- greedy merged quads;
- derived vertex and triangle counts assuming one quad -> four vertices/two triangles.

The report is proof evidence, not a new runtime contract. Hero.Voxel source inspection separately establishes that Unity baking uses `GreedyMesherJob`.

## Risks / Trade-offs

- **[Risk] VPU64 still looks too blocky** -> Treat visual failure as a failed proof and do not migrate Staff01; VPU96 remains a later option.
- **[Risk] Source voxel count grows rapidly around solid orb volumes** -> Keep the 65,536 source cap and record actual occupied count.
- **[Risk] Greedy complexity estimate diverges from a future Hero.Voxel implementation** -> Name the metric as proof-time greedy-merge evidence; do not expose it as store authority.
- **[Risk] Density decoupling hides a scale mismatch** -> Explicitly prove normalized sample bounds and pivot scale, while compatibility continues to require a valid assembly origin and socket/frame evidence.
- **[Trade-off] The proof sample is not publishable content** -> Intentional; visual acceptance must precede replacing a healthy published asset.

## Migration Plan

1. Add compatibility tests proving higher-VPU equipment can pass and missing assembly origin still fails.
2. Raise the store occupied-source-voxel cap to 65,536 and retain the existing budget gate.
3. Import the uploaded reference into the isolated proof package.
4. Author the VPU64 staff sample and compact product-style palette.
5. Compile with the pinned Hero.Voxel Tool and render focused/standalone previews.
6. Generate proof metrics and visually compare the sample with the uploaded reference.
7. Run the full VoxelStore suite and confirm the published 11-asset catalog remains healthy and unchanged.

Rollback removes the sample package and restores the compatibility equality check/budget value. No published asset migration needs reversal.
