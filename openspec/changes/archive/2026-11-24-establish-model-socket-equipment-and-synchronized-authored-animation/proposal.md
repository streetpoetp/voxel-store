# Proposal

## Why

The current compatibility model derives equipment placement from Body bounds ratios and the preview synthesizes motion procedurally, so independently authored equipment cannot own a true grip/mount point or its own animation while remaining physically locked to an animated body. Hero.Voxel already provides model sockets and authored animation sampling, so the store should make those model-owned contracts authoritative instead of maintaining a parallel ratio/DOTween attachment grammar.

## What Changes

- **BREAKING** Replace normalized Body-ratio equipment placement with model-authored socket attachment.
- Require reusable avatar/body VoxelSpecs to own physical equipment slot sockets using a shared capability vocabulary such as `Hand`, `Back`, `Head`, `Hip`, and `Chest`. Every physical slot has an explicit side (`Left`, `Right`, or `Center`) plus an instance index, for example `Equip.Hand.Left.0`, `Equip.Hand.Right.0`, `Equip.Back.Left.0`, and `Equip.Back.Right.0`; a body may expose multiple instances on the same side.
- Require each equipment asset to declare its logical attachment capability in its manifest and to own an exact `Mount` socket in its VoxelSpec. Equipment never names a concrete avatar socket instance and never creates form-specific slot types such as `SwordSlot`.
- Allow multiple instances of the same equipment asset or form to compose on one body whenever distinct compatible slot instances exist. Slot multiplicity belongs to the body model; equipment does not declare uniqueness or one-per-kind limits.
- Make the selected physical slot side part of semantic motion context. Side-sensitive body motion may author distinct variants such as `Attack.Left` and `Attack.Right`; the active equipment binding chooses the corresponding body variant while the same equipment asset remains reusable on either side.
- Align equipment by solving its root transform from the selected avatar socket and the equipment's evaluated `Mount` socket, without Body ratios, pair-specific corrections, or runtime corrective rotation.
- **BREAKING** Replace preview-authored procedural body/equipment motion as canonical animation authority with authored Hero.Voxel animation clips.
- Require every reusable avatar and equipment asset to own the baseline semantic clips `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death`; each asset owns its own clip curves.
- Drive an equipped body and every attached equipment item from one semantic normalized phase. The body clip owns wall-clock cadence; equipment clips are sampled at the same normalized phase so independently authored weapon motion cannot drift from body motion.
- Keep command-driven graphical Game View capture as the canonical visual proof for socket attachment and synchronized authored animation.
- Preserve the canonical Unity model frame and publication-stable equipment `form`; attachment capability and animation remain visual/physical asset contracts, not gameplay combat rules.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-publication`: publication must validate model-owned equipment slots, equipment mount anchors, attachment capability metadata, and authored semantic animation coverage.
- `asset-compatibility`: reusable composition changes from Body-ratio placement to avatar-socket/equipment-anchor alignment and adds normalized-phase synchronization between authored body and equipment animation.
- `unity-baked-preview`: preview composition and motion proof must use baked Hero.Voxel sockets and authored clips rather than Body-ratio placement plus procedural DOTween motion.

## Impact

- `Tool/VoxelStore.Tool/**`: manifest contracts, validation, compatibility evidence, catalog projection, and migration from ratio-placement gates.
- `Assets/Avatars/**/Source/**`: published avatars gain equipment slot sockets and authored semantic Body clips.
- `Assets/Equipment/**/asset.yaml` and `Source/**`: equipment gains logical attachment metadata, a model-owned `Mount` socket, and asset-owned semantic animation clips.
- `Preview/Unity/Assets/Preview/**`: baked composition resolves sockets dynamically and samples provider animation assets through one shared semantic phase.
- Compatibility/proof/catalog generated evidence must be regenerated and resealed after migration.
- Existing asset identities and canonical model orientation remain unchanged.
