# Design

## Context

See `proposal.md` for motivation. The generalized `V<n>` grammar, side+variant fallback, model-local selection, structured coverage projection, and shared-phase Hero.Voxel.Actor playback already exist in the completed `establish-generalized-model-animation-variants` change. This change consumes that foundation and raises the published content baseline.

The current audited publication set contains 25 reusable models: 11 avatars and 14 equipment models. Only `ZombieBrute01` and `WarPick01` currently author V1 takes. The remaining 23 models are baseline-only, and `ZombieBrute01` still lacks `Attack.Right.V1` under the proposed symmetric avatar coverage rule.

Current avatars are single-part `Body` models with side-aware equipment sockets. Current equipment is one-part with a model-owned `Mount` socket. Variant authoring must preserve those existing structural and attachment constraints.

## Goals / Non-Goals

**Goals:**
- Move the published store from variant-capable to genuinely multi-animation by requiring one alternate V1 take for every reusable motion family.
- Keep all new motion authored explicitly in model source and regenerate derived manifests/proofs from that source.
- Make duplicate/alias V1 clips fail publication mechanically.
- Keep exact model clip inventory authoritative for Unity Inspector choices.
- Verify all published models mechanically while keeping graphical runtime proof bounded and representative.

**Non-Goals:**
- Replacing the existing `V<n>` grammar or fallback order.
- Requiring V2 or equal variant counts beyond the minimum V1 publication floor.
- Adding random/weighted selection, gameplay meaning, combos, moods, or state-machine policy.
- Adding avatar limbs, weapon-specific runtime animation systems, attachment correction, or Hero Studio API changes.
- Making the preview Inspector or runtime gallery an authoring authority.

## Decisions

### 1. Raise publication to a kind-specific minimum V1 matrix

Avatar minimum:

```text
Idle.V1
WalkForward.V1
Attack.Left.V1
Attack.Right.V1
Hit.V1
Death.V1
```

Equipment minimum:

```text
Idle.V1
WalkForward.V1
Attack.V1
Hit.V1
Death.V1
```

Baseline clips remain mandatory. Existing avatar `Attack.Left` / `Attack.Right` remain the side-only fallbacks.

This gives every published motion at least two authored presentations while retaining side as an independent physical dimension.

**Alternative considered:** require only one V1 anywhere on the model. Rejected because a model could still expose only one Death, Hit, or locomotion take and still be described as multi-animation.

**Alternative considered:** require V1 only for avatars. Rejected because attached equipment is independently animated and should provide matching presentation diversity without relying on fallback for every alternate take.

### 2. Author V1 explicitly in each model source

Every required V1 clip is written into the model's authoritative `*.voxel.yaml`. Generated animation manifests, proof images, audit files, catalog/health output, and Unity baked assets remain derived.

A migration helper MAY assist with repetitive editing, but accepted source must contain explicit clip data and remain readable without the helper. No runtime alias table and no generated-only V1 layer is introduced.

Variants preserve the existing semantic action of their model. There is no universal replacement curve applied blindly across melee, ranged, caster, or creature assets. An authored V1 may vary timing, amplitude, direction, recoil, sway, fall direction, or keyed transforms while preserving the semantic motion and canonical frame.

**Alternative considered:** synthesize V1 procedurally in Unity from the base clip. Rejected because the model would still own one authored animation and preview/runtime behavior would become the hidden authority.

### 3. Compare compiled animation fingerprints to reject aliases

Publication computes a canonical authored-motion fingerprint for each required clip from compiled animation content excluding clip identity. The comparison includes duration, loop value, track set, interpolation, keyed normalized times, and keyed transform values.

Required comparisons are:

```text
Idle.V1          vs Idle
WalkForward.V1   vs WalkForward
Hit.V1           vs Hit
Death.V1         vs Death
Attack.Left.V1   vs Attack.Left       (avatar)
Attack.Right.V1  vs Attack.Right      (avatar)
Attack.V1        vs Attack            (equipment)
```

If the fingerprints are identical, publication fails. This gate proves a take is not an exact alias; it does not claim that numerical difference alone guarantees artistic quality.

**Alternative considered:** compare source YAML text. Rejected because formatting or key ordering could differ while compiled motion remains identical.

**Alternative considered:** require a minimum numeric delta threshold. Rejected because motion scale varies by model and such a threshold could reject valid subtle Idle variation or accept visually poor large deltas.

### 4. Preserve existing structural motion constraints during authoring

Avatar V1 clips continue to target actual authored parts only; current single-part avatars may animate `Body` only. Side-qualified Attack V1 clips preserve the existing left/right physical context.

Equipment V1 clips obey the existing Mount-bearing-part rule. In current one-part equipment, V1 clips may rotate/scale the part but must not translate the Mount-bearing part while equipped.

Authoring does not alter canonical forward, socket placement, Mount authority, asset identity, or UnitRef.

### 5. Migrate the current inventory in model-owned batches

The audited migration contains 126 new clips:

- 61 avatar clips.
- 65 equipment clips.
- `WarPick01` already satisfies the equipment minimum.
- `ZombieBrute01` already satisfies all avatar minimums except `Attack.Right.V1`.

Implementation should process bounded model groups and regenerate/validate after each group so one malformed source does not hide behind a repository-wide edit.

The first completion gate is not "126 YAML blocks exist"; it is "all 25 published models pass the new coverage and fingerprint validators."

### 6. Keep Inspector choices entirely model-driven

The `AvatarMotionPlayer` component Inspector enumerates exact clip names from the avatar's currently bound baked animation asset. Selecting an exact clip is parsed back into the existing semantic motion + optional side + optional variant state.

The Inspector does not own a table such as `Death, Death.V1, Death.V2`. If a future model authors `Death.V2`, it appears after rebake without changing Inspector code. If another model does not author it, it does not appear.

The currently unpublished Inspector correction is folded into this change rather than shipped as a separate semantic system.

### 7. Separate exhaustive coverage proof from bounded graphical integration proof

Every published model receives exhaustive machine checks for:

- required exact V1 names,
- loop policy,
- non-identical compiled fingerprint,
- existing socket/Mount animation constraints,
- generated catalog/health completeness,
- baked Unity clip presence.

Each changed model regenerates its standalone animation proof/audit evidence. Those proofs are visually reviewed for semantic readability and obvious source/attachment regressions before sealing.

The command-driven `avatar-motion-gallery` Game View capture remains the integration proof for actual composed playback, side-aware attachment, shared phase, and variant selection. It stays bounded rather than producing a combinatorial screenshot matrix for all 126 clips.

**Alternative considered:** capture every required V1 clip through Game View. Rejected because it adds high graphical execution cost while duplicating per-model publication/proof coverage; the integration path only needs to prove the shared runtime mechanism.

## Risks / Trade-offs

- [Large authored migration can introduce copy/paste mistakes] → process bounded model groups and run source validation/generation after each group.
- [A numerical difference can still be artistically weak] → compiled fingerprint is only the mechanical alias gate; standalone animation proof remains visual acceptance.
- [One generic V1 style could erase model identity] → preserve each model's existing semantic motion and action direction; do not apply one universal curve blindly.
- [Ranged equipment could receive melee-like alternate attacks] → vary the existing model's attack/recoil behavior rather than changing its semantic action.
- [Publication becomes stricter for future assets] → make the failure report list exact missing V1 clips and alias comparisons so authoring requirements are actionable.
- [Concurrent generalized-variant change has not yet been archived] → this change assumes that already-implemented grammar/coverage foundation remains present and must be archived/applied before this change is finalized into main specs.

## Migration Plan

1. Add minimum-coverage and compiled-fingerprint validation with focused tests, while keeping the change unaccepted until model migration is complete.
2. Author missing avatar V1 clips in bounded groups, including `ZombieBrute01 Attack.Right.V1`; regenerate each changed model and inspect its standalone animation proof.
3. Author missing equipment V1 clips in bounded groups; regenerate each changed model and inspect its standalone animation proof.
4. Regenerate catalog/health state and verify all 25 published models report complete required V1 coverage.
5. Complete the model-driven Inspector clip selector and tests using actual baked clip inventories rather than hard-coded choices.
6. Rebuild the full Unity baked preview tree and verify exact required clip presence for every published avatar/equipment animation asset.
7. Run Tool tests, store scan/verify, Unity EditMode tests, strict OpenSpec validation, and provider-attested `avatar-motion-gallery` capture; inspect the bounded runtime proof.
8. Seal accepted proof only after visual review, then publish the change when requested.

Rollback reverts the V1 source additions and stricter publication gate together, regenerates derived manifests/proofs/catalog state, and rebakes Unity preview assets. The pre-existing baseline/side-only motion contract remains usable after rollback.
