# Spec Delta

## ADDED Requirements

### Requirement: High-resolution source authoring remains explicitly bounded
The store SHALL permit equipment to use high-resolution voxel source geometry when it remains within the declared source-voxel and part budgets, is deterministically generated, and satisfies all ordinary reference, proof, compatibility, and publication gates.

#### Scenario: High-resolution equipment is within the declared budget
- **WHEN** an equipment asset uses a higher authoring density but its occupied source voxels and rigid-part count remain within the active store budget
- **THEN** publication does not reject the asset solely because its source voxel density is higher than the avatar's

#### Scenario: High-resolution equipment exceeds the declared source budget
- **WHEN** an equipment asset's occupied source voxels exceed the active store budget
- **THEN** the publication budget gate fails with the measured occupied-voxel count even if runtime greedy meshing would reduce mesh complexity
