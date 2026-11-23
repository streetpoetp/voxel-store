# Spec Delta

## ADDED Requirements

### Requirement: Preview composes equipment from baked socket frames
The Unity preview SHALL resolve carried equipment from the selected avatar equipment socket and the equipment's baked Mount socket.

#### Scenario: Equipment is attached in the runtime gallery
- **WHEN** the gallery composes an avatar and equipment pair
- **THEN** it resolves a compatible `Equip.<Capability>.<Side>.<Instance>` socket and aligns the equipment's `Mount` frame without a Body-ratio placement offset

#### Scenario: Attachment cannot be solved
- **WHEN** required socket/Mount data is missing, singular, or requires pair-specific correction
- **THEN** preview composition fails visibly instead of falling back to the prior Body-ratio path

### Requirement: Preview loads authored animation for every composed model
The runtime gallery SHALL load the Hero.Voxel baked animation asset associated with each discovered avatar and equipment assembly.

#### Scenario: Avatar and weapon are composed
- **WHEN** both models have required baked semantic clips
- **THEN** the preview binds their baked animation assets to the composed presentations

#### Scenario: Required animation asset is unavailable
- **WHEN** a model assembly exists but its current semantic animation asset cannot be resolved
- **THEN** synchronized motion proof fails instead of synthesizing canonical motion procedurally

### Requirement: Preview motion context preserves selected slot side
The runtime preview SHALL keep the selected physical driver binding, including Left/Right/Center side, as part of semantic motion state so body animation can react differently to the active attachment side.

#### Scenario: Left-hand Attack is selected
- **WHEN** the preview drives `Attack` from equipment bound to a Left Hand slot
- **THEN** it resolves the body's left-side Attack variant and shows the corresponding left-handed body response

#### Scenario: Right-hand Attack is selected
- **WHEN** the same equipment asset is instead bound to a Right Hand slot and drives `Attack`
- **THEN** it resolves the body's right-side Attack variant without requiring a different equipment asset identity

### Requirement: One semantic phase drives body and equipment authored clips
The runtime preview SHALL use one body-owned normalized semantic phase to sample both the body clip and every attached equipment clip.

#### Scenario: Attack plays in real time
- **WHEN** `Attack` playback advances
- **THEN** body wall-clock cadence advances one normalized phase and every equipment `Attack` clip is sampled at that exact phase

#### Scenario: Motion is sampled for capture
- **WHEN** command-driven capture requests a deterministic normalized phase
- **THEN** body/equipment authored clips, selected driver side variant, and attachment sockets are evaluated at that same phase before Game View capture

### Requirement: Preview re-solves attachment after pose evaluation
For every semantic phase, preview SHALL evaluate body pose, evaluate equipment pose, and then solve the equipment root so its Mount remains locked to the animated body socket.

#### Scenario: Weapon swings around the grip
- **WHEN** the equipment `Attack` clip rotates the part containing its Mount
- **THEN** the visible weapon swings around the attachment while Mount remains coincident with the body socket

#### Scenario: Body motion moves the attachment
- **WHEN** the body clip moves its socket frame
- **THEN** attached equipment follows the evaluated socket without accumulating drift

### Requirement: Graphical capture proves authored synchronized animation
The canonical `avatar-motion-gallery` capture scenario SHALL prove current model-owned socket attachment and authored body/equipment semantic animation through provider-attested Game View pixels.

#### Scenario: Capture runs
- **WHEN** `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` completes
- **THEN** attested checkpoints show body and equipment sampled at declared semantic phases, visibly locked to the correct side-aware socket, with separate left/right body reactions proven for a side-reactive hand motion

#### Scenario: Legacy procedural motion still appears authoritative
- **WHEN** visual proof depends on the old Body-ratio/DOTween motion path instead of baked sockets and authored clips
- **THEN** the change is not complete

## REMOVED Requirements

### Requirement: Preview composes equipment from canonical Body bounds ratios
**Reason**: Body-ratio placement is replaced by model-authored avatar sockets and equipment Mount alignment.
**Migration**: Update gallery composition to resolve baked socket frames.

### Requirement: Equipment presentation separates placement from animation
**Reason**: Static PlacementRoot plus temporary MotionRoot is superseded by pose evaluation and per-phase socket/Mount root solving.
**Migration**: Presentation owns evaluated assembly/animation state and current attachment binding.

### Requirement: One motion player owns body and equipment presentation animation
**Reason**: The semantic owner remains singular, but it must sample authored Hero.Voxel clips instead of building DOTween animation tracks.
**Migration**: Keep one semantic phase owner and replace procedural sequence construction with provider animation sampling.

### Requirement: Preview motion grammar is semantic and deterministic
**Reason**: The semantic grammar remains, but its canonical realization moves from procedural preview motion to authored catalog clips.
**Migration**: Preserve semantic names and deterministic selection while sourcing visual motion from baked clips.

### Requirement: Presentation proof remains derived and non-authoritative
**Reason**: Captures remain derived, but old wording specifically permits DOTween demonstration motion as the presentation path being superseded.
**Migration**: Keep capture outputs derived while using authored source clips as animation authority.
