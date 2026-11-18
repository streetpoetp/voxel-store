# Tasks

## 1. Compatibility density decoupling

- [x] 1.1 Remove equipment/profile VPU equality from equipment compatibility while retaining avatar/profile VPU conformance and canonical assembly-origin/frame checks; add regression coverage proving VPU64 equipment can pass a VPU8 profile and equipment without a valid assembly origin still fails, then verify the focused compatibility tests pass.

## 2. Bounded high-resolution publication

- [x] 2.1 Raise the active store occupied-source-voxel budget from 4096 to 65536 without changing the part budget; add or update budget regression coverage so an in-budget high-resolution asset passes and an over-budget asset still fails with its measured voxel count, then verify the focused publication/budget tests pass.

## 3. Product-quality staff proof sample

- [x] 3.1 Record exact provenance for the user's uploaded staff reference in `Art/Samples/HighResolutionStaffProof/Reference/source.json`, create a compact purple/cyan/gold/wood palette, and author a one-rigid-part VPU64 staff source near 24x24x96 with pivot `[12,12,12]`; verify Hero.Voxel compilation succeeds, occupied dimensions stay within the 256-axis limit, and normalized length equals 1.5 world units.
- [x] 3.2 Generate focused and standalone Hero.Voxel previews for the sample, visually audit that the shaft segmentation, stepped cyan orb, purple retaining cage, gold structural blocks, and layered depth are materially closer to the uploaded product-quality reference than the current Staff01, and keep published Staff01 unchanged.
- [x] 3.3 Generate `Proof/metrics.generated.json` containing VPU, occupied voxels, integer bounds, normalized world bounds, naive exposed faces, greedy-merged quads, vertices, and triangles; verify the report demonstrates meaningful greedy reduction and stays below the 65536 source-voxel cap.

## 4. Integration convergence

- [x] 4.1 Run the full VoxelStore build/test suite and authoritative scan; verify the published catalog remains healthy at 11 assets / 10 equipment / 40 passing compatibility rows / 0 gaps / `NextJob=null`, and verify no published weapon source or identity changed.
- [x] 4.2 Run strict OpenSpec validation, `git diff --check`, deterministic sample regeneration/preview checks, and workspace hygiene review; verify the change is strict-valid and only the intended compatibility/budget infrastructure plus isolated sample proof files were added or changed.
