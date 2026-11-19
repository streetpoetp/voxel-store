# Tasks

## 1. Simplify lifecycle navigation

- [x] 1.1 Rewrite `WORKFLOW.md` around the five-stage `Plan -> Reference -> Source -> Proof -> Publish` lifecycle.
- [x] 1.2 Keep only stage ownership, pass/fail/blocked navigation, bootstrap, and finalization in `WORKFLOW.md`; remove duplicated detailed Product Target, density, and reference semantics.
- [x] 1.3 Represent legacy v2 as a compatibility exception instead of a parallel primary workflow.

## 2. Thin the maintenance coordinator

- [x] 2.1 Rewrite `voxel-store-maintain` as a gate-driven coordinator: plan, claim one asset/current gate, execute owner, validate, repeat same asset, stop on published/blocked.
- [x] 2.2 Remove duplicated Product Target, density-selection, construction, and proof semantics from the coordinator while retaining pointers to their authoritative owners.
- [x] 2.3 Verify `voxel-store-product-target` remains the specialized owner for image-model Product Target creation and exact-byte delivery; modify only if an ownership-boundary clarification is needed.

## 3. Synchronize durable maintenance semantics

- [x] 3.1 Update `store-maintenance` so deterministic scan/plan output explicitly owns current asset/gate routing and the coordinator must not skip ahead of the earliest unresolved gate.
- [x] 3.2 Preserve one-asset-per-run, same-asset repair, healthy no-op, and explicit blocked-state semantics.

## 4. Validate

- [x] 4.1 Run strict OpenSpec validation.
- [x] 4.2 Run repository diff hygiene.
- [x] 4.3 Verify no production assets, style manifests, VPU policy, coverage policy, compatibility policy, or Hero.Voxel lock/workflow authority changed.
- [x] 4.4 Record `decouple-store-planning-from-hero-voxel-provider` as a separate follow-up rather than implementing it in this change.
