# reference-density-selection Specification

## Purpose
Defines how voxel-store derives each unit's minimum sufficient authoring density from accepted orthographic reference evidence, apparent voxel cadence, intended world size, and a shared low-VPU ruler while preserving bounded source complexity.

## Requirements

### Requirement: Accepted Part Sheets drive density analysis
Reference-driven equipment density selection SHALL use current accepted Part Sheets plus an explicit intended target world extent. Arbitrary source, marketing, hero, or collage images SHALL NOT be measured directly for VPU selection.

#### Scenario: Accepted Part Sheet is available
- **WHEN** an equipment Part Sheet has current accepted reference evidence and a target world extent is declared for density analysis
- **THEN** density analysis uses that Part Sheet and target extent as its measurement authority

#### Scenario: Only an external source image is available
- **WHEN** visual intent exists only as a perspective, marketing, hero, collage, or otherwise unaccepted source image
- **THEN** VPU selection does not measure that image directly and waits for the canonical Part Sheet to be accepted

### Requirement: VPU is selected per unit
Reference-driven VPU SHALL be selected independently for each unit from that unit's accepted reference evidence and intended world extent. The system SHALL NOT apply one global VPU to an equipment class, catalog, or project.

#### Scenario: Two equipment units use different visual granularity
- **WHEN** two accepted equipment references exhibit different apparent voxel cadence at their intended world sizes
- **THEN** density selection may choose different VPU values for the two units

#### Scenario: Asset-class prior exists
- **WHEN** the shared ruler defines a typical VPU range for the unit's asset class
- **THEN** that range guides candidate search but does not force the selected VPU

### Requirement: Shared VPU ruler owns numeric search policy
The repository SHALL keep the canonical VPU candidate ladder and broad asset-class priors in one shared density ruler. Ruler priors SHALL guide search order and SHALL NOT act as hard minimums, maximums, or selected VPU values.

#### Scenario: Reference estimate falls inside a typical range
- **WHEN** a unit's cadence-derived estimate falls near candidates within its asset-class prior
- **THEN** density selection evaluates nearby canonical ruler candidates beginning around that estimate

#### Scenario: Reference requires density outside the prior
- **WHEN** accepted reference evidence is faithfully represented only by a ruler candidate outside the asset-class typical range
- **THEN** the outside-prior candidate may be selected if it is the lowest sufficient candidate

### Requirement: Meaningful reference features are explicit density inputs
Density analysis SHALL record the object's accepted reference extent, apparent voxel-cadence measurements, intended world extent, and visually meaningful construction relationships used to judge candidate sufficiency. Antialiasing, compression noise, labels, shadows, and decorative context SHALL NOT become density-driving cadence or features.

#### Scenario: Thin cage bar is product-significant
- **WHEN** an accepted Part Sheet contains a cage bar whose thickness and spacing are required to remain readable in the voxel model
- **THEN** density input records the bar and gap relationship as meaningful construction evidence without imposing an arbitrary multi-voxel minimum

#### Scenario: Tiny edge pixels are not product-significant
- **WHEN** a Part Sheet contains subpixel antialiasing, compression artifacts, or non-construction decoration smaller than meaningful authored features
- **THEN** those pixels do not increase the estimated VPU

#### Scenario: Apparent voxel cadence is measurable
- **WHEN** accepted orthographic evidence exposes repeated cube steps or other stable voxel-scale cues along a known object extent
- **THEN** density input records the apparent voxel count or cadence measurement used to estimate per-unit VPU

### Requirement: Intentional one-voxel features are valid
A meaningful voxel-art feature SHALL NOT be required to occupy multiple voxels merely to pass density selection. One-voxel highlights, bevels, edge bands, straps, gaps, and silhouette steps MAY satisfy fidelity when they remain intentional, readable, and consistent with the accepted reference cadence.

#### Scenario: One-voxel edge is intentional
- **WHEN** a low-density candidate preserves a target cutting edge as a distinct intentional one-voxel band
- **THEN** the candidate is not rejected solely because the edge is only one voxel thick

#### Scenario: Feature disappears
- **WHEN** quantization removes, merges, or materially changes a meaningful reference feature
- **THEN** the candidate is rejected regardless of whether other features remain readable

### Requirement: Density estimation and candidate generation are deterministic
For identical accepted-reference hashes, cadence measurements, intended world extent, and shared ruler revision, the selector SHALL produce the same estimated per-unit VPU and the same ordered candidate sequence.

#### Scenario: Density analysis is repeated unchanged
- **WHEN** accepted reference evidence, cadence measurements, target world extent, and ruler revision are unchanged
- **THEN** the estimated VPU and candidate sequence are identical

#### Scenario: Estimated density cannot fit Hero.Voxel axis limits
- **WHEN** every sufficient ruler candidate would require any authored model axis to exceed Hero.Voxel's 256-voxel limit
- **THEN** density selection fails closed instead of silently changing world size or selecting an unproven lower density

### Requirement: Candidate evidence records structural fidelity and bounded complexity
Each evaluated VPU candidate SHALL record whether accepted reference cadence and meaningful construction survive, normalized world dimensions, occupied source voxels, authored integer bounds, Hero.Voxel axis-limit compliance, and greedy-mesh complexity evidence. Candidate acceptance SHALL be structural rather than based solely on whole-image pixel similarity.

#### Scenario: Candidate loses a meaningful reference feature
- **WHEN** a rendered candidate loses a declared meaningful feature such as a required cage gap, material region, silhouette step, or thickness relationship
- **THEN** that candidate is rejected even if its overall rendered image is otherwise similar to the reference

#### Scenario: Candidate exceeds source budget
- **WHEN** a candidate preserves all declared visual features but exceeds the active store source-voxel budget
- **THEN** that candidate is rejected and cannot be selected

### Requirement: The lowest sufficient VPU is selected
The selector SHALL choose the lowest evaluated canonical ruler candidate that preserves the accepted reference's apparent voxel style and meaningful construction relationships while satisfying normalized-size, axis-limit, and source-budget constraints. If no evaluated candidate passes, selection SHALL fail closed.

#### Scenario: Multiple low-density candidates pass
- **WHEN** VPU 12, 16, and 20 all preserve the accepted reference cadence and meaningful construction within hard constraints
- **THEN** the selected VPU is 12

#### Scenario: Lower candidate loses target construction
- **WHEN** VPU 12 materially merges a required guard gap but VPU 16 preserves it without introducing style-inappropriate excess density
- **THEN** VPU 16 is selected

#### Scenario: No candidate passes
- **WHEN** all valid candidates either lose required construction relationships, materially violate the accepted voxel cadence, or violate a hard constraint
- **THEN** no VPU is selected and source authoring cannot advance

### Requirement: Density evidence is provenance-bound and staleable
Density evidence SHALL identify accepted reference provenance, cadence measurements, intended world extent, shared ruler revision, estimated VPU, evaluated candidates, and selected per-unit VPU. Changes to upstream reference evidence, density measurements, world extent, or ruler revision SHALL stale the previous selection.

#### Scenario: Accepted Part Sheet changes
- **WHEN** a Part Sheet used by current density evidence is replaced or reaccepted with a different hash
- **THEN** the previous density selection becomes stale and must be recomputed before source publication

#### Scenario: Target world extent changes
- **WHEN** the intended world extent used by density analysis changes
- **THEN** the previous selected VPU becomes stale even if the Part Sheet image is unchanged

#### Scenario: Shared ruler changes
- **WHEN** the canonical VPU ladder or its revision changes
- **THEN** previous density evidence becomes stale until candidate selection is recomputed against the current ruler
