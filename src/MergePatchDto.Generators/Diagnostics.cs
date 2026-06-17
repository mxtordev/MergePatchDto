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
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor TargetlessMappingAttributeIgnored = new DiagnosticDescriptor(
            "MPD009",
            "Targetless patch property has ignored mapping attributes",
            "Patch property '{0}' uses {1}, but targetless merge patch types do not generate ApplyTo; the attribute is ignored",
            "MergePatchDto",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
