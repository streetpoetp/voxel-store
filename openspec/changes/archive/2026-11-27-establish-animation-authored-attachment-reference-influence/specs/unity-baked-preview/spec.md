# Spec Delta

## MODIFIED Requirements

### Requirement: Preview re-solves attachment after pose evaluation
The preview SHALL resolve the current body-socket/bind-Mount attachment reference and combine it with the attachment clip's sampled reference influence. Reference influence, not translation presence, clip name, or authored loop policy, SHALL determine how the current reference contributes to the attachment visual root.

#### Scenario: Weapon remains socket-relative
- **WHEN** an equipment clip samples reference influence `1`
- **THEN** preview applies the current attachment reference to the model root while preserving that model's independently authored position, rotation, and scale animation

#### Scenario: Weapon swings around the grip
- **WHEN** an equipment clip independently animates position or rotation on the part containing its Mount while sampled reference influence is `1`
- **THEN** preview keeps the model root on the current bind-Mount reference and preserves the authored part animation so the visible weapon can move around the grip without changing logical binding

#### Scenario: Weapon releases from body movement
- **WHEN** an equipment clip transitions from influence `1` toward `0`
- **THEN** preview retains a visual reference basis for that release and progressively reduces contribution from subsequent Body socket movement without changing the logical binding

#### Scenario: Body moves during zero influence
- **WHEN** the Body socket moves while an equipment clip samples reference influence `0`
- **THEN** the equipment visual root remains independent of that Body motion while the current attachment reference continues to update separately

#### Scenario: Body motion moves the attachment
- **WHEN** the Body clip moves its socket frame while an equipment take samples reference influence `1`
- **THEN** preview applies the updated reference without changing that equipment track's selected clip, phase, speed, effective loop state, or model-local animation

#### Scenario: Body moves during a translated non-looping take
- **WHEN** a translated non-looping equipment take has started from the current attachment reference, its authored influence releases below `1`, and the Body socket subsequently moves
- **THEN** Body motion contributes to the equipment visual root only according to sampled reference influence rather than because translation or non-looping policy implies a latch

#### Scenario: Weapon blends back to a moved socket
- **WHEN** a released attachment's authored influence rises continuously from `0` toward `1` after the Body socket has moved
- **THEN** the model root continuously converges on the then-current attachment reference without a terminal-frame snap or topology operation

#### Scenario: Authored translated take returns to the socket
- **WHEN** a translated equipment take raises authored reference influence back to `1` after a released interval
- **THEN** preview converges to and resumes the then-current attachment reference according to that authored influence rather than waiting for terminal translation to trigger a snap

#### Scenario: Authored translated take remains displaced
- **WHEN** a translated equipment take reaches its terminal phase with sampled reference influence below `1`
- **THEN** preview preserves the corresponding released/blended visual root and terminal authored pose rather than imposing a universal snap-back rule

#### Scenario: Clip contains translation but remains attached
- **WHEN** an equipment clip has authored position keys and samples reference influence `1`
- **THEN** preview keeps the model root on the live attachment reference and treats the position keys only as model-local visual animation

#### Scenario: Clip contains no translation but releases
- **WHEN** an equipment clip has no authored position keys and samples reference influence below `1`
- **THEN** preview permits independent visual-root motion according to the influence contract and does not require translation as an implicit detach marker

#### Scenario: Clip omits reference influence
- **WHEN** an equipment clip has no authored reference-influence channel
- **THEN** preview uses influence `1` for backward-compatible live-reference behavior

#### Scenario: Animated Mount differs from current socket
- **WHEN** authored equipment animation moves the evaluated Mount away from the current body socket
- **THEN** preview treats that difference as authored visual motion and keeps the logical binding/reference valid instead of solving from the evaluated Mount

#### Scenario: Attachment contract is incompatible
- **WHEN** the current provider cannot resolve the declared binding reference from the body socket and equipment bind Mount
- **THEN** preview composition fails visibly rather than introducing local compensation, evaluated-Mount feedback, or pair-specific correction

## ADDED Requirements

### Requirement: Preview samples reference influence from provider playback state
Preview SHALL consume reference influence sampled for the provider track's current selected clip and normalized phase. It SHALL NOT infer visual attachment authority from translation keys, clip names, configured loop intent, or duplicated consumer playback state.

#### Scenario: Runtime loop override loops a normally terminal clip
- **WHEN** a consumer plays an authored non-looping equipment clip with effective runtime loop enabled
- **THEN** reference influence repeats with the provider track's wrapped phase and preview does not force terminal attachment behavior from the clip's authored loop flag

#### Scenario: Runtime loop override terminates a normally looping clip
- **WHEN** a consumer plays an authored looping equipment clip with effective runtime loop disabled
- **THEN** reference influence follows the provider track to its terminal phase and preview does not keep looping attachment behavior from the clip's authored loop flag

#### Scenario: Provider track is reset
- **WHEN** an attachment model track is reset and has no selected clip
- **THEN** preview returns that attachment to default influence `1` and the current attachment reference without retaining stale release state

#### Scenario: Direct phase sampling is requested
- **WHEN** a consumer samples an attachment clip at an exact normalized phase
- **THEN** preview uses the reference influence at that same provider-sampled phase and produces the corresponding attachment visual root from current binding/reference state

### Requirement: Direct and streamed attachment preview share reference-influence semantics
Direct avatar composition and mutable streamed actor composition SHALL apply the same reference-influence contract for equivalent selected clips, phases, binding references, rebinds, replacements, resets, and pool reacquisition.

#### Scenario: Streamed attachment releases while root moves
- **WHEN** a streamed attachment samples influence `0` and the actor root/body socket moves
- **THEN** the streamed attachment visual root remains independent in the same way as direct avatar composition while its opaque handle remains current

#### Scenario: Streamed attachment rejoins a moved reference
- **WHEN** a streamed attachment's influence rises from `0` to `1` after its current attachment reference changed
- **THEN** its visual root converges continuously to that current reference without handle replacement or a streamed-only snap rule

#### Scenario: Streamed attachment is rebound while released
- **WHEN** a streamed attachment is rebound while influence is below `1`
- **THEN** the same handle remains valid, the current reference changes to the rebound socket, and subsequent visual convergence follows authored influence

#### Scenario: Streamed attachment is replaced
- **WHEN** a streamed attachment is replaced with a new model under the preserved public handle
- **THEN** the replacement starts with fresh provider-defined playback/reference-influence state while root and unrelated attachment playback remain unchanged

#### Scenario: Warm pooled actor is reacquired
- **WHEN** a streamed actor is released and reacquired from the warm pool
- **THEN** prior activation reference-influence state is absent together with prior mutable attachments and playback state

### Requirement: Canonical proof covers continuous reference release and return
Provider-attested preview proof SHALL demonstrate a moving Body together with an attachment release interval and a continuous return interval so a terminal snap cannot satisfy the proof.

#### Scenario: Axe attack releases from a moving body
- **WHEN** Axe03 Attack is captured while the Body socket moves after the attack begins
- **THEN** attested checkpoints show the Axe trajectory continuing independently during its low-influence interval while logical binding identity remains unchanged

#### Scenario: Axe attack returns continuously
- **WHEN** Axe03 Attack reference influence rises toward `1` after the Body socket has moved
- **THEN** ordered checkpoints show the Axe converging toward the current socket over multiple phases rather than teleporting only at phase `1`

#### Scenario: Axe V1 remains spatially distinct
- **WHEN** Axe03 Attack.V1 is captured at the same ordered phases as baseline Attack
- **THEN** its authored position/rotation trajectory remains observably distinct while both takes use the same generic reference-influence contract
