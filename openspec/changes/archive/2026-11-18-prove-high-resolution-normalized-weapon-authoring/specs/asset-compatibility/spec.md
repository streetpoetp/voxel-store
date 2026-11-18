# Spec Delta

## ADDED Requirements

### Requirement: Equipment authoring density is independent of avatar authoring density
Equipment that declares a valid compatibility profile SHALL be allowed to use a different positive `voxelsPerUnit` from the avatar/profile authoring density when its canonical assembly origin and mount-frame behavior remain physically compatible after normalization to world space.

#### Scenario: High-density equipment mounts to a lower-density avatar
- **WHEN** equipment uses a higher positive `voxelsPerUnit` than a conforming avatar but preserves its canonical mount frame and normalized physical scale
- **THEN** compatibility validation accepts the density difference and evaluates the ordinary profile/socket evidence

#### Scenario: Equipment has no valid normalized mount frame
- **WHEN** equipment uses any `voxelsPerUnit` but lacks a canonical assembly origin or cannot satisfy the declared mount-frame evidence
- **THEN** compatibility validation fails regardless of authoring density
