# Target-Driven Reference Proof

This sample is deliberately non-published: it has no `asset.yaml` and does not participate in catalog or coverage.

## Reference-fidelity verdict

PASS for the target-driven v3 reference workflow proof.

- Product Target is a canonical crop of the repository-owned high-quality bow source.
- Composition is image-first: the isolated bow dominates the sheet and detail is demonstrated with visual close-ups rather than prose.
- The stepped brown limbs, green wrap bands, dark center grip, squared tips, and thin string remain visibly consistent with the Product Target.
- No Product Target, Composition, or Part evidence declares VPU, exact voxel dimensions, source voxel budget, pivot, socket, or runtime transform.
- `Parts/Bow.png` remains construction evidence under the pinned Hero.Voxel rules; implementation density stays downstream.

## Model-authoring verdict

PASS for the bounded **target -> accepted reference -> VoxelSpec -> generated VOX -> Hero.Voxel preview** proof.

The first model attempt was rejected during visual review because its curve was too deep and its center grip too short. The retained model was re-authored against the Product Target and now preserves:

- the target's shallow U-shaped limb silhouette and broad overall proportions;
- the wide dark center grip with adjacent green wrap hierarchy;
- four readable green wrap regions rather than noisy decorative repetition;
- chunky stepped brown limb construction and squared reinforced tips;
- a taut light string visibly separated from the center grip;
- one rigid `Bow` part in both the individual-part and assembled previews.

Exact target/model hashes are sealed in `Proof/model-fidelity.audit.json`.

## Numeric model evidence

- Hero.Voxel provider revision: `56d2997250a8d49846a5246b52de5a03bc44b88e`
- VPU: 64
- Occupied voxels: 7543
- Integer bounds: 28 x 11 x 96
- Normalized long axis: 1.5 world units
- Generated VOX SHA-256: `c2baa287bec8d7763c682b42904876d9fc3f515cb2a161885e587d4a6a0543d6`
- Source voxel budget: 65536

This closes the missing authoring-model proof only. Because the sample is intentionally non-published, it does not claim the later Unity-baked runtime visual-acceptance gate that a production asset would still require.
