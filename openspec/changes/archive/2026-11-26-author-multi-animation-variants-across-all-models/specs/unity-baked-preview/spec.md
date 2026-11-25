# Spec Delta

## ADDED Requirements

### Requirement: Component Inspector discovers motion clips from the selected model
The Unity component Inspector SHALL derive selectable motion clips from the baked animation asset bound to the selected avatar model. The Inspector SHALL NOT maintain a separate hard-coded list of semantic motions or authored variants.

#### Scenario: Model publishes multiple Death takes
- **WHEN** the selected avatar's baked animation asset contains `Death`, `Death.V1`, and `Death.V2`
- **THEN** the Inspector offers those exact clip names as selectable motion clips

#### Scenario: Model publishes only one Death take
- **WHEN** the selected avatar's baked animation asset contains `Death` but no Death variants
- **THEN** the Inspector offers only the authored Death clip and does not invent `Death.V1` or another variant

#### Scenario: Two models publish different clip sets
- **WHEN** two avatars have different baked clip inventories
- **THEN** selecting each avatar yields that model's own exact motion choices rather than a shared global option list

### Requirement: Exact Inspector clip selection preserves semantic motion state
Selecting an exact authored clip in the Inspector SHALL map the clip back to its semantic motion, optional physical side, and optional model-local variant without changing the underlying animation vocabulary.

#### Scenario: Side-qualified variant is selected
- **WHEN** a developer selects `Attack.Left.V1`
- **THEN** preview state represents semantic `Attack`, physical side `Left`, and model-local variant `V1`

#### Scenario: Side-neutral variant is selected
- **WHEN** a developer selects `Death.V1`
- **THEN** preview state represents semantic `Death` and variant `V1` without inventing a side

#### Scenario: Baseline clip is selected
- **WHEN** a developer selects `Idle`
- **THEN** preview state represents semantic `Idle` with no optional variant selected

### Requirement: Runtime gallery exposes the current baked animation inventory
The generated runtime gallery SHALL bind each avatar to its own current baked animation asset so Inspector discovery and interactive playback observe the same authored clip inventory.

#### Scenario: Published source is rebaked
- **WHEN** a model gains a new required V1 clip and the runtime gallery is refreshed
- **THEN** the model's baked animation asset and Inspector choices include the new clip without an Inspector code change

#### Scenario: Baked animation asset is incomplete or stale
- **WHEN** the gallery cannot bind the current animation asset required by a discovered model
- **THEN** preview validation fails visibly instead of falling back to a manually maintained clip list

### Requirement: Preview verification covers store-wide required V1 availability
Preview verification SHALL mechanically confirm that every baked published avatar and equipment model exposes the minimum V1 clip set required by publication, while graphical capture remains the integration proof for actual composed playback.

#### Scenario: All baked models satisfy required coverage
- **WHEN** preview validation scans the baked published model set
- **THEN** every avatar and equipment animation asset reports the required exact V1 clip names for its model kind

#### Scenario: One baked model loses a required clip
- **WHEN** a baked model lacks a V1 clip required by publication
- **THEN** preview validation fails and identifies the model and missing clip before the change is accepted

#### Scenario: Graphical integration proof runs
- **WHEN** provider-attested Game View capture executes after store-wide coverage passes
- **THEN** it proves representative composed variant playback, socket/Mount lock, and shared-phase behavior without replacing the per-model coverage gate
