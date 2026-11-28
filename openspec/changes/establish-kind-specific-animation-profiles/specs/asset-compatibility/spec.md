# Spec Delta

## MODIFIED Requirements

### Requirement: Consumer coordination uses ordinary per-model animation commands
Coordination across body and attachments SHALL remain consumer-owned and use ordinary per-model animation operations. The shared actor SHALL NOT introduce semantic coordination commands for Death, Attack, Drop or any other gameplay/presentation events.

#### Scenario: Consumer presents unit death
- **WHEN** a consumer wants a body to die and its equipment to drop
- **THEN** it separately commands the body's Death and the equipment's Drop with ordinary model animation operations

#### Scenario: Weapon lacks a Death clip
- **WHEN** equipment omits the irrelevant Death family
- **THEN** its absence is valid and the consumer chooses Drop or another applicable operation without requesting a synthetic Death fallback

#### Scenario: No hard-coded death API exists
- **WHEN** the shared actor animation surface is inspected
- **THEN** it exposes no required Die, PlayDeath, synchronization-group, weapon-drives-body or equivalent event-specific operation

## ADDED Requirements

### Requirement: Compatibility uses independent authored motion pairs
Compatibility SHALL evaluate current compatible physical pairs and slots with independently selected exact body and equipment clips and phases. Every authored take SHALL be exercised at meaningful ordered checkpoints without requiring shared motion names or a Cartesian product of all takes.

#### Scenario: Body and equipment use different motion families
- **WHEN** the body plays WalkForward, Glide, Fly, Hit or Death and equipment lacks that family
- **THEN** compatibility uses an applicable independently selected equipment clip such as Idle or Drop, not a fabricated matching family

#### Scenario: Each authored take has compatibility coverage
- **WHEN** compatibility evaluates a physical pair and slot
- **THEN** each body take is covered with held equipment, each equipment take with a compatible body state, and explicit mixed scenarios include Death/Drop, independent phases and release/return while the body moves

#### Scenario: Proof requests an absent exact clip
- **WHEN** an explicit compatibility case names a clip absent from the selected model
- **THEN** validation fails with the exact model and clip rather than substituting a fallback

#### Scenario: Compatibility evidence is stale
- **WHEN** a profile, source, applicable effective animation, provider or compatibility convention changes
- **THEN** evidence records exact clip names, ordered independent phases, current input identity and results and obsolete shared-motion evidence cannot satisfy publication

### Requirement: Drop is terminal equipment presentation
Drop SHALL author a finite relative release trajectory using the existing reference-influence channel, ending with a plausible resting pose independent of subsequent socket movement. Logical binding, terrain contact, visibility and loot remain consumer-owned.

#### Scenario: Equipment drops as the body falls
- **WHEN** a consumer independently selects Death and Drop on their respective tracks
- **THEN** the equipment can release its reference and settle without inheriting subsequent body movement or automatically changing binding topology

#### Scenario: A released attack returns
- **WHEN** equipment performs a nonterminal Attack with reference release
- **THEN** its authored trajectory continuously rejoins the moving reference without a terminal snap
