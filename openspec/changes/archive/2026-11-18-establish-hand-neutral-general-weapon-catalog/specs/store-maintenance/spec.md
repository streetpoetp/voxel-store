# Spec Delta

## ADDED Requirements

### Requirement: Combat foundation provides ten distinct general catalog forms
`CombatFoundation.v1` SHALL require ten independently published equipment forms as declared by active coverage policy. Each required form SHALL remain a reusable physical classification without assigning side-specific consumer behavior.

#### Scenario: General catalog is complete
- **WHEN** one healthy published equipment asset satisfies each of the ten required physical forms under the active compatibility profile
- **THEN** the foundation reports no general catalog coverage gap

#### Scenario: One required form is missing
- **WHEN** nine required forms are satisfied but no healthy published asset satisfies the remaining form
- **THEN** maintenance reports that exact form as the next eligible required coverage gap after higher-priority repairs

## MODIFIED Requirements

### Requirement: Coverage policy drives autonomous expansion
Repository policy SHALL declare desired reusable foundation coverage independently from current generated catalog state and MAY distinguish equipment requirements by gameplay-neutral physical form. A maintenance scan SHALL derive uncovered required slots from policy and current healthy published assets using every declared coverage discriminator rather than from a manually maintained TODO list.

#### Scenario: Required equipment coverage is missing
- **WHEN** policy requires equipment for a compatibility profile, logical mount capability, and physical form and no healthy published asset matches all declared discriminators
- **THEN** the scan reports that exact coverage gap as eligible for autonomous asset design after higher-priority repairs are resolved

#### Scenario: Different form does not satisfy requirement
- **WHEN** a healthy published equipment asset matches the required profile and mount capability but declares a different physical form
- **THEN** that asset does not satisfy the form-specific coverage requirement

### Requirement: Derived outputs explain health and next work
Generated health and catalog outputs SHALL identify each asset's current publication state and failed gate when unhealthy, and SHALL expose uncovered policy requirements or the next eligible maintenance target with every active coverage discriminator in machine-readable form.

#### Scenario: Asset compatibility gate fails
- **WHEN** a scan detects an equipment/avatar compatibility failure
- **THEN** generated health data identifies the affected assets/profile and the compatibility gate as the reason the store is unhealthy

#### Scenario: Physical-form coverage is missing
- **WHEN** a coverage requirement is unmet because its required physical form has no healthy published match
- **THEN** generated health data identifies that form together with the relevant compatibility profile and logical mount capability

### Requirement: Intentional baseline replacement updates active coverage atomically
An approved foundation-baseline migration SHALL update active coverage requirements, compatibility-profile selection, and active asset inventory as one coherent authority change so retired requirements do not remain as autonomous repair targets.

#### Scenario: Three equipment slots are replaced by one bow slot
- **WHEN** an approved migration retires side-specific equipment identity/coverage, activates the new neutral compatibility profile, and introduces the ten required general forms
- **THEN** maintenance evaluates coverage only against the new requirement set and does not plan recreation of retired side-specific identities or coverage slots

#### Scenario: Retired equipment files are removed before coverage authority changes
- **WHEN** active coverage still requires a removed equipment slot
- **THEN** the store reports a required coverage gap until the approved migration updates coverage authority

### Requirement: Baseline replacement converges to a healthy no-op state
After an intentional baseline migration, maintenance SHALL converge when replacement assets satisfy the new active coverage and all publication/compatibility gates pass.

#### Scenario: Bow satisfies the replacement baseline
- **WHEN** the foundation avatar and all ten required forms are healthy published assets and compatibility evidence passes across every required attachment instance and representative pose
- **THEN** coverage gaps are empty and the next maintenance job is null

#### Scenario: Replacement bow is not yet publishable
- **WHEN** a required form exists but its active replacement asset fails a required publication or compatibility gate
- **THEN** maintenance targets that asset's earliest failing gate rather than creating a duplicate asset for the same form or restoring a retired side-specific identity
