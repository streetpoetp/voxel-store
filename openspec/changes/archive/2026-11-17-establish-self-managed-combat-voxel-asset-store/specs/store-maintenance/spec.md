# Spec Delta

## Purpose

Defines how the voxel asset store continuously derives its own health and coverage, chooses bounded maintenance work, and repairs or expands reusable assets without relying on manually curated status lists.

## ADDED Requirements

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
