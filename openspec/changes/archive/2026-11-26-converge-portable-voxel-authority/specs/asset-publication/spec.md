## ADDED Requirements

### Requirement: Sealed evidence is checkout-portable for repository text
Reference and proof evidence SHALL preserve content identity across Git checkout line-ending representation for repository text files while retaining byte-exact identity for binary files. Seal generation and verification SHALL use the same canonical evidence-hash rules.

#### Scenario: LF text is checked out as CRLF
- **WHEN** a sealed repository text input has identical Unicode bytes except for LF versus CRLF or lone-CR line endings
- **THEN** audit verification resolves the same evidence hash and does not mark the evidence stale solely because of line-ending representation

#### Scenario: Sealed text content changes
- **WHEN** any non-line-ending content in a sealed text input changes
- **THEN** audit verification reports stale evidence

#### Scenario: Binary evidence changes by one byte
- **WHEN** any byte in a sealed binary input changes
- **THEN** audit verification reports stale evidence without applying text normalization
