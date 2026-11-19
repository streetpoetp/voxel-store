# Spec Delta

## ADDED Requirements

### Requirement: Generated Product Targets use image-model authoring
A newly generated Product Target SHALL be created by an image-generation model from current target style authority plus asset intent. Procedural raster/vector drawing or scripted geometry rendering SHALL NOT become generated Product Target authority.

#### Scenario: Agent has style authority and form intent but no supplied target
- **WHEN** target-driven authoring must create a Product Target from style references and asset form intent
- **THEN** the agent uses an image-generation model, visually evaluates the result, and accepts only an image that reaches the approved visual language

#### Scenario: Procedural drawing can create a recognizable asset
- **WHEN** a local script, SVG renderer, pixel renderer, or deterministic drawing tool can draw the requested form
- **THEN** that output may support construction or diagnostics but cannot be accepted as a newly generated Product Target

#### Scenario: Image-generation capability is unavailable
- **WHEN** no usable image-generation capability can create the Product Target at the required visual level
- **THEN** the current asset remains blocked at Product Target authoring rather than falling back to a lower-fidelity procedural target

### Requirement: Generated Product Target delivery preserves exact accepted bytes
The exact visually accepted image-model output SHALL become the repository-owned Product Target. If the generated output cannot be transferred into the project without redrawing or substitution, target authoring SHALL remain blocked.

#### Scenario: Generator can write or transfer the accepted image into the project
- **WHEN** the accepted image-model output is available as exact bytes through a project-native write, public result download, or trusted exact-file import
- **THEN** those exact bytes are stored as the declared Product Target and bound by provenance

#### Scenario: Generator output is visible but not transferable
- **WHEN** the agent can inspect a generated image but its exact bytes cannot cross the host/project boundary
- **THEN** the workflow reports a Product Target delivery block and does not recreate the image with a procedural renderer

### Requirement: Generated Product Target provenance identifies the image-model request
New generated Product Targets SHALL use current provenance that identifies image-model authoring and binds a repository-owned generation request plus provider/model identity. Existing accepted generated provenance v1 MAY remain valid unchanged.

#### Scenario: New generated Product Target has complete image-model provenance
- **WHEN** a newly generated target is accepted
- **THEN** provenance records image-model generator kind, nonempty provider/model identity, the accepted target hash, current target style hashes, and a hash-bound repository request artifact

#### Scenario: Generated provenance claims a procedural generator
- **WHEN** current generated-target provenance identifies a procedural renderer instead of an image-generation model
- **THEN** generated Product Target provenance validation fails

#### Scenario: Bound generation request changes
- **WHEN** the repository-owned generation request bytes change after provenance is recorded
- **THEN** the generated Product Target provenance becomes stale until the target generation/acceptance evidence is intentionally refreshed

#### Scenario: Existing generated provenance v1 remains unchanged
- **WHEN** a previously accepted published target still uses valid generated provenance v1 and is not being intentionally regenerated
- **THEN** backward-compatible validation may continue to accept it without inventing image-model history
