# Spec Delta

## Purpose

Defines how voxel-store derives the minimum sufficient equipment authoring density from accepted orthographic reference evidence while preserving normalized product size and bounded source complexity.

## ADDED Requirements

### Requirement: Accepted Part Sheets drive density analysis
Reference-driven equipment density selection SHALL use current accepted Part Sheets plus an explicit normalized target world extent. Arbitrary source, marketing, hero, or collage images SHALL NOT be measured directly for VPU selection.

#### Scenario: Accepted Part Sheet is available
- **WHEN** an equipment Part Sheet has current accepted reference evidence and a target world extent is declared for density analysis
- **THEN** density analysis uses that Part Sheet and target extent as its measurement authority

#### Scenario: Only an external source image is available
- **WHEN** visual intent exists only as a perspective, marketing, hero, collage, or otherwise unaccepted source image
- **THEN** VPU selection does not measure that image directly and waits for the canonical Part Sheet to be accepted

### Requirement: Meaningful reference features are explicit density inputs
Density analysis SHALL record the object's reference pixel extent and one or more visually meaningful feature measurements, including each feature's pixel span, importance, and minimum required voxel span. Antialiasing, compression noise, labels, shadows, and decorative context SHALL NOT become density-driving features.

#### Scenario: Thin cage bar is product-significant
- **WHEN** an accepted Part Sheet contains a cage bar whose thickness is required to remain readable in the voxel model
- **THEN** density input records that cage bar as a meaningful feature with its measured pixel span and required voxel span

#### Scenario: Tiny edge pixels are not product-significant
- **WHEN** a Part Sheet contains subpixel antialiasing, compression artifacts, or non-construction decoration smaller than meaningful authored features
- **THEN** those pixels do not increase the estimated VPU

### Requirement: Density estimation and candidate generation are deterministic
For identical accepted-reference hashes, meaningful-feature measurements, required voxel spans, and normalized target world extent, the selector SHALL produce the same estimated VPU and the same monotonically increasing candidate sequence.

#### Scenario: Density analysis is repeated unchanged
- **WHEN** density analysis is rerun with unchanged accepted-reference evidence, feature measurements, and target world extent
- **THEN** the estimated VPU and candidate sequence are identical

#### Scenario: Estimated density cannot fit Hero.Voxel axis limits
- **WHEN** every candidate at or above the estimated VPU would require any authored model axis to exceed Hero.Voxel's 256-voxel limit
- **THEN** density selection fails closed instead of silently lowering resolution below the reference requirement

### Requirement: Candidate evidence records structural fidelity and bounded complexity
Each evaluated VPU candidate SHALL record whether meaningful reference features survive, normalized world dimensions, occupied source voxels, authored integer bounds, Hero.Voxel axis-limit compliance, and greedy-mesh complexity evidence. Candidate acceptance SHALL be structural rather than based solely on whole-image pixel similarity.

#### Scenario: Candidate loses a meaningful reference feature
- **WHEN** a rendered candidate loses a declared meaningful feature such as a required cage gap, material region, silhouette step, or thickness relationship
- **THEN** that candidate is rejected even if its overall rendered image is otherwise similar to the reference

#### Scenario: Candidate exceeds source budget
- **WHEN** a candidate preserves all declared visual features but exceeds the active store source-voxel budget
- **THEN** that candidate is rejected and cannot be selected

### Requirement: The lowest sufficient VPU is selected
The selector SHALL choose the lowest evaluated VPU that preserves every declared meaningful reference feature and satisfies normalized-size, axis-limit, and source-budget constraints. If no candidate passes, selection SHALL fail closed.

#### Scenario: Multiple candidates pass
- **WHEN** VPU 80, 96, and 112 all preserve every declared feature and satisfy all hard constraints
- **THEN** the selected VPU is 80

#### Scenario: No candidate passes
- **WHEN** all valid candidates either lose required features or violate a hard constraint
- **THEN** no VPU is selected and source authoring cannot advance

### Requirement: Density evidence is provenance-bound and staleable
Density evidence SHALL identify the accepted reference provenance, density-input measurements, normalized target world extent, estimated VPU, evaluated candidates, and selected VPU. A change to any upstream accepted Part Sheet or density input SHALL stale the previous selection.

#### Scenario: Accepted Part Sheet changes
- **WHEN** a Part Sheet used by current density evidence is replaced or reaccepted with a different hash
- **THEN** the previous density selection becomes stale and must be recomputed before source publication

#### Scenario: Target world extent changes
- **WHEN** the normalized target world extent used by density analysis changes
- **THEN** the previous selected VPU becomes stale even if the Part Sheet image is unchanged
