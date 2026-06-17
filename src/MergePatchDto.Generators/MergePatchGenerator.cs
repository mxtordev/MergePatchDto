using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MergePatchDto.Generators
{
    [Generator(LanguageNames.CSharp)]
    public sealed class MergePatchGenerator : IIncrementalGenerator
    {
        private const string MergePatchAttributeName = "MergePatch.MergePatchAttribute";
        private const string MergePatchTargetAttributeName = "MergePatch.MergePatchTargetAttribute";
        private const string PatchIgnoreAttributeName = "MergePatch.PatchIgnoreAttribute";
        private const string PatchToAttributeName = "MergePatch.PatchToAttribute";
        private const string PatchUsingAttributeName = "MergePatch.PatchUsingAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var patchDtos = context.SyntaxProvider.ForAttributeWithMetadataName(
                MergePatchAttributeName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (syntaxContext, _) => BuildModel(syntaxContext))
                .Where(static model => model != null);

            var patchDtosWithCompilation = patchDtos.Combine(context.CompilationProvider);

            context.RegisterSourceOutput(patchDtosWithCompilation, static (sourceContext, item) =>
            {
                var model = item.Left;
                var compilation = item.Right;
                if (model == null)
                {
                    return;
                }

                if (!model.IsPartial)
                {
                    sourceContext.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.PatchDtoMustBePartial,
                        model.Location,
                        model.TypeSymbol.Name));
                    return;
                }

                foreach (var location in model.UnresolvedTargetLocations)
                {
                    sourceContext.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.PatchTargetCannotBeResolved,
                        location,
                        model.TypeSymbol.Name));
                }

                var source = SourceTextEmitter.Emit(model, compilation, sourceContext);
                sourceContext.AddSource(SourceTextEmitter.GetHintName(model), SourceText.From(source, System.Text.Encoding.UTF8));
            });
        }

        private static PatchDtoModel? BuildModel(GeneratorAttributeSyntaxContext context)
        {
            var typeSymbol = context.TargetSymbol as INamedTypeSymbol;
            var classDeclaration = context.TargetNode as ClassDeclarationSyntax;
            if (typeSymbol == null || classDeclaration == null)
            {
                return null;
            }

            var properties = typeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(IsPatchProperty)
                .Where(property => !JsonNameResolver.HasJsonIgnore(property))
                .Select(BuildPropertyModel)
                .ToImmutableArray();

            var patchDtoAttribute = context.Attributes.FirstOrDefault(attribute => IsAttribute(attribute, MergePatchAttributeName));

            var targetAttributes = typeSymbol
                .GetAttributes()
                .Where(attribute => IsAttribute(attribute, MergePatchTargetAttributeName))
                .ToArray();

            var targets = targetAttributes
                .Concat(patchDtoAttribute == null ? Enumerable.Empty<AttributeData>() : new[] { patchDtoAttribute })
                .Select(BuildTargetModel)
                .Where(target => target != null)
                .Cast<PatchTargetModel>()
                .GroupBy(target => target.TargetType, SymbolEqualityComparer.Default)
                .Select(group => group.First())
                .ToImmutableArray();

            var unresolvedTargets = targetAttributes
                .Concat(patchDtoAttribute == null ? Enumerable.Empty<AttributeData>() : new[] { patchDtoAttribute })
                .Where(HasTargetConstructorArgument)
                .Where(attribute => BuildTargetModel(attribute) == null)
                .Select(attribute => attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation())
                .Where(location => location != null)
                .Cast<Location>()
                .ToImmutableArray();

            return new PatchDtoModel(
                typeSymbol,
                typeSymbol.Name,
                GetNamespace(typeSymbol),
                GetTypeName(typeSymbol),
                GetAccessibility(typeSymbol.DeclaredAccessibility),
                IsPartial(typeSymbol, classDeclaration),
                HasRejectUnknownPropertyHandling(patchDtoAttribute),
                properties,
                targets,
                unresolvedTargets,
                classDeclaration.Identifier.GetLocation());
        }

        private static PatchPropertyModel BuildPropertyModel(IPropertySymbol property)
        {
            string? patchToTargetName = null;
            string? patchUsingMethodName = null;
            var hasPatchIgnore = false;

            foreach (var attribute in property.GetAttributes())
            {
                if (IsAttribute(attribute, PatchIgnoreAttributeName))
                {
                    hasPatchIgnore = true;
                    continue;
                }

                if (IsAttribute(attribute, PatchToAttributeName))
                {
                    patchToTargetName = GetStringConstructorArgument(attribute);
                    continue;
                }

                if (IsAttribute(attribute, PatchUsingAttributeName))
                {
                    patchUsingMethodName = GetStringConstructorArgument(attribute);
                }
            }

            return new PatchPropertyModel(
                property,
                property.Name,
                ToTypeName(property.Type),
                property.SetMethod?.IsInitOnly == true,
                JsonNameResolver.GetJsonPropertyName(property),
                hasPatchIgnore,
                patchToTargetName,
                patchUsingMethodName,
                property.Locations.FirstOrDefault());
        }

        private static PatchTargetModel? BuildTargetModel(AttributeData attribute)
        {
            if (!HasTargetConstructorArgument(attribute))
            {
                return null;
            }

            var targetType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
            if (targetType == null || targetType.TypeKind == TypeKind.Error)
            {
                return null;
            }

            return new PatchTargetModel(targetType, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());
        }

        private static bool HasTargetConstructorArgument(AttributeData attribute)
        {
            return attribute.ConstructorArguments.Length == 1;
        }

        private static bool IsPatchProperty(IPropertySymbol property)
        {
            if (property.IsStatic ||
                property.DeclaredAccessibility != Accessibility.Public ||
                property.GetMethod == null ||
                property.SetMethod == null)
            {
                return false;
            }

            return property.GetMethod.DeclaredAccessibility == Accessibility.Public &&
                   property.SetMethod.DeclaredAccessibility == Accessibility.Public;
        }

        private static bool IsPartial(INamedTypeSymbol symbol, ClassDeclarationSyntax currentDeclaration)
        {
            if (currentDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return true;
            }

            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax() as ClassDeclarationSyntax;
                if (syntax != null && syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasRejectUnknownPropertyHandling(AttributeData? patchDtoAttribute)
        {
            if (patchDtoAttribute == null)
            {
                return false;
            }

            foreach (var argument in patchDtoAttribute.NamedArguments)
            {
                if (argument.Key != "UnknownPropertyHandling")
                {
                    continue;
                }

                return argument.Value.Value is int value && value == 1;
            }

            return false;
        }

        private static string? GetStringConstructorArgument(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length != 1)
            {
                return null;
            }

            return attribute.ConstructorArguments[0].Value as string;
        }

        private static bool IsAttribute(AttributeData attribute, string fullyQualifiedMetadataName)
        {
            return attribute.AttributeClass?.ToDisplayString() == fullyQualifiedMetadataName;
        }

        private static string GetNamespace(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : typeSymbol.ContainingNamespace.ToDisplayString();
        }

        private static string GetTypeName(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers));
        }

        private static string GetAccessibility(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Public:
                    return "public";
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.Private:
                    return "private";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.ProtectedAndInternal:
                    return "private protected";
                case Accessibility.ProtectedOrInternal:
                    return "protected internal";
                default:
                    return "internal";
            }
        }

        private static string ToTypeName(ITypeSymbol symbol)
        {
            return symbol.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                    SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));
        }
    }
}
