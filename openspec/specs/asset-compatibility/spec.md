# asset-compatibility Specification

## Purpose

Defines reusable physical compatibility between independently authored voxel avatars and equipment so shared assets can compose predictably without baking every avatar/equipment combination into a separate asset.

## Requirements

### Requirement: Compatibility profiles define physical mount contracts
The store SHALL define versioned compatibility profiles that name required and optional logical mount capabilities, the physical attachment instances that satisfy each capability, and their visual frame, scale, and envelope conventions. Profile identifiers SHALL be stable and SHALL describe physical visual compatibility rather than gameplay roles or handedness.

#### Scenario: Profile is declared
- **WHEN** a compatibility profile is added with a unique versioned id and a complete set of required mount-capability conventions
- **THEN** avatars and equipment may reference that profile as their shared physical compatibility contract

#### Scenario: Profile semantics change incompatibly
- **WHEN** required mount-capability, attachment-instance, frame, or scale semantics would change for an existing profile
- **THEN** the store requires a new profile version instead of silently redefining the old compatibility contract

### Requirement: Avatars prove profile conformance
An avatar that declares a compatibility profile SHALL provide every required logical mount capability with the profile's required number of physical attachment instances and expected frame semantics. Optional capabilities MAY be omitted, but every provided profile attachment instance SHALL conform to the same contract in neutral and animated poses.

#### Scenario: Avatar satisfies its declared profile
- **WHEN** an avatar provides every required mount capability with conforming physical attachment instances and verification confirms their bind and animated frames
- **THEN** the avatar passes profile conformance for that compatibility family

#### Scenario: Required avatar socket is missing
- **WHEN** an avatar declares a profile but lacks a required mount capability, exposes too few required physical attachment instances, or exposes an instance with incompatible frame semantics
- **THEN** avatar compatibility validation fails and the avatar cannot publish as conforming to that profile

### Requirement: Equipment declares one canonical mount contract
Mountable equipment SHALL remain an independent voxel asset and SHALL declare the compatibility profile and logical mount capability it requires, not a side-specific physical attachment instance. Its canonical local mount frame SHALL compose through every conforming physical instance of that capability without a per-avatar, per-instance, or per-pair corrective transform.

#### Scenario: Equipment mounts through the declared socket
- **WHEN** compatible equipment is composed with a conforming avatar at any physical attachment instance satisfying its declared mount capability
- **THEN** the equipment's canonical mount frame aligns through that instance without a pair-specific or side-specific corrective asset or transform record

#### Scenario: Hand-neutral equipment is evaluated on either physical hand
- **WHEN** equipment declares the logical `Hand` mount capability and a conforming avatar exposes multiple physical attachment instances for `Hand`
- **THEN** the same equipment asset is eligible for composition at every such hand instance without changing its identity or mount declaration

#### Scenario: Equipment requires a pair-specific correction
- **WHEN** an equipment/avatar/attachment-instance combination only becomes visually acceptable after adding a correction unique to that combination
- **THEN** compatibility proof fails and the owning avatar, equipment, or compatibility profile must be corrected or explicitly reclassified

### Requirement: Family compatibility is verified by composition evidence
For each declared avatar/equipment compatibility family, current published candidates SHALL be composition-tested at every physical attachment instance satisfying the equipment's declared logical mount capability under required canonical poses. Compatibility health SHALL be derived from those results and SHALL identify the failing asset pair, capability, attachment instance, or profile rule.

#### Scenario: New equipment is compatible with the family
- **WHEN** equipment declares a profile and mount capability and passes required composition evidence against every matching physical attachment instance on every current published avatar that claims that profile
- **THEN** the equipment passes the family compatibility gate for that store snapshot

#### Scenario: New avatar breaks existing family compatibility
- **WHEN** a newly added avatar claims a profile but any physical attachment instance for a required capability fails composition evidence with existing published equipment for that capability
- **THEN** regenerated compatibility health reports the failing pair and attachment instance and the avatar does not qualify as a conforming published member of the family

### Requirement: Avatar animation preserves equipment attachment semantics
Compatibility validation SHALL include attachment behavior under the profile's required representative avatar poses or animation samples. Equipment SHALL follow each evaluated physical attachment instance satisfying its logical mount capability without requiring equipment geometry to be merged into the avatar assembly.

#### Scenario: Animated socket moves
- **WHEN** a conforming avatar is evaluated at a required animated pose and equipment is composed at a physical attachment instance satisfying its declared mount capability
- **THEN** the equipment follows the evaluated attachment frame and remains within the profile's accepted attachment relationship

### Requirement: Compatibility contracts remain reusable across games
Compatibility profiles and equipment mount declarations SHALL use visual/physical terms only. They SHALL NOT encode combat class, damage type, inventory slot rules, AI behavior, balance restrictions, or other consuming-game semantics.

#### Scenario: Consumer assigns gameplay meaning
- **WHEN** a game uses a store asset as a sword, tool, cosmetic, or other product-specific role
- **THEN** that gameplay meaning remains outside the store compatibility contract and does not alter the asset's physical mount compatibility

### Requirement: Equipment authoring density is independent of avatar authoring density
Equipment that declares a valid compatibility profile SHALL be allowed to use a different positive `voxelsPerUnit` from the avatar/profile authoring density when its canonical assembly origin and mount-frame behavior remain physically compatible after normalization to world space.

#### Scenario: High-density equipment mounts to a lower-density avatar
- **WHEN** equipment uses a higher positive `voxelsPerUnit` than a conforming avatar but preserves its canonical mount frame and normalized physical scale
- **THEN** compatibility validation accepts the density difference and evaluates the ordinary profile/socket evidence

#### Scenario: Equipment has no valid normalized mount frame
- **WHEN** equipment uses any `voxelsPerUnit` but lacks a canonical assembly origin or cannot satisfy the declared mount-frame evidence
- **THEN** compatibility validation fails regardless of authoring density
