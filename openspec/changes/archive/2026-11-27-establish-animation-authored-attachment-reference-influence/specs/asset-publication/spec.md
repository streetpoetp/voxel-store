# Spec Delta

## ADDED Requirements

### Requirement: Published equipment animation may author attachment-reference influence
An equipment animation clip that authors attachment-reference influence SHALL use finite normalized values in `[0,1]` with supported animation interpolation. A clip that omits the channel SHALL behave as influence `1`. The channel SHALL remain presentation-only and SHALL NOT encode gameplay state or attachment topology.

#### Scenario: Clip omits reference influence
- **WHEN** a reusable equipment clip contains no reference-influence keys
- **THEN** publication accepts the clip under the legacy-compatible default influence of `1`

#### Scenario: Attack releases and returns
- **WHEN** an equipment Attack take authors reference influence that begins at `1`, reaches `0`, and later returns to `1`
- **THEN** publication accepts the channel when all values, times, interpolation, and ordinary animation data are valid

#### Scenario: Rotation-only independent take is authored
- **WHEN** an equipment take authors reference influence below `1` without a position channel
- **THEN** publication accepts the take without requiring translation as proof of animation-owned visual motion

#### Scenario: Translated socket-relative take is authored
- **WHEN** an equipment take authors finite position keys while its reference influence remains `1`
- **THEN** publication accepts the take without classifying translation itself as release from the attachment reference

#### Scenario: Reference influence is outside its domain
- **WHEN** any reference-influence key is NaN, infinite, below `0`, or above `1`
- **THEN** source validation fails before generated animation proof can be sealed

#### Scenario: Reference influence attempts gameplay semantics
- **WHEN** an authored contract attempts to use reference influence as damage, target-selection, inventory, AI, combo, or attack-result state
- **THEN** publication rejects that ownership because the channel describes only visual reference contribution

### Requirement: Compatibility evidence must be current for publication
Repository-tracked compatibility evidence SHALL match the current compatibility convention and current authoritative inputs. Store verification SHALL fail when required compatibility evidence is missing, stale, or encoded under an obsolete convention even if an in-memory compatibility recomputation passes.

#### Scenario: Current evidence matches current compatibility contract
- **WHEN** tracked compatibility evidence was generated from the current avatar/equipment sources and current provider/tool contract
- **THEN** verification accepts that evidence when its recorded compatibility samples also pass

#### Scenario: Evidence uses the retired socket-lock convention
- **WHEN** tracked compatibility evidence still declares an obsolete convention after the repository has adopted attachment-reference influence
- **THEN** verification reports stale compatibility proof and the affected published assets are not healthy

#### Scenario: Authoritative animation input changes
- **WHEN** an avatar or equipment source that participates in compatibility proof changes after evidence was generated
- **THEN** verification rejects the stale evidence until compatibility proof is regenerated under the current convention

#### Scenario: In-memory compatibility passes but tracked proof is stale
- **WHEN** current compatibility evaluation passes but the repository evidence does not match the current contract or input identity
- **THEN** the store remains unhealthy rather than reporting publication success from recomputation alone
