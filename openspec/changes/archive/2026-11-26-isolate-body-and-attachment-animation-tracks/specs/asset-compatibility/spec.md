# Spec Delta

## REMOVED Requirements

### Requirement: Driver slot side selects authored motion variant
**Reason**: Physical attachment side must not implicitly select or restart body animation or another model's animation.

**Migration**: Consumers explicitly choose the clip/candidates for each model instance. A consumer may explicitly request `Attack.Left` on the body, but equipment playback never drives that choice.

### Requirement: Authored body and equipment animation share one semantic phase
**Reason**: Body and attachment animations now own independent clocks, phases, loop state, speed, and playback lifecycle.

**Migration**: Issue ordinary animation commands to each model instance that should animate. If multiple models should begin related clips in one update, command each model independently.

## ADDED Requirements

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

## MODIFIED Requirements

### Requirement: Animated attachment uses current body socket and equipment bind Mount
Equipment attachment SHALL solve the equipment root from the current animated body socket and the equipment's bind Mount frame. Body and equipment animation SHALL be sampled independently around that physical attachment relationship.

#### Scenario: Body socket moves during animation
- **WHEN** the Body animation changes pose and therefore moves an equipment socket
- **THEN** the equipment attachment root follows the current evaluated socket frame regardless of the equipment animation's independent phase or playing state

#### Scenario: Single-part weapon rotates during animation
- **WHEN** an equipment clip independently rotates its Mount-bearing part around a pivot colocated with Mount
- **THEN** the weapon visibly rotates around the grip while its attachment root remains bound to the current body socket

#### Scenario: Runtime tries to solve against evaluated animated Mount
- **WHEN** composition would use the evaluated animated Mount matrix as the inverse root-solving input for a one-part weapon
- **THEN** compatibility rejects that method because it cancels the weapon's authored part transform

#### Scenario: Single-part weapon translates its Mount-bearing part
- **WHEN** an equipment animation contains a position track on the current one-part Mount-bearing part
- **THEN** compatibility fails because the grip would translate away from the body socket

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

### Requirement: Variant fallback remains model-local across composed actors
A composed actor SHALL resolve each model instance's candidate chain independently against that model's own animation catalog. Shared variant identifiers SHALL NOT imply shared choreography, timing, phase, semantic request, or playback state across models.

#### Scenario: Attachment lacks the root take
- **WHEN** the body independently resolves `Attack.Left.V2` while an attachment is commanded with `Idle`
- **THEN** the attachment remains on its own Idle candidate chain and does not inherit the body's Attack take

#### Scenario: Different attachments expose different variant coverage
- **WHEN** two attachments are independently requested to play the same semantic motion and variant but only one publishes that take
- **THEN** each attachment resolves its own ordered fallback independently and advances on its own playback clock
