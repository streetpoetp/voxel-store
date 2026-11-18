# Design

## Context

See `proposal.md` for motivation. Today the store has reference-contract v2 (`composition + requiredParts + partSheets`) and style-manifest v2 (`references.common/kind`). The current equipment scope contains `equipment-bow-source.png` and a simplified `equipment-general-reference.png`, while polished images such as `reference-02.png` are avatar-scoped and therefore intentionally invisible to equipment generation. Reference audits hash only the currently resolved style inputs plus Composition/Part sheets.

This produces two distinct problems: equipment Product design can inherit an already-simplified style board, and the reference contract has no asset-specific visual target against which a structurally valid construction sheet can be rejected for fidelity loss.

## Goals / Non-Goals

**Goals:**
- Preserve a high-fidelity asset-specific Product Target before construction authority is frozen.
- Keep global reusable style language, asset-specific Product Target, and Hero.Voxel construction authority as separate layers.
- Make target-to-sheet visual acceptance explicit, provenance-bound, and staleable.
- Introduce shared high-fidelity target-generation inputs without invalidating every existing v2 reference audit.
- Keep density/VPU decisions downstream of accepted visual authority.

**Non-Goals:**
- Replacing Hero.Voxel sheet semantics, compiler, preview renderer, or greedy mesher.
- Automatically deciding visual quality from SSIM/pixel similarity alone.
- OCR-enforcing every sentence embedded in a PNG.
- Migrating all currently published assets to v3 in this change.
- Rebuilding published Axe01 geometry merely to demonstrate the new reference contract.

## Decisions

### 1. Add target-driven reference-contract v3 while preserving v2

Reference-contract v2 remains valid for unchanged legacy assets. Target-driven packages use v3:

```yaml
version: 3
target:
  image: target.png
  provenance: target.provenance.json
composition: composition.png
requiredParts:
  - Axe
partSheets:
  Axe: Parts/Axe.png
fidelity: fidelity.audit.json
```

The Product Target has no required 3:2 aspect ratio; it is visual-fidelity authority. Composition remains exact 3:2 and Part Sheets remain governed by the pinned Hero.Voxel workflow.

Rationale: versioning makes the stronger contract enforceable for new/redesigned assets without making today's v2 catalog unhealthy.

Alternative considered: make `target.png` optional inside v2. Rejected because future agents could omit it and silently bypass the new fidelity behavior.

### 2. Give Product Target generation a separate high-fidelity style input set

Evolve the style manifest compatibly so target-driven generation can resolve a dedicated target-authority set while legacy construction-style resolution stays unchanged:

```yaml
version: 3

references:
  avatar: [...]
  equipment:
    - equipment-bow-source.png
    - equipment-general-reference.png

targetReferences:
  common:
    - reference-02.png
  equipment:
    - equipment-bow-source.png
    - equipment-general-reference.png
```

Legacy v2 reference validation continues to resolve only `references`. Target-driven Product Target creation resolves `targetReferences.common + targetReferences.<kind>`.

The current style-authority hash is built from resolved paths/hashes rather than the manifest file bytes. Therefore supporting manifest v3 while preserving the old `references` resolution keeps existing v2 audit hashes stable. Target-driven provenance receives a separate target-style authority hash.

Rationale: simply adding `reference-02.png` to today's legacy `references.common` would intentionally stale all existing assets. A separate target-authority set gives new work the polished visual language without forcing a catalog-wide re-audit.

### 3. Product Target is generated or imported before construction sheets

The target workflow is:

```text
applicable target style authority + asset intent
                    |
                    v
             Product Target
                    |
                    v
         target visual acceptance
                    |
                    v
             Composition Sheet
                    |
             fidelity audit
                    |
                    v
           RequiredParts frozen
                    |
                    v
              Part Sheet(s)
                    |
             fidelity audit
                    |
                    v
           density/source workflow
```

If the user supplies a suitable asset image, the target may be an exact repository-owned copy or canonical derivative. If no product image exists, the agent generates the target from target-style authority plus form intent.

Rationale: a Product Target gives construction-sheet generation a concrete "make this object" authority instead of asking it to infer product richness directly from a generic style board.

### 4. Product Target provenance is explicit

`target.provenance.json` records stable provenance without timestamps:

```json
{
  "version": 1,
  "origin": "generated",
  "source": null,
  "targetStyleAuthoritySha256": "...",
  "targetStyleSha256": {
    "Art/Style/low-resolution-voxel.v1/reference-02.png": "..."
  }
}
```

For imported targets, `source` records the external filename/identifier when available, source SHA-256, and whether `target.png` is exact or a canonical derivative.

Validation recomputes current target-style authority for generated targets. A target-style input change stales target provenance and therefore downstream fidelity.

### 5. Fidelity evidence is structured semantic evidence, not automated image scoring

`fidelity.audit.json` binds the exact Product Target and each audited construction sheet:

```json
{
  "version": 1,
  "target": {
    "path": "target.png",
    "sha256": "..."
  },
  "sheets": {
    "composition.png": {
      "sha256": "...",
      "criteria": {
        "silhouette": "pass",
        "proportions": "pass",
        "constructionRichness": "pass",
        "materialColorHierarchy": "pass",
        "meaningfulDetailDensity": "pass",
        "visualEvidenceDominance": "pass",
        "implementationNeutral": "pass"
      }
    },
    "Parts/Axe.png": {
      "sha256": "...",
      "criteria": {
        "...": "pass"
      }
    }
  }
}
```

The tool validates exact sheet coverage, hashes, required criterion names, and that every criterion is `pass`. The visual decision itself remains agent/human-owned, consistent with the existing maintenance skill's creative-audit boundary.

Rationale: raw pixel similarity is unreliable across hero/perspective vs orthographic construction views. The required criteria make semantic visual judgment explicit and auditable while deterministic tooling owns staleness.

### 6. Visual-first sheets are an acceptance rule, not a new sheet layout

Hero.Voxel remains authoritative for the Composition/Part layout. This change does not invent a second template. Instead the target-fidelity audit rejects sheets where:
- asset/construction views are too small to judge;
- explanatory text carries information the visible object does not;
- silhouette/material/detail richness is materially collapsed;
- the target's meaningful secondary/tertiary construction is described but not visible.

Rationale: the current Axe sheet is legal structurally; changing layout alone would not guarantee fidelity. Acceptance must judge the visual evidence inside the canonical layout.

### 7. Exact voxel bounds/VPU are forbidden from visual authority

The `implementationNeutral` fidelity criterion explicitly fails Product Target/Composition/Part evidence that attempts to freeze:
- exact VPU;
- exact W x H x D voxel dimensions;
- source voxel budget;
- runtime pivots/sockets/transforms.

Target sheets may communicate relative proportions and construction thickness visually. Numeric density/bounds become authoritative only downstream.

For equipment already using `density: reference-driven`, the sequence becomes:

```text
manifest
  -> target-driven reference v3
  -> density
  -> source
  -> generation
  -> proof
  -> budget
  -> compatibility
```

Rationale: the current `Target Bounds: 6 x 3 x 10 voxels` failure happened because a tiny implementation budget was frozen before the visual design was proven.

### 8. Reference sealing includes target/fidelity evidence

For v3 packages, reference enumeration includes:
- `reference.yaml`;
- Product Target image;
- target provenance;
- Composition;
- every required Part Sheet;
- fidelity audit.

Before sealing, validation checks target provenance and fidelity currentness. The generated `audit.generated.json` then hashes the full v3 reference package and existing construction-style authority.

V2 enumeration/sealing remains byte-for-byte behaviorally equivalent.

### 9. Maintenance skill becomes target-first only for target-driven jobs

Update `.agents/skills/voxel-store-maintain/SKILL.md`:

- unchanged v2 repairs may remain Composition-first;
- new asset design or intentional visual redesign uses v3;
- resolve target-style authority before Product Target creation;
- inspect Product Target visually before Composition;
- audit Composition against target before freezing `RequiredParts`;
- audit one Part Sheet at a time against Product Target and Hero.Voxel;
- never write exact VPU/voxel bounds into visual reference authority;
- after reference acceptance, proceed to density when applicable, then source/proof/compatibility;
- a failed target/fidelity check owns the same asset/sheet until repaired.

### 10. Use a bounded non-published visual proof

Do not migrate published Axe01 in this change; its current low-detail sheets remain useful negative evidence.

Add a non-published target-fidelity proof package under `Art/Samples/` using existing repository-owned high-fidelity visual inputs. The proof shall demonstrate:
- a Product Target that visibly inherits the polished common voxel language;
- a visual-first Composition example with large readable object evidence and minimal annotation;
- representative Part construction evidence tied to the same target;
- a passing fidelity audit with no exact VPU/voxel-bound declarations;
- only after reference acceptance, a non-published VoxelSpec/palette compiled by the pinned Hero.Voxel Tool into a generated VOX;
- individual rigid-part and assembled Hero.Voxel preview evidence plus a target-to-model visual audit showing the authored model preserves the target's silhouette, proportions, material/color hierarchy, and meaningful construction detail.

The model proof is downstream evidence only: it must not move VPU, voxel bounds, pivots, or implementation geometry back into Product Target/Composition/Part authority. Tool tests separately prove full v3 contract coverage, staleness, fail-closed reference behavior, and deterministic compilation of the sample model.

Rationale: the workflow can be proven without forcing an unrelated published asset/source migration.

## Risks / Trade-offs

- **[Risk] Fidelity audit can be falsely marked PASS by an agent.** -> Keep fixed criteria, bind exact hashes, require visual inspection in the maintenance skill, and treat the audit as review evidence rather than pretending code can infer artistic truth.
- **[Risk] Style manifest v3 accidentally stales legacy assets.** -> Legacy reference validation resolves only the unchanged `references` set; target-driven assets resolve `targetReferences` separately. Regression-test existing style authority hashes.
- **[Risk] Product Target becomes another text-heavy board.** -> Product Target is image authority, not a documentation sheet; visual-evidence-dominance applies to construction sheets and Product Target acceptance rejects diagrammatic substitutes.
- **[Risk] Generated target invents unrelated art direction.** -> Target provenance binds the exact common + kind-specific target-style inputs and requires visual acceptance before construction generation.
- **[Risk] Richer target drives excessive VPU.** -> Target fidelity preserves meaningful detail, while the existing downstream density selector still chooses the lowest sufficient VPU and can reject meaningless micro-detail as non-driving.
- **[Trade-off] V2 and v3 coexist temporarily.** -> Intentional incremental migration; v3 is mandatory for new/redesigned work while unchanged assets remain stable.

## Migration Plan

1. Extend style/reference contracts to read legacy versions and target-driven v3 data without changing existing v2 authority hashes.
2. Add target-style resolution and target provenance validation.
3. Add v3 Product Target/fidelity contracts and deterministic staleness validation.
4. Add v3 reference sealing/enumeration while preserving v2 reference behavior.
5. Update maintenance skill to target-first workflow and no-premature-density rule.
6. Add target-driven positive/negative fixtures and the non-published visual proof.
7. Verify existing published assets remain healthy and legacy v2 reference audit hashes do not change.
8. Verify a v3 package fails on missing/stale target, failed criterion, stale sheet hash, or implementation-density authority, and passes when all evidence is current.
9. Run full store tests, strict OpenSpec validation, deterministic regeneration/provenance checks, and workspace hygiene review.

Rollback removes v3/target-style support and the sample proof while leaving all legacy v2 assets and existing density/source data unchanged.
