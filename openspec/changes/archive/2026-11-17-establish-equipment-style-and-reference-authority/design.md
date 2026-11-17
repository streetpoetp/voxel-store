# Design

## Context

See `proposal.md` for motivation. The current `ReferenceStyleManifest` has one flat `references: List<string>`, and every asset seals the same style manifest plus all three current character PNG hashes into reference evidence. The existing three equipment assets already use ReferenceContract v2 and canonical Hero.Voxel Composition/Part Sheets, so the sheet architecture itself does not need an equipment variant.

The current reference evidence was migrated from existing Hero.Voxel proof. That was valid as migration reconstruction, but the steady-state equipment flow should make accepted visual reference authority upstream of VoxelSpec/proof and keep compatibility as a later gate.

## Goals / Non-Goals

**Goals:**

- Preserve one `low-resolution-voxel.v1` visual family while giving avatars and equipment independent concrete style examples.
- Resolve only style references applicable to the selected asset kind.
- Make provenance invalidation precise: equipment-only style changes stale equipment, avatar-only changes stale avatars, shared traits/common references stale both.
- Keep equipment reference authoring on the existing canonical Hero.Voxel Composition/Part Sheet grammar.
- Re-establish the current three equipment assets from style-first reference authority without changing stable identity or compatibility contracts.

**Non-Goals:**

- Do not create a separate equipment reference-sheet format or equipment-specific Hero.Voxel compiler path.
- Do not encode exact pivots, assembly origin, sockets, mount transforms, or pair correction data into reference images.
- Do not create style families per weapon/equipment category.
- Do not change `styleFamily: combat-foundation.v1`, `referenceStyle: low-resolution-voxel.v1`, UnitRefs, mount profiles, or gameplay semantics.
- Do not regenerate the three approved character style PNGs.

## Decisions

### 1. Evolve the style manifest to scoped references while keeping one style-family identity

Use a v2 manifest shaped conceptually as:

```yaml
version: 2
id: low-resolution-voxel.v1
references:
  common: []            # optional; omit or leave empty when no neutral image authority exists
  avatar:
    - reference-01.png
    - reference-02.png
    - reference-03.png
  equipment:
    - equipment-reference-01.png
    - equipment-reference-02.png
    - equipment-reference-03.png
traits:
  - chunky low-resolution voxel forms
  - simplified readable geometry
  - restrained detail density
  - clear separated color regions
  - high silhouette readability
```

The current character references move unchanged into `avatar`. New equipment examples populate `equipment`. `common` image references are optional; the top-level traits are always shared grammar. This avoids inventing a synthetic common image merely to satisfy schema.

**Alternative considered:** separate `low-resolution-avatar.v1` and `low-resolution-equipment.v1`. Rejected because they are one visual language and should share identity/traits.

**Alternative considered:** keep the three character images in a common scope. Rejected because it would continue coupling equipment acceptance to avatar-only visual examples.

### 2. Resolve style authority by asset kind

Change style resolution from `ResolveReferenceStyle(styleId)` to an asset-aware form that resolves:

```text
applicable references =
  optional common references
  + references[asset.kind]
```

The matching kind scope is mandatory and non-empty for supported authored kinds. Unknown kinds fail closed. Duplicate paths across applicable scopes are rejected.

The resulting authority object keeps the style id, shared traits, applicable scope identity, and only the resolved applicable PNG paths.

### 3. Hash a semantic applicable-style projection, not the whole manifest file

The current audit hashes the raw style manifest plus all reference images. That cannot support scoped invalidation because changing an unrelated scope changes the manifest hash.

Reference audit evidence should instead bind to:

- style family id;
- shared ordered traits;
- selected asset kind;
- ordered optional common reference paths;
- ordered selected-kind reference paths;
- hashes of only those applicable PNG files.

Serialize that semantic projection deterministically and store a `StyleAuthoritySha256` (or equivalent stable digest) alongside the applicable PNG hashes. Do not hash the raw full manifest as the authority digest.

This yields the intended invalidation behavior:

```text
shared traits/common change -> avatar + equipment stale
avatar scope change        -> avatar stale only
equipment scope change     -> equipment stale only
```

**Alternative considered:** split each scope into a separate manifest file and continue hashing files. Rejected as unnecessary file/ownership complexity for the current store.

### 4. Equipment style references are style-only examples, not construction sheets

Add repository-owned equipment style examples under the existing style directory. They should demonstrate the current foundation's representative physical form factors without defining runtime/game semantics:

- elongated hand-held item;
- broad hand-held item;
- back-mounted volume.

These examples define low-resolution voxel silhouette, section thickness, proportion language, readable grip/attachment-region treatment, material/color blocking, and detail density. They do not need to follow the Hero.Voxel Composition/Part Sheet layout because they are style authority only.

The three current V1 items provide the grounded form-factor coverage (RightBlade01, LeftGuard01, BackPack01), but the style examples should remain generic visual examples rather than carry those stable asset identities.

### 5. Keep one canonical equipment reference grammar

Do not add an equipment-specific reference contract. Each equipment asset continues to use:

```text
Composition Sheet
  -> freezes RequiredParts

one Part Sheet / RequiredPart
  -> Local Hero + six orthographics + geometry-subpart breakdown
```

A rigid equipment item can correctly remain `RequiredParts: [Item]`. Visual subparts such as grip/guard/blade or straps/body/flap remain labels inside the Item Part Sheet unless they independently move, pose, or attach.

### 6. Make the steady-state equipment flow style-first

For new design or intentional redesign:

```text
shared traits + equipment style refs
              |
              v
      equipment visual intent
              |
              v
       Composition Sheet
              |
              v
         Part Sheet(s)
              |
              v
           VoxelSpec
              |
              v
      Hero.Voxel proof
              |
              v
    compatibility proof
```

The reference package may communicate visual attachment intent such as a grip/contact region or back-facing attachment surface. Exact pivot, `assemblyOrigin`, socket/mount transform, and compatibility math remain downstream authoring/compatibility decisions.

### 7. Treat current V1 equipment as an explicit migration/recovery case

The current items have no earlier independent visual concepts, so their existing source/proof may be consulted to preserve recognizable identity while creating new style-first references. They must not be treated as immutable design truth.

For each item, one bounded migration run should:

1. resolve shared traits + equipment style references;
2. establish compact equipment intent and target bounds;
3. generate/audit only the Composition Sheet and freeze `RequiredParts`;
4. generate/audit one Part Sheet at a time;
5. compare accepted references to current VoxelSpec/proof;
6. if materially different, repair source to the accepted reference instead of rewriting the reference to preserve old geometry;
7. regenerate proof, compatibility, and publication evidence.

The avatar reference package does not need visual regeneration. Its audit is resealed once against the new avatar-scoped provenance after manifest v2 migration.

## Risks / Trade-offs

- **[Risk] Scoped provenance changes the audit schema and initially stales every asset.** -> Migrate the style manifest and resolver first, then reseal the unchanged avatar against avatar scope and migrate equipment one item at a time.
- **[Risk] Equipment examples become category-specific design authority.** -> Keep them generic and gameplay-neutral; use form factors only to demonstrate visual grammar.
- **[Risk] Existing equipment geometry disagrees with newly accepted style-first references.** -> Treat that as expected migration feedback and repair source after reference acceptance.
- **[Risk] Visual audit remains subjective.** -> Keep visual PASS agent-owned, but bind it deterministically to exact applicable style inputs and reference images.
- **[Risk] Optional common scope complicates validation.** -> Treat it as an ordered optional list; only the matching asset-kind scope is mandatory and non-empty.

## Migration Plan

1. Add scoped style-manifest contracts/tests and applicable-style semantic provenance while keeping current assets otherwise unchanged.
2. Move the three exact current character reference paths into the `avatar` scope and add approved generic equipment style PNGs to `equipment`; leave `common` absent/empty unless there is real shared image authority.
3. Reseal FoundationGuard01 against avatar-scoped style provenance and prove equipment-only reference mutations no longer stale it.
4. Re-author RightBlade01 reference authority style-first; compare/repair its source only after reference acceptance, regenerate proof/compatibility, and publish healthy.
5. Repeat the bounded sequence for LeftGuard01, then BackPack01.
6. Prove avatar-only style mutation stales only avatars, equipment-only mutation stales only equipment, and shared trait/common mutation stales both.
7. Run full store verification and two no-op planner runs.

Rollback is Git-level: revert the scoped-manifest/audit migration and the three equipment reference/source migrations together so style schema, evidence, and asset authority remain mutually consistent.
