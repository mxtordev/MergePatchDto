using System.Reflection;
using MergePatchDto.Generators;
using Microsoft.CodeAnalysis;

namespace MergePatchDto.Tests;

public class DiagnosticsTests
{
    [Fact]
    public void DiagnosticDescriptorsHaveAuditedSeverities()
    {
        var expectedSeverities = new Dictionary<string, DiagnosticSeverity>
        {
            ["MPD001"] = DiagnosticSeverity.Error,
            ["MPD002"] = DiagnosticSeverity.Error,
            ["MPD003"] = DiagnosticSeverity.Error,
            ["MPD004"] = DiagnosticSeverity.Error,
            ["MPD005"] = DiagnosticSeverity.Error,
            ["MPD006"] = DiagnosticSeverity.Error,
            ["MPD007"] = DiagnosticSeverity.Error,
            ["MPD008"] = DiagnosticSeverity.Error,
            ["MPD009"] = DiagnosticSeverity.Error,
            ["MPD010"] = DiagnosticSeverity.Error,
            ["MPD011"] = DiagnosticSeverity.Error,
            ["MPD012"] = DiagnosticSeverity.Error,
            ["MPD013"] = DiagnosticSeverity.Error,
            ["MPD014"] = DiagnosticSeverity.Error,
            ["MPD015"] = DiagnosticSeverity.Error,
            ["MPD016"] = DiagnosticSeverity.Error,
            ["MPD017"] = DiagnosticSeverity.Error
        };

        var descriptors = typeof(MergePatchGenerator).Assembly
            .GetType("MergePatchDto.Generators.Diagnostics", throwOnError: true)!
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (DiagnosticDescriptor)field.GetValue(null)!)
            .ToDictionary(descriptor => descriptor.Id);

        Assert.Equal(
            expectedSeverities.Keys.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            descriptors.Keys.OrderBy(id => id, StringComparer.Ordinal).ToArray());

        foreach (var expected in expectedSeverities)
        {
            var descriptor = descriptors[expected.Key];
            Assert.Equal(expected.Value, descriptor.DefaultSeverity);
            Assert.True(descriptor.IsEnabledByDefault);
        }
    }

    [Fact]
    public void ReportsErrorWhenPatchDtoIsNotPartial()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public class Patch
            {
                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD001" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorWhenPatchTargetCannotBeResolved()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            [MergePatchTarget(typeof(MissingTarget))]
            public partial class Patch
            {
                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD002" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForConflictingPatchAttributes()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public string? Name { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                [PatchIgnore]
                [PatchTo(nameof(Target.Name))]
                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD003" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForInvalidPatchToTarget()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public string? Name { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                [PatchTo("Missing")]
                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD004" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForUnmappedConventionProperty()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public string? Name { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                public string? Description { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD005" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForMissingPatchUsingMethod()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public int? Priority { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                [PatchUsing("ApplyPriority")]
                public int? Priority { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD006" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForInvalidPatchUsingSignature()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public int? Priority { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                [PatchUsing(nameof(ApplyPriority))]
                public int? Priority { get; set; }

                private static int ApplyPriority(Target target, int? value)
                {
                    return 0;
                }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD007" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForRefPatchUsingValueParameter()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public int? Priority { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                [PatchUsing(nameof(ApplyPriority))]
                public int? Priority { get; set; }

                private static void ApplyPriority(Target target, ref int? value)
                {
                }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD007" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForOutPatchUsingValueParameter()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public int? Priority { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                [PatchUsing(nameof(ApplyPriority))]
                public int? Priority { get; set; }

                private static void ApplyPriority(Target target, out int? value)
                {
                    value = null;
                }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD007" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForInPatchUsingValueParameter()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public int? Priority { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                [PatchUsing(nameof(ApplyPriority))]
                public int? Priority { get; set; }

                private static void ApplyPriority(Target target, in int? value)
                {
                }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD007" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForTargetPropertyWithoutSetter()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public string? Name { get; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD008" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void AllowsInternalTargetSetterWhenTargetIsInSameAssembly()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public string? Name { get; internal set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                public string? Name { get; set; }
            }
            """);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "MPD008");
    }

    [Fact]
    public void ReportsErrorForInternalTargetSetterInReferencedAssembly()
    {
        var domainReference = DiagnosticTestHelper.CreateReference(
            "Domain",
            """
            namespace Domain;

            public class Target
            {
                public string? Name { get; internal set; }
            }
            """);

        var diagnostics = DiagnosticTestHelper.GetAllDiagnostics(
            """
            using Domain;
            using MergePatch;

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                public string? Name { get; set; }
            }
            """,
            [domainReference]);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD008" && diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "CS0272");
    }

    [Fact]
    public void ReportsErrorForGenericPatchDto()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public partial class Patch<T>
            {
                public T? Value { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD010" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForNestedPatchDto()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Container
            {
                [MergePatch]
                public partial class Patch
                {
                    public string? Name { get; set; }
                }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD011" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorWhenPatchDtoHasNoAccessibleParameterlessConstructor()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public partial class Patch
            {
                public Patch(string name)
                {
                    Name = name;
                }

                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD012" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForRequiredPatchDtoMember()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public partial class Patch
            {
                public required string Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD013" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForIncompatibleConventionAssignment()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public int Count { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                public string? Count { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD014" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForIncompatiblePatchToAssignment()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public int Count { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                [PatchTo(nameof(Target.Count))]
                public string? DisplayCount { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD014" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorForNullableReferencePatchPropertyAssignedToNonNullableTargetProperty()
    {
        var diagnostics = DiagnosticTestHelper.GetAllDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public string Name { get; set; } = "";
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD014" && diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "CS8601");
    }

    [Fact]
    public void AllowsNullableReferencePatchPropertyAssignedToNullableTargetProperty()
    {
        var diagnostics = DiagnosticTestHelper.GetAllDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public string? Name { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                public string? Name { get; set; }
            }
            """);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id.StartsWith("MPD", StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "CS8601");
    }

    [Fact]
    public void ReportsErrorForAbstractPatchDto()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public abstract partial class Patch
            {
                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD015" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorWhenTargetlessPatchUsesTargetMappingAttributes()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public partial class Patch
            {
                [PatchTo("MissingBecauseNoTargetExists")]
                public string? Name { get; set; }

                [PatchUsing("MissingBecauseNoTargetExists")]
                public int? PriorityDelta { get; set; }
            }
            """);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id is "MPD004" or "MPD005" or "MPD006" or "MPD007");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD009" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsTargetlessMappingErrorInsteadOfConflictsForTargetlessPatchMappingAttributes()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public partial class Patch
            {
                [PatchIgnore]
                [PatchTo("MissingBecauseNoTargetExists")]
                public string? Name { get; set; }
            }
            """);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "MPD003");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD009" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorWhenTargetlessPatchUsesPatchIgnore()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public partial class Patch
            {
                [PatchIgnore]
                public string? Name { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD009" && diagnostic.Severity == DiagnosticSeverity.Error);
    }
}
