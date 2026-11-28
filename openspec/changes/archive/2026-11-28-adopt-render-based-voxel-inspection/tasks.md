## 1. Provider and grid

- [x] 1.1 Upgrade provider gitlink and lock; verify unchanged authoring workflow.
- [x] 1.2 Remove synthetic model hosts and use one provider preview scope; preserve canonical composition and layout.
- [x] 1.3 Update grid proof and workflow documentation.

## 2. Verification

- [x] 2.1 Run Tool tests and store verification.
- [x] 2.2 Run Unity Editor tests and real Play Mode grid proof.
- [x] 2.3 Run existing capture regression and inspect evidence; record limitations.
- [x] 2.4 Validate OpenSpec and Git hygiene; sync and archive.

## Verification evidence

- Provider bootstrap and workflow fingerprint passed; Tool tests and read-only store verification passed (Healthy=true, no next job).
- Canonical Tool regenerated 154 compatibility reports; structured comparison with HEAD confirms only provider revision changed. No authored content or visual seal changed.
- Unity 6000.3.24f1: 21/21 Preview Editor tests passed after refreshing revision-bound compatibility evidence; the initial 19/21 result was caused by an empty published plan while those reports were stale.
- Fresh Play Mode grid proof passed: 25 entries, 47 render instances, 47 delegated canonical tracks, 0 selection hosts, 22 carried equipment instances, 0 overlaps, 47 submitted parts; rebuild invalidates old render instances.
- Graphical avatar-motion-gallery returned CAPTURE_ATTESTED with 29 fresh PNGs; representative idle, independent weapon, axe release/rejoin frames inspected. No exhaustive artistic reacceptance or manual Scene View/Inspector interaction was performed. No Player/APK built.
