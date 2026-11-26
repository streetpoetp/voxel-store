# Spec Delta

## ADDED Requirements

### Requirement: Provider track is the sole runtime playback authority
The interactive preview SHALL treat each provider-backed model animation track as the sole runtime authority for selected clip, playing/stopped state, phase, speed, loop behavior, and reset lifecycle. Preview-owned fields MAY retain user configuration intent but SHALL NOT maintain a second runtime playback state that can diverge from the provider track.

#### Scenario: Provider track is reset outside the player
- **WHEN** avatar or model lifecycle code resets a model track while an interactive preview controller remains enabled
- **THEN** the controller observes the provider track as stopped/unselected and does not advance an empty track on a later frame

#### Scenario: Provider track completes a non-looping clip
- **WHEN** a model track reaches its non-looping terminal phase
- **THEN** interactive preview state reports playback stopped from provider state without requiring a separate local playing flag to converge

#### Scenario: Playback is stopped or reset
- **WHEN** preview code stops or resets one model
- **THEN** subsequent update behavior derives from the provider track's current selection and playing state while unrelated model tracks remain unchanged

#### Scenario: Preview configuration is edited
- **WHEN** a developer changes configured clip, loop, or speed intent in the preview control
- **THEN** those values may be retained as preview configuration, but actual runtime playback status remains whatever the provider track reports

### Requirement: Streamed presenter is the sole mutable topology authority
The mutable streamed preview SHALL treat the active streamed presenter as the sole authority for attachment membership, counts, identity, and topology revision. Preview code SHALL retain opaque handles only as identities and SHALL NOT mirror current membership in independent booleans or positional state.

#### Scenario: Attachment is added
- **WHEN** the preview successfully attaches equipment during one presenter activation
- **THEN** current membership and counts are read from presenter queries and topology revision advances from its prior value

#### Scenario: Attachment is detached
- **WHEN** one mutable attachment is detached
- **THEN** presenter membership no longer contains its handle, the old handle fails closed, topology revision advances, and unaffected tracks preserve their state

#### Scenario: Attachment is rebound
- **WHEN** an active attachment is rebound to another valid socket
- **THEN** the same handle remains current, topology revision advances, and the attachment track/playback state is preserved

#### Scenario: Attachment is replaced
- **WHEN** an active attachment is transactionally replaced
- **THEN** the same public handle identifies the replacement, topology revision advances, the replacement receives provider-defined fresh playback state, and root/unrelated attachment tracks remain unchanged

#### Scenario: Presenter is released and reacquired
- **WHEN** the proof releases the presenter to its warm pool and reacquires it
- **THEN** prior activation handles fail closed, provider-reported mutable membership is empty, provider-defined retained track state is reset, and no preview membership shadow requires manual synchronization
