# Spec Delta

## MODIFIED Requirements

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
