# Spec Delta

## MODIFIED Requirements

### Requirement: Reusable models publish semantic animation clips
Every model SHALL declare a valid kind/form-specific animation profile from the single authoritative store policy. Required families and allowed optional families SHALL be defined by that profile; irrelevant or undeclared families SHALL be rejected. Each authored family SHALL contain a default and V1, V2, V3 with the applicable loop policy.

#### Scenario: Semantic animation set is complete
- **WHEN** a grounded avatar publishes Idle, WalkForward, Attack, Hit and Death with all four takes per family, or equipment publishes Idle, Attack and Drop with all four takes
- **THEN** the required profile coverage passes when each take is valid and distinct

#### Scenario: Equipment owns unique attack motion
- **WHEN** equipment of different physical forms publishes Attack
- **THEN** each model owns its own form-appropriate authored choreography

#### Scenario: Required semantic clip is missing
- **WHEN** a required family, default or V1/V2/V3 take is absent, the loop policy is invalid or the profile is unknown or incompatible with kind/form
- **THEN** publication fails with the specific missing or invalid coverage

#### Scenario: Body declares side-reactive hand attack
- **WHEN** an avatar chooses to author a side-qualified Attack family
- **THEN** that side family also contains its default and three distinct variants without substituting for generic Attack coverage

#### Scenario: Current single-part body authors side reaction
- **WHEN** a rigid avatar with only Body authors a side-qualified clip
- **THEN** it targets existing authored parts and does not require anatomical articulation

#### Scenario: Current single-part equipment owns visible equipped animation
- **WHEN** equipment has one part and its Mount is parented to that part
- **THEN** publication retains the bind-pivot rule and permits finite authored position, rotation and scale tracks on that part

#### Scenario: Equipment variants use different spatial trajectories
- **WHEN** Attack and Attack.V1 author different supported trajectories
- **THEN** publication permits independently authored takes without requiring attachment-state metadata or gameplay semantics

#### Scenario: Irrelevant motion is omitted
- **WHEN** equipment omits WalkForward, Hit or Death, or floating and winged avatars omit WalkForward in favor of Glide and Fly respectively
- **THEN** coverage passes without the irrelevant clips or aliases

#### Scenario: Optional family is authored
- **WHEN** an asset declares an allowed optional family appropriate to its physical form
- **THEN** publication requires the default and V1/V2/V3 for that family as well

### Requirement: Generated catalog projects attachment and motion coverage
Generated catalog/health output SHALL project attachment capability, anchor identity, side-aware slot addresses, counts by capability and side, compatible equipment-side policy, and resolved animation profile and structured semantic-motion coverage including base, side, variant, and side+variant availability from authoritative manifests and compiled source.

#### Scenario: Equipment is cataloged
- **WHEN** a conforming equipment asset publishes
- **THEN** generated catalog state can report its form, attachment capability, Mount anchor, semantic motions, and model-local variant coverage without becoming their authority

#### Scenario: Avatar is cataloged
- **WHEN** a conforming avatar publishes
- **THEN** generated catalog state reports each available equipment slot capability, all concrete instance names, the count for each capability, and structured semantic-motion coverage that distinguishes base clips, physical sides, variants, and side-qualified variants

#### Scenario: Qualified clip names are projected
- **WHEN** a model publishes `Attack`, `Attack.Left`, `Attack.V2`, and `Attack.Left.V2`
- **THEN** generated catalog state reports one semantic `Attack` motion with its base, Left side, V2 variant, and Left+V2 coverage instead of listing four unrelated semantic motions

### Requirement: Reusable semantic motions may publish model-local animation variants
Every authored generic or side-qualified family SHALL publish a default and distinct V1, V2, V3 takes. Additional takes MAY use canonical positive V<n> suffixes and SHALL remain within their owning family.

#### Scenario: Idle publishes multiple authored takes
- **WHEN** a model authors Idle and Idle.V1 through Idle.V3
- **THEN** publication projects one Idle family with three variants

#### Scenario: Death publishes a different variant count
- **WHEN** a model authors Death and V1 through V4 while other families have only V1 through V3
- **THEN** publication accepts the extra distinct take without requiring equal counts

#### Scenario: Variant identifier is malformed
- **WHEN** a suffix does not match canonical V<n> with n greater than zero
- **THEN** publication rejects the clip identity

### Requirement: Animation variants inherit semantic loop policy
Every authored take SHALL follow its profile's family loop policy. Locomotion and Idle loop, while Attack, Hit, Death and Drop do not. Optional families use their declared loop policy.

#### Scenario: Looping semantic variant is valid
- **WHEN** WalkForward.V2 is authored as looping for a grounded avatar
- **THEN** publication accepts that loop policy

#### Scenario: Terminal semantic variant has wrong loop policy
- **WHEN** Death.V2 or Drop.V2 is authored as looping
- **THEN** source validation fails

## ADDED Requirements

### Requirement: Authored takes are visually distinct within each family
Publication SHALL reject duplicate or no-op choreography, including duration-only differences. Every authored take SHALL pass current visual acceptance for its profile, form, supported transforms and coherent trajectory.

#### Scenario: Duration changes but normalized pose trajectory does not
- **WHEN** two takes differ only in duration or redundant key encoding and have equivalent sampled normalized transforms and reference influence
- **THEN** validation rejects duplicate choreography

#### Scenario: Variant changes meaningful trajectory
- **WHEN** a take differs observably from every other take in its family, including non-base variants
- **THEN** mechanical distinctness can pass but visual review is still required

#### Scenario: Loop or terminal pose is visually invalid
- **WHEN** a loop snaps at its seam or a terminal clip has unexplained distortion, unsupported transforms or an implausible resting pose
- **THEN** visual proof fails rather than counting the clip as accepted coverage

### Requirement: Generated state reports profile-driven animation coverage
Catalog and health outputs SHALL report resolved profile identity, declared families, exact clip availability, missing/invalid/duplicate takes and completeness derived from current authoritative inputs.

#### Scenario: Required variant is missing
- **WHEN** a model lacks Attack.V3
- **THEN** health identifies the exact missing take and fails publication

### Requirement: Animation evidence is current and reviewed before sealing
Publication SHALL require current per-asset reviewed animation evidence for all authored takes, bound to effective source, profile policy, provider and capture identity. Sealing SHALL NOT substitute for visual acceptance.

#### Scenario: Animation-only source changes
- **WHEN** any authored animation, applicable profile policy, effective animation input or required proof changes
- **THEN** stale animation evidence is rejected until current capture and visual acceptance are recorded

#### Scenario: All takes are captured but not reviewed
- **WHEN** acquisition is attested but visual acceptance is absent or failing
- **THEN** the asset cannot publish

## REMOVED Requirements

### Requirement: Published avatars provide minimum V1 motion coverage
**Reason**: Replaced by profile-driven four-take coverage and pairwise meaningful choreography validation.
**Migration**: Validate all declared families against the applicable profile and report current coverage and duplicate takes.

### Requirement: Published equipment provides minimum V1 motion coverage
**Reason**: Replaced by profile-driven four-take coverage and pairwise meaningful choreography validation.
**Migration**: Validate all declared families against the applicable profile and report current coverage and duplicate takes.

### Requirement: Required V1 takes are authored motion rather than aliases
**Reason**: Replaced by profile-driven four-take coverage and pairwise meaningful choreography validation.
**Migration**: Validate all declared families against the applicable profile and report current coverage and duplicate takes.

### Requirement: Generated state reports required V1 completeness
**Reason**: Replaced by profile-driven four-take coverage and pairwise meaningful choreography validation.
**Migration**: Validate all declared families against the applicable profile and report current coverage and duplicate takes.
