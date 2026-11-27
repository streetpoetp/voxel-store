# Spec Delta

## Purpose

Defines the single reusable one-asset authoring pipeline that coordinates voxel-store intent, visual authority, source, provider presentation proof, compatibility, evidence sealing, and publication without duplicating stage validity rules.

## ADDED Requirements

### Requirement: One pipeline owns one asset job
The asset-authoring pipeline SHALL own exactly one asset-scoped job from its accepted intent until that asset is published, blocked, or intentionally stopped. It SHALL NOT switch to another asset to bypass an unresolved stage.

#### Scenario: New asset request enters the pipeline
- **WHEN** a user request or deterministic maintenance selection establishes one new production asset intent
- **THEN** the pipeline keeps that asset as the sole job through all applicable downstream stages

#### Scenario: Current asset is blocked
- **WHEN** the active asset cannot satisfy the authority or evidence required by its current stage
- **THEN** the pipeline reports that stage and blocking evidence and does not select or create another asset

### Requirement: Pipeline stages have one canonical order
The pipeline SHALL evaluate stages in this order: Intent, Product Target, Construction, Density, Source, Generate, Runtime Presentation Proof, Compatibility, Seal, Publish. A stage MAY be not applicable only when its owning contract explicitly permits that result.

#### Scenario: Earlier stage is unresolved
- **WHEN** Product Target or Construction authority is not accepted
- **THEN** the pipeline does not advance to Density, Source, Generate, proof, compatibility, sealing, or publication

#### Scenario: Density is not applicable
- **WHEN** the active asset's current density contract does not require reference-driven density selection
- **THEN** Density is recorded as not applicable and Source remains governed by the asset's existing source contract

#### Scenario: A stage is applicable
- **WHEN** the owning contract requires evidence for a pipeline stage
- **THEN** the pipeline completes and validates that evidence before advancing to the next stage

### Requirement: Pipeline delegates stage authority
The pipeline SHALL coordinate existing authority owners rather than restating or replacing their validity rules. Product Target, construction, density, source/generation, runtime presentation proof, compatibility, sealing, and publication remain governed by their current Tool, OpenSpec, Hero.Voxel, preview, or specialized-skill contracts.

#### Scenario: Product Target is required
- **WHEN** the active asset enters Product Target
- **THEN** the pipeline delegates acquisition and visual acceptance to the current Product Target authority and advances only after its required evidence is current

#### Scenario: Construction is required
- **WHEN** accepted Product Target authority exists and the asset requires Composition or Part construction evidence
- **THEN** the pipeline follows the pinned Hero.Voxel construction workflow and current target/style fidelity contracts

#### Scenario: Publication validation disagrees with pipeline progress
- **WHEN** deterministic Tool/spec validation reports an earlier unresolved gate than the pipeline's apparent stage
- **THEN** the earlier authoritative gate wins and the pipeline resumes from that owner rather than overriding it

### Requirement: Runtime presentation proof remains downstream evidence
Runtime Presentation Proof SHALL exercise the pinned Hero Studio presentation behavior needed by the current asset while remaining derived preview/evidence state. It SHALL NOT become Product Target, construction, source, compatibility, or publication authority by itself.

#### Scenario: Composed asset requires runtime presentation evidence
- **WHEN** the current asset's proof or compatibility contracts require composed/provider presentation behavior
- **THEN** the pipeline runs the applicable provider-backed preview/capture proof before advancing

#### Scenario: Runtime presentation proof is not required
- **WHEN** no current contract requires additional runtime presentation evidence for the active asset
- **THEN** the stage is explicitly not applicable and the pipeline does not invent a new proof requirement

#### Scenario: Runtime capture passes visually
- **WHEN** a provider-attested capture succeeds
- **THEN** that evidence may satisfy its owning proof contract but does not replace upstream source/reference or downstream compatibility/publication validation

### Requirement: Sealing follows accepted evidence
The pipeline SHALL seal reference or proof evidence only after the evidence has passed its owning visual and deterministic acceptance checks. It SHALL NOT use sealing as a substitute for inspection or validation.

#### Scenario: Proof exists but is not accepted
- **WHEN** generated or runtime proof files exist but required visual acceptance has not passed
- **THEN** the pipeline does not seal or publish that evidence

#### Scenario: Evidence changes after sealing
- **WHEN** an authoritative input changes and invalidates previously sealed evidence
- **THEN** the pipeline returns to the earliest affected stage and requires fresh acceptance before resealing

### Requirement: Creation and maintenance share the same pipeline
Direct model-creation requests and maintenance-selected asset work SHALL converge on the same asset-authoring pipeline. Compatibility entrypoints MAY select the initial intent/current stage, but SHALL NOT define a second end-to-end stage sequence.

#### Scenario: User directly requests a new model
- **WHEN** a create-model entrypoint receives one valid production asset request
- **THEN** it establishes the asset intent and delegates the job to the shared pipeline

#### Scenario: Maintenance selects an existing repair
- **WHEN** deterministic maintenance selects one asset and its earliest current gate
- **THEN** maintenance delegates that same asset into the shared pipeline at the corresponding owning stage

#### Scenario: Entry-point documentation changes
- **WHEN** create-model or maintenance guidance describes the workflow
- **THEN** it references the shared pipeline and does not copy a divergent stage definition
