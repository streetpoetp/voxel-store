# Design

## Context

The current preview runtime under Preview/Unity/Assets/Preview constructs a GameObject and MeshRenderer for each baked part. VoxelAnimatedAssembly evaluates poses, EquipmentPresentation applies inverse bind Mount, AvatarPresentation samples all models, and AvatarMotionPlayer advances a local phase. The provider now exposes VoxelRenderWorld.Spawn, VoxelRenderInstance and VoxelActorInstance (Attach, CreatePlan, Start, Advance, SampleNormalized, Reset, Dispose). See proposal.md and the delta for scope and behavior.

Observed at exploration: consumer gitlink and lock agree at 8b84ea93; provider local main 053c8955 contains actor foundation, remote main was c7307fe8. A published compatible provider revision is an application prerequisite, not a promise that the current local revision is remotely available. The consumer already has unrelated edits to Collections/combat-foundation.v1.yaml, Profiles/humanoid.medium.v2.yaml and the archived Body-ratio change. Preserve them. The archived design is historical and contradicts current authored socket implementation and parts of the canonical preview spec; this change reconciles only current preview requirements, not store compatibility/publication data.

## Goals / Non-Goals

**Goals:** one provider-backed render/actor execution path; explicit preview-only semantic adapter and lifetime; stable existing user controls and evidence; fail-closed dependency and authored-input compatibility.

**Non-Goals:** new gameplay semantics, asset or clip redesign, a new provider module, new streaming integration, changes to Hero Studio itself within this consumer change, alternate capture infrastructure, APK or production build.

## Decisions

### 1. Pin a published complete provider

Verify remote availability and required APIs, then update Dependencies/hero-studio gitlink and toolchain/hero-voxel.lock.yaml together, including exact workflow SHA-256; use existing whole-package bootstrap. Do not link to a mutable developer checkout, copy provider modules or assume the exploration SHA is final. If the provider requires fixes, stop at the owning boundary and report the separate provider change required before continuing.

### 2. One shared render world for the runtime gallery

The generated runtime gallery owns a VoxelRenderWorld and serialized baked assembly/animation references plus deterministic layout and semantic configuration. Realize nonserialized provider handles on Play Mode entry and dispose them on teardown/rebuild. Standalone equipment and unknown catalog models use direct render instances; composed avatars add VoxelActorInstance. Keep only logical scene transforms needed for controls and selection; synchronize root world matrices to instances, not a parallel per-part hierarchy. Determine bounds using baked geometry and provider-evaluated part matrices (transform mesh bounds corners and union); do not depend on child MeshRenderer discovery or normalize scale. Reuse provider render configuration and submission; do not change the separate authoring bake or provider-owned Editor inspection paths.

Alternative: keep current per-part rendering and use actor only for coordination. Rejected because VoxelActorInstance already composes VoxelRenderInstance and a second realization would duplicate pose/render authority.

### 3. Consumer adapter owns semantics, provider owns mechanics

Retain AvatarPresentation and AvatarMotionPlayer only as preview-facing semantic/lifecycle/control owners. EquipmentPresentation becomes binding metadata (asset, exact physical socket, side, instance and binding identity as needed), not a transform solver. Remove VoxelAnimatedAssembly and its local pose state. Gallery creates a distinct provider instance per occurrence, including repeated references to the same equipment asset. Attach with exact selected parent socket and Mount; use binding IDs rather than asset identity to address children. Consumer supplies ordered existing Motion.Side then Motion candidates per body and attachment; preserve existing driver behavior and current authored assets. Future weapon-to-reaction-profile selection belongs to consumer policy, not this adoption. Cache resolved plans at selection/topology changes; never resolve strings per frame. Reset/rebuild stale plans on rebind, stop, switch and disable. Dispose actor before releasing caller-owned children/root; provider world is last. Do not rely on actor disposal to release caller-owned render instances.

### 4. Playback and capture share resolved execution

Use provider Start/Advance when authored loop matches selected loop. Preserve explicit loop checkbox and speed. Prefer provider generic playback override if the pinned API supports one. Otherwise treat the missing override as a prerequisite provider gap and resolve in H:/projects/hero-studio through a separate scoped change; do not add an independent clip clock or silently disable the checkbox. Direct-phase sampling uses SampleNormalized for capture and SubmitNow before Game View acquisition. Keep the existing editor window, Inspector restart behavior and registered capture scenario. No direct camera output as accepted proof.

### 5. Attachment contract and orientation

Provider uses current parent socket world matrix multiplied by inverse child bind anchor, followed by child sampling at the common phase. Verify grip behavior using actual authored inputs, including clips that animate the Mount-containing part. The previous spec describes dynamic post-child-pose Mount locking; do not claim mathematical equivalence. Preserve visible grip acceptance, prove bind-anchor alignment and measure animated Mount/grip trajectory as relevant; no per-pair corrections or silent fallback. If current clips/provider cannot meet visual acceptance, block adoption and resolve at the owning provider or separately approved authored-input contract; do not edit assets as part of this change. Preserve canonical +X right, +Y up, -Z forward. Remove contradictory ratio/procedural requirements only in the preview delta; keep observable motion quality.

## Risks / Trade-offs

- [Provider not published or loop override absent] -> Fail early, report exact missing contract; no local substitute.
- [Bind-anchor vs animated Mount differs] -> Explicit matrix and dense visual trajectory proof; no weakened acceptance.
- [Provider-rendered objects have no local part GameObjects] -> Adapt grid bounds, selection, tests and capture to actual provider handles/matrices; preserve authored scale and camera framing.
- [Editor domain reload or repeated refresh leaks render handles] -> Serialized inputs only, deterministic initialization/teardown and lifecycle tests.
- [Existing dirty asset/archived changes collide with migration] -> Record baseline, scope edits to preview and dependency; no reset or unrelated commit.
- [Attestation proves acquisition, not visual quality] -> Review current pixels and report failures explicitly.

## Migration Plan

1. Confirm published provider revision and API compatibility; close loop/Mount prerequisites before declaring adoption ready.
2. Synchronize dependency and lock; verify existing bootstrap resolves the complete package.
3. Replace realization with provider world/instances, keeping deterministic layout and standalone entries.
4. Migrate composition, semantic selection, playback, controls and capture to shared actor plans; delete local mechanisms and obsolete references.
5. Adapt EditMode and PlayMode tests to provider-backed observable matrices, lifetimes and semantic controls; run focused tests and existing store verification without production build.
6. Run registered avatar-motion-gallery command when graphical proof is required and review dense current attested images; do not treat compile or attestation as visual PASS.
7. Review diff and workspace hygiene; sync/archive only after implementation gates pass. Keep unrelated edits untouched.

Rollback: revert only this change's scoped preview and dependency edits together if adoption cannot be completed; restore the previous exact pin and lock without resetting unrelated user changes. Do not ship a dual execution path.
