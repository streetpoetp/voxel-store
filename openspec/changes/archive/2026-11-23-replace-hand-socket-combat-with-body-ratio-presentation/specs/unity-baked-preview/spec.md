# Spec Delta

## ADDED Requirements

### Requirement: Preview composes equipment from canonical Body bounds ratios
The Unity preview SHALL be able to build a derived composition containing a published rigid `Body` avatar and one or more published equipment assets whose local positions are resolved from canonical Body local bounds and normalized offset ratios.

#### Scenario: Single equipment composition is requested
- **WHEN** preview proof selects one published rigid-body avatar, one published equipment asset, and a valid normalized offset ratio
- **THEN** the preview derives the equipment local position from canonical Body bounds without requiring an avatar socket or mount transform

#### Scenario: Multiple equipment composition is requested
- **WHEN** preview proof selects multiple published equipment assets with distinct ratio placements
- **THEN** all equipment instances are composed independently under the same derived presentation root

### Requirement: Preview proves canonical equipment frame behavior
The Unity preview SHALL preserve each equipment asset's canonical local origin/orientation when applying Body-ratio placement and SHALL expose composition failures without storing pair-specific corrective transforms.

#### Scenario: Canonical equipment frame composes correctly
- **WHEN** a published equipment asset is placed against the required Body-ratio evidence set
- **THEN** its derived Unity presentation retains the canonical equipment frame and the evidence identifies the evaluated placement

#### Scenario: Composition needs a pair-specific correction
- **WHEN** the preview would require an avatar/equipment-specific correction to look acceptable
- **THEN** proof reports the composition as failing rather than persisting the correction as preview or store authority

### Requirement: Preview captures dense deterministic presentation trajectories
The preview proof workflow SHALL support deterministic temporal sampling of a unified Body-plus-equipment presentation trajectory at enough ordered normalized times and views to reveal discontinuities, reversals, hidden equipment, and weak body motion.

#### Scenario: Presentation motion is evaluated
- **WHEN** a presentation proof is captured for a combat or feedback motion
- **THEN** the preview emits ordered samples covering ready, anticipation, strike/contact, follow-through, recovery, and intermediate trajectory states from deterministic views

#### Scenario: Key poses hide a trajectory defect
- **WHEN** sparse key-pose samples appear acceptable but intermediate temporal samples reveal a discontinuity or reversal
- **THEN** the dense capture evidence remains available to classify the presentation as visually failing

### Requirement: Presentation proof remains derived and non-authoritative
Body-ratio composition settings used only to exercise preview scenarios, DOTween sequences used only to demonstrate presentation motion, and generated captures SHALL remain disposable Editor evidence and SHALL NOT become voxel-store gameplay or simulation authority.

#### Scenario: Preview uses a DOTween demonstration sequence
- **WHEN** the Editor preview drives the derived presentation root through a demonstration motion
- **THEN** the sequence affects only preview presentation evidence and does not define attack results, damage timing, simulation position, or published asset identity

#### Scenario: Capture outputs are removed
- **WHEN** generated Body/equipment captures and reports are deleted
- **THEN** they can be recreated from current published assets and preview scenario inputs without changing store source authority
