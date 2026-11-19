# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

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

## REMOVED Requirements

### Requirement: Generated Product Targets use image-model authoring
**Reason**: Product Target authority must depend on accepted visual evidence and provenance, not on a mandatory image-generation technology.

**Migration**: Newly accepted targets use the producer-neutral acquisition requirement. Existing accepted provenance remains valid under its original contract.

### Requirement: Generated Product Target delivery preserves exact accepted bytes
**Reason**: Exact-byte ownership remains required, but it applies to every acquisition method rather than only image-model output.

**Migration**: Use the provider-neutral exact-repository-bytes requirement for newly accepted targets.

### Requirement: Generated Product Target provenance identifies the image-model request
**Reason**: Mandatory image-model provider/model/request evidence creates an unnecessary producer dependency.

**Migration**: Newly accepted targets use acquisition-neutral provenance; generator metadata is optional traceability evidence.
