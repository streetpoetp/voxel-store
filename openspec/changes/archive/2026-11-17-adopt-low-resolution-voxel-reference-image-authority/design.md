# Design

## Context

See `proposal.md` for motivation. The current store has one string-only `styleFamily`, a `ReferenceContract` containing only `composition` and `requiredParts`, and validation that derives every Part Sheet as `Parts/<PartId>.svg`. Reference sealing hashes files but does not bind acceptance to concrete style-reference inputs. Hero.Voxel already defines the canonical sequential Composition/Part Sheet visual-construction workflow, including a 3:2 Composition Sheet, one Part Sheet per Part ID, six local orthographics, and flat/unlit construction-authority regions.

The user-approved visual direction is the three uploaded low-resolution voxel character sheets. The uploaded fishing-boat construction board is useful quality/readability inspiration but is not canonical layout authority because it mixes orthographics and other panels into one board, which conflicts with Hero.Voxel's separate Composition/Part Sheet contract.

## Goals / Non-Goals

**Goals:**

- Make the approved low-resolution voxel character look concrete, repository-owned style authority.
- Make reference-sheet authority explicit and impossible to satisfy through hard-coded schematic SVG placeholders.
- Preserve Hero.Voxel as the sole construction-semantics authority.
- Make style-authority changes invalidate stale reference acceptance deterministically.
- Migrate the current V1 assets without changing their stable asset IDs, UnitRefs, compatibility families, or gameplay-neutral boundaries.

**Non-Goals:**

- Do not copy Hero.Voxel sheet semantics or visual-audit checklists into store policy.
- Do not make the fishing-boat board the canonical sheet layout.
- Do not change VoxelSpec compilation, generated VOX ownership, compatibility sockets, or runtime behavior.
- Do not introduce realistic/painterly rendering or higher-detail voxel art as alternate V1 styles.
- Do not regenerate the three approved style-reference images; preserve the exact uploaded images as style authority.

## Decisions

### 1. Separate asset style identity from reference-image rendering style

Keep the existing `styleFamily: combat-foundation.v1` and per-asset `style.family` contract unchanged. Add a separate store-level selector:

```yaml
referenceStyle: low-resolution-voxel.v1
```

The selected style resolves through a repository-owned style manifest:

```text
Art/Style/low-resolution-voxel.v1/
  style.yaml
  reference-01.png
  reference-02.png
  reference-03.png
```

`style.yaml` owns the stable style id, the ordered reference-image paths, and concise visual traits such as chunky low-resolution voxel forms, chibi proportions, simplified readable geometry, restrained detail density, clear color blocking, and strong silhouette readability.

The three character images supplied in this conversation are imported byte-for-byte as `reference-01.png` through `reference-03.png`. Neutral filenames avoid turning visible character archetypes into shared gameplay semantics.

**Alternative considered:** rename `combat-foundation.v1` to `low-resolution-voxel.v1`. Rejected because the former is the content/style-family identity already carried by assets, while the latter describes how visual construction references are rendered.

### 2. Hero.Voxel owns layout; style images own visual language only

The maintenance agent resolves both authorities before generation:

```text
low-resolution-voxel.v1 images
        |
        v
visual style authority
        |
        +-------------------+
                            |
Hero.Voxel workflow --------+
        |
        v
construction semantics/layout
        |
        v
generated Composition / Part Sheets
```

The approved character sheets may show hero/front/side/back arrangements, but those layouts are never copied as construction semantics. The fishing-boat board is not placed in the active style manifest and cannot override Hero.Voxel's canonical separate-sheet flow.

### 3. ReferenceContract v2 explicitly maps every accepted PNG

Replace implicit `.svg` derivation with a versioned explicit contract:

```yaml
version: 2
composition: composition.png
requiredParts:
  - Body
  - Head
partSheets:
  Body: Parts/Body.png
  Head: Parts/Head.png
```

Validation requires:

- `partSheets` keys equal `requiredParts` exactly;
- each mapped path is unique, relative, and remains inside the asset Reference directory;
- Composition and Part Sheet files are valid PNGs;
- the Composition PNG is exact 3:2 aspect ratio;
- the Composition path cannot also be a Part Sheet;
- unreferenced files do not become accepted construction authority.

The tool reads the PNG signature/IHDR directly for format and dimensions; no image-processing dependency is needed.

**Alternative considered:** continue deriving `Parts/<PartId>.png`. Rejected because explicit mapping is clearer, supports future naming changes, and makes accepted authority inspectable without filename convention inference.

### 4. Visual acceptance remains agent-owned but gains deterministic provenance

The deterministic tool cannot prove that a PNG visually contains seven coherent views or matches the approved art style. The maintenance agent remains responsible for visual audit. After acceptance, generated reference audit evidence records:

- active `referenceStyle` id;
- hashes of the style manifest and every concrete style-reference PNG;
- pinned Hero.Voxel workflow fingerprint;
- hashes of `reference.yaml`, Composition PNG, and every mapped Part Sheet PNG.

Reference health recomputes and compares this provenance. Any style image, style manifest, reference mapping, accepted sheet, or Hero.Voxel workflow change makes the prior acceptance stale.

**Alternative considered:** infer visual PASS from PNG existence/dimensions. Rejected because that would reproduce the current false-positive problem in a different file format.

### 5. The maintenance skill supplies a reusable low-resolution style clause, not Hero.Voxel semantics

The store skill will require the agent to inspect all active style-reference images before generation. It may summarize only the project-specific visual language:

```text
chunky low-resolution voxel forms
simplified readable geometry
chibi proportion language
restrained detail density
clear separated color regions
high silhouette readability
no realistic textures or painterly/non-voxel forms
```

For Composition/Part Sheet regions, orientation, views, lighting, part coverage, and semantic exclusions, the skill points to the resolved pinned `voxel-art-workflow.md` instead of duplicating those rules.

### 6. Existing V1 reference migration is a one-time retrospective baseline

The current four assets already have VoxelSpec/proof output. Migration therefore uses the current healthy asset as reconstruction evidence while creating new canonical visual references:

1. import the three exact style-authority images;
2. for one current asset at a time, use its current VoxelSpec/proof plus active style authority as migration input;
3. generate only its Composition PNG and audit it;
4. freeze/confirm `RequiredParts`;
5. generate and audit exactly one mapped Part Sheet PNG at a time;
6. replace the old SVG mapping only after complete accepted PNG coverage exists;
7. reseal reference/proof evidence and run full publication validation.

This retrospective use of current geometry is migration-only. After migration, the accepted PNG package becomes upstream construction authority for future edits.

### 7. Existing schematic SVGs leave the authority path

Once an asset is migrated, its old `composition.svg` and `Parts/*.svg` files are removed rather than retained beside the authoritative package. This avoids future agents mistaking them for accepted references.

Rollback is Git-level: reverting the change restores the previous schema and SVG reference packages together.

## Risks / Trade-offs

- **[Risk] Generated reference images can look stylistically correct but contradict the current model.** -> During migration compare each sheet against current Hero.Voxel proof before acceptance; regenerate the sheet rather than silently changing model geometry.
- **[Risk] Style matching is subjective and cannot be fully automated.** -> Keep style audit agent-owned and bind acceptance to exact style/reference hashes so deterministic tooling can at least detect stale provenance.
- **[Risk] Twelve V1 construction images require multiple generation/audit iterations.** -> Preserve Hero.Voxel's sequential one-output boundary and migrate one asset at a time; do not batch sheets into composite outputs.
- **[Risk] Binary style/reference images increase repository size.** -> Store only the three canonical style images and the minimum accepted Composition/Part Sheet set; do not keep rejected generations.
- **[Risk] Changing Hero.Voxel's workflow fingerprint invalidates reference acceptance.** -> This is intentional fail-closed behavior; re-audit against the new canonical construction contract.

## Migration Plan

1. Add the style manifest contract and import the three approved character images under `Art/Style/low-resolution-voxel.v1/`.
2. Add `referenceStyle: low-resolution-voxel.v1` to store policy and make style resolution/provenance validation deterministic.
3. Implement ReferenceContract v2, PNG/IHDR validation, exact Part-ID mapping, 3:2 Composition validation, and stale-style evidence checks.
4. Update tests so old SVG-backed packages fail the new reference gate before migration.
5. Update the maintenance skill to read style authority before reference generation while deferring sheet semantics to Hero.Voxel.
6. Migrate FoundationGuard01 sequentially, then each equipment asset in its own bounded maintenance run.
7. Remove old schematic SVG authority, regenerate audit/catalog/health state, and require all four assets plus compatibility/coverage to return healthy.
8. Run the planner twice to prove the migrated healthy store is again idempotent.

If migration must be rolled back, revert the change as one repository commit so schema, policy, references, and evidence remain mutually consistent.
