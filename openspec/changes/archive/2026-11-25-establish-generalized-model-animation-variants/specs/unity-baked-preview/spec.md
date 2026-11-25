# Spec Delta

## ADDED Requirements

### Requirement: Preview motion state supports model-local animation variants
The runtime preview SHALL allow a semantic motion request to carry an optional model-local variant selection for the avatar body and independently for attached equipment, while preserving existing side context and shared-phase playback.

#### Scenario: Developer previews an Idle variant
- **WHEN** preview state selects semantic `Idle` and body variant `V2`
- **THEN** the runtime gallery plays the resolved Idle V2 take using the normal semantic playback path

#### Scenario: Developer previews a Death variant
- **WHEN** preview state selects semantic `Death` and body variant `V3`
- **THEN** the runtime gallery plays the resolved non-looping Death V3 take and reaches its terminal pose through the normal authored playback path

#### Scenario: Body and equipment use different variants
- **WHEN** preview state selects body variant `V2` and a composed equipment instance variant `V1` for the same semantic motion
- **THEN** the body and equipment resolve their variants independently and sample the resulting clips at the same normalized phase

#### Scenario: Variant selection is cleared
- **WHEN** a developer clears the variant selection for a model instance
- **THEN** preview playback returns to the existing side/base candidate behavior without choosing an optional variant automatically

### Requirement: Preview candidate construction preserves side and variant precedence
Preview playback SHALL construct each model instance's ordered clip candidates so a requested side+variant take is preferred before generic variant, side-only, and base semantic clips.

#### Scenario: Left attack V2 is previewed
- **WHEN** preview requests `Attack`, driver side `Left`, and variant `V2`
- **THEN** the body candidate order is `Attack.Left.V2`, `Attack.V2`, `Attack.Left`, `Attack`

#### Scenario: Walk variant has no side context
- **WHEN** preview requests `WalkForward` variant `V2` without a physical side qualifier
- **THEN** preview resolves `WalkForward.V2` before `WalkForward`

### Requirement: Interactive preview can select variants without becoming verification authority
Interactive preview controls SHALL expose available or explicitly entered model-local variant selection for semantic motions, while registered command-driven Game View capture remains the canonical verification path.

#### Scenario: Developer switches variant during Play Mode
- **WHEN** a developer changes the selected variant while a generated gallery target is running
- **THEN** the target restarts or resamples through the normal authored motion path using the new variant selection

#### Scenario: Interactive preview is not used
- **WHEN** automated animation verification runs without opening the preview window or editing an Inspector
- **THEN** variant-aware capture remains fully executable from the repository capture command

### Requirement: Graphical capture proves generalized animation variant resolution
The canonical animation capture flow SHALL support deterministic checkpoints for generalized variants of `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death`, including side-qualified variants where applicable.

#### Scenario: Looping variants are captured
- **WHEN** the capture scenario declares checkpoints for authored `Idle` and `WalkForward` variants
- **THEN** provider-attested Game View pixels show the selected variant clips at deterministic normalized phases without changing their looping semantic policy

#### Scenario: Non-looping variants are captured
- **WHEN** the capture scenario declares checkpoints for authored `Attack`, `Hit`, and `Death` variants
- **THEN** provider-attested Game View pixels show the requested takes at deterministic normalized phases and preserve non-looping terminal behavior where applicable

#### Scenario: Side-qualified variant is captured
- **WHEN** capture requests `Attack`, side `Left`, variant `V2` for a model that publishes `Attack.Left.V2`
- **THEN** the attested capture samples that fully qualified take while attachment remains locked to the evaluated side-aware socket
