# Design: Purpose-driven voxel-store repository layout

## Domain model

The repository should expose its actual product concepts directly:

```text
Styles ----> Assets <---- Profiles
               ^
               |
          Collections

Store governs the library.
Catalog is derived state.
Samples are non-product experiments.
Tool validates/indexes/maintains the domain.
```

## Target layout

```text
Assets/
  Avatars/<Asset>/
    asset.yaml
    Reference/
    Source/
    Proof/
  Equipment/<Asset>/
    asset.yaml
    Reference/
    Density/
    Source/
    Proof/

Styles/
  low-resolution-voxel.v1/

Profiles/
  humanoid.medium.v2.yaml

Collections/
  combat-foundation.v1.yaml

Samples/

Store/
  policy.yaml
  vpu-ruler.yaml
  retired-assets.yaml

Catalog/
  catalog.generated.json
  health.generated.json
```

`Tool/`, `toolchain/`, `openspec/`, `.agents/`, `AGENTS.md`, and `WORKFLOW.md` remain support infrastructure.

## Asset identity

Published `id` and `unitRef` values remain unchanged during this migration. Filesystem ownership changes are not identity migrations.

Future identity cleanup, if desired, must be a separate explicit migration because retired/published identity stability is already an enforced contract.

## Collection ownership

The old coverage file becomes a collection manifest. The selected store collection is declared by `Store/policy.yaml`:

```yaml
version: 2
name: voxel-store.v1
collection: combat-foundation.v1
defaultStyle: low-resolution-voxel.v1
allowOptionalExpansion: false
budget: ...
```

The collection manifest owns desired reusable coverage only:

```yaml
version: 1
id: combat-foundation.v1
requirements: ...
```

Collection requirements select/require reusable assets by neutral properties; assets do not live under or depend on the collection path.

## Style ownership

`Styles/<style-id>/style.yaml` is reusable visual authority. Asset manifests continue to declare `style.family`, but that value now resolves directly to a style manifest.

The store may declare a `defaultStyle` for authoring convenience and fixture APIs, but asset validation no longer requires every asset style to equal one store-wide family. Existing active assets migrate from `combat-foundation.v1` to `low-resolution-voxel.v1` without changing visual evidence bytes.

## Profile ownership

Compatibility profiles are reusable domain contracts, not global policy knobs, so they move from `policy/families` to `Profiles` unchanged in semantic content.

## Samples boundary

Only `Assets/**/asset.yaml` participates in store discovery. `Samples/**` is explicitly non-published experimental content even if future samples contain similarly named files.

## Store policy boundary

`Store/**` contains global rules/history only. Coverage belongs to Collections; compatibility belongs to Profiles; style authority belongs to Styles.

## Catalog boundary

`Catalog/**` remains derived and safely regenerable. Moving it to a capitalized first-class root makes its role visible without mixing it with source authority.

## Asset self-containment

Each asset keeps `Reference`, optional `Density`, `Source`, and `Proof` together. Generated Hero.Voxel siblings remain beside editable source. The avatar's old shared foundation palette is copied into its own `Source` so the asset does not depend on a collection-owned palette path.

## Historical records

Archived OpenSpec artifacts are historical and keep old path descriptions. Active code/specs/workflow/tests must use the new layout.

## Migration sequence

1. Add new Store/Collection/Profile schema/path support in Tool.
2. Move authoritative roots and published assets while preserving ids/UnitRefs.
3. Update asset palettes/style declarations and active provenance paths.
4. Update Tool tests/fixtures to the new roots.
5. Regenerate reference/proof/catalog evidence using the pinned provider when available.
6. Strict-validate OpenSpec and repository diff hygiene.

## Risks

- Path-sensitive provenance becomes stale after moves. Regenerate/reseal deterministic evidence rather than hand-editing hashes.
- Tests contain many old fixture paths. Update active tests but leave archived historical documents unchanged.
- Changing store/style naming can accidentally look like asset identity migration. Explicitly preserve asset ids and UnitRefs and verify them before/after.
- Existing local Hero Studio checkout may not match the pinned revision; if so, perform all provider-independent validation and report the provider-bound regeneration blocker clearly.
