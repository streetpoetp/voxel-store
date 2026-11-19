# reference-target-fidelity Specification

## Purpose
Defines asset-specific Product Target authority and the evidence required to preserve its visual fidelity while converting visual intent into Hero.Voxel construction sheets.

## Requirements

### Requirement: Target-driven references declare an asset-specific Product Target
A target-driven reference package SHALL declare one repository-owned Product Target PNG that defines the asset's visual identity, silhouette, proportions, material/color hierarchy, and intended construction richness before Composition or Part Sheets become authoritative.

#### Scenario: Approved supplied image becomes the Product Target
- **WHEN** an approved supplied asset image is suitable as direct visual authority
- **THEN** the target-driven package declares a repository-owned exact copy or traceable canonical derivative as its Product Target

#### Scenario: Product Target is generated from style authority
- **WHEN** an asset has form intent and active style authority but no direct approved product image
- **THEN** the workflow acquires or authors a Product Target, visually accepts it against the asset intent and style authority, and freezes it before generating construction sheets

### Requirement: Product Target provenance is current and auditable
Target-driven acceptance SHALL bind the Product Target to its current bytes and to any declared source/style authority used to acquire or approve it. A change to the Product Target or its declared upstream authority SHALL stale downstream fidelity acceptance.

#### Scenario: Product Target changes after fidelity acceptance
- **WHEN** the declared Product Target bytes change after Composition or Part fidelity evidence was accepted
- **THEN** the previous fidelity evidence becomes stale and the affected construction sheets must be re-audited

#### Scenario: Upstream target authority changes
- **WHEN** source or style authority declared by the Product Target provenance changes
- **THEN** the Product Target must be revalidated before downstream construction fidelity may remain current

### Requirement: Product Target acquisition is producer-neutral
A newly accepted Product Target SHALL be valid based on repository-owned target bytes, current provenance, applicable style acceptance, and visual fidelity rather than on a required producer technology. Generator, model, application, or provider identity SHALL NOT be mandatory for Product Target validity.

#### Scenario: Repository-authored target reaches the required visual authority
- **WHEN** a Product Target is authored directly into the repository and passes the applicable style and visual acceptance criteria
- **THEN** it may become Product Target authority without image-model or external-provider evidence

#### Scenario: External generation metadata is available
- **WHEN** a Product Target was created using an external generator and provider/model/request metadata is available
- **THEN** that metadata may be retained as traceability evidence but is not required to make the accepted target valid

#### Scenario: No acceptable Product Target can be acquired
- **WHEN** no available acquisition path can produce repository-owned target bytes that pass current provenance, style, and visual acceptance
- **THEN** the production asset remains blocked at Product Target authority and downstream construction/source work does not begin

### Requirement: Product Target acceptance owns exact repository bytes
The visually accepted Product Target SHALL exist as repository-owned bytes and its provenance SHALL bind the exact accepted target hash. A candidate that cannot become repository-owned without substitution SHALL NOT itself become Product Target authority.

#### Scenario: Accepted candidate is stored exactly
- **WHEN** the accepted target candidate can be written or imported into the repository without changing its bytes
- **THEN** those bytes become the declared Product Target and are hash-bound by provenance

#### Scenario: External candidate cannot cross the project boundary
- **WHEN** a candidate is visible outside the repository but its accepted bytes cannot be written or imported exactly
- **THEN** that candidate is not accepted as Product Target authority; the workflow must acquire another valid target or remain blocked

### Requirement: New Product Target provenance is acquisition-neutral
New Product Target provenance SHALL bind the target hash and all declared upstream source/style authority. Producer-specific metadata MAY be recorded but SHALL be optional. Existing accepted provenance versions SHALL remain valid when unchanged and otherwise current under their original contracts.

#### Scenario: New target has no generator metadata
- **WHEN** a newly accepted Product Target has repository-owned bytes, current declared upstream authority, and no generator/provider/model metadata
- **THEN** provenance validation may pass without inventing generator evidence

#### Scenario: Optional generator metadata changes
- **WHEN** optional generator metadata changes without changing the accepted target bytes or declared authoritative source/style inputs
- **THEN** Product Target validity does not depend on that metadata unless the provenance explicitly declares it as hash-bound upstream authority

#### Scenario: Existing accepted provenance remains unchanged
- **WHEN** a previously accepted Product Target still uses a valid earlier provenance contract and is not intentionally re-acquired
- **THEN** backward-compatible validation continues without requiring migration to the new acquisition-neutral contract

### Requirement: Construction sheets must preserve Product Target fidelity
Before a target-driven Composition or Part Sheet is accepted, current fidelity evidence SHALL verify silhouette, proportions, construction richness, material/color hierarchy, and meaningful detail density against the Product Target. Every required criterion SHALL pass.

#### Scenario: Composition is structurally valid but visually simplified
- **WHEN** a Composition Sheet has the required dimensions/layout but materially collapses the Product Target's silhouette, proportions, construction richness, color hierarchy, or meaningful detail
- **THEN** target-fidelity acceptance fails and the sheet cannot become construction authority

#### Scenario: Part Sheet preserves target construction intent
- **WHEN** a Part Sheet preserves the Product Target's relevant shape, thickness, material regions, stepped contours, and meaningful features while satisfying Hero.Voxel construction rules
- **THEN** its target-fidelity criterion may pass

### Requirement: Visual evidence dominates explanatory annotation
Target-driven Composition and Part Sheets SHALL present sufficiently large readable asset/construction views for visual audit. Text, labels, manifests, or detail-hierarchy descriptions SHALL NOT substitute for visible geometry or color/material evidence.

#### Scenario: Sheet describes detail that is not visually present
- **WHEN** annotations claim secondary or tertiary construction detail that is absent or unreadable in the rendered asset views
- **THEN** target-fidelity acceptance fails regardless of the annotation text

#### Scenario: Sheet is primarily visual evidence
- **WHEN** the asset views visibly communicate the target's construction and annotations remain secondary
- **THEN** the sheet may proceed to the remaining fidelity and Hero.Voxel construction checks

### Requirement: Visual reference authority does not preselect implementation density
Product Target, Composition, and Part Sheets SHALL NOT own exact VPU, exact voxel dimensions, or source voxel budgets. Those implementation-density decisions SHALL remain downstream of accepted visual authority.

#### Scenario: Composition declares tiny target voxel bounds before density selection
- **WHEN** a target-driven Composition Sheet attempts to freeze an exact voxel bound such as a fixed W x H x D voxel count before density evaluation
- **THEN** the reference package fails visual-reference acceptance and must remove that implementation-density constraint

#### Scenario: Product Target requires richer construction than an earlier model
- **WHEN** accepted visual authority contains more meaningful detail than an existing low-density model
- **THEN** the workflow preserves the visual authority and lets downstream density/source authoring expand rather than simplifying the target to match the old model

### Requirement: Fidelity evidence is provenance-bound and staleable
Target-driven fidelity evidence SHALL identify the Product Target and audited sheet hashes plus the pass/fail result for every required visual criterion. Any audited input change SHALL invalidate the prior evidence.

#### Scenario: Audited Composition Sheet changes
- **WHEN** a Composition Sheet changes after its fidelity evidence is recorded
- **THEN** the evidence becomes stale until the changed sheet is visually re-audited against the current Product Target

#### Scenario: All audited inputs remain unchanged
- **WHEN** Product Target, audited sheets, and their declared upstream authority remain byte-identical
- **THEN** repeated validation recognizes the existing fidelity evidence as current
