# asset-compatibility Specification

## Purpose

Defines reusable physical compatibility between independently authored voxel avatars and equipment so shared assets can compose predictably without baking every avatar/equipment combination into a separate asset.

## Requirements

### Requirement: Compatibility profiles define physical mount contracts
The store SHALL define versioned compatibility profiles that name required and optional mount sockets plus their visual frame, scale, and envelope conventions. Profile identifiers SHALL be stable and SHALL describe physical visual compatibility rather than gameplay roles.

#### Scenario: Profile is declared
- **WHEN** a compatibility profile is added with a unique versioned id and a complete set of required mount conventions
- **THEN** avatars and equipment may reference that profile as their shared physical compatibility contract

#### Scenario: Profile semantics change incompatibly
- **WHEN** required socket/frame/scale semantics would change for an existing profile
- **THEN** the store requires a new profile version instead of silently redefining the old compatibility contract

### Requirement: Avatars prove profile conformance
An avatar that declares a compatibility profile SHALL provide every required socket with the profile's expected names and physical frame semantics. Optional sockets MAY be omitted, but any provided profile socket SHALL conform to the same contract in neutral and animated poses.

#### Scenario: Avatar satisfies its declared profile
- **WHEN** an avatar provides all required sockets and verification confirms their expected bind and animated physical frames
- **THEN** the avatar passes profile conformance for that compatibility family

#### Scenario: Required avatar socket is missing
- **WHEN** an avatar declares a profile but lacks a required socket or exposes it with incompatible frame semantics
- **THEN** avatar compatibility validation fails and the avatar cannot publish as conforming to that profile

### Requirement: Equipment declares one canonical mount contract
Mountable equipment SHALL remain an independent voxel asset and SHALL declare the compatibility profile and socket it mounts to. Its canonical local mount frame SHALL be authored so ordinary composition does not require a per-avatar or per-pair corrective transform.

#### Scenario: Equipment mounts through the declared socket
- **WHEN** compatible equipment is composed with a conforming avatar at its declared socket
- **THEN** the equipment's canonical mount frame aligns through the socket contract without a pair-specific corrective asset or transform record

#### Scenario: Equipment requires a pair-specific correction
- **WHEN** an equipment/avatar combination only becomes visually acceptable after adding a correction unique to that pair
- **THEN** compatibility proof fails and the owning avatar, equipment, or compatibility profile must be corrected or explicitly reclassified

### Requirement: Family compatibility is verified by composition evidence
For each declared avatar/equipment compatibility family, current published candidates SHALL be composition-tested at the declared mount under required canonical poses. Compatibility health SHALL be derived from those results and SHALL identify the failing asset pair or profile rule.

#### Scenario: New equipment is compatible with the family
- **WHEN** equipment declares a profile and passes required composition evidence against every current published avatar that claims that profile
- **THEN** the equipment passes the family compatibility gate for that store snapshot

#### Scenario: New avatar breaks existing family compatibility
- **WHEN** a newly added avatar claims a profile but fails composition evidence with existing published equipment for that profile
- **THEN** regenerated compatibility health reports the failing pairs and the avatar does not qualify as a conforming published member of that family

### Requirement: Avatar animation preserves equipment attachment semantics
Compatibility validation SHALL include attachment behavior under the profile's required representative avatar poses or animation samples. Equipment SHALL follow the avatar's animated socket frame without requiring equipment geometry to be merged into the avatar assembly.

#### Scenario: Animated socket moves
- **WHEN** a conforming avatar is evaluated at a required animated pose and the equipment is composed at the declared socket
- **THEN** the equipment follows the evaluated socket frame and remains within the profile's accepted attachment relationship

### Requirement: Compatibility contracts remain reusable across games
Compatibility profiles and equipment mount declarations SHALL use visual/physical terms only. They SHALL NOT encode combat class, damage type, inventory slot rules, AI behavior, balance restrictions, or other consuming-game semantics.

#### Scenario: Consumer assigns gameplay meaning
- **WHEN** a game uses a store asset as a sword, tool, cosmetic, or other product-specific role
- **THEN** that gameplay meaning remains outside the store compatibility contract and does not alter the asset's physical mount compatibility
