# Tasks

## 1. Adopt the provider revision and prove the dependency boundary

- [x] 1.1 Advance Dependencies/hero-studio to published Hero Studio commit 393d1ef196d90b094d86750c370173cab6aebde9; compute the target voxel-art-workflow.md SHA-256 and verify the existing semantic heroStudioRevision/workflowSha256 lock values remain valid because this Editor-only adoption does not change Tool/workflow semantics.
- [x] 1.2 Run Preview bootstrap verification and focused Tool/provider-resolution tests; verify the complete embedded hero.studio package exposes the Actor Editor inspection assembly and existing Tool/authoring resolution remains version-control independent at execution time.

## 2. Replace the canonical Edit Mode grid with Play Mode runtime realization

- [x] 2.1 Add focused tests for the intended Play Mode grid boundary before removing Scene Debug realization: refresh launch intent survives Edit-to-Play transition, runtime realization is unavailable outside Play Mode, and generated selection hosts remain transient and plain with no voxel-store preview/player MonoBehaviour.
- [x] 2.2 Split Refresh Preview Grid into existing bake/shell preparation plus transient Play Mode launch/resume orchestration; verify the saved VoxelStorePreview.unity scene contains no serialized runtime grid authority and a second refresh deterministically replaces the prior session.
- [x] 2.3 Realize all current baked units through one provider VoxelRenderWorld; use direct VoxelRenderInstance plus canonical model track for standalone equipment and VoxelActorInstance for avatars.
- [x] 2.4 Compose the existing deterministic one/two carried-equipment selection through VoxelActorInstance attachment using avatar socket and Mount; remove manual Scene Debug socket-transform alignment from the canonical grid and verify provider runtime bounds remain finite and current.
- [x] 2.5 Feed provider runtime bounds into the existing PreviewGridLayout, place entries through provider instance or Actor world transforms, submit the render world, and verify planned identity order, zero XZ overlaps, unit authored scale, and expected carried-equipment count.

## 3. Reuse Hero Studio runtime Inspector for all interactive animation controls

- [x] 3.1 Add the required Actor runtime and Actor Editor assembly references to the preview Editor surface without creating a voxel-store runtime assembly or Player dependency.
- [x] 3.2 Register each plain avatar root host with its root instance/track/Actor, each carried-equipment child host with its attachment instance/track/same Actor, and each standalone-equipment host with its direct instance/track; verify provider inspection sessions exist only in Play Mode and add no components to hosts.
- [x] 3.3 Remove canonical-grid calls to VoxelSceneDebugBuilder and VoxelSceneDebugController and remove any grid-local playback or clip state; verify selecting, scrubbing, or playing one registered model changes only its canonical provider track while Actor refresh preserves composition and peer tracks remain independent.
- [x] 3.4 Preserve direct Hero.Voxel Scene Debug as a separate baked-asset/Edit Mode regression and verify the existing provider Scene Debug test still passes independently of the Play Mode grid.

## 4. Remove obsolete Edit Mode grid proof assumptions and update workflow

- [x] 4.1 Remove or replace RefreshAllAndOpenGridForBatch and CaptureCurrentGridSceneViewForBatch behavior that assumes MeshRenderers exist under an Edit Mode grid hierarchy; move canonical grid assertions to Play Mode runtime proof and keep graphical content judgment in the registered Game View capture flow.
- [x] 4.2 Update WORKFLOW.md so Voxel Store/Refresh Preview Grid is explicitly bake -> shell -> Play Mode runtime realization -> shared provider Inspector, while Scene Debug remains direct baked-asset/Edit Mode tooling and avatar-motion-gallery remains separate attested capture proof.

## 5. Integration and release readiness

- [x] 5.1 Run the focused Preview Editor/Play Mode test suite with non-zero readable results; verify all current published models realize, runtime submission contains parts, all expected inspection hosts are registered, and no task-owned test fails.
- [x] 5.2 Run avatar-motion-gallery provider-attested capture regression and inspect the current images for valid body/equipment composition; verify the interactive-grid migration did not change authored visual content or capture checkpoint semantics.
- [x] 5.3 Run dotnet run --project Tool/VoxelStore.Tool.Tests, Tool verify, strict OpenSpec validation, git diff --check, and task-owned scope/dependency review; verify no asset-authority migration, gameplay semantics, store-owned runtime facade/component, RPC/protocol change, Player build, or APK build entered the change.

## Workflow follow-up

- After implementation review, sync the unity-baked-preview delta and archive adopt-play-mode-runtime-preview-grid.
- Commit and push the Hero Studio pin plus preview migration as one coherent downstream adoption change.