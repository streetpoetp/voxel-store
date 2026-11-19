# Spec Delta

## MODIFIED Requirements

### Requirement: Published identity is independent of repository collection paths
Moving a published asset between repository organization roots SHALL NOT change its published asset id or UnitRef unless an explicit identity migration is approved. Filesystem structure is organizational authority, not published identity authority.

#### Scenario: Foundation-owned asset path moves to reusable Assets root
- **WHEN** a published asset moves from a collection-specific legacy path to `Assets/**` with unchanged manifest id and UnitRef
- **THEN** the move is treated as repository-structure migration rather than retirement/republication under a new identity
