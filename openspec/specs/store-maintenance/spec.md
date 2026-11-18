# store-maintenance Specification

## Purpose

Defines how the voxel asset store continuously derives its own health and coverage, chooses bounded maintenance work, and repairs or expands reusable assets without relying on manually curated status lists.

## Requirements

### Requirement: Store state is derived by scanning authoritative inputs
The maintenance workflow SHALL scan authoritative manifests, policies, source/reference files, generated artifacts, and verification evidence to derive current asset health, compatibility health, publication state, and coverage gaps. It SHALL NOT trust generated catalog/health outputs as authoritative inputs to those decisions.

#### Scenario: Store scan runs with stale generated outputs
- **WHEN** generated catalog or health files are missing or stale but authoritative repository inputs are valid
- **THEN** the scan derives current state from authoritative inputs and can regenerate the derived outputs

### Requirement: Maintenance prioritizes broken published foundations before expansion
The maintenance workflow SHALL rank unhealthy or regressed published foundation assets ahead of creating new coverage. Missing required generated artifacts, failed verification, broken compatibility, and invalid manifests SHALL be treated as repair work before optional expansion gaps.

#### Scenario: Broken asset and uncovered slot coexist
- **WHEN** the store contains a failed published asset and also has an uncovered optional content slot
- **THEN** the next maintenance selection targets the failed published asset before generating a new asset for the optional slot

### Requirement: Coverage policy drives autonomous expansion
Repository policy SHALL declare desired reusable foundation coverage independently from current generated catalog state and MAY distinguish equipment requirements by gameplay-neutral physical form. A maintenance scan SHALL derive uncovered required slots from policy and current healthy published assets using every declared coverage discriminator rather than from a manually maintained TODO list.

#### Scenario: Required equipment coverage is missing
- **WHEN** policy requires equipment for a compatibility profile, logical mount capability, and physical form and no healthy published asset matches all declared discriminators
- **THEN** the scan reports that exact coverage gap as eligible for autonomous asset design after higher-priority repairs are resolved

#### Scenario: Different form does not satisfy requirement
- **WHEN** a healthy published equipment asset matches the required profile and mount capability but declares a different physical form
- **THEN** that asset does not satisfy the form-specific coverage requirement

### Requirement: One maintenance run owns one bounded asset job
An autonomous maintenance invocation SHALL select at most one asset-scoped design or repair job and carry that job through the applicable reference, source, generation, proof, compatibility, and publication gates. It SHALL stop with explicit evidence when the selected job cannot safely advance.

#### Scenario: Healthy store has one highest-priority gap
- **WHEN** a scan finds no higher-priority failures and selects one uncovered required asset slot
- **THEN** the maintenance run works only that asset job through its applicable gates and leaves other uncovered slots for later runs

#### Scenario: Selected job fails a gate
- **WHEN** the active asset fails a reference, generation, visual, budget, or compatibility gate
- **THEN** the run records the failing evidence and stops or repairs that same selected asset rather than silently skipping to another asset

### Requirement: Maintenance is idempotent when the store is healthy
Running maintenance against unchanged authoritative inputs with all required coverage healthy SHALL not invent new assets, mutate published identities, or create semantic changes merely to produce activity.

#### Scenario: No maintenance work is required
- **WHEN** current authoritative inputs satisfy all required health and coverage policy
- **THEN** the maintenance workflow reports the store as healthy, regenerates derived outputs only when needed, and performs no asset design or repair mutation

### Requirement: Derived outputs explain health and next work
Generated health and catalog outputs SHALL identify each asset's current publication state and failed gate when unhealthy, and SHALL expose uncovered policy requirements or the next eligible maintenance target with every active coverage discriminator in machine-readable form.

#### Scenario: Asset compatibility gate fails
- **WHEN** a scan detects an equipment/avatar compatibility failure
- **THEN** generated health data identifies the affected assets/profile and the compatibility gate as the reason the store is unhealthy

#### Scenario: Physical-form coverage is missing
- **WHEN** a coverage requirement is unmet because its required physical form has no healthy published match
- **THEN** generated health data identifies that form together with the relevant compatibility profile and logical mount capability

### Requirement: Tool dependency resolution is explicit and portable
Maintenance SHALL resolve the Hero.Voxel authoring/tool contract through repository configuration or a pinned tool revision/reference. The store SHALL NOT require a hard-coded developer-local Hero Studio checkout path to scan, verify, or maintain assets.

#### Scenario: Repository is moved to another machine
- **WHEN** the store is checked out at a different filesystem location with the declared Hero.Voxel tool dependency available
- **THEN** maintenance resolves the declared tool contract without requiring the original developer-local checkout path

#### Scenario: Declared Hero.Voxel tool cannot be resolved
- **WHEN** maintenance requires Hero.Voxel verification but the declared tool contract is unavailable or incompatible
- **THEN** the run fails closed with a dependency-resolution error and does not mark affected assets as healthy

### Requirement: Maintenance resolves style authority before reference generation
Before reference generation/repair, maintenance SHALL resolve the applicable style inputs and pinned Hero.Voxel workflow. For unchanged legacy v2 assets it may preserve the existing Composition-first flow. For new or intentionally redesigned target-driven assets it SHALL establish/accept Product Target first, then accept the Composition Sheet, then one Part Sheet at a time, and SHALL stop on unresolved style/target authority or visual-fidelity failure.

#### Scenario: Maintenance begins reference generation
- **WHEN** the selected asset job requires a new or repaired reference package and its applicable scoped style-reference set resolves successfully
- **THEN** maintenance follows the reference flow required by that package version, using Product Target first for target-driven v3 and Composition-first for unchanged legacy v2

#### Scenario: Style authority cannot be resolved
- **WHEN** any applicable scoped style-reference image is unavailable or the asset's style family cannot resolve its required authority
- **THEN** maintenance fails closed on the current asset before generating or accepting replacement construction sheets

#### Scenario: Generated sheet fails visual audit
- **WHEN** the current Product Target, Composition Sheet, or Part Sheet fails the applicable style, target-fidelity, or Hero.Voxel construction audit
- **THEN** maintenance repairs/regenerates that same evidence for the current asset and does not advance to another sheet or asset

#### Scenario: Equipment reference package is accepted
- **WHEN** an equipment package satisfies all visual/reference requirements of its declared contract version
- **THEN** maintenance may author or repair downstream density/source against the accepted authority and subsequently run generation, proof, and compatibility gates

#### Scenario: Maintenance begins legacy reference repair
- **WHEN** an unchanged v2 asset requires reference repair and its applicable scoped style-reference set resolves successfully
- **THEN** maintenance may follow the existing Composition-first sequential Part Sheet workflow without forcing target-driven migration

#### Scenario: Maintenance begins target-driven reference generation
- **WHEN** a new or intentionally redesigned asset requires a target-driven reference package and its applicable scoped style-reference set resolves successfully
- **THEN** maintenance first establishes and visually accepts the Product Target, then generates/audits Composition and Part Sheets against that target while following Hero.Voxel construction rules

#### Scenario: Style or Product Target authority cannot be resolved
- **WHEN** applicable style inputs are unavailable, Product Target provenance is unresolved, or the target itself fails visual acceptance
- **THEN** maintenance fails closed on the current asset before accepting downstream construction sheets

#### Scenario: Generated Composition Sheet fails target fidelity
- **WHEN** the Composition Sheet is structurally valid but materially simplifies the current Product Target's silhouette, proportions, construction richness, material/color hierarchy, or meaningful detail density
- **THEN** maintenance repairs/regenerates that Composition Sheet for the current asset and does not advance to Part Sheets, density, source, or another asset

#### Scenario: Generated Part Sheet fails target fidelity
- **WHEN** the current Part Sheet fails either target-fidelity or canonical Hero.Voxel construction audit
- **THEN** maintenance repairs/regenerates that same Part Sheet and does not advance to another part, source, or asset

#### Scenario: Target-driven equipment reference package is accepted
- **WHEN** Product Target, Composition Sheet, every required Part Sheet, target-fidelity evidence, and style acceptance are current
- **THEN** maintenance may advance to density selection when applicable and subsequently to source, generation, proof, and compatibility gates

#### Scenario: Existing equipment source disagrees with accepted reference authority
- **WHEN** an equipment migration/repair finds that existing VoxelSpec or proof output materially disagrees with the newly accepted Product Target/construction package
- **THEN** maintenance repairs downstream density/source/proof to match visual authority rather than simplifying the Product Target or construction sheets to preserve the existing model

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

### Requirement: Combat foundation provides ten distinct general catalog forms
`CombatFoundation.v1` SHALL require ten independently published equipment forms as declared by active coverage policy. Each required form SHALL remain a reusable physical classification without assigning side-specific consumer behavior.

#### Scenario: General catalog is complete
- **WHEN** one healthy published equipment asset satisfies each of the ten required physical forms under the active compatibility profile
- **THEN** the foundation reports no general catalog coverage gap

#### Scenario: One required form is missing
- **WHEN** nine required forms are satisfied but no healthy published asset satisfies the remaining form
- **THEN** maintenance reports that exact form as the next eligible required coverage gap after higher-priority repairs

### Requirement: Maintenance resolves reference density before equipment source authoring
After a `density: reference-driven` equipment reference package is accepted, autonomous maintenance SHALL resolve current density evidence for that same asset before creating or repairing its VoxelSpec source. Density failure SHALL remain the active asset's blocking work. Legacy equipment without the opt-in keeps the previous maintenance flow until migrated.

#### Scenario: Equipment reference package becomes accepted
- **WHEN** maintenance completes visual acceptance for a `density: reference-driven` equipment Composition Sheet and every required Part Sheet
- **THEN** the next authoring step for that asset is density analysis/selection rather than immediate VoxelSpec authoring

#### Scenario: Legacy equipment has not opted into density selection
- **WHEN** maintenance encounters equipment without `density: reference-driven`
- **THEN** it preserves the previous reference-to-source maintenance sequence and does not require density evidence

#### Scenario: Density selection succeeds
- **WHEN** current density evidence selects a VPU for the active equipment asset
- **THEN** maintenance authors or repairs that asset's source using the selected VPU and continues through ordinary generation/proof/publication gates

#### Scenario: Density selection fails
- **WHEN** no candidate VPU preserves all declared meaningful features within the hard constraints
- **THEN** maintenance records density as the blocking gate for that same asset and does not silently choose a different VPU or advance to another asset

#### Scenario: Existing equipment reference changes
- **WHEN** a published equipment Part Sheet changes and invalidates its previous density evidence
- **THEN** maintenance recomputes density before repairing source even if an older VoxelSpec remains compilable
