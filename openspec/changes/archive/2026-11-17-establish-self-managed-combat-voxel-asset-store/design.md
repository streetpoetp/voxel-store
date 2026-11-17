# Design

## Context

`voxel-store` is currently a greenfield Git repository. OpenSpec is now initialized, but there is no application/tooling implementation or existing asset convention to preserve. See `proposal.md` for motivation and the three capability specs for required behavior.

Hero Studio already provides the external voxel technology this repository must reuse: fixed-unit sources may live outside Unity, production identity uses immutable semantic `UnitRef`, generated Hero.Voxel siblings stay beside the source base, reference packages freeze construction intent before VoxelSpec authoring, and baked assemblies expose opaque sockets whose animated world frames can drive independently rendered assemblies. The store therefore needs source/library lifecycle and compatibility management, not another voxel compiler, Unity runtime, or renderer.

## Goals / Non-Goals

**Goals:**

- Make repository authority explicit so an agent can distinguish editable intent from reproducible/generated state.
- Make one avatar family reusable with multiple independently authored equipment assets through physical mount contracts.
- Separate deterministic scanning/validation/catalog logic from creative agent decisions and visual audits.
- Make a maintenance run self-directed but bounded to one asset job so failures are attributable and recoverable.
- Keep Hero.Voxel dependency resolution portable and fail closed when the expected authoring contract cannot be resolved.
- Prove the architecture with a small combat-foundation set before policy permits wider autonomous expansion.

**Non-Goals:**

- No duplicate VoxelSpec parser/compiler, MagicaVoxel writer, Unity baker, runtime asset format, streaming system, or renderer.
- No game-specific equipment stats, inventory rules, combat classes, AI, rarity, economy, progression, or balance metadata.
- No precomposed `AvatarWithEquipment` source variants and no per-avatar/per-equipment correction table.
- No automatic invention of reference-package construction semantics that the canonical Hero.Voxel workflow requires an agent to resolve and audit.
- No broad initial asset library; V1 proves the lifecycle and compatibility foundation first.

## Decisions

### 1. Split repository state into authoritative, reproducible, evidence, and derived layers

Use four ownership classes:

```text
authoritative
  policy/**
  toolchain/**
  Art/**/Palette.yaml
  Art/**/asset.yaml
  Art/**/Reference/** accepted construction authority
  Art/**/Source/*.voxel.yaml
  optional editable *.vox / *.animation.yaml overrides

reproducible machine-owned
  *.generated.vox
  *.animation.generated.yaml

evidence
  Art/**/Proof/**

derived machine-owned
  catalog/catalog.generated.json
  catalog/health.generated.json
```

The scanner always rebuilds decisions from authoritative inputs plus current verification evidence. Reproducible and derived files can be refreshed, but they never become the only authority for identity, compatibility, or coverage.

**Why:** this directly supports self-management without allowing an agent to mistake its previous outputs for source truth.

**Alternative considered:** one large generated database containing all asset metadata and state. Rejected because deleting or corrupting that database would destroy authoring authority and make autonomous repair circular.

### 2. Use a canonical content layout that preserves Hero.Voxel sibling ownership

The initial repository layout is:

```text
AGENTS.md
WORKFLOW.md

policy/
  store.yaml
  coverage.yaml
  families/
    humanoid.medium.v1.yaml

toolchain/
  hero-voxel.lock.yaml

Art/
  CombatFoundation/
    Palette.yaml
    Units/
      Avatars/<AssetName>/
        asset.yaml
        Reference/
        Source/<AssetName>.voxel.yaml
        Proof/
      Equipment/<AssetName>/
        asset.yaml
        Reference/
        Source/<AssetName>.voxel.yaml
        Proof/

catalog/
  catalog.generated.json
  health.generated.json

Tool/
  ... standalone store scanner/validator CLI ...

.agents/skills/
  voxel-store-maintain/
```

Generated Hero.Voxel siblings are written beside `Source/<AssetName>.voxel.yaml`, matching the external Hero.Voxel ownership contract. The store does not introduce `Generated/` as an alternate source convention.

**Alternative considered:** put all generated VOX under a central build directory. Rejected because it conflicts with canonical Hero.Voxel source/effective-source ownership and makes external tooling less direct.

### 3. Keep store manifests small and library-specific

Each `asset.yaml` owns only store concerns required to locate and classify an asset. The conceptual record is:

```yaml
id: foundation.avatar.humanoid.guard-01
kind: avatar
unitRef: voxel-store/avatar/humanoid/guard-01
family: humanoid.medium.v1
source:
  base: ./Source/Guard01
  palette: ../../../Palette.yaml
style:
  family: combat-foundation.v1
```

Equipment adds its mount declaration:

```yaml
id: foundation.equipment.blade.basic-01
kind: equipment
unitRef: voxel-store/equipment/blade/basic-01
mount:
  profile: humanoid.medium.v1
  socket: Hand.R
source:
  base: ./Source/Blade01
  palette: ../../../Palette.yaml
```

Publication status is intentionally absent. It is derived by verification. Manifest parsing rejects unknown store-authority gameplay fields rather than growing into a game Config system.

**Alternative considered:** encode all proof paths/status flags and gameplay tags directly in the manifest. Rejected because proof locations are derivable from the asset root, status becomes stale, and gameplay semantics would destroy cross-game reuse.

### 4. Compatibility profiles define mount frames; assets do not own pair corrections

`policy/families/<profile>.yaml` defines a versioned physical contract. V1 uses `humanoid.medium.v1` with required sockets sufficient to prove independent equipment composition:

```text
Hand.R
Hand.L
Back
```

The profile also defines the expected socket-axis convention, scale/VPU expectations, mount envelope, and representative pose/animation samples used by compatibility proof. Optional future sockets such as `Head` or `Chest` can be declared without making them required for V1.

Avatars own socket realization. Equipment owns a local canonical mount frame; for equipment VoxelSpecs, the selected `assemblyOrigin`/geometry orientation is authored so the equipment assembly can be placed directly at the avatar's evaluated socket frame. No `avatar X equipment -> correction transform` record is permitted.

At validation time, all current published candidates within a profile are composed for the required poses. A new avatar therefore proves it works with current equipment, and new equipment proves it works with current avatars.

**Alternative considered:** permit per-pair corrective transforms. Rejected because it creates an N x M maintenance surface and makes profile compatibility meaningless.

### 5. Separate deterministic store tooling from agent-owned creative work

Create a standalone repository CLI under `Tool/` with no Unity dependency. A small .NET tool is preferred because the external Hero.Voxel authoring tool is already .NET-based and the store can share the same cross-platform process model without introducing a second scripting runtime requirement.

The deterministic tool owns:

- manifest/policy/profile parsing and schema validation;
- stable-id/`UnitRef` uniqueness and immutable-publication checks against repository history where required;
- source-layout and generated-sibling checks;
- Hero.Voxel tool invocation and deterministic artifact verification;
- proof/evidence presence and machine-checkable report validation;
- profile/socket/mount compatibility checks and composition-report generation;
- asset budgets;
- derived health, catalog, coverage, and next-job planning data.

The agent skill owns:

- selecting the single planned repair/design job emitted by the deterministic scan;
- visual concept decisions within policy;
- invoking the canonical Hero.Voxel reference-package workflow one sheet at a time;
- auditing visual construction/reference/preview evidence;
- authoring or repairing VoxelSpec/palette/manifests within the selected job;
- re-running deterministic gates until the job passes or has a concrete blocking failure.

The skill must not copy the semantic rules of `voxel-art-workflow.md`; it resolves and reads the pinned Hero Studio authority before reference or source authoring.

**Alternative considered:** let the agent inspect the filesystem and decide all health/status itself. Rejected because status/catalog results would be non-deterministic, difficult to test, and vulnerable to stale conversational state.

### 6. Resolve Hero.Voxel through a pinned external contract, never a checked-in absolute path

`toolchain/hero-voxel.lock.yaml` records the expected Hero Studio/Hero.Voxel source revision or contract fingerprint used by the store. Machine-local resolution uses an explicit environment/configured tool root (for example a `HERO_STUDIO_ROOT` environment value) or an already built compatible tool executable. The checked-in lock never contains a developer-local absolute Hero Studio path.

Before compile/preview/reference work, maintenance verifies that the resolved tool/workflow matches the lock. If it cannot prove compatibility, affected gates fail closed.

**Alternative considered:** copy Hero.Voxel Tool and the reference workflow into this repository. Rejected because the store would drift into a second implementation and would no longer consume the canonical provider contract.

### 7. Publication is a gate graph with human-readable failure ownership

For an agent-authored asset, publication gates run in ownership order:

```text
manifest / identity
      -> reference Composition Sheet audit
      -> RequiredParts freeze
      -> one Part Sheet per RequiredPart + audit
      -> VoxelSpec / palette authoring
      -> deterministic Hero.Voxel generation
      -> isolated-part proof
      -> assembled proof
      -> source budgets
      -> profile conformance / family composition proof (when applicable)
      -> derived published state
```

A later gate never compensates for an earlier failure. Health output records a stable gate id plus affected asset/profile/pair and evidence paths so the maintenance agent can repair the earliest owner.

**Alternative considered:** a single boolean validation result. Rejected because autonomous repair needs causal ownership, not just pass/fail.

### 8. Maintenance uses a deterministic failure-first planner and one-job execution boundary

The scan emits ordered maintenance candidates. Priority classes are:

1. invalid authoritative manifests/policies/identity;
2. regressed or missing evidence for previously published foundation assets;
3. failed compatibility among current profile members;
4. stale/missing reproducible or derived outputs;
5. required coverage gaps;
6. optional expansion, only when policy explicitly allows it.

One `$voxel-store:maintain`-style invocation claims only the first eligible asset-scoped job. It may iterate on that same job until healthy or blocked, but it does not silently advance to a second asset. A healthy/no-gap scan is a valid no-op result.

**Alternative considered:** fill every discovered gap in one autonomous run. Rejected because long multi-asset sessions make visual failures, partial completion, and rollback difficult to reason about.

### 9. V1 proves one family across all required mount types before broad expansion

`CombatFoundation.v1` starts with one `humanoid.medium.v1` avatar plus representative independent equipment that exercises `Hand.R`, `Hand.L`, and `Back`. Exact visual designs are agent-authored from the store's art policy; they are not gameplay classes. The first milestone is complete publication and cross-composition evidence for this bounded set, followed by a second maintenance scan that derives no unplanned work except policy-declared future coverage.

Only after this proof should `coverage.yaml` be expanded to request more avatars, equipment families, or compatibility profiles.

**Alternative considered:** immediately generate a broad catalog of avatars and weapons. Rejected because content volume would obscure whether self-management and compatibility contracts actually work.

## Risks / Trade-offs

- **[Risk] Strict family compatibility makes one new avatar invalidate many equipment relationships.** -> Keep profiles physically narrow/versioned, prove V1 with a small set, and require a new profile version when physical semantics genuinely diverge.
- **[Risk] All-pairs composition proof grows quadratically inside large families.** -> V1 keeps families small; retain the strong contract first, then optimize proof caching/incremental invalidation without weakening observable compatibility semantics.
- **[Risk] Visual correctness cannot be reduced to deterministic structural checks.** -> Keep visual audits explicitly agent-owned, persist evidence, and never infer visual PASS merely from successful compilation.
- **[Risk] External Hero.Voxel changes can invalidate store assumptions.** -> Pin the expected contract/revision, fail closed on mismatch, and make intentional toolchain upgrades a separate reviewed change.
- **[Risk] Committed generated VOX/proof evidence increases repository size.** -> Keep only canonical generated siblings and publication evidence required by policy; do not generate redundant precomposed avatar/equipment assets.
- **[Risk] A self-managing agent could expand content indefinitely.** -> Coverage policy is authoritative, optional expansion is disabled unless declared, and every invocation is capped to one asset job.

## Migration Plan

This is a greenfield repository, so there is no data migration.

1. Add repository workflow/instruction files, deterministic Tool project, policy/toolchain schemas, and empty derived-output locations.
2. Add `CombatFoundation.v1`, `humanoid.medium.v1`, and bounded coverage requirements.
3. Implement scan/validation/catalog/maintenance-plan behavior and prove healthy-store idempotence before generating art.
4. Author and publish the canonical avatar through the pinned Hero.Voxel reference/source pipeline.
5. Author representative independent equipment for all required V1 mounts and prove family composition, including required animated poses.
6. Regenerate catalog/health, run maintenance again, and require the bounded foundation to report healthy with no unexpected job selection.

Rollback is repository-level because no external runtime state exists: revert the change's files/commit. Published semantic identities must not be silently reused for incompatible replacement assets in later attempts.
