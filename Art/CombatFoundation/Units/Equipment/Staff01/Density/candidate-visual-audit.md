# Staff01 Density Candidate Visual Audit

## Accepted reference features

- shaft-thickness-transition
- gold-collar
- purple-cage-bar
- purple-cage-gap
- crystal-facet

## Candidate results

### VPU 64

Reject.

The overall Staff silhouette is readable, but the accepted Part Sheet's thin cage gap and crystal-facet separation collapse into chunkier bands. Shaft thickness transitions, gold collars, and cage bars remain readable.

### VPU 80

Pass.

All declared features remain visually distinguishable while normalized long-axis size remains 1.5 world units. The cage gap and cyan crystal facet separation survive without requiring a denser source.

### VPU 96

Pass, completeness check only.

The render is slightly denser, but it does not reveal a new meaningful construction feature absent from `Density/input.yaml`. Therefore the VPU80 passing candidate remains the lowest sufficient candidate.

## Search stop

The search stops after VPU80 first passes plus the required next-higher VPU96 completeness check.
