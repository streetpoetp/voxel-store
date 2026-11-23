# Design

## Context

See `proposal.md` for motivation. The generated runtime gallery currently composes published avatars and equipment correctly, but `VoxelStoreRuntimeAvatarPresenter` owns too many responsibilities at once: canonical Body-bounds ratio placement, equipment attachment, presentation neutral state, random scheduling, random motion selection, DOTween body motion, and lifecycle/reset.

That coupling makes the preview hard to reason about and produces a random transform showcase instead of a controlled comparison surface. Equipment follows whole-avatar motion only because it is parented under `PresentationRoot`; there is no explicit equipment animation layer for attack, hit, or death.

The target is a preview-only composition/motion architecture that keeps the existing rigid Body contract and ratio placement unchanged while making body and equipment presentation motion explicit.

## Goals / Non-Goals

**Goals:**
- Replace ambiguous `*Wrapper`/mixed presenter naming with responsibility-owned presentation and motion types.
- Keep avatar composition, equipment attachment, and semantic motion playback independently testable.
- Preserve the current `UnitRoot` / `PresentationRoot` authority boundary.
- Give each carried equipment instance a stable placement transform and a separate animation transform.
- Let one motion player coordinate body and equipment motion.
- Replace random runtime selection with an explicit Editor-selected motion.
- Establish a small general combat-game motion grammar: `Idle`, `WalkForward`, `Attack`, `Hit`, `Death`.
- Make orientation semantic so motion code uses Forward/Up/Right rather than raw axis guesses.
- Make `WalkForward` read like a Crossy Road hop and make `Attack` visibly move toward the avatar's face/forward direction.

**Non-Goals:**
- Defining combat damage, hit frames, cooldowns, AI, target selection, or inventory semantics.
- Reintroducing bones, hand parts, model sockets, or mount metadata.
- Making voxel-store preview classes production gameplay architecture.
- Creating per-avatar animation clips or Mecanim AnimatorControllers.
- Creating weapon-type-specific gameplay logic in the store.
- Replacing DOTween.

## Decisions

### 1. Use presentation/motion naming, not Wrapper

The target runtime types are:

- `AvatarPresentation`
- `EquipmentPresentation`
- `AvatarMotion`
- `AvatarMotionPlayer`
- `AvatarMotionPreviewWindow`

`Wrapper` is rejected because it describes implementation shape rather than responsibility. `Animator` is also avoided because Unity already uses that term for Mecanim.

The existing `VoxelStoreRuntimeAvatarPresenter` is retired rather than retained as a compatibility facade.

### 2. AvatarPresentation owns composition and orientation

`AvatarPresentation` is the composition owner for one generated avatar entry.

```text
UnitRoot
├─ AvatarPresentation
└─ PresentationRoot
   ├─ Body
   ├─ EquipmentPresentation[0]
   ├─ EquipmentPresentation[1]
   └─ ...
```

It owns/references `UnitRoot`, `PresentationRoot`, the Body `MeshFilter`, canonical Body bounds, attached `EquipmentPresentation` instances, and semantic local directions.

The canonical model frame is semantic rather than derived from Hero.Voxel's source-coordinate conversion:

```text
LocalRight   = +X
LocalUp      = +Y
LocalForward = -Z
```

`Forward = -Z` means the outward normal of the model's semantic FRONT points toward Unity `-Z`: an observer rendering `front.png` stands on the Unity `-Z` side and looks back toward `+Z`. For avatars, FRONT is the face/gaze/chest outward direction. Source-axis conversion only maps coordinates; it does not define facing.
Preview implementation authority is `VoxelStore.Preview.Runtime.VoxelStoreModelFrame`. The pinned Hero.Voxel provider remains authoritative for actual source-to-Unity bake behavior, but voxel-store does not patch or depend on an unpublished provider runtime API. `VoxelStoreModelFrame` locally locks the matching source-vector mapping `(x,y,z) -> (x,z,y)` plus semantic Unity Right `+X`, Up `+Y`, Forward `-Z`; `AvatarPresentation`, `EquipmentPresentation`, Body-ratio placement, and focused tests consume that one preview-owned contract.

Motion code consumes these semantic directions rather than embedding `Vector3.forward`/`Vector3.back` assumptions independently.

### 3. EquipmentPresentation separates stable placement and temporary motion

Each carried equipment instance uses:

```text
EquipmentPresentation
└─ PlacementRoot       stable Body-ratio placement
   └─ MotionRoot       animation-only delta
      └─ ModelRoot
```

`PlacementRoot` is resolved once from the existing normalized Body-bounds ratio contract and stays stable unless the composition itself is changed. Avatar and equipment assets share one canonical Unity model frame: Right `+X`, Up `+Y`, Forward `-Z`. In Hero.Voxel source coordinates that means semantic Forward must point toward source `-Y`. Avatar Forward follows face/gaze/chest outward direction. Equipment Forward follows its intended functional action direction when mounted on a forward-facing avatar: projectile launch/target for ranged equipment, thrust/tip for pointed weapons, and the primary cut/strike/pick/sweep direction for directional melee equipment. The audit never infers Forward from geometry length, a broad presentation face, or coordinate-conversion names alone. Source geometry and pivot are normalized asset-by-asset before publication, so `PlacementRoot` keeps identity rotation and composition never compensates for model-facing differences.
The current published equipment audit is explicit:

| Asset | Semantic-front decision |
| --- | --- |
| `Axe03` | Functional cutting direction is original source `-X`; rotate geometry/pivot +90° in source XY so the cutting edge drives toward source `-Y` / Unity `-Z`. Broad blade face is presentation evidence, not semantic Forward. |
| `Bow02` | Projectile target direction is original source `-X` (string behind the grip/limbs); rotate geometry/pivot +90° in source XY so launch direction becomes source `-Y` / Unity `-Z`. |
| `Crossbow02` | Projectile target direction is original source `+X` along the stock/rail toward the front; rotate geometry/pivot -90° in source XY so launch direction becomes source `-Y` / Unity `-Z`. |
| `Dagger02` | Thrust direction from grip toward blade tip is original source `+Z`; rotate geometry/pivot +90° around source X so the tip points source `-Y` / Unity `-Z`. |
| `Halberd01` | Primary thrust direction toward the spear tip is original source `+Z`; rotate geometry/pivot +90° around source X so the tip points source `-Y` / Unity `-Z`. |
| `Hammer02` | Primary striking face is original source `-X`; rotate geometry/pivot +90° around source Z so the striking face points source `-Y` / Unity `-Z`. |
| `Kukri01` | Leading blade-tip/chop direction is original source `+Z`; rotate geometry/pivot +90° around source X so the leading end points source `-Y` / Unity `-Z`. |
| `Mace02` | Leading head/strike direction is original source `+Z`; rotate geometry/pivot +90° around source X so the head leads toward source `-Y` / Unity `-Z`. |
| `Rifle02` | Projectile target direction is original source `+X` toward the muzzle; rotate geometry/pivot -90° around source Z so muzzle direction becomes source `-Y` / Unity `-Z`. |
| `Scythe01` | Blade sweep/tip direction is original source `+X`; rotate geometry/pivot -90° around source Z so the blade leads toward source `-Y` / Unity `-Z`. |
| `Spear02` | Thrust direction toward the spear tip is original source `+Z`; rotate geometry/pivot +90° around source X so the tip points source `-Y` / Unity `-Z`. |
| `Staff02` | Leading staff-head direction is original source `+Z`; rotate geometry/pivot +90° around source X so the head points source `-Y` / Unity `-Z`. |
| `Sword02` | Thrust direction from grip toward blade tip is original source `+Z`; rotate geometry/pivot +90° around source X so the tip points source `-Y` / Unity `-Z`. |
| `WarPick01` | Pick/striking direction is original source `-X`; rotate geometry/pivot +90° around source Z so the pick points source `-Y` / Unity `-Z`. |

All fourteen published equipment sources now encode their semantic action direction toward source `-Y`; all fourteen were regenerated through the store tool, their generated semantic `front.png` evidence was reviewed, they were resealed, and store `scan --write` plus `verify` passed. The current eleven avatar sources were also reviewed: face/eye/glow/skin front cues are already authored on the minimum source-Y face, so avatar semantic Forward already points source `-Y` and no avatar orientation transform is required.
Here `front.png` means the generated Hero.Voxel proof view. A `FRONT` panel inside `Reference/Parts/*.png` remains target/reference presentation evidence and does not override semantic model-frame authority. This distinction is especially important for ranged equipment, whose useful reference sheet may show a broad side while semantic Forward is muzzle/target-facing.


`MotionRoot` starts at local identity and is the only equipment-local transform owned by `AvatarMotionPlayer`. Equipment animation offsets use the same canonical Forward/Up/Right basis as the avatar.

This prevents attack animation from corrupting attachment authority and makes reset behavior deterministic.

### 4. AvatarMotionPlayer coordinates both body and equipment

`AvatarMotionPlayer` receives an `AvatarPresentation` and owns the active DOTween sequence.

It may animate the avatar `PresentationRoot`, every attached equipment `MotionRoot`, and preview-only visual parameters such as scale. It must not mutate Body-ratio `PlacementRoot` transforms.

Every `Play(...)`, `Stop()`, switch, or disable path must:

1. kill active tweens owned by the player,
2. reset `PresentationRoot` to its neutral local transform,
3. reset all equipment `MotionRoot`s to identity,
4. then start the requested motion if applicable.

Destroy must kill the owned tween sequence; it does not need to restore transforms on objects that are themselves being destroyed. This gives one lifecycle owner for all visible animation.

### 5. Semantic motion set is small and general

The first motion contract is:

```text
AvatarMotion
├─ Idle
├─ WalkForward
├─ Attack
├─ Hit
└─ Death
```

No `Slash`, `Twist`, random recoil variant, or avatar-specific clip names are part of the public preview grammar.

### 6. WalkForward uses a Crossy Road hop grammar

The presentation track follows discrete phases:

```text
Launch → Rise → Apex → Descend → Land → Recover
```

Body presentation rises along semantic Up, applies a small readable forward pitch/lean, optionally uses subtle squash/stretch, lands, and returns to neutral.

Equipment receives secondary lag during launch, follow-through around apex/descent, and settles on landing.

Actual logical forward travel is not owned by `AvatarMotionPlayer`. If the gallery needs to demonstrate one-cell travel, a preview-only driver may move `UnitRoot` along `AvatarPresentation.LocalForward` while the motion player supplies the hop presentation.

### 7. Attack is explicitly forward and includes equipment motion

Attack phases are:

```text
Ready
  → backward anticipation
  → fast forward strike
  → contact hold
  → follow-through
  → recovery
```

Body translation is resolved using semantic Forward: anticipation moves slightly opposite Forward and strike moves strongly along Forward. Attack keeps the avatar presentation rotation at its neutral orientation so child equipment does not inherit pitch/yaw that would steer semantic Forward away from the canonical direction.

Each equipment `MotionRoot` receives a coordinated attack track. Wind-up may translate slightly opposite Forward; strike and follow-through translate along Forward. The generic attack track keeps `MotionRoot` rotation neutral so the weapon's semantic Forward remains aligned with avatar Forward throughout anticipation, strike, and follow-through.

### 8. Hit and Death coordinate equipment inertia

`Hit` recoils the body opposite Forward, applies brief equipment inertia relative to placement, then recovers all non-terminal transforms.

`Death` moves the body into a terminal fall presentation and gives equipment coordinated follow-through/drop-like relative motion while leaving `PlacementRoot` unchanged.

### 9. Editor selection is optional interactive control

`AvatarMotionPreviewWindow` owns optional human exploration control, not animation execution and not verification authority. The implementation must not require an agent or developer to repeatedly click Unity menus, Play, window buttons, or Inspector fields in order to prove the motion system.

Controls:

```text
Motion:   [ Idle | WalkForward | Attack | Hit | Death ]
Loop:     [x]
Speed:    [1.0]
Target:   [ All Avatars | Selected Avatar ]

[ Play ] [ Stop ] [ Reset ]
```

Before Play Mode, the window stores/applies the desired preview selection to the generated gallery configuration.

During Play Mode, it discovers live `AvatarMotionPlayer` targets and calls their public motion API directly. Switching motion goes through kill/reset/play. `AvatarMotionPlayer` exposes the serialized field as `Motion` rather than `DefaultMotion`; its custom Inspector may apply live changes to Motion/Loop/Speed immediately during Play Mode.

The Editor window does not run DOTween through `EditorApplication.update`. These controls are convenience UX only; completion evidence must come from the script-driven capture path below.

### 10. Borrow Core Raid's registered Game View capture architecture

Canonical animation proof follows the same ownership split already proven in Core Raid and the pinned Hero Studio capture contract. The former `AvatarMotionGalleryCapture` camera/RenderTexture automation path is removed after the registered command path is proven; the runtime gallery's one-off static RenderTexture screenshot remains diagnostic-only.

Preview-owned structure:

```text
Preview/Unity/Assets/Preview/
├─ Capture/
│  └─ CaptureRegistry.json
└─ Capture.Editor/
   ├─ VoxelStore.Preview.Capture.Editor.asmdef
   └─ AvatarMotionCaptureRunner.cs
```

The schema-v2 registry owns only opaque discovery metadata:

```text
scenario: avatar-motion-gallery
runner: VoxelStore.Preview.Capture.Editor.AvatarMotionCaptureRunner.RunFromCommandLine
outputDirectory: Library/Capture/AvatarMotionGallery
captureKind: GameView
```

Voxel-store is not a normal Hero Studio game-client consumer root: its Unity workspace is `Preview/Unity`, not `src/client/unity`, and its preview root is not `Assets/_Game`. Therefore the normal consumer-root command cannot be used unchanged. Voxel-store SHALL own one repository command, `tools/capture-game-view.ps1`, whose public contract mirrors the Hero Studio/Core Raid capture command while resolving the voxel-store preview layout. The normal invocation is `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`; omitting `-Scenario` MAY use the registry default. This command is the only automation entry point callers need. It resolves `Preview/Unity` plus the preview-owned registry and reuses pinned Hero Studio's shared Unity-project and Game View capture/attestation helpers. It must preserve the same Core Raid/provider policies: launch an owned graphical Unity child, never use `-batchmode` or `-nographics`, wait for that exact child, reject occupied/default-project misuse, reject stale captures, and require current provider attestation before reporting success.

The command hides the Unity runner mechanics from its caller:

```text
tools/capture-game-view.ps1 -Scenario avatar-motion-gallery
        ↓
resolve Preview/Unity + CaptureRegistry scenario
        ↓
internally launch owned graphical Unity
        ↓
internally invoke registered RunFromCommandLine runner
        ↓
runner rebuilds/opens runtime gallery
        ↓
store capture state in SessionState
        ↓
enter Play Mode
        ↓
domain reload
        ↓
[InitializeOnLoad] Poll rebinds live AvatarMotionPlayer targets
        ↓
for each semantic checkpoint
    SampleMotion(motion, normalizedTime)
    await Hero.Consumer.Editor.GameViewCapture.CaptureAsync(...)
        ↓
provider composed ScreenCapture PNG
        + game-view-capture.json attestation
        ↓
reset motion / advance checkpoint
        ↓
leave Play Mode, clear SessionState, EditorApplication.Exit(0/1)
```

Unlike Core Raid, voxel-store does not need an Evaluation application for this proof. Hero Studio explicitly permits a `Capture.Editor` adapter to drive production/runtime surfaces directly when Evaluation is unnecessary. Here the capture runner may drive the preview-owned `AvatarMotionPlayer` directly because the semantic checkpoints are the motion states themselves. Capture owns only acquisition/orchestration; `AvatarMotionPlayer` still owns motion semantics.

Representative checkpoints remain deterministic (`WalkForward` launch/apex/land, `Attack` anticipation/contact/follow-through, `Hit` recoil/recover, `Death` fall/fallen). Sampling may use the runtime player's deterministic normalized-time API, but each accepted screenshot must be acquired through provider `GameViewCapture.CaptureAsync` while the graphical Editor is in Play Mode.

The registered capture runner owns an evidence-only oblique Game View camera pose (30° pitch, 35° yaw around the current gallery bounds). The ordinary runtime gallery camera remains unchanged. The oblique proof view is required because semantic Forward is Unity `-Z`; a front-aligned orthographic camera can hide Forward translation in depth and make Attack/Hit pixels ambiguous. Canonical capture therefore gives Forward a visible screen-space component while preserving the same runtime scene and motion state.

Provider `CAPTURE_ATTESTED` means only that the owned graphical Unity run completed and the current composed Game View PNGs are valid/current. It does not prove semantic animation correctness. Agent/human review of the attested pixels still decides whether Walk/Attack/Hit/Death visually satisfy the change.

Manual `Voxel Store` menu interaction, `AvatarMotionPreviewWindow` buttons, hierarchy selection, Inspector editing, direct camera RenderTexture capture, and stale PNG reuse are not acceptable completion gates.

Direct caller use of Unity `-executeMethod`, fully-qualified runner names, or hard-coded `Library/Capture/...` paths is also not the normal verification workflow. Those are registry/command implementation details. Agent and developer automation should invoke the repository capture command and consume its machine-readable result lines/output paths.

### 11. Runtime gallery is human-import/catalog driven

The runtime gallery does not own baking or source regeneration. It discovers imported `VoxelAssemblyAsset` assets under `Assets/__VoxelStorePreview/Baked` using folder structure rather than provider plan or `Kind` metadata:

```text
.../<Catalog>/<Model>/Source/<Model>.asset
```

The folder immediately above `<Model>` is the catalog identity; arbitrary prefix folders are ignored. `Avatars` receives `AvatarPresentation`/`AvatarMotionPlayer`, `Equipment` participates in carried-equipment sampling and standalone display, and unknown catalogs remain valid standalone entries. The gallery realizes every baked part with bind hierarchy/transform; sockets are not required for visualization.

Imported catalog directories are human-owned gallery inputs, not machine-owned preview-bake outputs. Stale cleanup may delete only directories positively identified as generated output and must preserve imported catalog trees.

For avatar comparison, the builder deterministically samples carried equipment from the sorted `Equipment` catalog. Primary uses the avatar index modulo equipment count; secondary uses a deterministic offset and avoids duplicating primary when possible. This pairing exists only to exercise composition across the catalog; it does not define weapon class, inventory occupancy, handedness, loadout, or gameplay compatibility.

### 12. Runtime gallery defaults to synchronized comparison

The generated gallery continues to show all published avatars and standalone equipment.

For avatar entries, the builder creates `AvatarPresentation`, one or more `EquipmentPresentation` instances, and `AvatarMotionPlayer`.

All avatars use the same selected configured Motion so model/equipment behavior can be compared side-by-side.

Random seed fields, random delay ranges, `PlayNext()`, and random motion selection are removed.

### 13. Preview runtime remains evaluation-only

The preview runtime assembly may execute as ordinary MonoBehaviours while the Unity Editor is in Play Mode. It remains under the voxel-store preview workspace and must not become a required runtime package for production consumers.

Generated scenes/materials remain disposable ignored preview state.

## Risks / Trade-offs

- Generic equipment attack motion may not perfectly fit bow/rifle/melee forms. Keep the initial contract semantic and generic; evaluate whether form-specific presentation profiles are actually needed before adding them.
- Equipment motion can drift away from Body-ratio attachment. Tween only `MotionRoot`; never animate `PlacementRoot`; reset MotionRoot to identity on every lifecycle transition.
- Motion switching can leave residual body/equipment transforms. Centralize tween ownership and reset in `AvatarMotionPlayer`.
- Editor window can hold stale references across Play Mode reload. Discover current live players when applying commands instead of retaining long-lived scene-object references.
- Manual Editor interaction and ad-hoc camera rendering are slow/brittle proof mechanisms. Treat the Editor window/Inspector and camera RenderTexture captures as optional diagnostics; require the registered graphical Game View capture path with current provider attestation for canonical visual proof.
- Crossy Road hop can accidentally become gameplay root motion. Keep logical UnitRoot travel in a preview-only driver; `AvatarMotionPlayer` owns presentation only.
- Semantic facing can be ambiguous for visually symmetric equipment. Resolve it from authoring/game-use semantics and canonical `front.png` evidence; never substitute geometric inference or provider coordinate conversion. Keep runtime direction resolution centralized in `AvatarPresentation` and cover the published frame with baked-orientation tests.
- Two motion systems may coexist during migration. Remove `VoxelStoreRuntimeAvatarPresenter` random playback once the new vertical slice is proven; do not preserve a legacy random mode.

## Migration Plan

1. Introduce `AvatarMotion`, `AvatarPresentation`, `EquipmentPresentation`, and `AvatarMotionPlayer` beside the current presenter with focused tests.
2. Convert one runtime-gallery avatar entry to the new hierarchy and prove Body-ratio placement, equipment MotionRoot reset, canonical direction, and body/equipment Attack playback.
3. Implement the complete semantic motion set, including Crossy Road-style `WalkForward` and explicit forward `Attack`.
4. Add schema-v2 preview `CaptureRegistry.json`, a `Capture.Editor` registered runner, and repository command `tools/capture-game-view.ps1` that exposes scenario-based capture while reusing pinned Hero Studio capture helpers.
5. Make the registered runner survive Play Mode/domain reload with `SessionState` + persistent polling, drive deterministic motion checkpoints, and acquire each accepted image through `Hero.Consumer.Editor.GameViewCapture.CaptureAsync`.
6. Keep `AvatarMotionPreviewWindow` as optional interactive exploration and prove it calls the same runtime motion API without becoming verification authority.
7. Migrate the entire runtime gallery builder to the new presentation/motion components.
8. Remove `VoxelStoreRuntimeAvatarPresenter`, random scheduling fields/methods, and retire `AvatarMotionGalleryCapture` camera-render proof authority after the registered Game View path is proven.
9. Run EditMode, PlayMode, and provider-attested registered Game View capture through `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`, update workflow documentation, and verify generated gallery/captures remain derived and reproducible.

Rollback: until the new presentation/motion vertical slice is proven, keep the existing generated gallery builder behavior recoverable in Git. Do not retain both systems after migration completion.

## Open Questions

None. The approved scope includes explicit equipment animation under `AvatarMotionPlayer`, canonical forward attack behavior, Crossy Road-style `WalkForward`, Core Raid-style registered provider-attested graphical Game View capture as verification authority, optional Editor-selected motion for exploration, and responsibility-owned naming.