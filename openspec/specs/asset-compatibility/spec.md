# asset-compatibility Specification

## Purpose

Defines reusable physical compatibility between independently authored voxel avatars and equipment so shared assets can compose predictably without baking every avatar/equipment combination into a separate asset.

## Requirements

### Requirement: Compatible avatars are single rigid Body assets
An avatar participating in reusable equipment composition SHALL expose exactly one authored rigid part named `Body`; independently animated head, arm, hand, foot, or equivalent attachment parts SHALL NOT be required for compatibility.

#### Scenario: Avatar uses the rigid-body contract
- **WHEN** an avatar is evaluated for reusable equipment composition
- **THEN** its authored geometry resolves to exactly one `Body` part whose canonical local bounds define the placement space

#### Scenario: Avatar contains articulated compatibility parts
- **WHEN** an avatar relies on additional articulated parts to satisfy reusable equipment composition
- **THEN** compatibility validation fails for the rigid-body contract instead of treating those parts as attachment authority

### Requirement: Reusable models share one semantic canonical Unity model frame
All published reusable avatar and equipment models SHALL resolve through Hero.Voxel into one canonical Unity local frame: Right `+X`, Up `+Y`, Forward `-Z`. Under the provider source-to-Unity mapping, semantic Forward is authored toward source `-Y`. Forward SHALL represent semantic facing/action rather than a coordinate-conversion label or geometrically inferred axis.

#### Scenario: Avatar and equipment are composed
- **WHEN** a conforming avatar socket and conforming equipment Mount are composed
- **THEN** both models expose the same canonical local Right, Up, and Forward axes and attachment requires no pair-specific corrective rotation

#### Scenario: Equipment semantic front is audited
- **WHEN** equipment is normalized for publication
- **THEN** its semantic Forward is determined from intended game use and authoring evidence per asset, not from its longest dimension, an arbitrary mesh axis, or a blanket transform applied to the equipment catalog

#### Scenario: Projectile equipment is authored
- **WHEN** a bow, crossbow, rifle, or equivalent projectile model has a defined target/launch direction
- **THEN** that launch direction resolves to canonical Unity `-Z` even when doing so makes the canonical front view different from the model's broadest side view

#### Scenario: Directional melee equipment is authored
- **WHEN** a dagger, sword, spear, axe, hammer, pick, scythe, or equivalent melee model has a defined primary thrust/cut/strike direction
- **THEN** that functional action direction resolves to canonical Unity `-Z` even when its neutral broad-side presentation would otherwise suggest a different front

#### Scenario: Equipment is authored with opposite forward
- **WHEN** an equipment source would bake with a semantic forward direction different from canonical Unity `-Z`
- **THEN** the equipment source geometry/pivot SHALL be corrected before publication rather than storing a preview, avatar-specific, equipment-specific, or runtime corrective rotation

#### Scenario: Provider axis conversion is applied
- **WHEN** Hero.Voxel maps source coordinates into Unity coordinates
- **THEN** the store treats that mapping only as coordinate conversion and independently requires the resulting published model to satisfy the semantic canonical Unity model frame

#### Scenario: Canonical front proof is rendered
- **WHEN** Hero.Voxel renders `front.png` for a conforming published model
- **THEN** the camera is on the Unity `-Z` side looking toward `+Z`, so the image means the observer is standing in front of the model looking back at it and therefore proves semantic Forward points toward `-Z`

### Requirement: Avatar equipment slots are model-owned physical frames
Reusable equipment attachment SHALL resolve from Hero.Voxel sockets authored on the avatar model rather than from normalized Body-bounds ratios.

#### Scenario: Capability resolves to a concrete socket
- **WHEN** equipment requires capability `Hand` and an avatar exposes one or more `Equip.Hand.<Side>.<Instance>` sockets compatible with the equipment side policy
- **THEN** compatibility can select one concrete socket instance while preserving the equipment's capability-level identity

#### Scenario: Avatar lacks the required capability
- **WHEN** no avatar socket matches the equipment's declared logical capability
- **THEN** compatibility fails with the missing capability/socket evidence

### Requirement: Equipment attachment uses its authored Mount frame
Reusable equipment SHALL use its authored bind-pose Mount frame to establish the attachment reference for the selected evaluated avatar equipment socket. Animated visual motion MAY move the evaluated Mount away from that reference without invalidating logical attachment.

#### Scenario: Static attachment is solved
- **WHEN** body and equipment are evaluated at neutral pose
- **THEN** the equipment attachment reference is solved as avatar-socket matrix multiplied by the inverse equipment bind-Mount matrix

#### Scenario: Equipment pivot differs from its grip
- **WHEN** the equipment root/pivot is not located at the authored Mount socket
- **THEN** compatibility uses the bind-Mount frame and does not require a corrective Unity offset

#### Scenario: Pair-specific correction is required
- **WHEN** acceptable composition requires an extra transform unique to one avatar/equipment pair
- **THEN** compatibility fails instead of persisting the correction

#### Scenario: Authored animation moves the evaluated Mount
- **WHEN** an equipment clip changes the evaluated Mount-bearing part transform after a valid bind reference has been established
- **THEN** logical attachment remains valid and compatibility does not require the evaluated animated Mount to coincide with the avatar socket

### Requirement: Multiple equipment instances attach independently by slot
A reusable avatar SHALL support multiple equipment instances, including repeated instances of the same equipment asset or form, by resolving each instance against a distinct compatible physical socket without making another equipment item its attachment parent.

#### Scenario: Two equipment instances are composed
- **WHEN** a consumer selects two compatible avatar socket instances
- **THEN** each equipment root is solved independently from its selected socket and own Mount frame

#### Scenario: Same equipment asset is composed multiple times
- **WHEN** one body exposes three `Back` socket instances and a consumer composes three instances of the same bow asset requiring `Back`
- **THEN** compatibility accepts all three physical compositions when each bow is bound to a distinct compatible side-aware Back socket

#### Scenario: Consumer applies occupancy rules
- **WHEN** a game decides that two items cannot simultaneously occupy particular slots
- **THEN** that decision remains outside voxel-store physical compatibility

#### Scenario: Capability has finite physical capacity
- **WHEN** a body exposes two `Back` socket instances and a physical compatibility proof requests three simultaneously attached `Back` items
- **THEN** the third composition has no free physical slot instance and the proof reports insufficient body slot capacity without changing the equipment asset contract

#### Scenario: Left and right slots are physically distinct
- **WHEN** a body exposes `Equip.Hand.Left.0` and `Equip.Hand.Right.0`
- **THEN** compatibility preserves their distinct side semantics and never treats them as interchangeable anonymous `Hand.0`/`Hand.1` instances

### Requirement: Animated attachment uses current body socket and equipment bind Mount
Animated composition SHALL expose the current body socket combined with the equipment bind-Mount inverse as attachment reference data while body and equipment animation remain independently sampled. Equipment visual animation MAY depart from that reference; following the reference is an explicit presentation choice rather than a continuous compatibility invariant.

#### Scenario: Body socket moves during animation
- **WHEN** the Body animation changes pose and therefore moves an equipment socket
- **THEN** the current attachment reference changes with that socket without requiring compatibility to overwrite the equipment's independently authored visual animation state

#### Scenario: Single-part weapon rotates during animation
- **WHEN** an equipment clip independently rotates its Mount-bearing part around a pivot colocated with Mount
- **THEN** the weapon visibly rotates from its model-local animation while the bind-Mount reference remains available for socket-relative presentation

#### Scenario: Runtime tries to solve against evaluated animated Mount
- **WHEN** composition would use the evaluated animated Mount matrix as the inverse root-solving input for a one-part weapon
- **THEN** compatibility rejects that method because it would cancel or feed back the weapon's authored part transform instead of using the bind-Mount reference

#### Scenario: Single-part weapon translates its Mount-bearing part
- **WHEN** an equipment animation contains a finite position track on the current one-part Mount-bearing part
- **THEN** compatibility permits the authored visual translation and does not fail solely because the evaluated Mount moves away from the body socket

#### Scenario: Visual motion departs from the attachment reference
- **WHEN** equipment animation produces a visual pose whose evaluated Mount differs from the current attachment reference
- **THEN** the equipment remains the same logically attached instance and no detach, reattach, or pair-specific correction is required

### Requirement: Socket and animation compatibility remain density-independent
Different positive avatar/equipment authoring densities SHALL remain compatible when baked world scale, socket frames, Mount alignment, and semantic animation sampling satisfy the shared contracts.

#### Scenario: High-density equipment attaches to lower-density avatar
- **WHEN** equipment uses a different positive VPU from its avatar
- **THEN** compatibility evaluates baked world-space socket/animation frames and does not reject the pair solely for density difference

### Requirement: Socket and animation contracts remain gameplay-neutral
Attachment capabilities and model-local visual motions SHALL describe reusable physical/presentation behavior only and SHALL NOT encode damage, attack result, inventory occupancy, AI, balance, progression, consuming-game combat class, or cross-model reaction rules.

#### Scenario: Game gives a weapon gameplay meaning
- **WHEN** a consumer interprets an equipment asset as a primary weapon, secondary weapon, tool, or cosmetic
- **THEN** that meaning does not alter its store attachment capability, Mount frame, or model-local animation contract

#### Scenario: Consumer chooses active left or right equipment
- **WHEN** a game chooses one left/right equipment instance as active and wants a body reaction to accompany that weapon action
- **THEN** the game explicitly commands the body animation rather than relying on attachment side, weapon state, or shared actor inference

### Requirement: Consumer animation requests may select a model-local variant
An animation request SHALL target one model instance and MAY include that model's optional side/variant candidates. Variant or side selection for one model SHALL NOT imply any animation request for the body or another attachment.

#### Scenario: Consumer selects a body Death take
- **WHEN** a consumer requests semantic `Death` with body variant `V2`
- **THEN** only the body resolves its V2-aware Death candidates

#### Scenario: Body and equipment select different takes
- **WHEN** the consumer requests body `Idle.V2` and weapon `Attack.V1`
- **THEN** each model independently resolves its own candidate chain and playback state

#### Scenario: Consumer supplies no variant
- **WHEN** a model-local request omits a variant
- **THEN** resolution follows the side/base candidate behavior for that model without selecting an optional take on its behalf

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
A composed actor SHALL resolve each model instance's candidate chain independently against that model's own animation catalog. Shared variant identifiers SHALL NOT imply shared choreography, timing, phase, semantic request, or playback state across models.

#### Scenario: Attachment lacks the root take
- **WHEN** the body independently resolves `Attack.Left.V2` while an attachment is commanded with `Idle`
- **THEN** the attachment remains on its own Idle candidate chain and does not inherit the body's Attack take

#### Scenario: Different attachments expose different variant coverage
- **WHEN** two attachments are independently requested to play the same semantic motion and variant but only one publishes that take
- **THEN** each attachment resolves its own ordered fallback independently and advances on its own playback clock

### Requirement: Variant selection remains presentation-only
Animation variant choice SHALL describe only which authored visual take is sampled and SHALL NOT encode damage, attack result, AI state, inventory rules, balance, progression, or other consuming-game semantics.

#### Scenario: Game maps combo state to a take
- **WHEN** a consumer maps its own combo step to `Attack` variant `V3`
- **THEN** compatibility receives only the requested semantic motion and variant context and does not acquire ownership of combo rules

#### Scenario: Game randomizes presentation externally
- **WHEN** a consumer chooses among available Idle variants using its own deterministic or nondeterministic policy
- **THEN** voxel-store compatibility resolves the explicitly supplied variant and does not perform the random selection itself

### Requirement: Each composed model instance owns independent animation playback
The body and every attached model SHALL own independent animation selection and playback state. No model SHALL inherit another model's clip, semantic motion, phase, duration, loop state, speed, or playing/stopped state.

#### Scenario: Weapon attacks while body idles
- **WHEN** the consumer plays `Idle` on the body and `Attack` on one attached weapon
- **THEN** the body continues its own Idle playback while that weapon advances its own Attack playback

#### Scenario: Two weapons use different playback state
- **WHEN** two attached weapons are commanded with different clips, phases, speeds, or loop settings
- **THEN** each weapon advances and samples only its own playback state

#### Scenario: Weapon command does not create body reaction
- **WHEN** the consumer starts, stops, samples, or changes a weapon animation
- **THEN** body playback remains unchanged unless the consumer separately commands the body

### Requirement: Consumer coordination uses ordinary per-model animation commands
Coordination across body and attachments SHALL remain consumer-owned and SHALL use the same ordinary per-model animation operations as uncoordinated playback. The shared actor contract SHALL NOT introduce semantic coordination commands for Death, Attack, or other gameplay/presentation events.

#### Scenario: Consumer presents unit death
- **WHEN** a consumer wants the body and weapon to present Death together
- **THEN** it separately commands `Death` on the body and `Death` on the weapon using ordinary model animation operations

#### Scenario: Weapon lacks a Death clip
- **WHEN** a consumer handles unit death and one attachment has no desired Death take
- **THEN** the consumer decides whether to stop, freeze, reset, detach, or otherwise command that attachment without a shared actor fallback

#### Scenario: No hard-coded death API exists
- **WHEN** the shared actor animation surface is inspected
- **THEN** it exposes no required `Die`, `PlayDeath`, synchronization-group, weapon-drives-body, or equivalent event-specific coordination operation

### Requirement: Attachment topology changes preserve unrelated playback
Adding, removing, rebinding, or replacing one attachment SHALL NOT reset or restart body animation or playback owned by unaffected attachments.

#### Scenario: Weapon is attached during body playback
- **WHEN** a new weapon is attached while the body animation is already playing
- **THEN** body clip, phase, elapsed time, loop state, speed, and playing state continue unchanged

#### Scenario: One weapon is removed
- **WHEN** one attachment is removed while the body and another weapon are playing
- **THEN** only the removed attachment's playback is discarded and all unaffected model tracks continue

#### Scenario: Attachment is rebound
- **WHEN** an existing attachment is rebound to another compatible body socket
- **THEN** its physical placement target changes without implicitly restarting body or unrelated attachment playback

#### Scenario: Attachment model is replaced
- **WHEN** a binding is replaced with a different model whose animation catalog may differ
- **THEN** the replacement receives model-local playback state chosen for that model while body and unrelated attachment playback remain unchanged
