# Design

## Context

See `proposal.md` for motivation. The current baseline has one avatar plus three published equipment assets. Coverage requires Hand.R, Hand.L, and Back equipment, compatibility therefore emits three equipment pairs across two representative Idle samples, and the active equipment style scope points to three style boards derived from those same equipment models.

The replacement bow is supplied as visual source imagery, while Hero.Voxel requires construction authority to flow through an accepted Composition Sheet and Part Sheet(s) before VoxelSpec authoring. The current compatibility profile already exposes `Hand.L`, so the bow does not require a new profile.

## Goals / Non-Goals

**Goals:**

- Remove every active/source/generated/reference/proof trace of RightBlade01, LeftGuard01, and BackPack01 from the current baseline.
- Preserve the retired asset identities as permanently unavailable for reuse.
- Make the supplied ranger/bow image the independent equipment-style/source authority rather than deriving style from a generated equipment model.
- Build one new Bow01 through canonical Hero.Voxel reference-first authoring and verify it through the existing `humanoid.medium.v1 / Hand.L` contract.
- End with a deterministic healthy store containing exactly one avatar and one equipment asset.

**Non-Goals:**

- Do not change the avatar's `Hand.R`, `Hand.L`, or `Back` socket definitions merely because coverage no longer requires equipment for every socket.
- Do not preselect final bow voxel bounds outside the accepted Composition Sheet.
- Do not encode damage, weapon class, inventory semantics, draw behavior, projectile behavior, or other game-specific bow semantics.
- Do not merge the bow into the avatar source or create a precomposed avatar+bow asset.
- Do not preserve the retiring equipment style boards as hidden visual authority.

## Decisions

### 1. Retire published identities through an explicit tombstone registry

Add a repository-owned retirement policy such as `policy/retired-assets.yaml` containing the retired asset id and UnitRef pairs for RightBlade01, LeftGuard01, and BackPack01. Store validation rejects any active manifest that reuses either field.

This makes retirement deterministic in shallow clones and does not require scanning arbitrary Git history to discover deleted manifests.

**Alternative considered:** infer retired identities by traversing Git history. Rejected because it is slower, depends on repository history depth, and makes validation behavior sensitive to clone shape.

### 2. Replace coverage, not the compatibility profile

Update `policy/coverage.yaml` from:

```text
avatar + Hand.R equipment + Hand.L equipment + Back equipment
```

to:

```text
avatar + required Hand.L bow equipment
```

Keep `humanoid.medium.v1` unchanged. Its sockets are reusable avatar capabilities, not a declaration that the foundation must always publish one equipment item per socket.

The final compatibility snapshot should therefore contain two passing rows: Bow01 at `Hand.L` under the two existing representative Idle samples.

### 3. Import the supplied image as independent equipment visual authority

Store an exact repository-owned copy of the supplied ranger/bow PNG when the exact bytes are available. Record/verify its SHA-256 before changing the style manifest.

Replace the three model-derived `equipment-reference-*.png` inputs with the imported source image in the `equipment` scope of `low-resolution-voxel.v1`.

If exact-byte transfer is unavailable at apply time, stop at that gate rather than silently regenerating, re-encoding, or substituting a different image.

### 4. Separate style/source imagery from Hero.Voxel construction authority

The supplied image controls visual identity: chunky low-resolution voxel language, wood/green wrap/string treatment, silhouette, and proportions. It is not itself the construction package.

Bow01 must be authored in this order:

```text
supplied ranger/bow image
        |
        v
canonical Composition Sheet
        |
        | freeze bounds + RequiredParts
        v
Part Sheet(s)
        |
        v
Bow01 VoxelSpec
        |
        v
Hero.Voxel generation/proof
        |
        v
Hand.L compatibility proof
```

The Composition Sheet must use Hero.Voxel's fixed semantic region layout. Construction regions and all Part Sheet views remain flat/unlit and implementation-neutral.

### 5. Default to one rigid Bow part

The expected construction contract is `RequiredParts: [Bow]`. Limb sections, center grip, green wraps, tips, and string are visual geometry subparts, not separate rigid parts.

The Composition Sheet remains authoritative: if visual audit proves an independently moving/attachable construction part is genuinely required, freeze that before source authoring rather than forcing the one-part assumption.

### 6. Freeze bounds from the accepted Composition Sheet

Do not encode a guessed `12 x 4 x 8` (or any other bounds) in policy/tasks as source truth. During implementation, generate the canonical Composition Sheet from the supplied image, visually audit silhouette/proportions, and freeze target `W x H x D` there. The subsequent Part Sheet and VoxelSpec must agree with those frozen bounds.

### 7. Use a Bow01-local palette

Create a palette beside Bow01 source and derive only the colors required by the accepted bow reference: wood tones, green wraps, dark grip/accents, and pale string as visually justified.

This avoids changing `Art/CombatFoundation/Palette.yaml`, which is shared with the avatar and would unnecessarily invalidate unrelated avatar generation/proof.

### 8. Mount through Hand.L; implementation frame stays downstream

Bow01 declares `humanoid.medium.v1 / Hand.L`. The reference package may show the visual grip/contact region from the supplied image, but exact `assemblyOrigin`, pivot, and mount alignment are authored in VoxelSpec and verified by compatibility proof.

No pair-specific corrective transform is allowed.

### 9. Remove obsolete evidence rather than leaving ghost projections

Delete all three retiring equipment directories and their corresponding compatibility JSON files under the avatar proof tree. Regenerate catalog/health and current Bow01 compatibility evidence after the new baseline is authoritative.

The apply work should verify there are no remaining paths, ids, UnitRefs, style inputs, or compatibility evidence names referring to the three retired assets outside the retirement tombstones and archived OpenSpec/history.

### 10. Update deterministic tests around the new baseline

Replace hard-coded expectations of four assets / three equipment / six compatibility rows with two assets / one equipment / two compatibility rows. Replace blade-specific stale-generation and compatibility-evidence fixtures with Bow01.

Add retirement tests proving:
- active assets cannot reuse a retired id;
- active assets cannot reuse a retired UnitRef;
- removing retired files does not itself create a repair job when coverage no longer requires them.

## Risks / Trade-offs

- **[Risk] Exact supplied image bytes cannot cross into the project runtime.** -> Fail closed and ask for/import the exact file; do not manufacture a substitute.
- **[Risk] Bow proportions exceed the existing Hand.L envelope convention.** -> Author from the accepted reference first, then evaluate compatibility. If actual profile semantics must change, create a new profile version rather than mutating `humanoid.medium.v1` in place.
- **[Risk] One rigid part is too restrictive.** -> Let the Composition Sheet freeze a different RequiredParts set only when independent movement/attachment is visually required.
- **[Risk] Old compatibility/style files survive deletion.** -> Add explicit no-ghost audits over ids, UnitRefs, paths, style references, and compatibility evidence before finalization.
- **[Risk] Deleting published manifests loses identity history for ordinary validation.** -> Keep explicit retirement tombstones as permanent repository authority.

## Migration Plan

1. Import and hash the exact supplied ranger/bow image as independent equipment visual authority.
2. Add retirement tombstones for the three currently published equipment identities and extend validation/tests before deleting their manifests.
3. Change active coverage to avatar + one Hand.L bow requirement and switch equipment style scope away from the three retiring model-derived boards.
4. Create Bow01 with new id/UnitRef and a local palette.
5. Generate/audit the canonical Composition Sheet from the supplied image; freeze bounds and RequiredParts.
6. Generate/audit one Part Sheet per frozen part.
7. Author VoxelSpec only after reference acceptance, generate Hero.Voxel outputs, visually inspect proof, and seal.
8. Verify Bow01 against FoundationGuard01 at `Hand.L` for both required Idle samples.
9. Remove RightBlade01, LeftGuard01, BackPack01 and obsolete compatibility/style-board files; regenerate catalog/health.
10. Run no-ghost, retirement, coverage, compatibility, determinism, and idempotence verification.

Rollback is Git-level: revert the baseline replacement as one change so old assets, old coverage, old style authority, and old generated evidence return together. Do not partially resurrect one retired asset while leaving the new coverage/style authority in place.
