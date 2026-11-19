# KhopeshStyleTest model intent

This non-published sample exists to test one specific claim: assets may share `low-resolution-voxel.v1` visual grammar without sharing the same palette, material hierarchy, or decorative recipe.

It intentionally does **not** reuse the brown-wood + green-wrap + bronze-accent direction used by the nearby WarHammerStyleTest and SpearStyleTest samples.

Design goals:

1. Read immediately as a khopesh: compact grip, short neck, forward-curving hooked blade, and enlarged outer cutting arc.
2. Keep the same shared voxel grammar: chunky low-resolution forms, simplified readable geometry, restrained tertiary detail, clear material regions, and strong silhouette readability.
3. Use a distinct material identity: charcoal/black blade, pale ivory grip, muted violet binding, and a small turquoise inset.
4. Avoid brown wood, green wraps, bronze/gold accents, and the previous samples' repeated banding pattern.
5. Use stepped blade growth to express the hook rather than smoothing the curve with high VPU.
6. Keep the whole sample as one rigid `Khopesh` part with the long axis along +Z.

The sample uses VPU16. The silhouette relies on coarse stepped blade modules and intentional one-voxel highlight bands rather than high-resolution smoothing.
