# Design

## Context

See `proposal.md` for motivation. Current active assets already have a strong structural convention: `Assets/Avatars/<ModelName>` or `Assets/Equipment/<ModelName>`, with `source.base` pointing to `./Source/<ModelName>`. The weak point is identity: `id` and `unitRef` are only checked for presence/uniqueness and therefore still encode retired collection/origin concepts such as `dungeon`, `foundation`, and `humanoid`.

Published identity is intentionally explicit and stable, and `Store/retired-assets.yaml` is the durable reservation authority for identities that are no longer active. Compatibility evidence embeds avatar/equipment ids in both filenames and JSON payloads, so the migration must treat identity-bearing evidence as derived/reproducible state rather than hand-maintained authority.

## Goals / Non-Goals

**Goals:**

- Make active model directory, source model name, asset id, and UnitRef mechanically consistent.
- Make the convention deterministic and enforceable by Tool validation.
- Migrate all 25 active published identities atomically and reserve every previous identity pair.
- Regenerate identity-bearing derived/evidence state from unchanged model inputs.
- Keep the rule understandable enough that a new asset's identity can be predicted from its folder without consulting collection history.

**Non-Goals:**

- Do not rename the 25 current model directories or Hero.Voxel source model names.
- Do not redesign geometry, reference images, animation, sockets, forms, palettes, style authority, or compatibility semantics.
- Do not add active aliases from legacy ids/UnitRefs to new ids/UnitRefs.
- Do not make collection names, gameplay domains, or consuming-game ownership part of reusable asset identity.
- Do not automatically rewrite a published identity merely because an unrelated parent repository directory moves.
- Do not modify external consumer repositories in this change; expose a deterministic old-to-new migration map for downstream updates.

## Decisions

### 1. Canonical identity is derived from `kind + ModelName`, while remaining explicit in the manifest

For an active manifest, Tool resolves:

```text
Assets/Avatars/<ModelName>/asset.yaml    -> kind = avatar
Assets/Equipment/<ModelName>/asset.yaml -> kind = equipment

source.base = ./Source/<ModelName>
ModelKey   = Canonicalize(<ModelName>)
id         = <kind>.<ModelKey>
unitRef    = voxel-store/<kind>/<ModelKey>
```

The manifest continues to store `id` and `unitRef`; Tool computes the expected pair and rejects mismatches. This keeps the published contract visible and serializable while eliminating independent naming freedom.

Alternative considered: stop storing id/UnitRef and derive them at runtime. Rejected because published identity should remain explicit authority, reviewable in `asset.yaml`, and independently reservable in retired history.

### 2. `ModelName -> ModelKey` uses one deterministic tokenizer

Use a shared Tool helper that tokenizes:

- UpperCamel words,
- contiguous acronym runs,
- contiguous decimal runs.

Alphabetic tokens are lowercased; decimal tokens preserve all digits including leading zeroes; tokens join with `-`.

Examples:

```text
Wraith01          -> wraith-01
FoundationGuard01 -> foundation-guard-01
WarPick01         -> war-pick-01
Rifle02           -> rifle-02
NPCGuard01        -> npc-guard-01
```

The helper is the only implementation of this normalization used by manifest validation, tests, migration checks, and any identity-report tooling.

Alternative considered: store a separate editable `modelKey` field. Rejected because it reintroduces drift between structure and identity.

### 3. Catalog path and kind must agree

The asset directory immediately below `Assets/Avatars` or `Assets/Equipment` is the canonical ModelName. Tool validates:

```text
Assets/Avatars/*   <-> kind: avatar
Assets/Equipment/* <-> kind: equipment
```

and requires `source.base` to resolve to `./Source/<ModelName>` for active v3 manifests. A manifest nested under another identity-bearing directory or pointing to a differently named source base fails validation.

This intentionally tightens active content structure. Parent organization outside these canonical catalog/model segments remains non-semantic.

### 4. The 25 active identities migrate once; old pairs become retired authority

Migration keeps every current ModelName and changes only identity fields and identity-bearing evidence/projections.

Canonical active mapping:

| ModelName | New id |
| --- | --- |
| CultistAcolyte01 | `avatar.cultist-acolyte-01` |
| FoundationGuard01 | `avatar.foundation-guard-01` |
| Gargoyle01 | `avatar.gargoyle-01` |
| Ghoul01 | `avatar.ghoul-01` |
| LichLord01 | `avatar.lich-lord-01` |
| Necromancer01 | `avatar.necromancer-01` |
| SkeletonArcher01 | `avatar.skeleton-archer-01` |
| SkeletonGrunt01 | `avatar.skeleton-grunt-01` |
| SkeletonKnight01 | `avatar.skeleton-knight-01` |
| Wraith01 | `avatar.wraith-01` |
| ZombieBrute01 | `avatar.zombie-brute-01` |
| Axe03 | `equipment.axe-03` |
| Bow02 | `equipment.bow-02` |
| Crossbow02 | `equipment.crossbow-02` |
| Dagger02 | `equipment.dagger-02` |
| Halberd01 | `equipment.halberd-01` |
| Hammer02 | `equipment.hammer-02` |
| Kukri01 | `equipment.kukri-01` |
| Mace02 | `equipment.mace-02` |
| Rifle02 | `equipment.rifle-02` |
| Scythe01 | `equipment.scythe-01` |
| Spear02 | `equipment.spear-02` |
| Staff02 | `equipment.staff-02` |
| Sword02 | `equipment.sword-02` |
| WarPick01 | `equipment.war-pick-01` |

Each UnitRef is `voxel-store/<kind>/<ModelKey>`. Each previous active `(id, UnitRef)` pair is appended to `Store/retired-assets.yaml`. Existing retired identities remain untouched.

Alternative considered: keep aliases for old ids. Rejected because aliases would make two active names refer to one reusable asset and preserve the exact legacy namespace ambiguity this migration removes.

### 5. Identity-bearing evidence is regenerated, not manually re-authored

Compatibility proof files currently use equipment ids as filenames and store both avatar/equipment ids in payloads. After manifest migration:

1. Remove obsolete compatibility evidence keyed by old active ids.
2. Re-run the repository's canonical compatibility/write path from unchanged source/palette/provider inputs.
3. Verify new evidence uses canonical ids and current input hashes.
4. Regenerate catalog/health and preview-plan outputs from authority.

Visual reference/proof images and source geometry/animation are not regenerated merely because identity changed unless their existing evidence contract itself includes the migrated identity and Tool reports it stale.

Alternative considered: textual search/replace through proof JSON. Rejected because proof is derived evidence and should be re-issued by its owning verifier rather than edited by hand.

### 6. Path independence applies after canonical identity establishment

The existing path-independence principle remains, but its boundary becomes precise:

- kind + canonical ModelName establish identity at creation or explicit migration;
- moving the asset without changing those semantic segments does not change identity;
- changing canonical ModelName or kind is an explicit identity migration and must reserve the previous pair.

This prevents both extremes: arbitrary ids unrelated to model structure, and accidental published-id churn from ordinary repository reorganization.

## Risks / Trade-offs

- **[Breaking consumer references]** Existing consumers may refer to legacy ids/UnitRefs. → Publish a deterministic old-to-new mapping in change evidence/review output; do not hide the break behind aliases.
- **[Large evidence churn]** Compatibility proof filenames/payloads change for many avatar/equipment pairs. → Regenerate through the canonical Tool path and verify source/palette hashes remain unchanged.
- **[Retired-history growth]** The migration adds 25 reserved pairs permanently. → This is intentional; retired identity history is the store's durable anti-reuse authority.
- **[Tokenizer edge cases]** Future acronym/digit naming can produce ambiguous keys if normalization is duplicated. → Centralize tokenization in one Tool helper and test round examples plus collision detection.
- **[Path enforcement is stricter]** A manifest that previously worked from a noncanonical nested path will fail. → Report expected path/ModelName/id/UnitRef in one diagnostic so repair is mechanical.
- **[False visual churn]** Identity migration could unnecessarily regenerate source/proof pixels. → Assert authored model/reference/source hashes are unchanged and regenerate only identity-bearing evidence/projections.

## Migration Plan

1. Add the canonical identity helper and focused Tool tests first; use fixtures to prove both accepted and rejected structures without touching production manifests.
2. Add an explicit migration-aware validation/test fixture showing that an old pair can be moved to retired history while the new canonical pair becomes active.
3. Update all 25 active manifests to canonical id/UnitRef values in one change; do not rename model directories/source bases.
4. Append all 25 previous active identity pairs to `Store/retired-assets.yaml` and verify no collision with existing retired entries.
5. Remove obsolete compatibility evidence keyed by the legacy ids and regenerate compatibility evidence through the canonical Tool write flow.
6. Regenerate derived catalog/health state and preview-plan output; verify all 25 active assets are healthy/published under canonical identities.
7. Run stale-reference scans for legacy active ids/UnitRefs across active repository authority/evidence, allowing occurrences only in retired history and archived change history.
8. Run Tool tests, `verify`, strict OpenSpec validation, preview grid/capture checks proportional to identity-bearing preview changes, and `git diff --check`.

Rollback before publication is a single atomic revert of manifests, retired-history additions, and regenerated evidence. After downstream consumers adopt the new ids, rollback would itself be another explicit identity migration and is not treated as an alias switch.
