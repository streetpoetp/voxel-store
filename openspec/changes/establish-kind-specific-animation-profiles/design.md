# Design

## Context

See proposal.md. Tool's SemanticAnimationClipName and StoreEngine currently hardcode the same five motions and a V1 floor. Compatibility samples the same motion and phase on both models. PreviewVariantCoverage duplicates the V1 arrays; AvatarMotionCaptureRunner hardcodes old shared clip names. CreatePreviewPlan admits only Published assets, while editing source invalidates proof seals. Hero Studio already supports arbitrary exact-name clips, independent tracks, and animation-authored referenceInfluence. No provider change is necessary.

## Goals / Non-Goals

**Goals:** one profile authority for future and migrated models; meaningful, readable dungeon motion; exactly the same source-to-proof workflow for animation upgrades; complete current catalog coverage with real visual evidence.

**Non-Goals:** new model geometry, multipart rigs, procedural physics, gameplay state, random variant selection policy, per-pair correction data, new runtime/inspector, provider enum extension, and APK builds.

## Decisions

### 1. Store policy, not provider semantics

Add Store/animation-profiles.yaml as a versioned policy. Each profile has a kind, permitted forms where relevant, required families, allowed optional families, loop policy, and visual brief. Global four-take rule (base, V1, V2, V3) applies to every declared family; extra variants are permitted if distinct. Optional families are opt-in per asset, not placeholders. AssetManifest.animationProfile explicitly identifies the profile. Unknown/mismatched profiles, meaningless or undeclared clips, wrong loop policy, missing base or required variants fail closed. A side-qualified family is optional, but if authored it also has base+V1/V2/V3; side does not substitute for generic coverage. Variant names retain existing positive V<n> grammar and model-local fallback. No name-derived classification or copied curves.

| Profile | Existing asset assignments | Required families | Choreography |
|---|---|---|---|
| avatar.grounded | FoundationGuard01, CultistAcolyte01, SkeletonArcher01, SkeletonGrunt01, SkeletonKnight01 | Idle, WalkForward, Attack, Hit, Death | compact breathing/weight shifts, distinct grounded hop cycles, attacks and collapse |
| avatar.heavy | ZombieBrute01 | same grounded set | slow anticipation, weighted impact and heavy grounded collapse |
| avatar.agile | Ghoul01 | same grounded set | quick irregular bounces, lunges, sharp stagger |
| avatar.floating | Wraith01 | Idle, Glide, Attack, Hit, Death | hover, glide, spectral contraction or dispersal using supported rigid transforms |
| avatar.winged | Gargoyle01 | Idle, Fly, Attack, Hit, Death | lift, banking, dive and fall; rigid wing silhouette, no invented wing joints |
| avatar.caster | LichLord01, Necromancer01 | grounded set | ritual sway, controlled hops, casting gestures, collapse |
| equipment.blade | Sword02, Dagger02, Kukri01 | Idle, Attack, Drop | grip-relative sway, slash/thrust/cut, release/settle |
| equipment.heavy | Axe03, Hammer02, Mace02, WarPick01 | same equipment set | weight, heavy arcs, chop/smash/return, release/settle |
| equipment.polearm | Spear02, Halberd01, Scythe01 | same equipment set | long reach, thrust/sweep/lunge, tip-aware drop |
| equipment.ranged | Bow02, Crossbow02, Rifle02 | same equipment set | distinct aim/release/recoil, shape-aware drop |
| equipment.magical | Staff02 | same equipment set | cast arcs, channel-like movement, release/settle |

Idle, WalkForward, Glide, Fly loop; Attack, Hit, Death, Drop do not. Optional Aim (loop), Reload (nonloop), Cast (nonloop) only when the profile and physical model can express them. Equipment never defines WalkForward, Glide, Fly, Hit or Death for this catalog. No legacy aliases; remove redundant side clips unless retained and fully authored. Grounded WalkForward retains launch/apex/landing/recovery; hovering/winged models use their own locomotion grammar. Profile assignment is a proposed visual default, not a gameplay class. Preserve existing UnitRef/id and canonical +X/+Y/-Z Unity orientation.

### 2. Authoring and mechanical/visual quality

Model source owns actual curves. Use pinned Hero.Voxel source schema and coordinate conversion, existing one-part rigid models and current sockets/Mount. Do not create body or equipment joints. Author meaningful different poses/paths and anticipation/action/recovery, loop closure, non-loop terminal holds. Preserve proportions; avoid arbitrary axis rotations, clipping, floating ground corpses, tiny or singular transforms. Spectral death can contract to a finite positive scale and finish with consumer-owned visibility; no literal zero-scale while bound. Bow/crossbow single-part rigid forms cannot animate a string/trigger independently: use readable whole-object release/recoil, not fictional part motion. Drop begins socket-relative and releases reference, ends detached visually (influence 0) with a plausible relative descent and terminal rest. No real ground collision, loot or attachment removal implied; consumer supplies world contact and visibility. Attack takes may remain attached or release and smoothly return to reference, preserving existing Axe03 release/return proof.

Validate canonical exact clip families and required takes against profile; compare all pairs including non-base variants at normalized times across compiled pose trajectories and reference influence (not just duration or serialized keys). Reject same normalized motion with timing-only differences and unchanged/no-op authored takes. Use deterministic tolerance and dense checkpoints to catch aliases; numerical difference is necessary, not sufficient. Require visual review for distinct choreography, not arbitrary numerical thresholds or auto-PASS. Keep normalized endpoints sensible and validate loop closure/finite supported transforms. Distinct trajectories across models do not require variant numbering synchronization.

### 3. Compatibility proof with independent motions

Replace CanonicalCompatibilitySamples with deterministic cases derived from resolved actual clip inventories. For each compatible physical avatar/equipment pair and slot, sample each authored body take against held equipment Idle, each equipment take against body Idle or applicable locomotion, plus explicit mixed cases (body Death with equipment Drop, different phases, moving body and released Attack with continuous return). Sample ordered sequences with per-track elapsed/normalized phases and the provider's release basis carried across checkpoints; avoid Cartesian product of every variant combination. Report exact clip names, phases, sample order, profile/input hashes, convention version, provider identity and result. Existing Mount bind and no pair correction rules remain. A missing optional motion on a peer is not a compatibility failure; a missing exact requested proof take is. Do not infer motion names from one another or add hardcoded actor death/sync methods. Use exact-name resolver; existing typed VoxelStandardMotion enum is insufficient for new families and remains unchanged.

### 4. Authoring proof without publication deadlock

Tool remains sole authority for resolved profile/required exact clips and proof case planning. Normal preview-plan still emits published assets only. Add explicit candidate proof planning by exact asset id(s), restricted to current validated source, applicable upstream authority and current generated artifacts; allow only proof/compatibility/seal-related incompleteness, never silently relax source/profile or reference/density validity. Candidate mode is labeled, separate from normal published grid and never changes Published flags. Allow selected candidate to compose with known valid peers or another explicitly validated candidate for migration pilot. Capture and compatibility can consume that validated candidate plan before seal; publication continues to demand accepted current evidence and all pair compatibility. Use existing preview baker/runtime grid/registered graphical capture infrastructure; plan supplies resolved exact names and independent checkpoints, not a second clip policy in Unity. Check baked inventory and loop policy; enumerate standalone and composed takes, all four variants, terminal frames, loops and relevant release/return samples. Inspect current attested multi-view ordered captures/contact sheets, including intermediate phases, not just one screenshot. Keep capture outputs disposable under Library; persist compact reviewed per-asset motion evidence with source/effective animation/profile hashes, provider revision, capture case/checkpoint inventory and attestation/image hashes, review outcome and evidence provenance. Retain required visual evidence according to existing repository proof contract; never manufacture acceptance by hashing files. Update proof seal exact input set to cover policy/profile and reviewed motion evidence and all effective authoring inputs. Reject stale or missing motion evidence. Candidate plan never acts as publication authority.

### 5. Preserve one-asset workflow

Update AGENTS/WORKFLOW/specs and pipeline/create/maintenance guidance; Source stage resolves applicable profile and authors required takes. Product Target/Construction/Density remain authoritative for geometry and unchanged upstream accepted evidence can be reused only after verifying exact applicable inputs. If animation-only changes invalidate a density/reference hash under current schema, validate and regenerate affected evidence at earliest owning stage; do not arbitrarily weaken hashes. Replace obsolete ratio-placement guidance with actual model-owned sockets and Mount; no new attachment system. Migrate sequentially as explicit catalog-wide OpenSpec change, each asset remains the single job through proof/compatibility/seal or reports blocker. Pair proof may be evaluated against eligible candidates before their final publication; convergence recomputes all required pair evidence once all are base-valid. Do not use ordinary maintenance to skip a blocked first asset.

## Risks / Trade-offs

- Rigid single-part visuals limit articulation: use expressive silhouette and rigid transforms only; no skeleton redesign.
- 388 minimum authored takes (220 avatar, 168 equipment), additional optional/side takes increase review cost: pilot SkeletonGrunt01, Wraith01 and Axe03 first; then migrate one asset at a time, reuse accepted good takes only, batch graphical captures while reviewing each take independently.
- Legacy consumer clip names removed: explicit breaking content migration, preserve asset IDs and use exact-name selection; no misleading alias or provider API change.
- Preview/publication circularity: explicit candidate plan and pre-seal proof, final published-only grid and verification.
- Evidence staleness: bind to exact content, profile, current proof convention and provider; preserve upstream evidence only when validated.
- Drop is not terrain-aware: authored relative release trajectory, consumer resolves world collision and loot; avoid guaranteed ground-contact claims.
- Numeric trajectory distinction is not artistic acceptance: enforce recorded visual review and fail closed on unavailable graphical proof.

## Migration Plan

1. Capture current clean state and asset inventories, preserve existing four local commits. Implement policy, validation, candidate proof, independent compatibility and workflow changes together; use fixtures for red/green tests.
2. Pilot three assets: SkeletonGrunt01, Wraith01, Axe03. Verify provider generation, actual baked clip inventories, ordered visual proof, compatible independent tracks and safe terminal poses. Correct pilot before expanding.
3. Migrate remaining 22 assets through same pipeline. Keep geometry, palettes, pivots, socket metadata, VPU and ids unchanged; compare compiled geometry/frames and authoritative input differences. Regenerate only affected outputs using pinned Tool, not manual generated edits.
4. Finalize all current pair evidence, per-asset visual acceptance, proof seals and derived catalog. Run Tool tests, real Unity Editor tests, registered provider-attested capture, scan/verify, strict specs and diff check. Require all 25 published, Healthy true, NextJob null and 388+ exact distinct takes. No APK. If proof is blocked, leave migration incomplete with precise task/gate status rather than publish unverified content.
5. Sync/archive only after all required tasks pass. Rollback if necessary by reverting the coherent policy/content/evidence migration together; never mix obsolete seals with new policy. Publication of existing unrelated local commits is not implied.
