# Spec Delta

## ADDED Requirements

### Requirement: Shared style grammar does not implicitly own asset-specific visual identity
Shared style authority SHALL define reusable visual grammar and the family quality bar. Unless active style authority explicitly declares a palette, material hierarchy, accent family, decorative motif, or equivalent visual choice as a family-wide invariant, those choices SHALL remain asset-specific and SHALL be established by asset intent and the accepted Product Target. An agent SHALL NOT turn incidental choices from one or more style exemplars into undeclared global requirements.

#### Scenario: Two equipment assets share grammar but use different palettes
- **WHEN** two equipment Product Targets both preserve the active style's chunky low-resolution geometry, readable silhouette, detail restraint, and construction-quality language but use different coherent palettes and material hierarchies
- **THEN** both may satisfy the same shared style authority if no active style rule explicitly requires one common palette or material recipe

#### Scenario: Equipment exemplar contains recurring green wraps and brown wood
- **WHEN** an equipment style exemplar uses green wraps, brown wood, and gold accents but those choices are not declared family-wide invariants
- **THEN** a new equipment asset may use a different coherent grip material, palette, accent color, and motif while preserving the shared voxel grammar and quality bar

#### Scenario: Style authority explicitly fixes a family-wide visual invariant
- **WHEN** active style authority explicitly declares a visual property as global rather than merely depicting it in an exemplar
- **THEN** generated Product Targets preserve that declared invariant while remaining free to establish other asset-specific visual identity choices
