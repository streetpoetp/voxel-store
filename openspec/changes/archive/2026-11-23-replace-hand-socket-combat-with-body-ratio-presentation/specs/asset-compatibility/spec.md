# Spec Delta

## ADDED Requirements

### Requirement: Compatible avatars are single rigid Body assets
An avatar participating in reusable equipment composition SHALL expose exactly one authored rigid part named `Body`; independently animated head, arm, hand, foot, or equivalent attachment parts SHALL NOT be required for compatibility.

#### Scenario: Avatar uses the rigid-body contract
- **WHEN** an avatar is evaluated for reusable equipment composition
- **THEN** its authored geometry resolves to exactly one `Body` part whose canonical local bounds define the placement space

#### Scenario: Avatar contains articulated compatibility parts
- **WHEN** an avatar relies on additional articulated parts to satisfy reusable equipment composition
- **THEN** compatibility validation fails for the rigid-body contract instead of treating those parts as attachment authority

### Requirement: Equipment placement derives from normalized Body bounds ratios
Reusable equipment composition SHALL derive local equipment position from the avatar's canonical `Body` local bounds and a normalized offset ratio, without model-authored sockets, mount transforms, or per-avatar corrective transforms.

#### Scenario: Ratio placement is evaluated
- **WHEN** a consumer composes equipment at a normalized offset ratio
- **THEN** the local equipment position is derived deterministically from canonical `Body` bounds center and extents

#### Scenario: Avatar size changes
- **WHEN** two conforming avatars have different canonical Body dimensions
- **THEN** the same normalized offset ratio scales placement proportionally to each avatar's Body bounds

### Requirement: Equipment exposes a canonical local composition frame
Reusable equipment SHALL provide a canonical local origin and orientation that can be placed against any conforming Body-ratio position without avatar-specific, slot-specific, or pair-specific corrective transforms.

#### Scenario: Equipment composes at different ratios
- **WHEN** the same equipment asset is placed at different valid normalized Body ratios
- **THEN** its stable asset identity and canonical local frame remain unchanged

#### Scenario: Equipment needs an avatar-specific correction
- **WHEN** acceptable composition requires a transform unique to one avatar/equipment pair
- **THEN** compatibility proof fails and the owning asset or global composition convention must be corrected

### Requirement: Multiple equipment instances use deterministic independent ratio placements
A conforming Body SHALL support simultaneous composition of multiple independent equipment instances by assigning each instance a deterministic normalized Body-bounds ratio and canonical equipment frame; one instance SHALL NOT require a model-authored socket or another equipment instance as its parent.

#### Scenario: Two equipment assets are composed simultaneously
- **WHEN** a consumer assigns two equipment instances distinct normalized Body ratios
- **THEN** both instances resolve independently under the same rigid presentation composition

#### Scenario: More equipment instances are requested
- **WHEN** a consumer provides additional valid ratio placements
- **THEN** the composition model remains unchanged and does not require new avatar authoring metadata

### Requirement: Compatibility proof uses canonical composition evidence
Compatibility health SHALL be derived from deterministic Body-plus-equipment composition evidence using canonical Body bounds, normalized placement ratios, and canonical equipment frames rather than profile/socket attachment matrices.

#### Scenario: Representative composition passes
- **WHEN** current avatar and equipment candidates compose successfully under the canonical ratio-placement evidence set
- **THEN** compatibility health reports the applicable assets and placements as passing

#### Scenario: Representative composition fails
- **WHEN** equipment intersects, scales, or orients incompatibly under required canonical ratio-placement evidence
- **THEN** compatibility health reports the failing avatar, equipment, and placement ratio or convention

### Requirement: Rigid presentation motion preserves composition relationships
Once Body and equipment local placement is resolved, presentation motion SHALL preserve their relative composition by moving the unified presentation transform rather than re-solving avatar/equipment attachment during the motion.

#### Scenario: Presentation motion is applied
- **WHEN** a consumer applies presentation-only translation or rotation to the unified composition
- **THEN** Body and every composed equipment instance preserve their established local relationships

### Requirement: Equipment authoring density is independent of Body-ratio composition
Equipment SHALL be allowed to use a different positive `voxelsPerUnit` from an avatar when its canonical local origin, orientation, normalized physical scale, and Body-ratio composition behavior remain compatible after normalization to world space.

#### Scenario: High-density equipment composes with a lower-density avatar
- **WHEN** equipment uses a higher positive `voxelsPerUnit` than a conforming avatar but preserves its canonical local frame and normalized physical scale
- **THEN** compatibility validation accepts the density difference and evaluates ordinary Body-ratio composition evidence

#### Scenario: Equipment has no valid normalized composition frame
- **WHEN** equipment uses any `voxelsPerUnit` but lacks a canonical local origin/orientation or cannot satisfy required Body-ratio composition evidence
- **THEN** compatibility validation fails regardless of authoring density

## MODIFIED Requirements

### Requirement: Compatibility contracts remain reusable across games
Body-ratio and canonical equipment-frame conventions SHALL use visual and physical terms only. They SHALL NOT encode combat class, damage type, inventory slot rules, AI behavior, balance restrictions, attack behavior, or other consuming-game semantics.

#### Scenario: Consumer assigns gameplay meaning
- **WHEN** a game treats a composed asset as a sword, tool, cosmetic, primary weapon, secondary weapon, or another product-specific role
- **THEN** that gameplay meaning remains outside the store compatibility contract and does not alter Body-ratio composition semantics

## REMOVED Requirements

### Requirement: Compatibility profiles define physical mount contracts
**Reason**: The approved rigid-body strategy removes profile-owned mount capabilities and physical attachment instances.
**Migration**: Replace profile/socket declarations with global single-Body bounds and canonical equipment-frame conventions.

### Requirement: Avatars prove profile conformance
**Reason**: Avatars no longer conform by exposing required mount capabilities or sockets.
**Migration**: Validate exactly one `Body` part and its canonical local bounds as the avatar composition contract.

### Requirement: Equipment declares one canonical mount contract
**Reason**: Equipment no longer declares `mount.profile` or `mount.capability`.
**Migration**: Preserve independent equipment identity and require only a canonical local composition origin/orientation.

### Requirement: Family compatibility is verified by composition evidence
**Reason**: Profile families and per-socket composition matrices are being removed.
**Migration**: Generate compatibility health from canonical Body-ratio composition evidence.

### Requirement: Avatar animation preserves equipment attachment semantics
**Reason**: Rigid-body avatars no longer animate attachment parts or sockets.
**Migration**: Resolve composition once in local space and preserve it by moving the unified presentation transform.

### Requirement: Equipment authoring density is independent of avatar authoring density
**Reason**: The density rule is retained but rewritten around canonical equipment frames and Body-ratio composition instead of profile/socket mount evidence.
**Migration**: Use `Equipment authoring density is independent of Body-ratio composition`.
