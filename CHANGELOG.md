# Changelog

## 0.2.0

Breaking changes:

- Removed `[MergePatchTarget]`; use `[MergePatch(typeof(Target))]` for generated `ApplyTo`. Additional generated target overloads are no longer supported.
- Non-nullable reference patch properties now reject explicit JSON `null`.
- Nullable patch values now fail build-time validation when they would flow into non-nullable `ApplyTo` targets or `[PatchUsing]` parameters.

Changes:

- Inherited patch DTO properties now participate in presence tracking, JSON metadata, and generated `ApplyTo`.
- Added clearer diagnostics for unsupported patch shapes, inaccessible targets, duplicate names, and ambiguous mappings.
- Fixed strict unknown-property rejection, property-level `JsonConverterAttribute` handling, and release packaging metadata.

## 0.1.0 - Initial release

MergePatchDto provides source-generated DTO presence tracking for merge-patch-style ASP.NET Core endpoints.

Supported surface:

- top-level JSON property presence tracking, including explicit `null`
- generated `Has` API for domain-driven update logic
- targeted patch DTOs with generated `ApplyTo` methods
- targetless patch DTOs for presence tracking without generated mapping
- `System.Text.Json` converter generation with common property-level JSON attributes
- build-time diagnostics for invalid patch shapes and mappings

Intentional limitations and non-goals:

- not a full RFC 7396 JSON document merge engine
- no recursive merge behavior for nested object properties
- arrays are replacement values, not element-level merges
- no JSON Patch operation arrays
- no expression-tree mapping, wrapper-property model, or DI-driven `ApplyTo`
