# Spec Delta

## ADDED Requirements

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

## REMOVED Requirements

### Requirement: Equipment placement derives from normalized Body bounds ratios
**Reason**: Model-authored avatar sockets and equipment Mount frames become the single attachment authority.
**Migration**: Migrate avatar/equipment source to sockets and regenerate compatibility evidence.

### Requirement: Equipment exposes a canonical local composition frame
**Reason**: Canonical orientation remains required, but physical attachment origin is now the authored Mount socket rather than the equipment root against a Body ratio.
**Migration**: Preserve canonical Right/Up/Forward and author a Mount socket for each equipment model.

### Requirement: Multiple equipment instances use deterministic independent ratio placements
**Reason**: Multiple equipment now resolves against independent physical socket instances.
**Migration**: Replace preview/compatibility ratio inputs with selected compatible socket instances.

### Requirement: Compatibility proof uses canonical composition evidence
**Reason**: Ratio-placement evidence is superseded by socket/Mount plus semantic-phase compatibility evidence.
**Migration**: Regenerate compatibility evidence using evaluated socket, Mount, and animation phase data.

### Requirement: Rigid presentation motion preserves composition relationships
**Reason**: A unified procedural presentation transform is no longer sufficient once body sockets and equipment Mount frames animate independently.
**Migration**: Evaluate authored poses first and re-solve attachment every semantic phase.

### Requirement: Equipment authoring density is independent of Body-ratio composition
**Reason**: Density independence remains, but compatibility is no longer Body-ratio based.
**Migration**: Use the added density-independent socket/animation requirement.

### Requirement: Compatibility contracts remain reusable across games
**Reason**: Reusability remains but the old requirement names Body-ratio conventions that are being removed.
**Migration**: Use the added gameplay-neutral socket and animation requirement.
