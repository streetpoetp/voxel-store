# Spec Delta

## ADDED Requirements

### Requirement: Reusable models share one semantic canonical Unity model frame
All published reusable avatar and equipment models SHALL resolve through Hero.Voxel into one canonical Unity local frame: Right `+X`, Up `+Y`, Forward `-Z`. Under the provider source-to-Unity mapping, semantic Forward is authored toward source `-Y`. Forward SHALL represent semantic facing/action rather than a coordinate-conversion label or geometrically inferred axis.

#### Scenario: Avatar and equipment are composed
- **WHEN** a conforming avatar and conforming equipment asset are baked and composed at a Body-ratio position
- **THEN** both models expose the same canonical local Right, Up, and Forward axes and the equipment placement rotation remains identity

#### Scenario: Equipment semantic front is audited
- **WHEN** equipment is normalized for publication
- **THEN** its semantic Forward is determined from intended game use and authoring evidence per asset, not from its longest dimension, an arbitrary mesh axis, or a blanket transform applied to the equipment catalog

#### Scenario: Projectile equipment is authored
- **WHEN** a bow, crossbow, rifle, or equivalent projectile model has a defined target/launch direction
- **THEN** that launch direction resolves to canonical Unity `-Z` even when doing so makes the canonical front view different from the model's broadest side view

#### Scenario: Directional melee equipment is authored
- **WHEN** a dagger, sword, spear, axe, hammer, pick, scythe, or equivalent melee model has a defined primary thrust/cut/strike direction
- **THEN** that functional action direction resolves to canonical Unity `-Z` even when its neutral broad-side presentation would otherwise suggest a different front

#### Scenario: Equipment is authored with opposite forward
- **WHEN** an equipment source would bake with a semantic forward direction different from canonical Unity `-Z`
- **THEN** the equipment source geometry/pivot SHALL be corrected before publication rather than storing a preview, avatar-specific, equipment-specific, or runtime corrective rotation

#### Scenario: Provider axis conversion is applied
- **WHEN** Hero.Voxel maps source coordinates into Unity coordinates
- **THEN** the store treats that mapping only as coordinate conversion and independently requires the resulting published model to satisfy the semantic canonical Unity model frame

#### Scenario: Canonical front proof is rendered
- **WHEN** Hero.Voxel renders `front.png` for a conforming published model
- **THEN** the camera is on the Unity `-Z` side looking toward `+Z`, so the image means the observer is standing in front of the model looking back at it and therefore proves semantic Forward points toward `-Z`
