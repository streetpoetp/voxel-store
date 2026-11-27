# Spec Delta

## ADDED Requirements

### Requirement: Preview proof exposes no voxel-store runtime surface
The Unity preview SHALL remain an Editor proof workspace and SHALL NOT publish a voxel-store runtime assembly, reusable voxel runtime model, playback controller, actor wrapper, presenter wrapper, streaming abstraction, pool, or duplicated model-frame/motion convention.

#### Scenario: Preview assemblies are inspected
- **WHEN** the Unity preview project is refreshed after this change
- **THEN** store-owned preview code is limited to Editor, capture, and validation orchestration and no voxel-store runtime API is available to consumers

#### Scenario: A proof needs voxel execution
- **WHEN** a store-specific proof needs rendering, animation, actor composition, attachment mutation, streaming, or pooling
- **THEN** the proof consumes the pinned Hero Studio capability directly instead of defining an equivalent store-owned runtime abstraction

### Requirement: Preview gallery discovery remains Editor-owned
The preview gallery SHALL discover current baked catalog assets and construct deterministic review layouts through Editor-only orchestration without requiring runtime components to retain model identity, composition state, or playback state.

#### Scenario: Imported baked model is discovered
- **WHEN** a baked assembly exists under the preview baked catalog tree
- **THEN** Editor discovery derives its catalog/model identity and includes it in review without adding a runtime registration layer

#### Scenario: Gallery is refreshed
- **WHEN** the current published/baked set changes
- **THEN** Editor orchestration rebuilds the generated review state from current data without relying on serialized runtime wrapper state from the previous gallery

### Requirement: Store proof consumes Hero Studio directly
Store-specific composition and capture SHALL operate directly on pinned Hero Studio public render, Actor, animation, and Editor-preview capabilities. Voxel-store SHALL supply only proof-case content choices, socket/anchor names, exact authored clips/phases, layout, checkpoints, and visual acceptance rules.

#### Scenario: Avatar equipment proof is composed
- **WHEN** capture composes a published avatar with equipment
- **THEN** Hero Studio owns render instances, actor bindings, animation tracks, attachment composition, and bounds while voxel-store owns only the selected store assets and proof inputs

#### Scenario: Provider behavior is already covered upstream
- **WHEN** a proof would only restate track lifecycle, handle staleness, pool reuse, warm reacquire, or mutable-topology semantics already owned by Hero Studio
- **THEN** voxel-store does not add or retain a separate behavioral proof for that provider mechanism

### Requirement: Baked animation inspection uses provider Editor capability
Interactive animation inspection SHALL use the pinned Hero Studio Editor inspection and Scene Debug behavior over the current baked animation asset rather than a voxel-store-owned runtime motion player or duplicated clip inventory.

#### Scenario: Developer inspects authored variants
- **WHEN** a baked animation asset contains baseline and variant clips
- **THEN** provider Editor inspection exposes the asset's actual authored clip inventory and supports selecting/scrubbing those clips without a store-maintained motion list

#### Scenario: Baked clip inventory changes
- **WHEN** rebaking adds or removes authored clips
- **THEN** inspection reflects the current baked animation asset without changing voxel-store runtime code because no such runtime layer exists

### Requirement: Preview consumes provider attachment composition directly
Store-specific avatar/equipment proof SHALL use pinned Hero Studio attachment composition directly and SHALL NOT insert a voxel-store runtime owner or mirrored composition state between proof inputs and provider objects.

#### Scenario: Avatar equipment proof is composed
- **WHEN** a proof composes a Body with one or more equipment assets
- **THEN** Hero Studio owns root/attachment bindings, model tracks, reference-influence composition, refresh/reset, and bounds while voxel-store supplies only selected assets, socket/anchor names, and exact proof clips/phases

#### Scenario: Provider capability is unavailable
- **WHEN** the pinned Hero Studio revision lacks a capability required by an existing proof
- **THEN** the proof fails clearly or is redesigned around available provider behavior rather than restoring, copying, or locally emulating the missing runtime abstraction

### Requirement: Preview proof retains only content selection
The preview SHALL own only proof-case choices such as asset, socket, exact authored clip, variant, phase, and acceptance checkpoint. Clip execution and model-local playback semantics SHALL remain provider-owned, and voxel-store SHALL NOT maintain a second motion vocabulary or candidate-resolution engine.

#### Scenario: Proof selects a side-qualified clip
- **WHEN** a capture case selects an authored clip such as `Attack.Left.V1`
- **THEN** that exact baked clip is requested for the selected model without a voxel-store candidate builder or actor-wide motion state

#### Scenario: The same equipment asset is inspected twice
- **WHEN** two proof placements use one equipment asset with different exact clips or phases
- **THEN** the provider instances remain independent while voxel-store retains only the two proof-case selections

#### Scenario: Required authored clip is missing
- **WHEN** a proof case names a clip that is absent from the selected baked animation asset
- **THEN** preview validation identifies the model and missing authored clip without procedural fallback or local semantic substitution

### Requirement: Graphical capture proves store content composition
The canonical composed-animation capture SHALL prove authored body/equipment content can be sampled independently and composed correctly through pinned Hero Studio behavior using deterministic provider-attested pixels.

#### Scenario: Mixed motions are captured
- **WHEN** capture declares body `Idle` and weapon `Attack` at independent checkpoints
- **THEN** attested pixels show the requested mixed authored motion state without a body reaction inferred from the weapon

#### Scenario: Independent phases are captured
- **WHEN** body and equipment are declared at different normalized phases
- **THEN** attested pixels preserve the requested authored states and visually valid attachment composition

#### Scenario: Separate Death clips are captured
- **WHEN** capture independently samples authored `Death` clips on body and equipment
- **THEN** attested pixels show each model's selected authored Death state without requiring a shared synchronization API in voxel-store

## MODIFIED Requirements

### Requirement: Preview remains Editor-only
The preview workspace SHALL exist only for Editor inspection, validation, and capture orchestration and SHALL NOT introduce a production game runtime, build target, gameplay-facing voxel ownership boundary, or reusable voxel runtime API into voxel-store.

#### Scenario: Developer inspects a baked unit
- **WHEN** a developer selects or previews a baked assembly or animation in the workspace
- **THEN** existing Hero.Voxel Editor preview and Scene Debug behavior can be used without adding gameplay semantics or store-owned runtime components

#### Scenario: Grid is generated
- **WHEN** the all-units grid is built in the canonical preview scene
- **THEN** its generated scene objects remain disposable Editor proof state and are not treated as production runtime content

#### Scenario: Proof requires Play Mode execution
- **WHEN** a registered capture scenario enters Play Mode to exercise the real provider runtime
- **THEN** Editor/capture orchestration drives Hero Studio directly and does not depend on a voxel-store runtime assembly or reusable runtime wrapper

### Requirement: Preview verification covers store-wide required V1 availability
Preview verification SHALL mechanically confirm that every baked published avatar and equipment model exposes the minimum V1 clip set required by publication, while graphical capture remains content-integration evidence rather than a duplicate provider-runtime conformance suite.

#### Scenario: All baked models satisfy required coverage
- **WHEN** preview validation scans the baked published model set
- **THEN** every avatar and equipment animation asset reports the required exact V1 clip names for its model kind

#### Scenario: One baked model loses a required clip
- **WHEN** a baked model lacks a V1 clip required by publication
- **THEN** preview validation fails and identifies the model and missing clip before the change is accepted

#### Scenario: Graphical integration proof runs
- **WHEN** provider-attested graphical capture executes after store-wide coverage passes
- **THEN** it proves representative store content can be composed and animated through Hero Studio without requiring voxel-store to re-test provider lifecycle mechanics

## REMOVED Requirements

### Requirement: Runtime gallery discovers human-imported baked catalogs
**Reason**: Catalog discovery remains required, but the runtime-gallery contract couples discovery to store-owned runtime behavior.
**Migration**: Use the new Editor-owned gallery discovery requirement and keep catalog discovery/layout in `Preview/Editor`.

### Requirement: Preview delegates generic attachment presentation to pinned provider
**Reason**: The old requirement couples store proof to both direct and streamed provider lifecycle semantics that no longer belong in the voxel-store contract.
**Migration**: Use `Preview consumes provider attachment composition directly` for content-level composition proof and leave streamed lifecycle conformance to Hero Studio.

### Requirement: Preview retains consumer semantic selection
**Reason**: The old requirement makes voxel-store own motion vocabulary/candidate behavior beyond content-proof selection.
**Migration**: Use `Preview proof retains only content selection` and name exact authored clips/phases directly.

### Requirement: Graphical capture proves independent composed animation
**Reason**: The old requirement includes topology mutation and provider-runtime behavior beyond store content acceptance.
**Migration**: Use `Graphical capture proves store content composition` for deterministic mixed-motion/content evidence only.

### Requirement: Runtime gallery uses shared provider realization
**Reason**: This requirement still defines a store runtime realization lifecycle around provider objects.
**Migration**: Editor/capture proof code consumes Hero Studio directly under `Store proof consumes Hero Studio directly`.

### Requirement: Shared preview execution preserves playback controls
**Reason**: Playback control lifecycle belongs to provider animation/editor capabilities, not voxel-store runtime state.
**Migration**: Store proofs select exact clips/phases; interactive playback uses Hero Studio Editor inspection.

### Requirement: Runtime avatar composition uses explicit presentation ownership
**Reason**: A store-owned avatar presentation owner is a redundant runtime wrapper over Hero Studio Actor.
**Migration**: Compose proof cases directly with Hero Studio and keep only Editor/capture proof metadata in voxel-store.

### Requirement: Editor control optionally selects preview motion
**Reason**: Store-owned runtime motion controls duplicate Hero Studio animation inspection and Scene Debug behavior.
**Migration**: Use provider Editor inspection for interactive playback and registered capture scenarios for canonical evidence.

### Requirement: Play Mode presentation remains preview-only
**Reason**: The old requirement assumes store-owned Play Mode presentation components.
**Migration**: Play Mode may still be entered by Editor capture orchestration, but provider runtime objects are driven directly and no voxel-store runtime surface remains.

### Requirement: Preview re-solves attachment after pose evaluation
**Reason**: Attachment reference solving is a Hero Studio Actor implementation guarantee, not a voxel-store behavior contract.
**Migration**: Keep only store-specific visual acceptance scenarios that exercise authored socket/Mount composition through the provider.

### Requirement: Preview samples reference influence from provider playback state
**Reason**: Reference-influence evaluation is provider runtime behavior and is already owned/tested upstream.
**Migration**: Store graphical proof may observe authored release/return trajectories without re-specifying the provider state machine.

### Requirement: Direct and streamed attachment preview share reference-influence semantics
**Reason**: Equivalence between provider direct and streamed runtime paths belongs to Hero Studio conformance, not voxel-store.
**Migration**: Remove the duplicate cross-path proof; retain content-level composition evidence only.

### Requirement: Runtime gallery provides bounded mutable streamed actor proof
**Reason**: Mutable topology, pooling, handle lifetime, and warm reacquire are provider runtime contracts unrelated to store content acceptance.
**Migration**: Delete the store mutable-streamed proof surface and rely on Hero Studio tests/workbench for those mechanics.

### Requirement: Graphical capture proves mutable streamed composition
**Reason**: The capture scenario primarily re-tests Hero Studio mutable Actor behavior rather than voxel-store content.
**Migration**: Remove `mutable-streamed-actor-gallery`; keep graphical scenarios that prove current store assets compose and animate correctly.

### Requirement: Preview motion state supports model-local animation variants
**Reason**: Store-owned runtime motion state is redundant; authored variants are already represented by exact baked clip names.
**Migration**: Proof cases select exact authored clips/variants and provider Editor inspection exposes the baked inventory.

### Requirement: Preview candidate construction preserves side and variant precedence
**Reason**: Candidate construction duplicates Hero Studio's common motion-candidate convention and is unnecessary for store content proof.
**Migration**: Store validation/capture names exact authored clips; any consumer candidate policy remains outside voxel-store.

### Requirement: Interactive preview can select variants without becoming verification authority
**Reason**: The interactive store runtime preview controller is being removed.
**Migration**: Use Hero Studio Editor inspection for ad hoc variant selection and repository capture scenarios for canonical evidence.

### Requirement: Component Inspector discovers motion clips from the selected model
**Reason**: The requirement targets a voxel-store runtime component Inspector that duplicates provider animation-asset inspection.
**Migration**: Use Hero Studio's baked animation Editor/Scene Debug inspection over the current asset.

### Requirement: Exact Inspector clip selection preserves semantic motion state
**Reason**: Mapping Inspector state into a store-owned semantic runtime model is unnecessary once the runtime layer is removed.
**Migration**: Inspect exact authored clip names directly; proof-case semantics remain Editor/capture data only.

### Requirement: Runtime gallery exposes the current baked animation inventory
**Reason**: Current clip inventory remains important, but it does not require runtime gallery components.
**Migration**: Store-wide validation reads current baked animation assets and Hero Studio Editor inspection exposes their authored clips.

### Requirement: Preview exposes independent model animation tracks
**Reason**: Animation-track lifecycle is a Hero Studio Actor capability, not a voxel-store API.
**Migration**: Store capture may drive provider tracks directly as proof implementation detail without specifying or wrapping their lifecycle.

### Requirement: Provider track is the sole runtime playback authority
**Reason**: This requirement exists only to constrain the redundant voxel-store motion player around the provider track.
**Migration**: Remove the store motion player; provider ownership becomes implicit through direct consumption.

### Requirement: Streamed presenter is the sole mutable topology authority
**Reason**: Mutable topology semantics belong entirely to Hero Studio and no longer need a voxel-store shadow-prevention contract once the shadow layer is deleted.
**Migration**: Remove store mutable-streamed runtime code and its duplicated topology tests.
