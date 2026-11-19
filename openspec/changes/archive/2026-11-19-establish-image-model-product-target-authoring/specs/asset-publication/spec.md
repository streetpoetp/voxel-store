# Spec Delta

## ADDED Requirements

### Requirement: New generated target-driven publication requires image-model Product Target authority
For new target-driven authoring that generates rather than imports a Product Target, reference acceptance SHALL require current image-model generation provenance before Composition, Part, density, or source authority may advance.

#### Scenario: New generated target has current image-model provenance
- **WHEN** a new target-driven asset has an accepted image-model Product Target with current generation/style provenance
- **THEN** the workflow may derive and audit Composition/Part construction evidence before later density/source gates

#### Scenario: New generated target exists only as procedural output
- **WHEN** a new target-driven asset has a generated PNG but no current image-model generation provenance
- **THEN** publication remains blocked at Product Target/reference authority and downstream geometry does not qualify

#### Scenario: Imported approved Product Target is used
- **WHEN** the asset uses a valid imported Product Target with current source provenance
- **THEN** image-model generator provenance is not required because the imported source is the visual authority
