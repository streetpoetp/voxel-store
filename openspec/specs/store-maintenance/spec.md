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
Repository policy SHALL declare the desired reusable foundation coverage independently from current generated catalog state. A maintenance scan SHALL derive uncovered required slots from policy and current healthy published assets rather than from a manually maintained TODO list.

#### Scenario: Required equipment coverage is missing
- **WHEN** policy requires a mount-compatible equipment category and no healthy published asset satisfies that requirement
- **THEN** the scan reports a coverage gap eligible for autonomous asset design after higher-priority repairs are resolved

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
Generated health and catalog outputs SHALL identify each asset's current publication state and failed gate when unhealthy, and SHALL expose uncovered policy requirements or the next eligible maintenance target in machine-readable form.

#### Scenario: Asset compatibility gate fails
- **WHEN** a scan detects an equipment/avatar compatibility failure
- **THEN** generated health data identifies the affected assets/profile and the compatibility gate as the reason the store is unhealthy

### Requirement: Tool dependency resolution is explicit and portable
Maintenance SHALL resolve the Hero.Voxel authoring/tool contract through repository configuration or a pinned tool revision/reference. The store SHALL NOT require a hard-coded developer-local Hero Studio checkout path to scan, verify, or maintain assets.

#### Scenario: Repository is moved to another machine
- **WHEN** the store is checked out at a different filesystem location with the declared Hero.Voxel tool dependency available
- **THEN** maintenance resolves the declared tool contract without requiring the original developer-local checkout path

#### Scenario: Declared Hero.Voxel tool cannot be resolved
- **WHEN** maintenance requires Hero.Voxel verification but the declared tool contract is unavailable or incompatible
- **THEN** the run fails closed with a dependency-resolution error and does not mark affected assets as healthy

### Requirement: Maintenance resolves style authority before reference generation
Before reference generation/repair, maintenance SHALL resolve the style inputs applicable to the selected asset kind and the pinned Hero.Voxel workflow. It SHALL accept the Composition Sheet first, then one Part Sheet at a time. Equipment jobs SHALL treat accepted reference authority as upstream of source/proof and SHALL stop on unresolved style authority or visual audit failure.

#### Scenario: Maintenance begins reference generation
- **WHEN** the selected asset job requires a new or repaired reference package and its applicable scoped style-reference set resolves successfully
- **THEN** maintenance conditions generation on that scoped style authority while following Hero.Voxel's canonical Composition-first sequential Part Sheet workflow

#### Scenario: Style authority cannot be resolved
- **WHEN** any applicable scoped style-reference image is unavailable or the asset's style family cannot resolve the required asset-kind reference set
- **THEN** maintenance fails closed on the current asset before generating or accepting replacement construction sheets

#### Scenario: Generated sheet fails visual audit
- **WHEN** the Composition Sheet or current Part Sheet fails either the active scoped-style audit or canonical Hero.Voxel construction audit
- **THEN** maintenance repairs/regenerates that same sheet for the current asset and does not advance to another Part Sheet or asset

#### Scenario: Equipment reference package is accepted
- **WHEN** an equipment Composition Sheet and every required equipment Part Sheet pass visual audit
- **THEN** maintenance may author or repair that equipment's source against the accepted reference package and subsequently run generation, proof, and compatibility gates

#### Scenario: Existing equipment source disagrees with accepted reference authority
- **WHEN** an equipment migration/repair finds that existing VoxelSpec or proof output materially disagrees with the newly accepted visual reference package
- **THEN** maintenance repairs the equipment source to match reference authority rather than rewriting the reference package merely to preserve the existing model

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
