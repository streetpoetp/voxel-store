# Spec Delta

## ADDED Requirements

### Requirement: Source authoring resolves applicable animation profile
The canonical pipeline SHALL establish the model's applicable visual animation profile at Intent and author its declared motion families within Source, using the pinned Hero.Voxel authoring contract. Creation and maintenance SHALL share that same policy.

#### Scenario: New reusable equipment is requested
- **WHEN** a new weapon enters the canonical pipeline
- **THEN** its physical form resolves an applicable profile and Source authors all required meaningful takes without irrelevant motions

#### Scenario: Existing model receives animation-only upgrade
- **WHEN** an explicit catalog migration or a maintenance repair changes only animation authority
- **THEN** the same asset re-enters at its earliest affected stage, retaining current accepted upstream reference/construction/density evidence only when its owning validation confirms that evidence remains applicable and current

#### Scenario: Animation source is ready but not published
- **WHEN** current source/generation is valid but new visual proof is pending
- **THEN** the pipeline uses explicit candidate proof, completes applicable compatibility, accepts current evidence and seals before publication

#### Scenario: Visual review fails
- **WHEN** an authored variant fails profile choreography, physical-form or composition review
- **THEN** the same asset returns to Source or the earliest affected stage and is not sealed or published as complete
