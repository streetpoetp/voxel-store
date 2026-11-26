# Design

## Context

See `proposal.md` for motivation. The current store is pinned to Hero Studio `d94c4818`, whose consumer preview continuously re-applies actor binding placement. Current store policy also rejects any position channel on the one-part Mount-bearing equipment part and compatibility fails when the evaluated animated Mount origin drifts from the body socket.

Hero Studio `61bf50c135d509d4b59e051143475f708dd78aa1` changes the reusable boundary: a binding exposes a current `parentSocketWorld * inverse(bindMount)` reference, while `VoxelModelAnimationTrack` owns the model's final visual world transform. The provider no longer exposes `RefreshBindings`/actor placement reset. Logical attachment identity and visual transform are therefore separate.

The voxel-store preview remains an Editor-only consumer. Store source remains authoritative; no production game runtime is introduced here.

## Goals / Non-Goals

**Goals:**
- Adopt the published provider revision without copying or locally patching Hero Studio.
- Let one-part equipment author finite position tracks as part of ordinary semantic clips and variants.
- Keep bind-pose `Mount` as the physical attachment reference while allowing evaluated animated Mount drift.
- Make preview socket-relative behavior an explicit consumer operation through attachment reference + model track, rather than provider-driven child placement.
- Prove the new behavior with `Axe03` baseline `Attack` and `Attack.V1` spatially distinct authored trajectories while `Idle` remains a stable carried hold.
- Preserve independent model playback, opaque attachment identity, warm-pool behavior, and existing capture authority.

**Non-Goals:**
- No new VoxelSpec or asset-manifest schema.
- No `SocketAttached`, detached/reattached timeline state, placement-mode enum, or clip-name interpretation in Hero Studio.
- No damage, hit timing, target, projectile, inventory, AI, or other gameplay semantics.
- No Player/APK build.
- No redesign of every existing equipment animation in this change; `Axe03` is the bounded proof asset.

## Decisions

### 1. Upgrade the provider pin before adapting consumer code

`Dependencies/hero-studio` and `toolchain/hero-voxel.lock.yaml` will move together to `61bf50c135d509d4b59e051143475f708dd78aa1`. The workflow fingerprint will be recomputed from the pinned provider document rather than copied from another checkout.

This keeps the repository-owned submodule as the only provider authority and ensures compile failures expose every obsolete `RefreshBindings`/`Reset` dependency before migration.

Alternative considered: emulate the new API locally while retaining the old provider. Rejected because it would duplicate provider ownership and leave the store testing a contract consumers cannot actually pin.

### 2. Keep bind-Mount reference alignment, remove animated-Mount coincidence as a gate

Static composition remains:

```text
attachmentReference = currentBodySocket * inverse(bindMount)
```

Compatibility will continue to prove that the bind Mount resolves the selected body socket within tolerance. For animated samples it will evaluate both the current attachment reference and the authored animated equipment pose, require finite matrices/data, and record animated Mount displacement as evidence rather than treating non-zero displacement as failure.

The Tool will remove the categorical rejection of a position channel on the one-part Mount-bearing root part. Existing one-part, single-Mount, Mount-at-bind-pivot, canonical-frame, semantic-clip, and loop-policy rules remain.

Alternative considered: use the evaluated animated Mount as the inverse placement input so the visible grip always remains locked. Rejected because this feeds authored animation back into placement and cancels the very position/rotation motion the asset owns.

### 3. Animation data decides whether preview follows or latches the attachment reference

The preview consumes the provider reference through the model track, but it does not blindly overwrite every equipment root whenever the body moves. The current authored clip decides the generic visual behavior without a separate placement state:

```text
new equipment clip selection
    -> query current attachment reference
    -> attachment track.SetWorldMatrix(reference)       # start from the live socket

looping clip OR clip with no authored translation
    -> keep applying current attachment reference       # socket-relative

non-looping clip with authored translation
    -> keep the selection-start model world matrix      # body/socket motion cannot drag it
    -> authored part translation/rotation runs normally
    -> if terminal authored translation returns to zero,
       apply the then-current attachment reference      # authored return to socket
    -> if terminal translation remains displaced,
       keep the latched model world matrix               # e.g. a planted/fallen take
```

This rule is derived only from the authored clip's ordinary loop/position data. It introduces no `SocketAttached` state, clip-name table, attack/throw meaning, or second placement schema. Applying the model root reference does not cancel authored part-local position/rotation channels, while latching the selection-start root lets a translated non-looping take complete its world-space visual path even if the body socket moves.

`AvatarPresentation` will expose the current reference for one `EquipmentPresentation`, an operation to apply one current reference at clip selection, and a refresh that updates only equipment whose current authored clip remains reference-following under the rule above. Its old `RefreshBindings`/actor `Reset` path is removed.

`VoxelModelMotionPlayer` will classify its current selected equipment take from the baked clip itself. A looping clip is reference-following. A non-looping clip is also reference-following when it has no meaningful authored position channel. A non-looping clip with meaningful authored translation latches its selection-start root until its terminal pose; when all authored position channels terminate at zero it resumes the current reference at phase 1, otherwise it remains visually displaced.

`VoxelRenderPresentation` will stop re-synchronizing an externally owned render instance from `Transform.localToWorldMatrix` on every `EnsureInstance`/bounds access. For externally owned models the provider/model track is the visual world-transform authority after realization; body synchronization and equipment reference application are explicit through the composition owner.

Alternative considered: add a preview-only placement-mode enum per clip. Rejected because the animation curves already express the spatial difference and a second mode system would duplicate authored take semantics.

### 4. Capture validates reference ownership, not animated grip lock

Avatar capture will no longer compare the evaluated animated `Mount` origin to the body socket for every checkpoint. It will verify:
- the equipment still has its original valid binding identity;
- the current attachment reference is finite and matches the expected body-socket/bind-Mount relationship;
- the equipment model track world matrix equals the current reference only when the current authored take is reference-following;
- a translated non-looping take retains its latched selection-start world matrix while body/socket motion changes the separately queryable current reference;
- authored evaluated part/Mount transforms may differ from that root reference.

Mutable streamed capture will use handle-addressed `GetAttachmentReferenceWorldMatrix(handle)` plus the attachment track world-transform surface. Stale-handle/topology assertions remain unchanged.

Alternative considered: keep a large grip tolerance for animated motion. Rejected because any tolerance still encodes continuous Mount coincidence as the intended invariant.

### 5. `Axe03` proves spatially distinct V0/V1 takes using ordinary authored tracks

`Idle` keeps a stable carried pose with no authored translation.

Baseline `Attack` (the semantic V0/base take) gains a modest semantic-forward chop trajectory plus its rotation curve and returns its authored translation to zero at the terminal key. `Attack.V1` gains a materially different position+rotation trajectory with a stronger lateral/forward flight/sweep and also returns its authored translation to zero. Both remain non-looping and use the same one-part `Axe`/single-`Mount` source structure. Their model root is latched at the selection-start reference while the translated take is in flight, so concurrent body motion does not alter the weapon trajectory; at terminal zero translation the preview resumes the then-current socket reference.

The exact curve values will stay in the asset's existing source coordinate/scale conventions and will be accepted only if Tool proof plus Game View capture shows:
- both takes visibly leave the Idle trajectory;
- the same normalized attack checkpoint produces materially different V0/V1 world poses;
- the attachment binding/reference remains valid throughout.

No metadata tells runtime code that one take is "detached", "thrown", or otherwise special.

## Risks / Trade-offs

- **[Risk] Existing equipment position tracks could become valid accidentally after the blanket rejection is removed.** → Keep all other composition/frame/semantic validation and add focused Tool tests proving only finite authored motion under a valid bind-Mount contract is accepted.
- **[Risk] Externally owned preview models may be silently overwritten by `VoxelRenderPresentation.SyncWorldMatrix`.** → Gate synchronization by lifecycle ownership and add PlayMode tests that a track-authored/reference-applied world matrix survives bounds/submission operations.
- **[Risk] Capture could become too weak if grip equality is simply deleted.** → Replace it with explicit binding/reference/world-root assertions and add positive proof that authored Mount drift occurs while identity remains valid.
- **[Risk] V0/V1 differences are technically different but visually trivial.** → Require deterministic pose comparison plus provider-attested Game View review before sealing Axe03 proof.
- **[Risk] Other store docs still describe continuous lock.** → Search source/spec/docs for `RefreshBindings`, `Mount lock`, `locked to`, and equivalent active wording during implementation and update only affected durable authorities.

## Migration Plan

1. Fetch and checkout Hero Studio `61bf50c1` in the repository-owned submodule; update lock revision and workflow fingerprint.
2. Run compile/focused tests to expose all old provider API call sites.
3. Update Tool policy/tests first: accept finite equipment position channels and convert compatibility from animated-Mount equality to bind-reference validation plus drift evidence.
4. Migrate ordinary and streamed Unity preview/capture paths to explicit attachment-reference/model-track transform ownership; update PlayMode tests.
5. Author the bounded `Axe03` V0/V1 spatial trajectories, regenerate provider-owned generated siblings/proof, and run store verification.
6. Run Unity EditMode/PlayMode and registered Game View capture for the affected preview surfaces; visually inspect the current attested Axe evidence before sealing.
7. Sync/archive the validated OpenSpec deltas, run Git hygiene, commit, reconcile upstream, push normally, and verify remote HEAD.

Rollback is a normal source/provider-pin revert. No persistent data, package layout, GUID, serialized schema, RPC, or protocol migration is introduced.
