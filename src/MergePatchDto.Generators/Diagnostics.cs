using Microsoft.CodeAnalysis;

namespace MergePatchDto.Generators
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticDescriptor PatchDtoMustBePartial = new DiagnosticDescriptor(
            "MPD001",
            "Patch DTO must be partial",
            "Patch DTO '{0}' must be declared partial",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PatchTargetCannotBeResolved = new DiagnosticDescriptor(
            "MPD002",
            "Patch target cannot be resolved",
            "Patch target for '{0}' cannot be resolved",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ConflictingPatchAttributes = new DiagnosticDescriptor(
            "MPD003",
            "Patch property has conflicting mapping attributes",
            "Patch property '{0}' cannot combine {1}",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PatchToTargetMissing = new DiagnosticDescriptor(
            "MPD004",
            "PatchTo target property does not exist",
            "Patch property '{0}' maps to missing target property '{1}' on '{2}'",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ConventionTargetMissing = new DiagnosticDescriptor(
            "MPD005",
            "Patch property cannot be mapped by convention",
            "Patch property '{0}' has no same-name target property on '{1}'; add PatchTo, PatchIgnore, or PatchUsing",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PatchUsingMethodMissing = new DiagnosticDescriptor(
            "MPD006",
            "PatchUsing method does not exist",
            "PatchUsing method '{0}' for patch property '{1}' does not exist",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PatchUsingMethodInvalid = new DiagnosticDescriptor(
            "MPD007",
            "PatchUsing method signature is invalid",
            "PatchUsing method '{0}' for patch property '{1}' must be static void {0}({2} target, {3} value) or static void {0}({4} patch, {2} target, {3} value)",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor TargetPropertyNoSetter = new DiagnosticDescriptor(
            "MPD008",
            "Target property has no accessible setter",
            "Target property '{0}' on '{1}' has no accessible setter; MergePatchDto will not generate an assignment for patch property '{2}'",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor TargetlessMappingAttributeIgnored = new DiagnosticDescriptor(
            "MPD009",
            "Targetless patch property has target-specific mapping attributes",
            "Patch property '{0}' uses {1}, but targetless merge patch types do not generate ApplyTo; add a target type or remove the target-specific attribute",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor GenericPatchDtoNotSupported = new DiagnosticDescriptor(
            "MPD010",
            "Generic merge patch DTOs are not supported",
            "Merge patch DTO '{0}' cannot be generic",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NestedPatchDtoNotSupported = new DiagnosticDescriptor(
            "MPD011",
            "Nested merge patch DTOs are not supported",
            "Merge patch DTO '{0}' cannot be nested inside another type",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PatchDtoParameterlessConstructorRequired = new DiagnosticDescriptor(
            "MPD012",
            "Merge patch DTO requires an accessible parameterless constructor",
            "Merge patch DTO '{0}' must have an accessible parameterless constructor so its generated JSON converter can create it",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor RequiredPatchDtoMembersNotSupported = new DiagnosticDescriptor(
            "MPD013",
            "Required merge patch DTO members are not supported",
            "Merge patch DTO '{0}' uses required member '{1}', which is not supported by the generated converter",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PatchPropertyCannotAssignToTargetProperty = new DiagnosticDescriptor(
            "MPD014",
            "Patch property cannot be assigned to target property",
            "Patch property '{0}' of type '{1}' cannot be assigned to target property '{2}' of type '{3}' on '{4}'",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor AbstractPatchDtoNotSupported = new DiagnosticDescriptor(
            "MPD015",
            "Abstract merge patch DTOs are not supported",
            "Merge patch DTO '{0}' cannot be abstract because its generated JSON converter must create an instance",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor GeneratedPublicMemberConflict = new DiagnosticDescriptor(
            "MPD016",
            "Merge patch DTO member conflicts with generated API",
            "Merge patch DTO '{0}' already defines member '{1}', which conflicts with the generated MergePatchDto API",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateExplicitJsonPropertyName = new DiagnosticDescriptor(
            "MPD017",
            "Merge patch DTO properties have duplicate JSON property names",
            "Patch properties '{0}' and '{1}' both use JSON property name '{2}'",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor OpenGenericPatchTargetNotSupported = new DiagnosticDescriptor(
            "MPD018",
            "Open generic patch targets are not supported",
            "Patch target '{0}' for merge patch DTO '{1}' cannot be an open generic type",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor RecordPatchDtoNotSupported = new DiagnosticDescriptor(
            "MPD019",
            "Record class merge patch DTOs are not supported",
            "Merge patch DTO '{0}' cannot be a record class; use a partial class",
            "MergePatchDto",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
