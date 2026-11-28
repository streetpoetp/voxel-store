# Spec Delta

## MODIFIED Requirements

### Requirement: WalkForward follows a Crossy Road-style hop grammar
Grounded, heavy, agile and grounded caster avatars SHALL use profile-appropriate readable launch, apex, landing and recovery in WalkForward. Floating and winged profiles SHALL use Glide and Fly respectively instead of a compulsory hop.

#### Scenario: WalkForward presentation is played
- **WHEN** the preview plays WalkForward on an applicable grounded body
- **THEN** its authored motion presents a coherent profile-specific hop and equipment playback stays independently selected

#### Scenario: Gallery demonstrates forward travel
- **WHEN** the preview visualizes a logical forward step or aerial travel
- **THEN** preview-only root motion can show canonical Forward without turning model tracks into movement authority

#### Scenario: A floating or winged avatar is reviewed
- **WHEN** the profile declares Glide or Fly
- **THEN** proof shows its own movement language rather than requiring WalkForward

### Requirement: Animation verification uses registered provider-attested Game View capture
Canonical animation proof SHALL run through `tools/capture-game-view.ps1`, backed by a preview-owned schema-v2 registry and graphical `Capture.Editor` runner. Accepted images SHALL use `Hero.Consumer.Editor.GameViewCapture` and current provider attestation under `Library/Capture/**`; direct camera/RenderTexture output is diagnostic only.

#### Scenario: Capture scenario is discovered
- **WHEN** avatar-motion visual verification is requested
- **THEN** the launcher resolves the declared preview capture scenario from schema-v2 registry metadata rather than requiring an agent to know or type an arbitrary runner/output pair

#### Scenario: Caller uses only the repository command
- **WHEN** automated verification needs avatar-motion captures
- **THEN** the caller SHALL NOT need to invoke Unity `-executeMethod`, know the fully-qualified runner class, hard-code the output directory, or operate Editor menus/buttons; those details SHALL remain behind `tools/capture-game-view.ps1` and the capture registry

#### Scenario: Capture is invoked by command
- **WHEN** an agent or developer runs `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`
- **THEN** that command resolves the non-consumer preview workspace/registry, reuses pinned capture-attestation helpers, launches an owned graphical Editor, rejects stale/unattested output, and reports current capture artifacts plus `CAPTURE_ATTESTED`

#### Scenario: Play Mode reload occurs
- **WHEN** the registered `Capture.Editor` runner enters Play Mode and Unity reloads the domain
- **THEN** capture state survives and resumes against the live runtime gallery without manual Play/menu interaction

#### Scenario: Representative combat motion is captured
- **WHEN** any declared body or equipment motion family or take requires visual proof
- **THEN** the runner drives the same independent model-track path used by interactive playback to declared deterministic checkpoints before invoking Game View capture

#### Scenario: Semantic translation must be visible in pixels
- **WHEN** a captured model motion translates along semantic Forward or opposite Forward
- **THEN** the capture runner uses an evidence-only oblique Game View camera pose around the same runtime gallery bounds so that motion has a visible screen-space component without changing the ordinary gallery camera contract

#### Scenario: Capture completes
- **WHEN** all declared checkpoints have been acquired
- **THEN** the runner resets declared preview track state, exits Play Mode cleanly, clears persisted capture state, exits the owned Editor with success, and reports success only after validating current provider attestation and readable current PNGs

#### Scenario: Capture pixels are reviewed
- **WHEN** the shared capture contract reports `CAPTURE_ATTESTED`
- **THEN** that result proves only current graphical Game View acquisition and file integrity; agent or human inspection remains responsible for judging whether the motion is visually correct

#### Scenario: Interactive controls are unavailable
- **WHEN** the custom Editor window is closed and no hierarchy object is manually selected
- **THEN** registered Game View capture remains fully executable and does not lose coverage

### Requirement: Graphical capture proves generalized animation variant resolution
Canonical graphical capture SHALL exercise every authored family and take, including optional and side-qualified families, from the current profile-driven plan using deterministic ordered checkpoints and exact clip identities.

#### Scenario: Looping variants are captured
- **WHEN** Idle or applicable locomotion variants are reviewed
- **THEN** attested ordered views cover their poses, intermediate trajectory and loop seam

#### Scenario: Non-looping variants are captured
- **WHEN** a non-looping Attack, Hit, Death, Drop or optional motion is reviewed
- **THEN** attested views cover anticipation, action, recovery or terminal hold as applicable

#### Scenario: Side-qualified variant is captured
- **WHEN** a model publishes Attack.Left.V2 and a proof case selects it
- **THEN** exact authored motion is sampled with independent attachment/reference identity

### Requirement: Preview verification covers store-wide required V1 availability
Preview SHALL consume the Tool-resolved exact family/take coverage and loop policy rather than duplicate kind-specific requirements. Every baked asset in its declared proof plan SHALL match that resolved inventory and include base plus V1/V2/V3 for each declared family.

#### Scenario: All baked models satisfy required coverage
- **WHEN** the published grid or a candidate proof plan is baked
- **THEN** each model reports the exact resolved required clips and loop settings

#### Scenario: One baked model loses a required clip
- **WHEN** a required V1, V2, V3 or default clip is missing
- **THEN** validation fails with the asset and missing clip, without inventing a replacement

#### Scenario: Graphical integration proof runs
- **WHEN** graphical capture executes after coverage passes
- **THEN** it proves current authored content through the existing provider path, not a duplicate lifecycle proof

### Requirement: Graphical capture proves store content composition
Canonical composed-animation capture SHALL demonstrate independently selected authored body/equipment states and valid composition through pinned provider-attested pixels.

#### Scenario: Mixed motions are captured
- **WHEN** body Idle and weapon Attack are declared at independent checkpoints
- **THEN** capture shows both authored states without inferred reactions

#### Scenario: Independent phases are captured
- **WHEN** body and equipment are declared at different normalized phases
- **THEN** attested pixels preserve the requested states and composition

#### Scenario: Separate Death clips are captured
- **WHEN** capture independently samples avatar Death and equipment Drop
- **THEN** attested pixels demonstrate the distinct terminal motions without a shared synchronization API

## ADDED Requirements

### Requirement: Authoring candidate preview precedes publication
An explicit candidate proof plan SHALL accept only named assets with current upstream authority, valid profile/source and matching generated artifacts, while allowing pending proof, seal or compatibility gates. Ordinary preview SHALL remain published-only; candidate proof SHALL NOT mark assets published.

#### Scenario: Animation edit invalidates a proof seal
- **WHEN** the selected asset has valid current source and generation but stale visual evidence
- **THEN** candidate capture can bake and inspect it before resealing

#### Scenario: Candidate has invalid upstream evidence
- **WHEN** a candidate fails reference, applicable density, source or profile validation
- **THEN** candidate proof fails with that gate rather than bypassing it

#### Scenario: Candidate pair composition is required
- **WHEN** a named candidate needs a compatible peer
- **THEN** capture can use a valid published peer or separately validated named candidate without publishing either

### Requirement: Visual motion review covers all declared takes
Current provider-attested ordered captures SHALL cover every authored take of each selected model, standalone and composed as applicable, with adequate views and checkpoints for readable weight, trajectory, orientation, seam or terminal rest and reference release/return.

#### Scenario: Capture succeeds without visual review
- **WHEN** all checkpoints are acquired but no current per-asset review accepts the motion
- **THEN** the asset remains blocked at its proof gate

#### Scenario: A variant differs only by timing
- **WHEN** review reveals repeated choreography despite different clip durations or angles
- **THEN** that take must be repaired before acceptance
