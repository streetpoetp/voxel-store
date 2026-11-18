# Spec Delta

## ADDED Requirements

### Requirement: Reference-driven equipment requires current density evidence
Equipment that explicitly opts into `density: reference-driven` SHALL pass a density gate after reference acceptance and before source authority. The density gate SHALL require current reference-density evidence, and the subsequent source gate SHALL require authored source to use the selected VPU. Equipment without the opt-in remains valid legacy content during incremental migration and is not subject to the density gate.

#### Scenario: Current density evidence matches source
- **WHEN** equipment declares `density: reference-driven`, has current accepted reference evidence, current density-selection evidence, and source authored at the selected VPU
- **THEN** the density gate passes and the source gate verifies the authored VPU before later generation/proof gates run

#### Scenario: Legacy equipment has not opted into density selection
- **WHEN** equipment does not declare `density: reference-driven`
- **THEN** publication follows the pre-density gate sequence and does not require density evidence during incremental migration


#### Scenario: Density evidence is missing
- **WHEN** equipment declares `density: reference-driven` and has accepted Part Sheets but no density-selection evidence
- **THEN** publication fails at the density gate before source authority is accepted

#### Scenario: Density evidence is stale
- **WHEN** any accepted Part Sheet or density input changes after VPU selection
- **THEN** publication fails at the density gate until density selection is recomputed

#### Scenario: Source VPU disagrees with selected VPU
- **WHEN** current density evidence selects one VPU but the authoritative equipment source declares another VPU
- **THEN** publication fails at the source gate instead of silently accepting the unselected density
