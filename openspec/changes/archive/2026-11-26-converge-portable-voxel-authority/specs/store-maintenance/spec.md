## REMOVED Requirements

### Requirement: Store state is derived by scanning authoritative inputs
**Reason**: The current requirement still names retired `Profiles/**` as active authority.

**Migration**: Use the replacement requirement below, which scans only current authoritative roots and model-owned compatibility inputs.

## ADDED Requirements

### Requirement: Store state is derived by scanning current authoritative inputs
The maintenance workflow SHALL scan authoritative Assets, Styles, Collections, Store policy/history, source/reference files, generated artifacts, and verification evidence to derive current asset health, compatibility health, publication state, and selected-collection coverage gaps. It SHALL NOT require a separate retired compatibility-authority root and SHALL NOT trust generated Catalog outputs as authoritative inputs to those decisions.

#### Scenario: Store scan runs with stale generated outputs
- **WHEN** generated catalog or health files are missing or stale but current authoritative repository inputs are valid
- **THEN** the scan derives current state from authoritative inputs and can regenerate the derived outputs

## REMOVED Requirements

### Requirement: Store domain roots have single responsibilities
**Reason**: The requirement still assigns active physical compatibility authority to retired `Profiles/**`.

**Migration**: Use the replacement requirement below, where model-owned sockets/Mount metadata and current asset metadata own reusable physical compatibility.

### Requirement: Intentional collection baseline replacement updates active requirements atomically
**Reason**: The current requirement still requires compatibility-profile selection as part of baseline authority.

**Migration**: Use the replacement requirement below, which atomically updates only current Collection requirements and active reusable asset inventory.

## ADDED Requirements

### Requirement: Store domain roots reflect current authority
The active repository layout SHALL separate reusable Assets, Styles, Collections, Store policy/history, and derived Catalog outputs. Product asset discovery SHALL be restricted to Assets. Reusable physical compatibility SHALL be derived from model-owned sockets/Mount data and current asset metadata rather than from a separate profile root.

#### Scenario: Model-owned compatibility metadata changes
- **WHEN** an avatar socket contract or equipment attachment metadata changes
- **THEN** compatibility/health evaluation derives the new physical result from those current model-owned inputs independently of collection policy

### Requirement: Intentional collection baseline replacement updates current authority atomically
An approved collection-baseline migration SHALL update collection requirements and active asset inventory as one coherent authority change so retired requirements do not remain as autonomous repair targets.

#### Scenario: General reusable equipment baseline replaces side-specific coverage
- **WHEN** an approved migration retires side-specific equipment identity/coverage and introduces a general reusable form set
- **THEN** maintenance evaluates coverage only against the new collection requirement set and does not plan recreation of retired side-specific identities or slots

#### Scenario: Retired equipment files are removed before collection authority changes
- **WHEN** the selected collection still requires a removed equipment form
- **THEN** the store reports a required coverage gap until the approved migration updates the collection authority

## MODIFIED Requirements

### Requirement: Reusable assets are independent of collection ownership
Published reusable assets SHALL be discovered from the repository's asset root independently of the currently selected collection. Collections SHALL express desired coverage over reusable asset properties and SHALL NOT own the asset filesystem identity or require assets to live under a collection-specific directory.

#### Scenario: Combat collection selects reusable equipment
- **WHEN** `combat-foundation.v1` requires a sword and a healthy reusable sword asset exists under the asset root with matching physical form
- **THEN** that asset satisfies the collection requirement without being stored beneath a CombatFoundation directory

#### Scenario: Collection is replaced
- **WHEN** the store selects a different collection while existing assets remain valid reusable content
- **THEN** those assets retain their ids, UnitRefs, source/proof authority, and publication identity; only derived collection coverage changes

### Requirement: New production models are target-first and template-independent
New production asset authoring SHALL begin from the selected collection requirement or explicit user-requested form, current reusable style authority, applicable model-owned compatibility contract, and a new asset-specific design intent. It SHALL acquire and accept a new Product Target before construction, density, VoxelSpec, proof, compatibility, or publication, and SHALL NOT depend on another asset as visual/model authority.

#### Scenario: Agent is asked to create a new weapon
- **WHEN** the requested physical form, style authority, and intended attachment capability are known
- **THEN** the agent establishes a distinct asset identity and acquires/accepts a new Product Target before authoring VoxelSpec source

#### Scenario: Existing asset could be copied as a shortcut
- **WHEN** another asset has similar geometry, palette, materials, or decorative motifs
- **THEN** the agent may inspect it for repository conventions but does not treat its visual/model choices as authority for the new asset

#### Scenario: Product Target creation is blocked
- **WHEN** no acquisition path can establish repository-owned Product Target bytes that pass current provenance, style, and visual acceptance
- **THEN** production model creation stops before Composition, density, or VoxelSpec authoring rather than fabricating a model-first substitute

#### Scenario: Blocked production creation has no sample fallback
- **WHEN** Product Target authority remains blocked for a new production asset
- **THEN** the workflow reports the blocked Product Target gate and does not continue the asset as Samples content, experimental source, or any alternate publication lane

### Requirement: Selected collection drives autonomous expansion
The selected Collection SHALL declare desired reusable asset coverage independently from current generated Catalog state and MAY distinguish equipment requirements by gameplay-neutral physical form. A maintenance scan SHALL derive uncovered required slots from the Collection and current healthy published assets using every discriminator actually declared by the active Collection schema.

#### Scenario: Required equipment coverage is missing
- **WHEN** the selected collection requires an equipment physical form and no healthy published equipment asset matches that declared form
- **THEN** the scan reports that exact coverage gap as eligible for autonomous asset design after higher-priority repairs are resolved

#### Scenario: Different form does not satisfy requirement
- **WHEN** a healthy published equipment asset declares a different physical form than the selected requirement
- **THEN** that asset does not satisfy the form-specific coverage requirement

### Requirement: Derived outputs explain health and next work
Generated health and catalog outputs SHALL identify each asset's current publication state and failed gate when unhealthy, and SHALL expose uncovered selected-collection requirements or the next eligible maintenance target using the discriminators actually present in active authority.

#### Scenario: Asset compatibility gate fails
- **WHEN** a scan detects an equipment/avatar socket/Mount compatibility failure
- **THEN** generated health data identifies the affected assets and compatibility gate as the reason the store is unhealthy

#### Scenario: Physical-form coverage is missing
- **WHEN** a coverage requirement is unmet because its required physical form has no healthy published match
- **THEN** generated health data identifies that form without inventing retired discriminators not present in the Collection contract

### Requirement: Combat collection provides ten distinct general catalog forms
`combat-foundation.v1` SHALL require ten independently published equipment forms through its Collection manifest. Each required form SHALL remain a reusable physical classification without assigning side-specific consumer behavior.

#### Scenario: General catalog is complete
- **WHEN** one healthy published equipment asset satisfies each of the ten required physical forms
- **THEN** the selected collection reports no general catalog coverage gap

#### Scenario: One required form is missing
- **WHEN** nine required forms are satisfied but no healthy published asset satisfies the remaining form
- **THEN** maintenance reports that exact form as the next eligible required coverage gap after higher-priority repairs
