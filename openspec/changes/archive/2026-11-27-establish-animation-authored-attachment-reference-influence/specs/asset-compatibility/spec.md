# Spec Delta

## MODIFIED Requirements

### Requirement: Animated attachment uses current body socket and equipment bind Mount
Animated composition SHALL expose the current body socket combined with the equipment bind-Mount inverse as attachment reference data while body and equipment animation remain independently sampled. An attached model's authored reference influence SHALL determine how strongly that current reference controls its visual root without changing logical binding identity.

#### Scenario: Body socket moves while influence is one
- **WHEN** the Body animation moves an equipment socket and the attachment's sampled reference influence is `1`
- **THEN** the attachment visual root follows the current body-socket/bind-Mount reference while its model-local authored part animation remains independently sampled

#### Scenario: Body socket moves during animation
- **WHEN** the Body animation changes pose and therefore moves an equipment socket
- **THEN** the current attachment reference changes with that socket while the attachment visual root follows, blends toward, or remains independent from that reference according to its sampled reference influence

#### Scenario: Body socket moves while influence is zero
- **WHEN** an attachment has entered an authored interval whose sampled reference influence is `0` and the Body socket subsequently moves
- **THEN** the attachment visual root remains on its retained animation-owned reference basis while the separately queryable current attachment reference continues to move with the Body

#### Scenario: Reference influence is between zero and one
- **WHEN** an attachment's sampled reference influence is strictly between `0` and `1`
- **THEN** its visual root continuously blends between the retained animation-owned basis and the current body-socket/bind-Mount reference without changing attachment topology

#### Scenario: Clip omits reference influence
- **WHEN** an attachment clip contains no authored reference-influence channel
- **THEN** compatibility treats its reference influence as `1` and preserves legacy socket-relative presentation

#### Scenario: Single-part weapon rotates or translates during animation
- **WHEN** an equipment clip independently animates finite position or rotation on its Mount-bearing part
- **THEN** compatibility permits that model-local visual motion independently of whether the same clip follows, releases, or blends toward the attachment reference

#### Scenario: Single-part weapon rotates during animation
- **WHEN** an equipment clip independently rotates its Mount-bearing part around a pivot colocated with Mount
- **THEN** the weapon visibly rotates from its model-local animation while reference influence independently determines contribution from the current bind-Mount attachment reference

#### Scenario: Runtime tries to solve against evaluated animated Mount
- **WHEN** composition would use the evaluated animated Mount matrix as the inverse root-solving input for a one-part weapon
- **THEN** compatibility rejects that method because the attachment reference remains defined by the bind Mount rather than feedback from the animated visual pose

#### Scenario: Single-part weapon translates its Mount-bearing part
- **WHEN** an equipment animation contains a finite position track on the current one-part Mount-bearing part
- **THEN** compatibility permits the authored visual translation and does not infer reference release or compatibility failure solely because the evaluated Mount moves away from the body socket

#### Scenario: Visual motion departs from the attachment reference
- **WHEN** equipment animation and sampled reference influence produce a visual pose whose evaluated Mount differs from the current attachment reference
- **THEN** the equipment remains the same logically attached instance and no detach, reattach, or pair-specific correction is required

#### Scenario: Reference influence changes during one logical binding
- **WHEN** an authored clip transitions its reference influence from `1` toward `0` and later back toward `1`
- **THEN** the equipment remains the same logically attached instance throughout and no detach, reattach, handle replacement, or topology revision is implied

#### Scenario: Attachment is rebound during released visual motion
- **WHEN** an attachment is rebound to another compatible socket while its sampled reference influence is below `1`
- **THEN** the logical binding targets the new socket immediately, the current attachment reference updates to that socket, and the visual root converges according to subsequent authored influence rather than snapping solely because of the rebind

## ADDED Requirements

### Requirement: Attachment-reference influence is model-local presentation data
Reference influence SHALL belong only to the animation clip of the model that authors it. It SHALL NOT infer or command another model's animation, attachment topology, gameplay target, damage, AI state, inventory rule, or combat event.

#### Scenario: Weapon releases from its socket reference
- **WHEN** one weapon clip samples reference influence below `1`
- **THEN** only that weapon's visual attachment basis changes and body plus other attachment playback remain unchanged

#### Scenario: Rotation-only take releases from body motion
- **WHEN** an attachment clip authors no position channel but authors reference influence `0` during part of the take
- **THEN** compatibility permits its visual root to remain independent of Body socket movement without requiring translated motion as an implicit detach signal

#### Scenario: Translated take remains socket-relative
- **WHEN** an attachment clip authors a position channel while its reference influence remains `1`
- **THEN** the translated part animation remains relative to the live attachment reference instead of being treated as detached merely because translation exists
