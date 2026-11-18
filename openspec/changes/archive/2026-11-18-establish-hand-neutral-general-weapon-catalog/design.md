# Design

## Context

See `proposal.md` for motivation. The active store currently uses manifest/profile/coverage version 1. `AssetMount` stores one concrete socket, `humanoid.medium.v1` publishes separate `Hand.R`, `Hand.L`, and `Back` mount contracts, and compatibility evaluation resolves exactly the equipment-declared socket for each avatar/equipment pair. Bow01 therefore owns a side-specific identity and mount.

Coverage currently matches only `kind + profile + mount`, so multiple requirements for the same hand socket can all be satisfied by one published asset. The generated catalog also projects only id/kind/UnitRef, which means it cannot explain physical-form coverage.

FoundationGuard01 already has two physical hand attachment points parented to its two arm parts. The equipment contract does not need those arm-part names to become gameplay handedness. The new design separates a logical equipment mount capability from the avatar's concrete attachment instances.

The active `low-resolution-voxel.v1` equipment style scope contains the repository-owned bow source image plus shared style traits. It remains style authority only; each asset's accepted Hero.Voxel Composition/Part Sheets remain construction authority.

## Goals / Non-Goals

**Goals:**

- Make one hand-mounted equipment asset compose through either physical hand attachment without side-specific asset identity or mount metadata.
- Move the active foundation to a new versioned compatibility/profile contract rather than mutating `humanoid.medium.v1`.
- Give coverage an exact physical-form discriminator so ten required catalog forms mean ten independently published assets.
- Preserve source-authoritative, reference-first publication and the existing one-asset-per-maintenance-run boundary.
- End with no active `Hand.R` / `Hand.L` equipment-mount semantics in code-facing policy, manifests, active avatar sockets, or generated evidence.

**Non-Goals:**

- Do not encode gameplay occupancy such as one-hand versus two-hand use, attack behavior, damage, class, inventory, AI, progression, or balance.
- Do not rename avatar arm/body part identities merely because equipment mounting becomes side-neutral; rig-side names can remain internal anatomy/animation structure.
- Do not add IK, secondary-hand grip solving, per-item pose authoring, or pair-specific corrective transforms.
- Do not create a separate Hero.Voxel compiler path or per-form visual style family.

## Decisions

### 1. Introduce profile/manifest/coverage version 2 for active side-neutral contracts

Do not redefine the existing version-1 shapes in place. Add version-2 contract support first, migrate active content, then retire version-1 active policy/content.

The version-2 compatibility profile is shaped conceptually as:

```yaml
version: 2
id: humanoid.medium.v2
voxelsPerUnit: 8
requiredMountCapabilities:
  - capability: Hand
    sockets:
      - Hand.0
      - Hand.1
    forward: [0, 0, 1]
    up: [0, 1, 0]
    envelope: <shared hand envelope>
  - capability: Back
    sockets:
      - Back
    forward: [0, 0, 1]
    up: [0, 1, 0]
    envelope: <existing back envelope>
representativePoseSamples:
  - clip: Idle
    normalizedTime: 0
  - clip: Idle
    normalizedTime: 0.5
```

`Hand` is one logical capability with two neutral physical attachment instances. The shared hand envelope is derived from the union of the two published v1 hand envelopes so both instances have one capability contract; after v2 is published, incompatible envelope/frame changes require another profile version.

Version-2 active equipment manifests use:

```yaml
version: 2
form: bow
mount:
  profile: humanoid.medium.v2
  capability: Hand
```

Version-2 coverage uses `profile + capability + form` rather than a concrete socket. Active avatar/equipment manifests migrate to version 2 as one foundation migration.

**Alternative considered:** keep manifest/profile version 1 and replace `socket` with `capability`. Rejected because that would silently change the meaning of already-published contracts.

### 2. Keep physical attachment instances on the avatar; equipment sees only capabilities

FoundationGuard01 renames its active hand sockets from side-bearing names to `Hand.0` and `Hand.1`. They remain parented to the existing arm parts, so avatar anatomy/animation does not need to be redesigned. Equipment never declares `Hand.0` or `Hand.1`; it declares only `Hand`.

Compatibility evaluation resolves the equipment's logical capability through the profile, enumerates every socket listed for that capability, and evaluates every required pose at every matching attachment instance. A single avatar/equipment evidence file contains rows keyed by capability, attachment instance, and pose. Generated compatibility health exposes both capability and attachment so failures are diagnosable.

For the current foundation baseline, ten hand-capability assets x two attachment instances x two Idle samples yields forty passing equipment compatibility rows for the one published avatar.

**Alternative considered:** keep `Hand.R` and `Hand.L` as physical socket names but hide them behind `Hand`. Rejected because the active store would still preserve the side-specific concept the migration is intended to remove.

### 3. Add publication-stable physical form as equipment classification

Version-2 equipment manifests declare one gameplay-neutral `form`. The form is required for equipment, is included in derived catalog projection, and is treated as publication-stable classification: changing the form of an already published identity requires an explicit migration rather than silently moving that identity between coverage slots.

Coverage requirements may declare `form`; when present, satisfaction requires exact equality in addition to kind/profile/capability. This closes the current loophole where one published item can satisfy multiple same-mount requirements.

The ten active foundation form values are `sword`, `dagger`, `axe`, `hammer`, `mace`, `spear`, `staff`, `bow`, `crossbow`, and `rifle`. These names classify physical visual form only and carry no behavior or balance meaning.

**Alternative considered:** encode the form only in asset id/UnitRef and parse it from strings. Rejected because published identity strings are not a queryable coverage contract and would make coverage dependent on naming conventions.

### 4. Replace the published side-specific Bow01 identity instead of mutating it

Record `foundation.equipment.bow.left-01` / `voxel-store/equipment/bow/left-01` as retired identities, then publish a new Bow01 identity:

```text
foundation.equipment.bow.01
voxel-store/equipment/bow/01
```

The accepted Bow01 visual reference package may be reused as construction input when its visual design is unchanged, but all authority/evidence that binds identity, manifest/profile, source generation, proof, or compatibility is regenerated and resealed for the new v2 asset. The new manifest declares `form: bow` and `mount: humanoid.medium.v2 / Hand`.

**Alternative considered:** rename the existing published id/UnitRef in place. Rejected because publication history explicitly reserves published identities and requires replacement assets to receive new identities.

### 5. Establish one canonical asset per required general form

The active catalog uses these side-neutral identities:

| Directory | Asset id | UnitRef | Form |
| --- | --- | --- | --- |
| `Sword01` | `foundation.equipment.sword.01` | `voxel-store/equipment/sword/01` | `sword` |
| `Dagger01` | `foundation.equipment.dagger.01` | `voxel-store/equipment/dagger/01` | `dagger` |
| `Axe01` | `foundation.equipment.axe.01` | `voxel-store/equipment/axe/01` | `axe` |
| `Hammer01` | `foundation.equipment.hammer.01` | `voxel-store/equipment/hammer/01` | `hammer` |
| `Mace01` | `foundation.equipment.mace.01` | `voxel-store/equipment/mace/01` | `mace` |
| `Spear01` | `foundation.equipment.spear.01` | `voxel-store/equipment/spear/01` | `spear` |
| `Staff01` | `foundation.equipment.staff.01` | `voxel-store/equipment/staff/01` | `staff` |
| `Bow01` | `foundation.equipment.bow.01` | `voxel-store/equipment/bow/01` | `bow` |
| `Crossbow01` | `foundation.equipment.crossbow.01` | `voxel-store/equipment/crossbow/01` | `crossbow` |
| `Rifle01` | `foundation.equipment.rifle.01` | `voxel-store/equipment/rifle/01` | `rifle` |

Every item declares `humanoid.medium.v2 / Hand`. A consuming game may decide how it is used, including whether a pose visually involves a second hand; voxel-store owns only the one canonical attachment frame needed to compose the independent asset.

Each form is authored in a separate maintenance invocation. Bow01 replacement counts as one of the ten; the other nine are new asset jobs.

### 6. Keep one shared equipment style authority and per-asset construction authority

Do not create weapon-specific style families. Resolve the existing `low-resolution-voxel.v1` equipment style authority before each asset job and treat its bow source image as art-direction evidence, not as shape authority for every form. The named physical form plus the shared style traits establish visual intent; the accepted Composition Sheet and Part Sheet(s) become the actual construction authority for that asset.

Before the first non-bow asset is authored, audit whether the current equipment style scope is sufficiently general to support the catalog while preserving the intended low-resolution voxel language. If it is not, add or replace one repository-owned general equipment style reference under the same `equipment` scope before accepting any affected construction sheets. Do not create per-form style scopes as a workaround.

Rigid assets default to one required construction part. Visual regions such as grip, head, blade, limbs, stock, string, or guard remain geometry subparts unless independent movement/attachment is visually required by the accepted Composition Sheet.

### 7. Make coverage and derived evidence form-aware without weakening bounded maintenance

Coverage policy version 2 contains one avatar requirement for `humanoid.medium.v2` plus ten required equipment rows, all using capability `Hand` and a distinct form. `EvaluateCoverage` matches every declared discriminator. Generated catalog entries include the physical form and mount capability; generated health gaps include profile, capability, and form.

`PlanNext` keeps the existing repair-before-expansion ordering and selects only the first required gap in deterministic policy order. The one-maintenance-run/one-asset-job invariant remains unchanged, so expanding the catalog cannot cause one invocation to author multiple assets.

**Alternative considered:** bypass coverage and add the ten asset directories directly as a one-off content batch. Rejected because the store's self-management contract would no longer prove that future form coverage is derivable and repairable from policy.

## Risks / Trade-offs

- **[Risk] One canonical local mount frame may align in one attachment instance but not the other.** -> Compatibility must fail on the exact capability/attachment/pose row; repair the equipment frame, avatar socket realization, or versioned profile rather than introducing a per-instance correction.
- **[Risk] The shared v2 hand envelope is too restrictive for a long or broad form.** -> Derive v2 from the union of the published v1 hand envelopes, then fail closed if a catalog asset cannot satisfy it without distortion; publish another profile version instead of mutating v2.
- **[Risk] Expanding one equipment pair to two attachment instances doubles compatibility evidence.** -> Keep one evidence file per avatar/equipment pair with deterministic attachment+pose rows; the current target remains bounded at forty rows.
- **[Risk] The current equipment style reference is too bow-specific for clear non-bow construction sheets.** -> Audit style authority before non-bow authoring and, only if necessary, add one general equipment reference under the existing style scope before accepting affected sheets.
- **[Risk] A form label drifts into gameplay taxonomy.** -> Keep the approved values strictly physical/visual and reject gameplay fields such as occupancy, damage, attack behavior, rarity, or class.
- **[Risk] Partial migration leaves v1 side-specific and v2 neutral contracts active simultaneously.** -> Sequence the migration so v2 support lands first, active assets/policy move coherently, and v1 active policy/content is removed only after the v2 baseline is healthy.

## Migration Plan

1. Add transitional reader/validator support for version-2 manifests, coverage, and compatibility profiles while the current v1 baseline still scans; add deterministic tests for capability expansion, exact form matching, and historical form stability.
2. Add `humanoid.medium.v2` with logical `Hand` capability and neutral `Hand.0` / `Hand.1` physical instances plus the existing `Back` capability. Derive the shared hand envelope from the v1 hand envelopes.
3. Migrate FoundationGuard01 to manifest/profile v2, rename only its active hand sockets to `Hand.0` / `Hand.1`, regenerate source-derived output/proof, and verify the avatar conforms under both required Idle samples. Keep arm-part names unchanged.
4. Add form-aware version-2 coverage and catalog/health projection. Switch the avatar requirement to `humanoid.medium.v2`; do not activate the ten equipment requirements until the side-neutral Bow01 replacement can be established in the same coherent baseline transition.
5. Record the current side-specific Bow01 id/UnitRef as retired. Establish the new Bow01 id/UnitRef with `form: bow` and `humanoid.medium.v2 / Hand`, reseal reference/source/proof evidence, and prove both hand attachment instances across both Idle samples.
6. Activate all ten required equipment-form coverage rows. Verify the planner reports exactly the nine missing non-bow forms and selects only the first gap.
7. Audit the shared equipment style authority for the expanded catalog. If it cannot support clear form-specific reference authoring, establish one general equipment style input under the existing equipment scope and re-audit affected references before continuing.
8. Run nine separate bounded maintenance jobs for Sword01, Dagger01, Axe01, Hammer01, Mace01, Spear01, Staff01, Crossbow01, and Rifle01. For each: accept Composition Sheet -> freeze parts -> accept Part Sheet(s) -> author source -> generate proof -> verify both hand attachments -> publish before moving to the next form.
9. Regenerate catalog/health and verify one avatar + ten equipment assets are healthy, all ten form requirements are satisfied, compatibility contains forty passing equipment rows for the current avatar, and the next maintenance job is null.
10. Remove v1 active compatibility policy/content and transitional assumptions no longer needed by active scanning. Audit active source/policy/art/evidence for `Hand.R` / `Hand.L` equipment-mount semantics; archived OpenSpec/history is exempt.
11. Run the full Tool tests, deterministic clean regeneration, no-op maintenance planner twice, strict OpenSpec validation, and repository hygiene checks.

Rollback is Git-level: revert the profile/schema migration, Bow identity replacement, coverage expansion, and catalog assets together. Do not partially restore a side-specific identity while leaving v2 coverage/profile authority active.
