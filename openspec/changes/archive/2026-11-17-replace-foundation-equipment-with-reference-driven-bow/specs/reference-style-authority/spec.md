# Spec Delta

## ADDED Requirements

### Requirement: Active style authority remains independent of retired asset outputs
An active style-reference scope SHALL resolve to repository-owned visual inputs that remain valid independently of any retired asset's generated source, proof, or construction-reference outputs.

#### Scenario: Retired asset-derived style board remains referenced
- **WHEN** an active style scope points only to an image derived from an asset that has been retired from the baseline
- **THEN** style authority is invalid until that dependency is removed or replaced by independent visual authority

#### Scenario: Supplied source image becomes equipment style authority
- **WHEN** an approved external source image is imported as a repository-owned style input and listed in the equipment scope
- **THEN** equipment reference generation may use that image without depending on any currently published equipment model

### Requirement: Style-source identity is preserved through import
A repository-owned copy of an approved supplied style image SHALL preserve the source image bytes or an explicitly recorded canonical derivative so visual provenance can be audited independently of generated asset evidence.

#### Scenario: Exact supplied image is imported
- **WHEN** the approved source image can be stored directly in the repository
- **THEN** its hash becomes the concrete equipment-style provenance input

#### Scenario: Canonical derivative is required
- **WHEN** repository constraints require a derivative rather than the exact source image
- **THEN** the derivative relationship and source hash are recorded so the style authority remains traceable to the supplied image
