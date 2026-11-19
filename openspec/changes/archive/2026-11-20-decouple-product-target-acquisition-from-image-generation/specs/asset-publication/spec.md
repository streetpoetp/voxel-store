# Spec Delta

## ADDED Requirements

### Requirement: New target-driven publication requires accepted Product Target authority
For new target-driven authoring, reference acceptance SHALL require a current accepted Product Target with valid acquisition provenance before Composition, Part, density, or source authority may advance. Publication SHALL NOT require a specific generator, provider, model, or acquisition technology.

#### Scenario: New target has current provider-neutral provenance
- **WHEN** a new target-driven asset has an accepted Product Target whose bytes, declared upstream authority, style acceptance, and provenance are current
- **THEN** the workflow may derive and audit Composition/Part construction evidence before later density/source gates

#### Scenario: Target exists without valid provenance
- **WHEN** a new target-driven asset has a target PNG but its acquisition provenance, declared source/style authority, or target hash is missing or stale
- **THEN** publication remains blocked at Product Target/reference authority and downstream geometry does not qualify

#### Scenario: Target has no generator metadata
- **WHEN** a new target-driven asset has an accepted Product Target with valid acquisition provenance but no generator/provider/model metadata
- **THEN** publication eligibility is unaffected because producer identity is not Product Target authority

## REMOVED Requirements

### Requirement: New generated target-driven publication requires image-model Product Target authority
**Reason**: Publication must depend on accepted Product Target authority and current provenance rather than a mandatory image-model producer.

**Migration**: Newly accepted targets use the provider-neutral Product Target publication requirement. Existing accepted provenance remains valid under its original contract.
