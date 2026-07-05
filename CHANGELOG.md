# Changelog

## 0.2.0 - Unreleased

Breaking changes:

- Removed `[MergePatchTarget]`. Pass the target type directly to `[MergePatch(typeof(Target))]` instead.
- Tightened generated `ApplyTo` validation so unsafe nullable-to-non-nullable assignments fail at build time.

Added:

- Support for inherited patch DTO properties.
- Build-time diagnostics for more unsupported patch shapes and mappings, including generic, nested, abstract, record, required-member, constructor, duplicate-name, inaccessible-target, and open-generic-target cases.
- Duplicate JSON property name diagnostics for explicit and naming-policy-derived names.

Fixed:

- Strict unknown-property handling now rejects unknown JSON members while reading the generated converter.
- Property-level JSON converter detection now supports attributes derived from `JsonConverterAttribute`.
- Release packaging now includes the analyzer and symbols in the expected NuGet package locations.

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
