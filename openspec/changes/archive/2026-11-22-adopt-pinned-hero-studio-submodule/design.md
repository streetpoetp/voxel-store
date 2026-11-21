# Design

## Context

See `proposal.md` for motivation. Today the Tool resolves Hero Studio through `HERO_STUDIO_ROOT`, while the Unity preview constructs an ignored selective package projection. The latest Hero Studio `origin/main` is `363e590268ec28a32a09e1805d0772d79277aecb`; its Hero.Voxel workflow fingerprint remains `192f18ddb94dd0e077f00bf976607d00045de604625139bac4a672383ff3cb88`. The latest full Unity package includes corrected Hero.Voxel fixture metadata and declares URP 17.3.0.

## Goals / Non-Goals

**Goals:**
- Make one repository-owned Hero Studio checkout authoritative for both Tool and Unity.
- Pin that checkout with normal Git submodule semantics.
- Remove production dependency on `HERO_STUDIO_ROOT` and selective Hero.Voxel package projections.
- Consume the complete Hero Studio Unity package and its declared rendering stack.
- Preserve temp-store test isolation without restoring machine-global provider configuration.

**Non-Goals:**
- Do not modify Hero Studio source as part of voxel-store.
- Do not vendor/copy Hero Studio package internals into voxel-store.
- Do not change voxel-store publication, asset identity, or gameplay semantics.

## Decisions

### 1. Pin Hero Studio as `Dependencies/hero-studio`

Add a Git submodule using `git@github.com:streetpoetp/hero-studio.git` and pin the gitlink to `363e590268ec28a32a09e1805d0772d79277aecb`.

The submodule gitlink is the checkout authority. `toolchain/hero-voxel.lock.yaml` remains as the Hero.Voxel contract/fingerprint check and SHALL carry the same revision.

**Alternative considered:** keep a separately cloned provider resolved by environment variable. Rejected because it creates machine-specific state and two independently movable revisions.

### 2. Tool builds and runtime provider resolution use the submodule directly

`Tool/Directory.Build.props` resolves `HeroStudioRoot` from the repository-relative submodule. `VoxelStore.Tool.csproj` keeps its project reference through that property.

`HeroVoxelProvider` resolves production provider files from `Dependencies/hero-studio`, verifies the submodule Git revision and workflow fingerprint against the lock, and fails closed when missing or mismatched.

Temp-store tests receive an explicit provider-root injection from the canonical repository submodule. No production code reads `HERO_STUDIO_ROOT`.

### 3. Unity consumes the complete package from the submodule

Hero Studio's own Unity project consumes `Packages/hero.studio` as an **embedded package**. The preview workspace SHALL match that semantic boundary: `Preview/Bootstrap.ps1` creates one ignored directory junction at:

`Preview/Unity/Packages/hero.studio`

pointing to the complete package root:

`Dependencies/hero-studio/src/client/unity/Packages/hero.studio`

This is one whole-package link. It does not select Hero Studio modules, copy package source, construct a reduced package, or replace provider-owned files.

`Preview/Unity/Packages/manifest.json` supplies the production consumer environment required by the complete embedded package: exact SSH-pinned overrides for UniTask, DOTween Pro, StompyRobot, and HybridCLR, plus the Unity engine modules used by Hero Studio. HybridCLR is included because `hero.dynamicload/Runtime.Unity` has a compile-time dependency on it even though the package descriptor does not currently declare that dependency. The preview intentionally omits Hero Studio development-project-only Rider/test-runner settings and does not declare a second `hero.studio` file-package dependency.

**Alternative considered:** consume the same package through a `file:` manifest dependency. Rejected after integration proof showed that the external-file mount repeatedly stopped at the initial AssetDatabase refresh, while the one-package embedded junction matches Hero Studio's own package boundary and completes import/compilation successfully.
`Preview/Bootstrap.ps1` initializes/verifies the submodule and maintains only that single full-package junction. Repeated bootstrap is idempotent and fails closed rather than replacing a real package directory.

### 4. Preview uses URP rather than a local shader override
Because the complete Hero Studio package declares URP and Hero.Voxel preview shaders target URP, the preview project owns a minimal URP asset/renderer configuration and sets it as the project render pipeline.

The temporary `Preview/VoxelAssetPreview.BuiltIn.shader` override is removed. Provider-owned shaders load unchanged from the submodule package.

### 5. Unity Editor preview-plan invocation no longer passes provider state

The preview Editor runs `dotnet run --project Tool/VoxelStore.Tool ...` directly. Provider resolution is already repository-local through the submodule, so no provider-root marker, MSBuild property override, or environment variable is required.

## Risks / Trade-offs

- **[Submodule is not initialized after clone]** → Bootstrap and Tool fail with an explicit `git submodule update --init --recursive` remediation.
- **[Full Hero Studio package resolves more dependencies than the previous projection]** → This is intentional; the preview now tests the actual package contract rather than a curated subset.
- **[URP package import increases first-open cost]** → Accept the one-time import in exchange for matching provider runtime/editor assumptions.
- **[Submodule revision and lock diverge]** → Provider verification fails closed; update both in the same dependency bump.

## Migration Plan

1. Add and pin `Dependencies/hero-studio` at latest `origin/main`.
2. Align the Hero.Voxel lock revision/fingerprint.
3. Move Tool provider/build resolution to the submodule and update tests.
4. Point Unity at the complete submodule package and remove generated package projection logic.
5. Configure preview URP and remove the Built-in shader override.
6. Run Tool tests, Unity EditMode tests, Inspector preview proof, and live grid refresh.
