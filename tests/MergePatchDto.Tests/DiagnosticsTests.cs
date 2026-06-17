using Microsoft.CodeAnalysis;

namespace MergePatchDto.Tests;

public class DiagnosticsTests
{
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
    public void WarnsWhenTargetlessPatchUsesTargetMappingAttributes()
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
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD009" && diagnostic.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void WarnsInsteadOfReportingConflictsForTargetlessPatchMappingAttributes()
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
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD009" && diagnostic.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void WarnsWhenTargetlessPatchUsesPatchIgnore()
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

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD009" && diagnostic.Severity == DiagnosticSeverity.Warning);
    }
}
