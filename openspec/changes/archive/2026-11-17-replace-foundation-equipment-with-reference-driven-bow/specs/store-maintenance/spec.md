# Spec Delta

## ADDED Requirements

### Requirement: Intentional baseline replacement updates active coverage atomically
An approved foundation-baseline migration SHALL update active coverage requirements and active asset inventory as one coherent authority change so retired requirements do not remain as autonomous repair targets.

#### Scenario: Three equipment slots are replaced by one bow slot
- **WHEN** an approved migration removes the Hand.R, Hand.L-guard, and Back equipment requirements and introduces one required Hand.L bow requirement
- **THEN** maintenance evaluates coverage only against the new active requirement set and does not plan recreation of the retired slots

#### Scenario: Retired equipment files are removed before coverage authority changes
- **WHEN** active coverage still requires a removed equipment slot
- **THEN** the store reports a required coverage gap until the approved migration updates coverage authority

### Requirement: Baseline replacement converges to a healthy no-op state
After an intentional baseline migration, maintenance SHALL converge when the replacement assets satisfy the new active coverage and all publication/compatibility gates pass.

#### Scenario: Bow satisfies the replacement baseline
- **WHEN** the avatar and bow are healthy published assets, the bow satisfies the active Hand.L coverage requirement, and compatibility evidence passes
- **THEN** coverage gaps are empty and the next maintenance job is null

#### Scenario: Replacement bow is not yet publishable
- **WHEN** the old equipment has been retired but the new bow fails a required publication or compatibility gate
- **THEN** maintenance targets the bow's earliest failing gate rather than resurrecting a retired equipment asset
