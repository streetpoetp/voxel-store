# Tasks

## 1. Repository-owned Hero Studio provider

- [x] 1.1 Add `Dependencies/hero-studio` as a Git submodule pinned to `363e590268ec28a32a09e1805d0772d79277aecb`; verify `git submodule status` reports that exact revision and no developer-local absolute path is committed.
- [x] 1.2 Align `toolchain/hero-voxel.lock.yaml`, Tool MSBuild/project references, and `HeroVoxelProvider` with the repository submodule; remove production `HERO_STUDIO_ROOT` resolution, update temp-store tests to use explicit provider injection, and verify the full Tool test suite passes with missing/mismatched submodule cases failing closed.
- [x] 1.3 Update `AGENTS.md` and `WORKFLOW.md` to make submodule initialization and the single provider pin authoritative; verify documented commands work without setting `HERO_STUDIO_ROOT`.

## 2. Full Hero Studio Unity package

- [x] 2.1 Expose the complete `hero.studio` package from the submodule as one ignored embedded-package junction at `Preview/Unity/Packages/hero.studio`, provide the production consumer dependency environment required by the full package, remove the selective package/shader workaround, and verify the full provider imports with 14 Hero.Voxel fixture metas and 0 invalid metas.
- [x] 2.2 Make `Preview/Bootstrap.ps1` initialize/verify the submodule and maintain only the single whole-package junction; verify repeated bootstrap is idempotent and never copies, selects, or replaces individual Hero Studio package modules.
- [x] 2.3 Configure the preview Unity project for URP as required by the full Hero Studio package and verify the real Hero.Voxel animation Inspector renders non-blank preview pixels without a voxel-store shader replacement.
- [x] 2.4 Remove provider-root marker/property/environment handling from the Unity preview Editor client and update its EditMode tests; verify the owned preview EditMode suite passes 11/11.
## 3. Integration

- [x] 3.1 Run strict OpenSpec validation, `git diff --check`, the full Tool suite, pinned-submodule bootstrap, and a current-store Unity refresh; verify `preview-plan` count equals generated Scene grid count with no overlap or scale normalization.