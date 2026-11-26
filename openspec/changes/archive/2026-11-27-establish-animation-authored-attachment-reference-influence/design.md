# Design

## Context

See `proposal.md` for motivation and the three delta specs for required behavior.

The pinned Hero.Voxel provider currently represents a clip as duration, authored loop policy, and part-local position/rotation/scale tracks. `VoxelModelAnimationTrack` already owns selected clip, normalized phase, effective runtime loop state, speed, and model world matrix. Actor bindings expose current body-socket/bind-Mount reference matrices but intentionally do not continuously force child placement.

Voxel-store currently fills that gap with a consumer heuristic: translated non-looping equipment clips latch their selection-start model root, while other clips follow the live attachment reference. Mutable streamed proof applies live references separately. This duplicates policy and cannot express all visual cases.

Compatibility evidence is deterministic generated repository state, but `verify` calls `Scan(writeCompatibility: false)`; therefore all 154 tracked compatibility files can remain on the retired v1 convention while the store still reports healthy.

## Goals / Non-Goals

**Goals:**

- Make visual attachment authority explicit animation data rather than an inference from translation, clip name, or loop metadata.
- Preserve logical binding/handle identity and current attachment-reference queries independently of visual release.
- Support smooth release and smooth return to a moving socket.
- Use one reusable reference-influence resolver for direct and streamed composition.
- Preserve the provider track as the sole playback-phase/loop authority.
- Make tracked compatibility evidence freshness part of normal store verification.

**Non-Goals:**

- No gameplay-facing `AttackTarget`, `ThrowWeapon`, damage, combo, inventory, AI, or Death synchronization API.
- No physical detach/reattach operation for animation-only visual motion.
- No target-dependent trajectory parameterization in this change; Axe03 remains a fixed authored visual trajectory.
- No actor-wide phase clock or body reaction inferred from weapon animation.
- No Player/APK build.

## Decisions

### 1. Add one optional clip-level scalar `referenceInfluence` channel in Hero.Voxel

The provider source grammar gains an optional channel alongside clip metadata:

```yaml
clip: Attack
duration: 0.45
loop: false
referenceInfluence:
  interpolation: smoothstep
  keys:
    - { time: 0.00, value: 1.0 }
    - { time: 0.18, value: 1.0 }
    - { time: 0.30, value: 0.0 }
    - { time: 0.72, value: 0.0 }
    - { time: 1.00, value: 1.0 }
tracks:
  ...
```

Hero.Voxel adds scalar source, compiled, generated-manifest, baked-asset, and sampling types rather than encoding the value as a fake vector/part track. Values are finite and bounded to `[0,1]`; interpolation uses the existing step/linear/smoothstep vocabulary. Missing channel samples as `1`.

`VoxelAnimationClip` owns the channel because the value describes how that whole model take consumes an external attachment reference; it does not belong to one voxel part.

**Alternative considered:** infer release from position tracks. Rejected because translated socket-relative motion and rotation-only released motion are both legitimate.

**Alternative considered:** `SocketAttached` boolean/events. Rejected because it creates an attachment state machine and cannot express continuous return.

### 2. Provider playback exposes sampled influence; consumers never inspect authored loop or tracks

`VoxelModelAnimationTrack` exposes the sampled reference influence for its current selected clip and normalized phase, with `1` when no clip is selected. The value is sampled from the same clip/phase already used for part animation.

No consumer checks `clip.Loop`, position-channel presence, semantic clip names, or terminal translation. `Advance()` already advances/wraps phase from `EffectiveLoop`, so loop overrides automatically drive influence correctly. Exact `SampleNormalized` uses the same influence sampler at the requested phase.

**Alternative considered:** make the consumer combine authored `clip.Loop` with position data more carefully. Rejected because it would preserve duplicate playback authority and still conflate motion shape with attachment authority.

### 3. Use one voxel-store presentation resolver with one retained release basis

Hero Studio stops at the accepted provider boundary:

```text
selected clip + current normalized phase -> sampled referenceInfluence
binding id/handle -> current attachment reference
```

Voxel-store presentation owns one gameplay-neutral resolver implementation shared by both direct and streamed preview composition. Conceptually it owns:

```text
releaseBasisWorld
hasReleaseBasis

Resolve(currentReferenceWorld, influence):
  if no basis:
      releaseBasisWorld = currentReferenceWorld

  if influence >= 1:
      result = currentReferenceWorld
      releaseBasisWorld = currentReferenceWorld
  else:
      result = BlendTRS(releaseBasisWorld, currentReferenceWorld, influence)

  return result
```

The basis is initialized from the current reference when a model/binding becomes active. While influence is exactly `1`, it continuously tracks the live reference. Once influence drops below `1`, the last fully attached reference becomes the retained animation-owned basis. Returning influence toward `1` blends continuously toward the current reference; reaching `1` resumes live reference tracking and refreshes the basis.

Matrix blending decomposes to position/rotation/scale, uses linear interpolation for position/scale and quaternion interpolation for rotation, and fails closed on non-finite/non-decomposable inputs rather than linearly interpolating raw matrix elements.

This supports multiple release intervals in one take: every later interval that begins after influence has returned to `1` captures the then-current reference as its new basis.

The resolver owns no clip selection and no topology. Actor binding APIs continue to own only binding identity and current attachment reference.

The resolver is a reusable voxel-store presentation primitive rather than private logic embedded in `AvatarPresentation`; this prevents direct and streamed preview paths from duplicating the rule while keeping transform policy outside Hero Studio actor/root ownership.

**Alternative considered:** place the resolver in `hero.voxel.actor`. Rejected because the provider's accepted boundary exposes animation/reference primitives and deliberately leaves final visual-transform composition to presentation.

### 4. Direct and streamed composition use the same resolver lifecycle

Direct avatar composition keeps one shared presentation-resolver state per current equipment binding. Mutable streamed composition keeps the same resolver type per current opaque attachment handle.

Lifecycle rules:

- attach/new direct binding: initialize basis from current reference;
- clip selection: does not detach or reset topology; influence at current phase controls the resolver;
- reset/no selection: provider influence becomes `1`, returning to live reference and refreshing basis;
- rebind while influence < 1: preserve release basis, update current reference target, and let current/subsequent influence determine convergence;
- replace: discard replaced model's resolver/playback state and initialize fresh state for the replacement under the preserved public handle;
- detach: discard resolver state for that binding/handle;
- pool release/reacquire: activation-local resolver state is cleared with activation-local attachments/playback.

`AvatarPresentation` and streamed preview request provider-sampled influence plus current binding references and apply the shared presentation resolver output to the corresponding `VoxelModelAnimationTrack.WorldMatrix`. They do not implement separate influence interpretations.

### 5. Axe03 explicitly authors release and return; part trajectories remain ordinary animation

Axe03 `Idle`/Walk remain default influence `1`. Baseline `Attack` and `Attack.V1` receive explicit influence curves:

- start at `1` from the current socket;
- fall to `0` before the independent chop/flight segment;
- stay at `0` while body movement must not drag the Axe;
- rise over a visible interval back to `1`.

Existing position/rotation tracks remain the trajectory itself. Reference influence only controls how the model root relates to the current socket; it does not encode a target or synthesize weapon motion.

Provider-attested capture adds ordered return checkpoints while the body socket has moved, so a one-frame terminal snap cannot pass.

### 6. Compatibility evidence advances to a current deterministic convention and `verify` checks tracked bytes

Compatibility evidence advances from `model-socket-reference-semantic-phase.v2` to a reference-influence-aware convention. Each evidence payload records enough deterministic data to prove the current contract, including current provider identity/fingerprint and sampled equipment reference influence at compatibility checkpoints.

`EvaluateCompatibility` produces the expected evidence payload regardless of whether writes are enabled. Behavior splits only at persistence:

- `scan --write`: serialize the expected payload to each tracked `Proof/Compatibility/*.json`;
- `verify`: serialize the expected payload in memory and compare it with the tracked file using the repository's canonical text normalization rules; missing/mismatched/obsolete evidence fails the compatibility/publication gate;
- ordinary read-only planning may compute compatibility without rewriting evidence.

The comparison is deterministic and semantic-contract-aware; `verify` cannot report healthy when tracked proof is stale merely because live recomputation passes.

All 154 current compatibility files are regenerated once during migration.

**Alternative considered:** only check the `convention` string. Rejected because source/provider changes could leave a correctly named but stale proof file.

### 7. Remove stale shared-clock documentation as part of the migration

`WORKFLOW.md` is updated to name `VoxelModelMotionPlayer` and independent provider tracks only. The retired `AvatarMotionPlayer` hierarchy/API and “one semantic phase clock” sentence are removed from active documentation.

## Risks / Trade-offs

- **[Risk] Adding a clip-level provider field changes generated/baked animation serialization.** → Keep the field optional with default influence `1`; add provider round-trip/backward-compatibility tests before publishing the Hero Studio revision.
- **[Risk] TRS interpolation can behave poorly with singular or negative/non-uniform scales.** → Validate finite decomposable transforms and fail closed; current attachment references are expected to be ordinary presentation transforms.
- **[Risk] Exact random-access sampling cannot reconstruct historical body motion during a release interval.** → The release basis is defined by the binding/reference state when the model becomes active or was last fully influenced, not by hidden historical replay. Capture initializes the same state explicitly before sampling.
- **[Risk] The provider workflow fingerprint changes even though this provider revision only adds animation-reference guidance.** → Compare the pinned workflow diff before migration; when reference-package construction/acceptance rules are unchanged, mechanically re-attest existing reference audits to the new workflow fingerprint without changing accepted reference inputs. Compatibility evidence still regenerates deterministically under its own current convention.
- **[Risk] A consumer could mistake influence for gameplay attachment state.** → Provider naming/docs/specs state that it is visual reference contribution only; voxel-store keeps the resolver in preview presentation rather than actor/topology code, and logical binding remains independently queryable and unchanged.

## Migration Plan

1. In an idle Hero Studio feature workspace, add the optional scalar channel, compiler/generated/baked serialization, sampling, and track exposure with provider tests while preserving presentation-owned transform composition.
2. Publish Hero Studio and advance voxel-store's submodule pin plus `hero-voxel.lock.yaml` together; if the full-workflow fingerprint changes only because of animation-reference guidance, re-attest unchanged reference audits to that exact new fingerprint before running legacy preview/bake baselines.
3. Add voxel-store failing tests for loop overrides, translated-following, rotation-only release, moving-body smooth return, reset/rebind/replace, and streamed/direct equivalence.
4. Replace `VoxelModelMotionPlayer.ShouldFollowAttachmentReference` and streamed unconditional reference application with one shared voxel-store presentation resolver.
5. Author Axe03 influence curves and extend deterministic/provider-attested proof checkpoints.
6. Advance compatibility evidence convention, make `verify` validate tracked proof freshness, and regenerate all compatibility evidence.
7. Update active workflow docs, run full Tool/OpenSpec/Unity/capture/hygiene gates, then sync/archive/publish.

Rollback is a normal Git revert of the consumer commit plus provider pin. Because missing influence defaults to `1`, pre-change assets remain source-compatible with either side of the migration as long as the corresponding provider pin is restored.
