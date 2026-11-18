# Design

## Context

See `proposal.md` for motivation. The repository already has accepted Composition/Part Sheet authority, reference audit hashes, Hero.Voxel preview tooling, a 256-voxel-per-axis compiler limit, VPU-independent equipment compatibility, and a 65,536 occupied-source-voxel budget from the high-resolution proof. What is missing is a reproducible decision between accepted reference detail and canonical VoxelSpec density.

The current asset manifest does not own product world bounds before source exists. To avoid expanding semantic identity or compatibility contracts, this change treats normalized target size as an explicit density-analysis input rather than adding it to `asset.yaml`.

## Goals / Non-Goals

**Goals:**
- Make VPU selection reference-driven, auditable, staleable, and deterministic for identical analysis inputs.
- Preserve semantic visual judgment where it is needed while moving arithmetic, candidate ordering, hard constraints, and provenance validation into voxel-store.
- Choose the minimum sufficient VPU instead of globally preferring a fixed density.
- Prove the entire flow on Staff01 using the approved high-detail staff target and canonical reference-first sequencing.

**Non-Goals:**
- Building a general-purpose computer-vision model inside voxel-store.
- Measuring arbitrary perspective/marketing images directly for density selection.
- Changing Hero.Voxel compilation, preview, or greedy meshing.
- Adding a new runtime greedy-mesh budget; greedy complexity is evidence only in this change.
- Migrating the other nine weapon sources.

## Decisions

### 1. Density selection occurs after Part Sheet acceptance and before canonical source

The authoritative sequence becomes:

```text
external visual intent
  -> accepted Composition Sheet
  -> accepted Part Sheet(s)
  -> density input + candidate evidence
  -> selected VPU
  -> canonical VoxelSpec source
  -> generation / proof / compatibility / publication
```

Rationale: Part Sheets are orthographic, neutral-local, consistently scaled construction authority. Arbitrary source images can contain perspective, labels, multiple subjects, shadows, or decorative context that should not control density.

Alternative considered: estimate directly from the original uploaded image. Rejected because it would require voxel-store to understand non-canonical image composition and would bypass the existing reference-first boundary.

### 2. Separate semantic feature analysis from deterministic density math

Agent vision creates an auditable `Density/input.yaml` beside each reference-driven equipment asset. It contains:
- `targetWorldLongAxis`;
- the accepted Part Sheet/Part ID being measured;
- the object's long-axis pixel span;
- meaningful feature records with stable ids, pixel spans, importance, and minimum voxel spans.

Example:

```yaml
version: 1
targetWorldLongAxis: 1.5
parts:
  Staff:
    partSheet: Parts/Staff.png
    objectLongAxisPixels: 520
    features:
      - id: purple-cage-bar
        pixelSpan: 10
        importance: secondary
        requiredVoxelSpan: 3
      - id: crystal-facet
        pixelSpan: 8
        importance: tertiary
        requiredVoxelSpan: 2
```

The input is not generated publication truth; it is reviewable semantic analysis. Voxel-store validates its structure and binds generated density evidence to its hash and the current reference audit hash.

Rationale: code can deterministically compute from measurements, but it cannot reliably infer whether a tiny edge is product-significant versus antialiasing/noise without semantic visual judgment.

Alternative considered: raw edge-frequency/image-resolution heuristics. Rejected because high-resolution simple references would be over-densified and antialiasing artifacts could dominate VPU.

### 3. Use a deterministic feature-to-VPU estimate

For each meaningful feature:

```text
featureWorldSpan
  = targetWorldLongAxis * featurePixelSpan / objectLongAxisPixels

requiredVpu
  = requiredVoxelSpan / featureWorldSpan
```

Equivalently:

```text
requiredVpu
  = objectLongAxisPixels * requiredVoxelSpan
    / (featurePixelSpan * targetWorldLongAxis)
```

The unquantized estimate is the maximum required VPU across all declared meaningful features.

Rationale: the estimate scales with actual feature size relative to the product, not with source-image resolution alone.

### 4. Search a bounded VPU ladder around the estimate

Voxel-store quantizes the estimate to a 16-VPU ladder. Candidate search starts one quantum below the rounded-up estimate, never below 16, then increases by 16.

Example:

```text
estimated = 104
candidates = 96, 112, 128, 144, ...
```

A candidate is not eligible if its compiled authored bounds exceed 256 on any axis or its occupied source voxels exceed the active store budget.

Rationale: one step below the estimate tests whether semantic feature requirements were conservative, while the monotonic ladder makes repeated selection deterministic and avoids an arbitrary fixed list.

Alternative considered: use every integer VPU. Rejected because it multiplies candidate authoring/render cost without meaningful visual benefit.

### 5. Candidate sources and previews are evidence, not canonical source authority

Candidate work lives under:

```text
<Density>/
  input.yaml
  Candidates/
    vpu-096/
      Source/
      Proof/
      audit.json
    vpu-112/
      Source/
      Proof/
      audit.json
  evidence.generated.json
```

Candidate VoxelSpecs may be authored/resampled by the maintenance agent against the same accepted Part Sheet, but they are not the asset's canonical `Source/`. Each candidate audit records:
- every declared feature as preserved/lost;
- normalized world dimensions;
- integer bounds;
- occupied source voxels;
- axis-limit result;
- greedy merged quads/derived mesh metrics;
- hashes of candidate source and preview evidence.

Whole-image similarity may be recorded as supplemental evidence but cannot override a lost required feature.

Rationale: VPU cannot be validated visually without geometry, yet temporary candidate geometry must not become source authority before selection completes.

### 6. Select the first passing candidate and validate feature-analysis completeness

Candidates are evaluated in ascending VPU order. The first candidate preserving every declared meaningful feature and satisfying hard constraints is the provisional selection.

The maintenance workflow also evaluates the next higher eligible candidate. If that higher candidate reveals a meaningful reference feature that materially improves but was absent from `input.yaml`, the feature analysis is incomplete: the new feature is added to the density input and the deterministic selection reruns. Otherwise the lower passing candidate remains selected.

Rationale: this guards against a too-sparse semantic feature list while preserving the rule that density should not increase merely because more voxels are available.

### 7. Generated density evidence is provenance-bound

`Density/evidence.generated.json` records:
- current reference audit SHA-256;
- density input SHA-256;
- target world long axis;
- deterministic estimate;
- candidate sequence;
- candidate audit/source/proof hashes and metrics;
- selected VPU and selection reason.

Any change to the accepted reference audit, Part Sheet, or density input invalidates the evidence. Candidate artifacts do not become upstream authority.

### 8. Add density to publication without collapsing gate ownership

Equipment adopts the new gate through an explicit manifest migration flag:

```yaml
density: reference-driven
```

For opted-in equipment the health order becomes:

```text
manifest
  -> reference
  -> density
  -> source
  -> generation
  -> proof
  -> budget
  -> compatibility
```

Legacy equipment without the flag keeps the pre-density `manifest -> reference -> source -> ...` sequence until it is migrated in a later asset job. The density gate validates current evidence and a selected VPU. The source gate retains source ownership checks and additionally requires the canonical VoxelSpec VPU to equal the selected VPU for opted-in equipment.

Rationale: this enables asset-by-asset migration without making all currently healthy equipment fail the moment the new gate lands. Missing/stale density should report as density failure for opted-in equipment, while missing/invalid source remains a source failure.

### 9. Voxel-store owns selection; Hero.Voxel remains provider technology

Add selector/evidence contracts and density validation to `VoxelStore.Tool`. Add a CLI operation that can evaluate/write density evidence for one asset after semantic input and candidate audits exist. Hero.Voxel remains responsible only for compiling/rendering candidate and canonical VoxelSpecs.

Rationale: reference fidelity and publication policy belong to the store; putting product-density policy in Hero.Voxel would make the generic renderer aware of one store's art-quality rules.

### 10. Staff01 is the end-to-end validation asset

Staff01 first opts into `density: reference-driven`. Before density analysis, its canonical reference package is refreshed from the approved product-quality staff target using the normal Composition-first / Part-Sheet-first workflow. Its normalized long-axis target remains 1.5 world units.

The selector, not a task constant, chooses Staff01's VPU. Candidate evidence is generated around that estimate. Only after selection is canonical Staff01 source authored at the chosen VPU and regenerated/resealed.

Other weapon sources remain unchanged.
Their manifests also remain without the density opt-in in this change, preserving healthy incremental migration.

## Risks / Trade-offs

- **[Risk] Semantic feature annotations vary between agents** -> Require stable feature ids, explicit measurements, importance, minimum voxel spans, reviewable Part Sheet provenance, and deterministic selection after annotation.
- **[Risk] Candidate authoring changes shape between VPUs and confounds density comparison** -> Require every candidate to target the same accepted Part Sheet, normalized product size, feature list, and construction intent; candidate audits record source/proof hashes.
- **[Risk] Candidate search becomes expensive** -> Start near the mathematical estimate, use a 16-VPU ladder, stop after the first pass plus one higher completeness check, and fail on hard bounds early.
- **[Risk] Target world size is arbitrary** -> Make `targetWorldLongAxis` explicit evidence. For Staff01 migration, freeze the current 1.5-unit product length. Future asset jobs must supply their own physical authoring constraint rather than deriving it from image resolution.
- **[Risk] Higher VPU always appears visually smoother** -> Selection is based on declared feature survival, not generic smoothness or raw pixel similarity.
- **[Trade-off] Density/input.yaml contains agent-authored semantic analysis** -> Intentional; visual significance is semantic. The reproducible part begins once those measurements are accepted.

## Migration Plan

1. Add density input/evidence contracts and deterministic estimate/candidate-selection tests.
2. Add density gate validation and source-VPU agreement without changing avatar behavior.
3. Extend maintenance planning so accepted equipment reference with missing/stale density becomes the next blocking work before source.
4. Refresh Staff01's product-quality canonical reference package and accept it.
5. Produce Staff01 density input from the accepted Part Sheet and target world length 1.5.
6. Generate/evaluate VPU candidates until the lowest sufficient density is selected.
7. Author Staff01 canonical source at the selected VPU, regenerate/reseal proof and compatibility, and verify the catalog converges healthy.
8. Run strict OpenSpec, deterministic evidence, full Tool tests, and planner no-op validation.

Rollback restores the previous Staff01 reference/source/proof together and removes density evidence/gating. The other nine weapon sources are unaffected.
