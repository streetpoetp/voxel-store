# Design: Simplify gate-driven voxel-store workflow

## Current behavior

The repository expresses the same lifecycle in several places:

- `WORKFLOW.md` describes visual authority, Product Target creation, v2/v3 package flows, density, publication, and maintenance.
- `voxel-store-maintain` repeats style resolution, Product Target rules, construction sequencing, density rules, source generation, proof, and publication.
- durable specs already define the detailed validity and staleness behavior for those same stages.

This creates two problems:

1. **Navigation cost** — the agent must read and reconcile a long sequence before acting.
2. **Drift risk** — stage semantics can diverge because the same rule is restated in several owners.

## Decision 1: Tool owns routing

The deterministic Tool is the source of truth for **what work is next**. Maintenance begins from scan/plan output and claims one asset plus its earliest failing/current gate.

The coordinator does not manually infer a later stage when an earlier gate is unresolved.

Conceptually:

```text
plan
  -> one asset + current gate
  -> execute gate owner
  -> verify
  -> plan/scan same asset again
  -> next gate | blocked | published
```

## Decision 2: Main lifecycle has five stages

The human-facing workflow becomes:

```text
PLAN
  Tool selects one asset + earliest gate

REFERENCE
  Product Target / Composition / Parts as required

SOURCE
  Density when required -> VoxelSpec

PROOF
  Generate -> inspect -> seal

PUBLISH
  Scan -> verify -> compatibility/catalog -> commit/push
```

These are navigation concepts, not new technical gates. Existing detailed gates and validation remain unchanged.

## Decision 3: One owner per kind of knowledge

### `AGENTS.md`

Owns repository invariants only:

- authoritative/derived data boundary;
- Hero.Voxel boundary;
- gameplay-neutral content boundary;
- one asset per run;
- earliest failing gate remains owned by the same asset.

### `WORKFLOW.md`

Owns lifecycle navigation only:

- bootstrap;
- five-stage flow;
- stage owner pointers;
- pass/fail/blocked behavior;
- finalization.

It should not duplicate detailed provenance schemas, density math, fidelity criteria, or Hero.Voxel construction semantics.

### `voxel-store-maintain`

Becomes a coordinator:

1. run plan/scan;
2. claim one asset/current gate;
3. invoke the owner for that gate;
4. validate;
5. repeat for the same asset;
6. stop when published, healthy, or explicitly blocked.

The coordinator may name stage owners, but should not restate their detailed rules.

### `voxel-store-product-target`

Remains specialized because it owns a genuinely different creative operation: image-model generation, visual acceptance, and exact-byte Product Target delivery.

### OpenSpec specs / domain authorities

Remain the durable detailed contract for validity, staleness, density, compatibility, publication, and selected-collection coverage. Current repository authority is split across `Store`, `Collections`, `Profiles`, and `Styles` rather than one generic policy root.

### Hero.Voxel workflow

Remains the canonical source for voxel construction semantics, Composition/Part structure, VoxelSpec authoring constraints, and proof semantics.

## Decision 4: Legacy v2 is an exception, not the primary path

New/redesigned assets follow target-driven v3. The primary workflow does not carry a parallel v2 diagram.

When the Tool identifies an unchanged legacy repair, the coordinator preserves that asset's compatible legacy reference flow without forcing migration. Detailed compatibility behavior remains in specs.

## Decision 5: Pass/fail behavior is uniform

Every stage follows the same rule:

```text
PASS    -> ask Tool for the same asset's next gate
FAIL    -> repair the same gate
BLOCKED -> report evidence and stop
HEALTHY -> stop
```

This removes stage-specific navigation prose from the coordinator.

## Separate follow-up: provider-independent planning

Current `Tool/Directory.Build.props` requires `HERO_STUDIO_ROOT` before the Tool can even build, so `plan` currently inherits a provider dependency even though planning conceptually should be repository-local.

That is a useful follow-up architecture change, but it is intentionally excluded here because it changes Tool dependency boundaries rather than workflow documentation/coordination.

Suggested follow-up change:

`decouple-store-planning-from-hero-voxel-provider`

## Risks

- **Over-thinning the coordinator** could make an agent miss a required owner. Mitigation: each lifecycle stage names its authoritative owner explicitly.
- **Tool output may not expose enough gate detail for every creative substage.** Mitigation: keep existing deterministic gate model unchanged in this change; simplify only navigation around it.
- **Legacy v2 behavior may become less visible.** Mitigation: retain one explicit compatibility-exception rule and keep full behavior in durable specs.

## Validation

- Strict OpenSpec validation.
- `WORKFLOW.md` presents one clear primary lifecycle.
- `voxel-store-maintain` is materially shorter and acts as a coordinator rather than a duplicate specification.
- Product Target, density, compatibility, and Hero.Voxel semantics remain owned by their existing authorities.
- This workflow change itself does not own product/domain migrations; later purpose-driven repository restructuring may move those authorities independently.
- `git diff --check` passes.
