# Changelog

## 0.2.0

Breaking changes:

- Removed `[MergePatchTarget]`. Use `[MergePatch(typeof(Target))]` for the generated `ApplyTo` target; additional generated target overloads are no longer supported.
- Generated converters now reject explicit JSON `null` for non-nullable reference patch properties.
- Generated `ApplyTo` and `[PatchUsing]` validation now fail at build time when nullable patch values could flow into non-nullable target members or parameters.

Added:

- Support for inherited patch DTO properties, including inherited JSON and mapping attributes.
- Build-time diagnostics for record patch DTOs, open generic target types, less-accessible target types, duplicate inherited CLR property names, and duplicate explicit JSON property names.
- Runtime JSON-name collision validation for naming-policy-derived and case-insensitive duplicate names.

Fixed:

- Strict unknown-property handling no longer needs to read an unknown property's value before rejecting it.
- Property-level JSON converter handling now supports custom `JsonConverterAttribute` subclasses and factory-style converter attributes.
- Release packaging no longer treats the generator project as a separate package and enables CI deterministic/source-link builds.

Changed:

- README and sample API docs now use the current `[MergePatch(typeof(Target))]` shape and `NoContent` PATCH responses.

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
