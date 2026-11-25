# Spec Delta

## ADDED Requirements

### Requirement: Published avatars provide minimum V1 motion coverage
Every published reusable avatar SHALL author a V1 take for each reusable motion family while retaining the required baseline clips and side-reactive Attack clips.

#### Scenario: Avatar satisfies minimum V1 coverage
- **WHEN** a reusable avatar is validated for publication
- **THEN** it contains `Idle.V1`, `WalkForward.V1`, `Hit.V1`, `Death.V1`, `Attack.Left.V1`, and `Attack.Right.V1` in addition to its required baseline and side-reactive Attack clips

#### Scenario: Avatar is missing one required V1 take
- **WHEN** a reusable avatar omits any required V1 clip
- **THEN** publication fails with the missing semantic/side variant reported before generated proof is sealed

#### Scenario: Current single-part avatar publishes V1 takes
- **WHEN** an avatar still consists only of the authored `Body` part
- **THEN** its required V1 clips may animate only `Body` and publication does not require invented anatomical parts

### Requirement: Published equipment provides minimum V1 motion coverage
Every published reusable equipment model SHALL author `Idle.V1`, `WalkForward.V1`, `Attack.V1`, `Hit.V1`, and `Death.V1` in addition to its required baseline semantic clips.

#### Scenario: Equipment satisfies minimum V1 coverage
- **WHEN** a reusable equipment model is validated for publication
- **THEN** all five required V1 clips exist and inherit the loop policy of their corresponding semantic motions

#### Scenario: Equipment is missing one required V1 take
- **WHEN** a reusable equipment model omits any required V1 clip
- **THEN** publication fails before generated proof is sealed

#### Scenario: One-part equipment publishes V1 motion
- **WHEN** current one-part equipment authors its required V1 clips
- **THEN** the Mount-bearing part remains compatible with the existing grip-preservation rules and the V1 set does not create attachment-time correction

### Requirement: Required V1 takes are authored motion rather than aliases
A required V1 clip SHALL contain authored animation data that is observably different from the fallback clip it supplements. Publication SHALL reject a V1 clip whose compiled timing and animation tracks are identical to that fallback.

#### Scenario: V1 differs from its baseline
- **WHEN** `Death.V1` changes duration, keyed transforms, interpolation, track coverage, or another compiled animation property relative to `Death`
- **THEN** publication accepts the take as independently authored when all other animation rules pass

#### Scenario: Side-qualified Attack V1 differs from side-only Attack
- **WHEN** `Attack.Left.V1` is required for an avatar
- **THEN** publication compares it with `Attack.Left` rather than treating the generic `Attack` clip as its authored-difference authority

#### Scenario: V1 is an exact copy
- **WHEN** a required V1 clip has compiled timing and track data identical to its fallback clip
- **THEN** publication fails instead of counting the duplicate as multi-animation coverage

### Requirement: Generated state reports required V1 completeness
Generated catalog and health state SHALL expose whether each published reusable model satisfies the store's minimum V1 coverage, derived from authoritative compiled animation data.

#### Scenario: Published model has complete required variants
- **WHEN** catalog and health state are generated for a conforming model
- **THEN** the model reports complete required V1 coverage together with its exact semantic/side variant availability

#### Scenario: Generated state observes incomplete coverage
- **WHEN** a model is missing a required V1 clip
- **THEN** generated validation reports the missing coverage and the model is not considered publication-complete
