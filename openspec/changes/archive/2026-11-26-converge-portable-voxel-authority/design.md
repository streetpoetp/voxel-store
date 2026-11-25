# Design

## Context

The repository declares LF checkout policy in `.gitattributes`, but a Windows working tree can still contain CRLF bytes for already-materialized files. Reference/proof audit evidence currently stores SHA-256 over raw file bytes. As a result, a semantically identical text input can invalidate evidence after a clean checkout even though Git and repository policy consider the text equivalent.

At the same time, the active data model no longer contains compatibility-profile fields: collection coverage is `kind + form`, equipment manifests own `attachment.capability/side/anchor`, avatar source owns concrete `Equip.*` sockets, and runtime composition uses socket × inverse-Mount. Active prose still references the retired `Profiles/**` layer and obsolete Body-ratio placement.

## Decisions

### 1. Canonicalize text only at the evidence-hash boundary

Evidence hashing will classify repository text by the extensions already governed as text in `.gitattributes`. For those files, hashing normalizes CRLF and lone CR to LF before SHA-256. Binary files continue to use raw bytes.

This avoids a repository-wide 343-file line-ending churn while making evidence portable across clean checkouts. It also preserves byte-exact identity for PNG/VOX and other binary proof.

### 2. One hash function owns seal and verify semantics

Seal generation and audit verification must call the same evidence-hash function. Tests cover:

- LF and CRLF representations of identical text hash equally.
- substantive text mutation produces a different hash and fails audit verification.
- binary bytes remain byte-sensitive.

The same canonical text representation also governs reproducibility checks for
machine-generated text siblings such as `*.animation.generated.yaml`; a clean
checkout with LF bytes must not fail generation validation merely because the
generating Windows checkout previously materialized CRLF bytes.

### 3. Remove Profiles from active authority, not from history

No historical archive is rewritten. Active `AGENTS.md`, active creation skill, and durable maintenance requirements stop naming `Profiles/**` as current authority. Compatibility remains gameplay-neutral and is expressed through:

- model-owned avatar sockets,
- equipment Mount + capability/side metadata,
- physical form,
- current publication/compatibility proof.

### 4. Compatibility spec has one attachment model

The duplicated canonical-frame requirement is collapsed and all active composition scenarios use socket/Mount language. Body-ratio references are removed because they contradict the later authoritative socket requirements and runtime.

### 5. Performance evidence is explicitly out of scope

The existing Reference Android campaign predates the current actor-streaming pin. This change does not rebuild or rerun it. Runtime code remains untouched; performance re-characterization can be a separate explicitly authorized operation.

## Risks / Trade-offs

- Canonical text hashing intentionally treats line endings as representation rather than semantic evidence. This is appropriate because repository policy already defines those files as text and LF-normalized.
- Extension classification must stay narrow and explicit; unknown/binary files remain raw-byte hashed.
- Removing stale Profiles prose may expose archived documents that still mention profiles. Those archives remain historical and are not active authority.

## Verification

- Tool tests including new line-ending portability cases.
- `dotnet run --project Tool/VoxelStore.Tool -- verify`.
- strict OpenSpec validation.
- searches proving active guidance/specs no longer require `Profiles/**` or Body-ratio attachment.
- `git diff --check` and clean scope review.
