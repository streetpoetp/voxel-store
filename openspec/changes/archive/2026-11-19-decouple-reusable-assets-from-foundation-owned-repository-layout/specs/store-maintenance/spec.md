# Spec Delta

## MODIFIED Requirements

### Requirement: Reusable assets are independent of collection ownership
Published reusable assets SHALL be discovered from the repository's asset root independently of the currently selected collection. Collections SHALL express desired coverage over reusable asset properties and SHALL NOT own the asset filesystem identity or require assets to live under a collection-specific directory.

#### Scenario: Combat collection selects reusable equipment
- **WHEN** `combat-foundation.v1` requires a sword and a healthy reusable sword asset exists under the asset root with matching profile/capability/form
- **THEN** that asset satisfies the collection requirement without being stored beneath a CombatFoundation directory

#### Scenario: Collection is replaced
- **WHEN** the store selects a different collection while existing assets remain valid reusable content
- **THEN** those assets retain their ids, UnitRefs, source/proof authority, and publication identity; only derived collection coverage changes

### Requirement: Store domain roots have single responsibilities
The active repository layout SHALL separate reusable Assets, Styles, Profiles, Collections, Samples, Store policy/history, and derived Catalog outputs. Product asset discovery SHALL be restricted to Assets and SHALL NOT treat Samples as publishable store content.

#### Scenario: Sample contains asset-like files
- **WHEN** an experiment under `Samples/**` contains files that resemble store source or manifests
- **THEN** deterministic store discovery ignores it unless it is intentionally migrated into `Assets/**`

#### Scenario: Compatibility profile changes
- **WHEN** a profile under `Profiles/**` changes
- **THEN** compatibility/health evaluation uses that profile as reusable physical contract authority independently of collection policy
