# Spec Delta

## ADDED Requirements

### Requirement: Published assets can be retired without identity reuse
The store SHALL allow a previously published asset to be removed from the active baseline while preserving its historical asset id and UnitRef as reserved identities that cannot be reassigned to a different asset.

#### Scenario: Published equipment is intentionally retired
- **WHEN** an approved baseline migration removes a previously published equipment asset and its active authoring/evidence files
- **THEN** subsequent scans omit that asset from active publication/catalog state without treating its absence as an in-place identity mutation

#### Scenario: Replacement equipment attempts to reuse a retired identity
- **WHEN** a new replacement asset declares an asset id or UnitRef previously published by a retired asset
- **THEN** validation fails and requires a new semantic identity

### Requirement: Replacement assets pass normal publication gates
A replacement asset SHALL be treated as a new independently authored asset and SHALL pass the same reference-first, source, deterministic generation, proof, compatibility, and budget gates as any other new asset.

#### Scenario: Replacement bow is added after retiring prior equipment
- **WHEN** the new bow has a unique id/UnitRef and its current evidence satisfies every required publication gate
- **THEN** the bow may publish independently of the retired equipment history

#### Scenario: Replacement bow bypasses reference-first authority
- **WHEN** bow source or generated proof exists without an accepted canonical reference package
- **THEN** publication fails at the reference/source ordering boundary
