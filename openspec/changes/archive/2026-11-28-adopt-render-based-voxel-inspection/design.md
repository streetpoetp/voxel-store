## Context

The grid already realizes ordinary provider render instances, actors and independent tracks. Synthetic GameObjects exist only for the retired registration API.

## Decisions

Keep the single world host, layout, asset selection and authored bindings. Remove the empty grid/entry/attachment hosts. Retain canonical tracks and hold the provider world preview scope from build to disposal; revoke before disposing actors and instances. Open the provider window after interactive launch. Proof checks real instance count, no proxies, scoped authority and rebuild invalidation, relying on provider tests for picker/UI behavior.

## Risks

Align gitlink and lock identity without changing the unchanged authoring fingerprint. Compatibility evidence records the exact provider revision: regenerate through the canonical Tool scan and verify that only the revision changes. No visual/reference resealing, source changes or provider edits. Manual Scene View UX is distinct from automated proof.
