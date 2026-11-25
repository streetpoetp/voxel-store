# Spec Delta

## ADDED Requirements

### Requirement: Consumer animation requests may select a model-local variant
A semantic animation request MAY include an optional model-local variant key for the root model and independently for each attached model. Variant selection SHALL remain explicit caller context and SHALL NOT be chosen randomly or inferred from gameplay state by voxel-store compatibility.

#### Scenario: Consumer selects a body Death take
- **WHEN** a consumer requests semantic `Death` with root variant `V2`
- **THEN** compatibility resolves the root model using V2-aware candidates while preserving semantic `Death`

#### Scenario: Body and equipment select different takes
- **WHEN** a consumer requests `Attack` with body variant `V2` and one attached equipment variant `V1`
- **THEN** compatibility allows each model instance to resolve its own requested take while all resolved clips remain on the same semantic phase

#### Scenario: Consumer supplies no variant
- **WHEN** a semantic animation request omits the variant for a model instance
- **THEN** compatibility follows the existing side/base resolution behavior for that instance without selecting an optional take on its behalf

### Requirement: Variant-aware clip resolution uses deterministic ordered fallback
For a model instance with semantic motion `M`, physical side `S`, and requested variant `V`, compatibility SHALL resolve candidates in the order `M.S.V`, `M.V`, `M.S`, `M`, omitting qualifiers that are absent from the request.

#### Scenario: Fully qualified take exists
- **WHEN** `Attack.Left.V2` exists and the request is motion `Attack`, side `Left`, variant `V2`
- **THEN** compatibility selects `Attack.Left.V2` before every less-specific candidate

#### Scenario: Side-qualified take is absent but generic take exists
- **WHEN** `Attack.Left.V2` is absent, `Attack.V2` exists, and the same request is evaluated
- **THEN** compatibility selects `Attack.V2` before falling back to `Attack.Left` or `Attack`

#### Scenario: Requested take is unavailable
- **WHEN** neither `Attack.Left.V2` nor `Attack.V2` exists but `Attack.Left` exists
- **THEN** compatibility selects `Attack.Left` and preserves the physical side response

#### Scenario: Only the baseline clip exists
- **WHEN** all qualified candidates are absent and the required base `Attack` exists
- **THEN** compatibility selects `Attack`

### Requirement: Variant fallback remains model-local across composed actors
A composed actor SHALL resolve the candidate chain independently against each model instance's own animation catalog. A variant identifier shared by two models SHALL NOT require either model to author equivalent choreography or the same qualified clips.

#### Scenario: Attachment lacks the root take
- **WHEN** the body resolves `Attack.Left.V2` but attached equipment has no V2 clip and does have `Attack.Left`
- **THEN** the equipment resolves `Attack.Left` while the body keeps `Attack.Left.V2`

#### Scenario: Different attachments expose different variant coverage
- **WHEN** two attached models receive the same requested variant but only one publishes that take
- **THEN** each attachment resolves its own ordered fallback independently and both remain sampled at the shared body-owned normalized phase

### Requirement: Variant selection remains presentation-only
Animation variant choice SHALL describe only which authored visual take is sampled and SHALL NOT encode damage, attack result, AI state, inventory rules, balance, progression, or other consuming-game semantics.

#### Scenario: Game maps combo state to a take
- **WHEN** a consumer maps its own combo step to `Attack` variant `V3`
- **THEN** compatibility receives only the requested semantic motion and variant context and does not acquire ownership of combo rules

#### Scenario: Game randomizes presentation externally
- **WHEN** a consumer chooses among available Idle variants using its own deterministic or nondeterministic policy
- **THEN** voxel-store compatibility resolves the explicitly supplied variant and does not perform the random selection itself
