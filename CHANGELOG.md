# Changelog

## 0.1.1 - README polish

Docs-only release to refresh the NuGet package README.

- Clarify the DTO-shaped PATCH problem statement.
- Explain how MergePatchDto differs from ASP.NET Core JSON Patch operation documents.
- Make quick-start, API overview, mapping attributes, compatibility, and limitations easier to scan.

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
