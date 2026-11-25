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

### Requirement: Reusable models share one canonical Unity model frame
All published reusable avatar and equipment models SHALL resolve through Hero.Voxel into the same canonical Unity local orientation: Right `+X`, Up `+Y`, Forward `-Z`. Model category SHALL NOT introduce another forward axis or require attachment-time orientation compensation.

#### Scenario: Avatar and equipment are composed
- **WHEN** a conforming avatar and conforming equipment asset are baked and composed at a Body-ratio position
- **THEN** both models expose the same canonical local Right, Up, and Forward axes and the equipment placement rotation remains identity

#### Scenario: Equipment source faces another direction
- **WHEN** an equipment source would bake with a forward direction different from canonical Unity `-Z`
- **THEN** the owning voxel source and pivot are corrected before publication rather than persisting a preview, avatar-specific, equipment-specific, or runtime corrective rotation

#### Scenario: Provider coordinate conversion is applied
- **WHEN** Hero.Voxel maps source coordinates into Unity coordinates
- **THEN** coordinate conversion and authored facing remain separate concerns and the resulting published model still satisfies the global canonical Unity model frame

### Requirement: Reusable models share one semantic canonical Unity model frame
All published reusable avatar and equipment models SHALL resolve through Hero.Voxel into one canonical Unity local frame: Right `+X`, Up `+Y`, Forward `-Z`. Under the provider source-to-Unity mapping, semantic Forward is authored toward source `-Y`. Forward SHALL represent semantic facing/action rather than a coordinate-conversion label or geometrically inferred axis.

#### Scenario: Avatar and equipment are composed
- **WHEN** a conforming avatar and conforming equipment asset are baked and composed at a Body-ratio position
- **THEN** both models expose the same canonical local Right, Up, and Forward axes and the equipment placement rotation remains identity

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
Reusable equipment SHALL align its evaluated Mount socket to the selected evaluated avatar equipment socket.

#### Scenario: Static attachment is solved
- **WHEN** body and equipment are evaluated at neutral pose
- **THEN** equipment root is solved as avatar-socket matrix multiplied by the inverse equipment-Mount matrix

#### Scenario: Equipment pivot differs from its grip
- **WHEN** the equipment root/pivot is not located at the authored Mount socket
- **THEN** compatibility uses the Mount frame and does not require a corrective Unity offset

#### Scenario: Pair-specific correction is required
- **WHEN** acceptable composition requires an extra transform unique to one avatar/equipment pair
- **THEN** compatibility fails instead of persisting the correction

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

### Requirement: Driver slot side selects authored motion variant
A semantic equipped action SHALL carry one driver binding whose physical slot side selects the preferred side-specific body/equipment clip variant before falling back to the base semantic clip.

#### Scenario: Left-hand attack drives body response
- **WHEN** semantic `Attack` is driven by equipment bound to `Equip.Hand.Left.0`
- **THEN** body playback resolves `Attack.Left` before `Attack`, and the driver equipment resolves `Attack.Left` before `Attack`

#### Scenario: Right-hand attack drives body response
- **WHEN** semantic `Attack` is driven by equipment bound to `Equip.Hand.Right.0`
- **THEN** body playback resolves `Attack.Right` before `Attack`, producing a body response distinct from the left-hand variant when both are authored

#### Scenario: Side-specific equipment clip is absent
- **WHEN** a side-reusable equipment asset does not author a side-specific variant for the selected driver side
- **THEN** equipment playback uses its base semantic clip while the body may still use its side-specific body variant

### Requirement: Authored body and equipment animation share one semantic phase
Equipped presentation SHALL sample the selected body semantic clip and every attached equipment clip at the same normalized phase.

#### Scenario: Attack is sampled
- **WHEN** equipped presentation evaluates `Attack` at normalized phase `p`
- **THEN** the body `Attack` clip and each equipment `Attack` clip are sampled at exactly `p`

#### Scenario: Authored clip durations differ
- **WHEN** body and equipment semantic clips have different authored duration metadata
- **THEN** equipped playback uses the body clip as wall-clock phase authority and samples equipment by normalized body phase without independent clock drift

#### Scenario: Multiple equipment items are animated
- **WHEN** multiple equipment instances are attached during one semantic motion
- **THEN** every item samples its resolved semantic clip at the same body-owned normalized phase while the declared driver binding alone determines the body side variant

### Requirement: Animated attachment uses current body socket and equipment bind Mount
Equipment attachment SHALL solve the equipment root from the current animated body socket and the equipment's bind Mount frame, then sample equipment animation relative to that root.

#### Scenario: Body socket moves during animation
- **WHEN** the single `Body` part changes pose and therefore moves a left/right equipment socket
- **THEN** the equipment attachment root follows that current socket frame at the same semantic phase

#### Scenario: Single-part weapon rotates during animation
- **WHEN** a current one-part equipment clip rotates its Mount-bearing part around a pivot colocated with Mount
- **THEN** the weapon visibly rotates around the grip while the attachment root remains bound to the body socket

#### Scenario: Runtime tries to solve against evaluated animated Mount
- **WHEN** composition would use the evaluated animated Mount matrix as the inverse root-solving input for a one-part weapon
- **THEN** compatibility rejects that method because it cancels the weapon's authored part transform

#### Scenario: Single-part weapon translates its Mount-bearing part
- **WHEN** equipped semantic animation contains a position track on the current one-part Mount-bearing part
- **THEN** compatibility fails because the grip would translate away from the body socket

### Requirement: Socket and animation compatibility remain density-independent
Different positive avatar/equipment authoring densities SHALL remain compatible when baked world scale, socket frames, Mount alignment, and semantic animation sampling satisfy the shared contracts.

#### Scenario: High-density equipment attaches to lower-density avatar
- **WHEN** equipment uses a different positive VPU from its avatar
- **THEN** compatibility evaluates baked world-space socket/animation frames and does not reject the pair solely for density difference

### Requirement: Socket and animation contracts remain gameplay-neutral
Attachment capabilities and semantic visual motions SHALL describe reusable physical/presentation behavior only and SHALL NOT encode damage, attack result, inventory occupancy, AI, balance, progression, or consuming-game combat class.

#### Scenario: Game gives a weapon gameplay meaning
- **WHEN** a consumer interprets an equipment asset as a primary weapon, secondary weapon, tool, or cosmetic
- **THEN** that meaning does not alter its store attachment capability, Mount frame, or semantic animation contract

#### Scenario: Consumer chooses active left or right equipment
- **WHEN** a game chooses which equipped instance drives an action
- **THEN** that runtime choice supplies the driver binding/side context without changing the reusable asset's catalog identity

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
