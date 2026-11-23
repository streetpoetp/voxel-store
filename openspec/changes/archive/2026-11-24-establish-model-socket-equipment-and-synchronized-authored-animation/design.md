# Design

## Context

See proposal.md for motivation. Current published avatars are constrained to one rigid `Body` part with no model sockets or authored clips; equipment is constrained to one untransformed root part with no sockets or authored clips. Compatibility uses Body-bounds ratios, while the Unity preview constructs procedural DOTween motion around a static equipment placement. Hero.Voxel already bakes sockets, propagates sockets through animated parent-part poses, and provides normalized animation sampling.

## Goals / Non-Goals

**Goals:**

- Make physical attachment authority live in the authored models.
- Keep equipment reusable across avatars by separating logical capability from concrete body socket instance.
- Let every equipment asset own its own grip/mount transform and animation curves.
- Keep body and equipment animation deterministically synchronized without requiring identical authored durations.
- Reuse Hero.Voxel socket, animation, pose-evaluation, and baked-asset contracts.
- Keep compatibility and Game View proof deterministic and machine-runnable.

**Non-Goals:**

- No gameplay occupancy, handedness rules, damage timing, hit results, attack selection, AI, balance, progression, or inventory semantics.
- No IK, secondary-hand solving, retargeting, skeletal rig requirement, or pair-specific corrective transforms.
- No form-specific slot vocabulary such as `SwordSlot`, `BowSlot`, or `RifleSlot`.
- No parallel Unity-only socket or animation authoring format.
- No requirement that independently authored body/equipment clips have identical duration values.

## Decisions

### 1. Avatar models own physical slot instances; equipment declares only logical capability

Avatar VoxelSpecs own concrete socket transforms using the naming convention:

```text
Equip.<Capability>.<Side>.<Instance>
```

Examples:

```text
Equip.Hand.Left.0
Equip.Hand.Right.0
Equip.Back.Left.0
Equip.Back.Right.0
```

All current rigid avatars keep one authored part named `Body`; equipment sockets are parented to `Body`. Additional anatomy parts are not required.

Equipment manifests move to the new attachment contract:

```yaml
version: 3
attachment:
  capability: Hand
  anchor: Mount
```

The equipment VoxelSpec owns exactly one attachment socket named `Mount`, parented to the appropriate equipment part. The manifest declares capability `Hand`, never a concrete address such as `Equip.Hand.Left.0`. Equipment may optionally constrain compatible sides (`Any`, `Left`, or `Right`) without changing asset identity; `Any` is the reusable default.

Capability matching is ordinal/case-sensitive and uses a small shared physical vocabulary. A consumer or preview may select one compatible concrete instance, but slot occupancy policy remains outside voxel-store.

A capability is a **slot kind**, not a singleton slot. One body may publish zero, one, or many instances of the same capability:

```text
Back -> Equip.Back.Left.0, Equip.Back.Right.0, Equip.Back.Center.0
Hand -> Equip.Hand.Left.0, Equip.Hand.Right.0
```

The number and side of physical instances are derived from the avatar's authored sockets and are therefore body-specific. Compatibility treats those addresses as an ordered deterministic set for proof/preview selection, but does not impose one-equipment-per-capability uniqueness. The same equipment asset may be instantiated repeatedly and bound to different compatible addresses; e.g. three copies of one bow asset may attach to `Equip.Back.Left.0`, `Equip.Back.Right.0`, and `Equip.Back.Center.0` if the body authors those slots.

Generated catalog state should project both the concrete instance names and a per-capability count, for example `Back: 3`, so consumers can discover physical capacity without inventing it from equipment metadata.

Side is semantic, not decorative metadata. The catalog should also project counts by side, for example `Hand.Left: 1`, `Hand.Right: 1`, so a consumer can choose a physically meaningful binding before motion selection.

**Alternative considered:** let equipment name the concrete body socket. Rejected because that reintroduces body-specific/hand-specific equipment identities.

**Alternative considered:** define one slot type per weapon form. Rejected because bodies would become coupled to catalog forms and compatibility would scale combinatorially.

**Alternative considered:** let equipment manifests declare slot count/capacity. Rejected because capacity is a property of the body geometry and its authored socket instances, not of the equipment catalog item.

### 2. Attachment solves socket-to-anchor alignment, not pivot-to-ratio placement

For an evaluated body socket matrix `S_body` and evaluated equipment `Mount` matrix relative to equipment root `M_mount`:

```text
EquipmentRoot = S_body × inverse(M_mount)
```

This equation is evaluated after sampling both current poses. The exact grip/pivot offset therefore belongs to each equipment model rather than to Unity preview code.

Current published equipment is single-part. Hero.Voxel animation offsets apply to that part, and sockets inherit the animated part matrix. Therefore re-solving the equipment root from the **evaluated animated Mount** would cancel a one-part weapon's own animation. The runtime instead aligns the equipment **bind Mount frame** to the current animated body socket, then applies the equipment clip relative to that attachment root. For current one-part weapons, publication requires the Mount position to coincide with the authored part pivot and equipped semantic motion on the Mount-bearing part to avoid position tracks; rotation therefore occurs around the grip point without translating the grip away from the body socket. Multipart equipment may later animate child parts independently while keeping the Mount-bearing part stable.

No pair-specific correction is persisted. If a conforming pair still requires one, compatibility fails.

**Alternative considered:** keep Body ratio as a coarse placement and add a Mount offset on top. Rejected because it leaves two competing attachment authorities.

### 3. Both body and equipment own authored semantic animation catalogs

Semantic animation supports side variants without duplicating asset identity. Base clip names remain `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death`. A model may additionally author `Motion.Left`, `Motion.Right`, or `Motion.Center` variants, for example `Attack.Left` and `Attack.Right`.

A semantic motion request carries an optional **driver binding** identifying the equipment instance whose action drives body response. The selected slot's side resolves the body clip variant:

```text
motion = Attack
driver = Equip.Hand.Left.0

body clip resolution:
  Attack.Left -> Attack

equipment clip resolution:
  Attack.Left -> Attack
```

Side-specific clip is preferred; the base semantic clip is the fallback. This keeps common `Idle`/`WalkForward` assets concise while allowing visibly different left/right attack mechanics. For a body that publishes both left and right `Hand` slots and claims side-reactive `Attack` support, publication requires both `Attack.Left` and `Attack.Right` so one side cannot silently mirror the other.

Current published avatars have exactly one authored part, `Body`; there are no authored Torso, Shoulder, Arm, or Hand parts. Hero.Voxel animation tracks target parts, so current body clips—including `Attack.Left` and `Attack.Right`—can only animate the whole `Body` part through position/rotation/scale offsets. Left/right body reaction therefore means different whole-body motion (for example different yaw/lean/translation), not limb articulation. Left/right equipment sockets are distinct static offsets parented to `Body` and inherit the same whole-Body pose. Independent arm/hand articulation requires a separate future multipart-avatar migration and is not assumed by this change.

Every reusable avatar and equipment VoxelSpec owns clips named:

```text
Idle
WalkForward
Attack
Hit
Death
```

The curves are asset-owned. A sword, spear, bow, crossbow, rifle, or axe may therefore implement a materially different `Attack` clip while retaining the same semantic name.

Baseline loop semantics are:

```text
Idle         loop
WalkForward  loop
Attack       non-loop
Hit          non-loop
Death        non-loop
```

Publication verifies semantic clip presence and loop semantics. Generated catalog state may project available semantic motions for diagnostics, but VoxelSpec remains animation authority.

**Alternative considered:** keep procedural DOTween as the canonical clip source. Rejected because weapon-specific animation would remain preview-owned instead of catalog-owned.

### 4. One body-owned clock drives one normalized semantic phase

Equipped playback does not independently advance the body and weapon clip clocks.

For semantic motion `m` and selected driver slot side `s`:

```text
bodyClip      = Resolve(body, m, s)       // m.s -> m
equipmentClip = Resolve(equipment, m, s)  // m.s -> m

phase p = body semantic clock normalized into [0,1]

bodyPose      = SampleNormalized(bodyClip, p)
equipmentPose = SampleNormalized(equipmentClip, p)
```

The body clip owns wall-clock duration and looping cadence. Equipment clip duration metadata remains meaningful for standalone asset preview, but equipped composition samples the equipment by the body's normalized phase. This guarantees phase lock without requiring all compatible assets to author identical durations.

For multiple equipped items—including repeated instances of the same equipment asset bound to different same-kind body slots—every item samples its own clip at the same `p`.

Only one binding is the semantic driver for a single action timeline in this change. If the left-hand item drives `Attack`, body variant resolution uses `Left`; if the right-hand item drives it, body variant resolution uses `Right`. Simultaneous independent left/right attacks would require multiple concurrent body motion authorities and remain out of scope.

**Alternative considered:** require equal body/equipment clip durations and run independent clocks. Rejected because tiny clock or authored-duration differences can drift and unnecessarily reduce reuse.

### 5. Pose evaluation order keeps animated sockets physically locked

Each frame/sample follows this order:

1. reset body pose;
2. sample the resolved body semantic clip at `p` (current avatars animate only `Body`);
3. evaluate the current body socket matrix;
4. solve each equipment attachment root from current body socket × inverse(equipment **bind Mount** frame);
5. reset equipment pose;
6. sample the resolved equipment semantic clip at `p` relative to that attachment root;
7. evaluate/render the equipment parts.

For current one-part equipment, Mount position must equal the part pivot and the Mount-bearing part does not use authored position tracks while equipped, so rotation animates around the grip without moving the grip point. The evaluated animated Mount frame is evidence, not the root-solving input.

This uses Hero.Voxel `VoxelAnimationSampler`, assembly pose evaluation, and baked socket data. Unity Transform objects may realize evaluated output, but they do not become authoring authority.

### 6. Compatibility evidence evaluates attachment and animation together

Compatibility proof for an avatar/equipment pair records:

- logical capability;
- selected concrete body socket;
- body and equipment semantic motion;
- normalized sample phase;
- evaluated body socket frame;
- evaluated equipment Mount frame;
- final equipment root/frame;
- geometric/visual compatibility result.

Representative deterministic phases cover each semantic motion, with denser samples for `Attack` where necessary. Missing socket, missing Mount anchor, missing semantic clip, invalid loop semantics, singular mount transform, or required pair-specific correction fails compatibility.

### 7. Unity preview becomes provider-authored motion playback

`AvatarPresentation`/equipment presentation continue to be preview-only composition owners, but Body-ratio placement and procedural DOTween transforms are no longer canonical attachment/motion authority.

The runtime gallery loads the baked assembly and matching `VoxelAnimationAsset` for every discovered model. The semantic motion player owns one phase clock, samples body/equipment clips through Hero.Voxel, evaluates body sockets, solves equipment roots, and presents the resulting poses.

`tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` remains the canonical visual proof entry point. Capture checkpoints select semantic motions and normalized phases, not procedural DOTween state.

### 8. Migrate published assets without changing identity or model frame

This is a contract migration, not an asset replacement. Existing ids, UnitRefs, forms, palette/style authority, VPU decisions, and canonical Unity frame remain unchanged.

All published avatars/equipment are migrated to manifest/source contract version 3 as one baseline migration. Generated VOX/baked/proof/compatibility/catalog evidence is regenerated and resealed after source changes.

## Risks / Trade-offs

- **[More authoring work per asset]** Every asset now owns sockets and five clips. → Keep the semantic set small and deterministic; allow asset-specific curves while sharing validation/tooling.
- **[Bad Mount pivots can create large root compensation]** → Validate finite/invertible evaluated mount matrices and capture representative phases before publication.
- **[Socket naming drift]** → Validate `Equip.<Capability>.<Side>.<Instance>` with supported side semantics and exact `Mount` naming at publication.
- **[Single-part body limits realism]** Current avatars can only animate the whole `Body`, so left/right variants cannot articulate arms or hands. → Keep this change honest to the current representation; a future multipart-avatar change is required for limb articulation.
- **[Single-part weapon attachment can cancel animation]** Solving against the evaluated animated Mount would mathematically remove the weapon's own transform. → Solve attachment from the bind Mount, require Mount-at-pivot for current single-part weapons, and prohibit equipped position tracks on the Mount-bearing part.
- **[Migration invalidates broad generated evidence]** → Preserve identities, regenerate through existing store commands, and require store verify + graphical capture before resealing.

## Migration Plan

1. Add version-3 manifest/validation support and socket/semantic-animation validators while preserving read support needed to diagnose version-2 assets during migration.
2. Add physical equipment sockets and semantic clips to every published avatar source.
3. Add `attachment.capability`, `attachment.anchor: Mount`, a `Mount` socket, and semantic clips to every published equipment asset.
4. Replace Body-ratio compatibility generation with socket/anchor + normalized-phase evidence and regenerate compatibility/catalog health.
5. Replace preview ratio/procedural motion authority with provider socket/animation playback.
6. Run deterministic store verification, Unity EditMode/PlayMode tests, and command-driven graphical Game View capture across representative forms.
7. Remove obsolete Body-ratio attachment and procedural canonical-motion code/docs after the new path proves complete.

Rollback before archive is source-level: restore version-2 manifests/source/evidence and the previous ratio-preview implementation. Do not mix both attachment authorities in the published baseline.
