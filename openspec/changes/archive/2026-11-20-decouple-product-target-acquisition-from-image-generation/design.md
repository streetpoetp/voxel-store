# Design

## Context

See proposal.md for motivation. Today Product Target provenance accepts v1/v2 only. `generated` v2 requires `generator.kind=image-model`, provider/model identity, and a hash-bound generation request; `imported` v1 requires source evidence. The Product Target skill also treats an image-generation model and exact-byte transfer as the canonical creation path. Active production guidance already has no Samples lane and must remain production-only.

## Goals / Non-Goals

**Goals:**
- Make Product Target authority depend on accepted repository bytes, provenance, style/fidelity acceptance, and declared upstream authority rather than producer technology.
- Preserve target-first ordering and fail closed before construction/source when Product Target authority is unavailable.
- Keep existing accepted v1/v2 provenance valid without rewriting published assets.
- Keep one production lane under `Assets/**`; do not introduce a Samples or model-first fallback.

**Non-Goals:**
- No Hero.Voxel semantic changes.
- No VPU selection algorithm changes.
- No asset identity, collection, profile, or compatibility model changes.
- No migration of historical archived OpenSpec material.
- No new external image-generation dependency.

## Decisions

### 1. Add provenance v3 for newly accepted provider-neutral targets

Newly accepted Product Targets use provenance version 3. Existing v1/v2 validation stays intact for backward compatibility.

Version 3 distinguishes acquisition authority rather than producer technology:

- `origin: repository-authored`: the accepted target bytes are authored/acquired directly into repository ownership. No generator/provider/model evidence is required. Current target-style authority remains required because the repository authored the target from asset intent plus shared style grammar.
- `origin: imported`: an external approved image or canonical derivative becomes the target. Source path/hash/relation evidence remains required as today.
- `targetSha256` remains mandatory for every origin.
- Declared source/style authority remains hash-bound and staleable.
- Producer/generator metadata is optional traceability only for v3. It does not decide Product Target validity and is not required to exist.

Alternative considered: relax v2 by making its generator fields optional. Rejected because v2 already has established image-model semantics; mutating that meaning would make old evidence ambiguous. A new version gives a clean compatibility boundary.

### 2. Keep Product Target visual acceptance independent from acquisition origin

The same style/fidelity gates apply regardless of how `target.png` was obtained. A hand-authored, locally generated, externally generated, or supplied image does not become authority merely because provenance is structurally valid.

The accepted authority chain remains:

```text
asset identity + shared style grammar
  -> acquire target bytes
  -> provenance validation
  -> visual/style acceptance
  -> freeze Product Target
  -> Composition / Part Sheets
```

Alternative considered: let trusted acquisition methods bypass visual review. Rejected because producer trust is not visual fidelity.

### 3. Keep exact repository bytes, but remove transport as a workflow dependency

An externally visible candidate becomes Product Target authority only when the accepted bytes are repository-owned and hash-bound. The existing import helper may remain as one convenience path, but ChatGPT/WebCodex download behavior, public URLs, or any specific transfer mechanism are not canonical workflow requirements.

If one candidate cannot cross the boundary exactly, the workflow may acquire another valid target through another method. If no acceptable repository-owned target can be established, production remains blocked.

### 4. Retain the existing Product Target skill name but change its responsibility

`.agents/skills/voxel-store-product-target/SKILL.md` remains the specialized owner so callers do not need a new command surface. Its responsibility changes from image-model generation to provider-neutral Product Target acquisition, visual acceptance, freezing, and provenance handoff.

The skill may use:
- an approved supplied/imported image;
- a repository-authored image produced by any available authoring capability;
- an external generator whose exact accepted bytes can be imported;
- any future acquisition method that satisfies the same authority contract.

It must not require ChatGPT, a particular model, provider credentials, or a provider-specific request artifact.

### 5. Keep `voxel-store-create-model` as a single production-only lane

The canonical creation flow remains:

```text
Collection / user form
  -> distinct weapon identity
  -> shared style grammar
  -> acquire + accept Product Target
  -> Composition / Part Sheet(s)
  -> lowest-sufficient VPU
  -> VoxelSpec + Palette
  -> Hero.Voxel
  -> multi-view proof
  -> visual iteration
  -> compatibility
  -> publish to Assets/**
```

If Product Target acquisition/acceptance is blocked, the run stops at that gate. It must not continue with provisional VoxelSpec, Samples content, experimental publication, or any alternate lane.

### 6. Update deterministic validation without weakening legacy contracts

`StoreEngine.ValidateTargetProvenance` gains a v3 branch while preserving existing v1/v2 branches unchanged. New validation tests cover:

- repository-authored v3 target with current style hashes and no generator metadata;
- imported v3 target with current source evidence;
- stale target hash;
- stale declared style/source authority;
- missing required authority for each v3 origin;
- optional producer metadata not being required for validity;
- existing v1 generated/imported and v2 image-model provenance still passing unchanged.

The publication and maintenance tests should assert that current Product Target authority—not image-model identity—is the gate that unlocks Composition/source work.

## Risks / Trade-offs

- [Producer-neutral acquisition can admit low-quality target images] -> Keep existing visual/style acceptance mandatory and independent from provenance structure.
- [Two legacy provenance versions plus v3 increase validator complexity] -> Keep version branches explicit and do not reinterpret v1/v2 semantics.
- [Optional producer metadata can become stale or misleading] -> Treat it as non-authoritative traceability; do not use it to unlock or block production.
- [Imported versus repository-authored origin can be confused] -> Define strict v3 origin rules and test them independently.
- [Workflow wording could accidentally reintroduce a fallback lane] -> Keep active docs/skills explicit that blocked Product Target authority stops production before construction/source.

## Migration Plan

1. Add provenance v3 contracts and validation while leaving v1/v2 behavior unchanged.
2. Update Product Target tests to prove provider-neutral v3 acceptance and legacy compatibility.
3. Update Product Target/create-model/maintenance guidance and `WORKFLOW.md` to use acquire/accept terminology and remove provider-specific canonical steps.
4. Keep `Tool/import-product-target.ps1` only as an optional transport helper; it must not be referenced as a required production dependency.
5. Run Tool tests, scan/verify where applicable, strict OpenSpec validation, and diff hygiene.

Rollback is safe because existing published v1/v2 targets are not migrated. Reverting the v3 validator and guidance restores the previous behavior without rewriting existing target evidence.
