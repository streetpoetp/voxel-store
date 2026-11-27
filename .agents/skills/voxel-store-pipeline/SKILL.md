---
name: voxel-store-pipeline
description: Coordinate exactly one voxel-store asset through the canonical production pipeline while delegating every stage's validity to its existing authority.
---

# Voxel Store Pipeline

Use this skill only inside the `voxel-store` repository after one asset job has been established by an explicit user request or deterministic maintenance selection.

## Contract

- Own exactly one asset-scoped job per invocation. Keep that same asset until it is published, intentionally stopped, or blocked.
- Follow the canonical stage order below. Never skip an unresolved earlier stage.
- This skill coordinates stage ownership only. It does not redefine Product Target acceptance, Hero.Voxel construction, density, source, proof, compatibility, sealing, or publication rules.
- A stage is **not applicable** only when that stage's owning contract explicitly permits that result. Do not infer a shortcut from asset kind or convenience.
- After completing a stage, re-read the owning deterministic state/evidence. If an authoritative check reports an earlier unresolved gate, return to that gate for the same asset.
- On a blocked stage, report the exact owning authority/evidence that blocks progress and stop. Do not select a second asset.

## Canonical stages

1. **Intent**
   - Owner: explicit user intent or deterministic Collection/Tool selection plus current store identity/style contracts.
   - Establish the one asset job and the requested reusable form/identity boundary.

2. **Product Target**
   - Owner: `voxel-store-product-target` plus current Product Target/style/provenance contracts.
   - Advance only when that owner reports current accepted authority.

3. **Construction**
   - Owner: the repository-pinned Hero.Voxel `voxel-art-workflow.md` plus current target/style fidelity contracts.
   - Use the construction evidence required by those owners; do not restate their sheet semantics here.

4. **Density**
   - Owner: `reference-density-selection`, active store policy, and deterministic Tool evaluation.
   - Treat this stage as not applicable only when those authorities say density evidence is not required.

5. **Source**
   - Owner: pinned Hero.Voxel authoring workflow plus deterministic Tool/source validation.
   - Source must consume current upstream authority; this skill does not define VoxelSpec semantics.

6. **Generate**
   - Owner: repository Tool and pinned Hero.Voxel generation contract.
   - Generate/re-generate only from authoritative current source.

7. **Runtime Presentation Proof**
   - Owner: current `unity-baked-preview`, proof, and compatibility contracts.
   - Run provider-backed runtime/capture proof when those owners require it. Otherwise record the stage as not applicable; do not invent a universal Unity-capture gate.

8. **Compatibility**
   - Owner: `asset-compatibility` plus deterministic Tool evaluation.
   - Use the current compatibility evidence selected by that authority.

9. **Seal**
   - Owner: current reference/proof acceptance contracts and Tool sealing commands.
   - Seal only evidence already accepted by its owner; sealing is never acceptance by itself.

10. **Publish**
    - Owner: `asset-publication` plus Tool scan/verify and derived publication state.
    - Completion means authoritative current state reports the same asset healthy/published.

## Gate loop

After each stage for the same asset:

```text
PASS           -> re-derive authoritative state and continue
NOT APPLICABLE -> continue only when the stage owner permits it
FAIL           -> repair that same stage for the same asset
BLOCKED        -> report owner/evidence and stop
PUBLISHED      -> stop
```

Do not maintain a second manual validation checklist beside the Tool, OpenSpec specs, specialized skills, or pinned Hero.Voxel workflow.
