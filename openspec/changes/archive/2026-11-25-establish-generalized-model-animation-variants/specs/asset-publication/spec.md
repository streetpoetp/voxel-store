# Spec Delta

## ADDED Requirements

### Requirement: Reusable semantic motions may publish model-local animation variants
A reusable model MAY author optional take variants for any required semantic motion using the canonical suffix `V<n>`, where `n` is a positive integer. Variant clips SHALL remain members of their base semantic motion rather than becoming new semantic motions.

#### Scenario: Idle publishes multiple authored takes
- **WHEN** a model authors `Idle.V1`, `Idle.V2`, and the required base `Idle`
- **THEN** publication treats all three clips as semantic `Idle` coverage with two optional model-local variants

#### Scenario: Death publishes a different variant count
- **WHEN** the same model authors `Death.V1`, `Death.V2`, `Death.V3`, and the required base `Death`
- **THEN** publication accepts the Death variants without requiring Idle, WalkForward, Attack, or Hit to publish matching variant counts

#### Scenario: Variant identifier is malformed
- **WHEN** a clip attempts to use a variant suffix outside the canonical `V<n>` form
- **THEN** publication rejects it as invalid generalized variant identity instead of projecting an ambiguous variant

### Requirement: Animation variants inherit semantic loop policy
Every optional variant SHALL use the loop policy of its base semantic motion: `Idle` and `WalkForward` variants loop, while `Attack`, `Hit`, and `Death` variants do not loop.

#### Scenario: Looping semantic variant is valid
- **WHEN** `WalkForward.V2` is authored as looping
- **THEN** publication accepts its loop policy when the rest of the clip is valid

#### Scenario: Terminal semantic variant has wrong loop policy
- **WHEN** `Death.V2` is authored as looping
- **THEN** publication fails source validation before generated proof is sealed

### Requirement: Physical side and authored variant are independent animation qualifiers
A reusable model MAY combine supported side context with a model-local variant using `<Motion>.<Side>.V<n>`. Side qualification SHALL retain its existing physical meaning, while the variant suffix SHALL identify only an authored take.

#### Scenario: Side-specific attack take is authored
- **WHEN** an avatar authors `Attack.Left.V2` and `Attack.Right.V2`
- **THEN** publication records V2 takes for the existing Left and Right physical contexts without creating new semantic motions or asset identities

#### Scenario: Generic take is authored without side qualification
- **WHEN** a model authors `Attack.V2`
- **THEN** publication records V2 as a side-neutral authored take that may be resolved independently of a side-specific clip

### Requirement: Animation variant identity is local to one model
Variant identifiers SHALL be interpreted only within the animation catalog of the model that owns them and SHALL NOT imply shared choreography, personality, combat meaning, or aligned numbering across different models.

#### Scenario: Body and equipment both publish V2
- **WHEN** an avatar and an attached equipment model each publish an `Attack.V2` clip
- **THEN** publication treats the two V2 identifiers as independent authored takes owned by their respective models

#### Scenario: Models publish different variant sets
- **WHEN** one model publishes `Idle.V1` through `Idle.V3` and another publishes only `Idle.V1`
- **THEN** both models may publish without requiring global variant-set alignment

## MODIFIED Requirements

### Requirement: Generated catalog projects attachment and motion coverage
Generated catalog/health output SHALL project attachment capability, anchor identity, side-aware slot addresses, counts by capability and side, compatible equipment-side policy, and structured semantic-motion coverage including base, side, variant, and side+variant availability from authoritative manifests and compiled source.

#### Scenario: Equipment is cataloged
- **WHEN** a conforming equipment asset publishes
- **THEN** generated catalog state can report its form, attachment capability, Mount anchor, semantic motions, and model-local variant coverage without becoming their authority

#### Scenario: Avatar is cataloged
- **WHEN** a conforming avatar publishes
- **THEN** generated catalog state reports each available equipment slot capability, all concrete instance names, the count for each capability, and structured semantic-motion coverage that distinguishes base clips, physical sides, variants, and side-qualified variants

#### Scenario: Qualified clip names are projected
- **WHEN** a model publishes `Attack`, `Attack.Left`, `Attack.V2`, and `Attack.Left.V2`
- **THEN** generated catalog state reports one semantic `Attack` motion with its base, Left side, V2 variant, and Left+V2 coverage instead of listing four unrelated semantic motions
