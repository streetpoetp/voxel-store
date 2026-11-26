# Spec Delta

## MODIFIED Requirements

### Requirement: Reusable models publish semantic animation clips
Every reusable avatar and equipment asset SHALL author the baseline Hero.Voxel semantic clips `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death`.

#### Scenario: Semantic animation set is complete
- **WHEN** source validation compiles a reusable avatar or equipment asset
- **THEN** all five required semantic clips exist with `Idle` and `WalkForward` looping and `Attack`, `Hit`, and `Death` non-looping

#### Scenario: Equipment owns unique attack motion
- **WHEN** two equipment assets share semantic clip name `Attack`
- **THEN** each asset may author different animation tracks/curves while publication treats both as satisfying the same semantic motion contract

#### Scenario: Required semantic clip is missing
- **WHEN** a reusable model omits one required semantic clip or declares incompatible loop semantics
- **THEN** publication fails before generated proof is sealed

#### Scenario: Body declares side-reactive hand attack
- **WHEN** a body publishes both left and right Hand slots and declares side-reactive `Attack` behavior
- **THEN** publication requires distinct `Attack.Left` and `Attack.Right` authored clips in addition to the baseline semantic set

#### Scenario: Current single-part body authors side reaction
- **WHEN** a current avatar has only the authored `Body` part
- **THEN** its side-specific clips may target only `Body`; publication does not assume Torso, Shoulder, Arm, Hand, or other anatomical parts exist

#### Scenario: Current single-part equipment owns visible equipped animation
- **WHEN** an equipment model has one authored part and its Mount is parented to that part
- **THEN** publication requires Mount position to coincide with that part's bind pivot and permits finite authored position, rotation, and scale tracks on the Mount-bearing part so semantic takes can produce distinct visual trajectories

#### Scenario: Equipment variants use different spatial trajectories
- **WHEN** baseline `Attack` and `Attack.V1` on one equipment model author different finite position and rotation curves
- **THEN** publication accepts them as independent model-local takes without requiring matching spatial curves, attachment-state metadata, or gameplay semantics
