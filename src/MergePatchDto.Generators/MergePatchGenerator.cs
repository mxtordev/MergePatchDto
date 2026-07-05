using System.Collections.Immutable;
using System.Collections.Generic;
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
                static (node, _) => IsPatchDtoCandidate(node),
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

                if (model.IsRecordClass)
                {
                    sourceContext.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.RecordPatchDtoNotSupported,
                        model.Location,
                        model.TypeSymbol.Name));
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

                if (ReportUnsupportedPatchTypeShape(sourceContext, model))
                {
                    return;
                }

                foreach (var location in model.UnresolvedTargetLocations)
                {
                    sourceContext.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.PatchTargetCannotBeResolved,
                        location,
                        model.TypeSymbol.Name));
                }

                if (ReportOpenGenericTargets(sourceContext, model))
                {
                    return;
                }

                if (ReportLessAccessibleTargets(sourceContext, model))
                {
                    return;
                }

                if (ReportGeneratedPublicMemberConflicts(sourceContext, model))
                {
                    return;
                }

                if (ReportDuplicatePatchPropertyNames(sourceContext, model))
                {
                    return;
                }

                if (ReportDuplicateExplicitJsonPropertyNames(sourceContext, model))
                {
                    return;
                }

                var source = SourceTextEmitter.Emit(model, compilation, sourceContext);
                sourceContext.AddSource(SourceTextEmitter.GetHintName(model), SourceText.From(source, System.Text.Encoding.UTF8));
            });
        }

        private static bool ReportUnsupportedPatchTypeShape(SourceProductionContext context, PatchDtoModel model)
        {
            var hasErrors = false;

            if (model.TypeSymbol.TypeParameters.Length > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.GenericPatchDtoNotSupported,
                    model.Location,
                    model.TypeSymbol.Name));
                hasErrors = true;
            }

            if (model.TypeSymbol.ContainingType != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.NestedPatchDtoNotSupported,
                    model.Location,
                    model.TypeSymbol.Name));
                hasErrors = true;
            }

            if (model.TypeSymbol.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.AbstractPatchDtoNotSupported,
                    model.Location,
                    model.TypeSymbol.Name));
                hasErrors = true;
            }

            if (!HasAccessibleParameterlessConstructor(model.TypeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.PatchDtoParameterlessConstructorRequired,
                    model.Location,
                    model.TypeSymbol.Name));
                hasErrors = true;
            }

            foreach (var member in model.TypeSymbol.GetMembers())
            {
                if (member is IPropertySymbol { IsRequired: true } property)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.RequiredPatchDtoMembersNotSupported,
                        property.Locations.FirstOrDefault() ?? model.Location,
                        model.TypeSymbol.Name,
                        property.Name));
                    hasErrors = true;
                }

                if (member is IFieldSymbol { IsRequired: true } field)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.RequiredPatchDtoMembersNotSupported,
                        field.Locations.FirstOrDefault() ?? model.Location,
                        model.TypeSymbol.Name,
                        field.Name));
                    hasErrors = true;
                }
            }

            return hasErrors;
        }

        private static bool ReportOpenGenericTargets(SourceProductionContext context, PatchDtoModel model)
        {
            var hasErrors = false;

            foreach (var target in model.OpenGenericTargets)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.OpenGenericPatchTargetNotSupported,
                    target.Location,
                    ToDiagnosticTypeName(target.TargetType),
                    model.TypeSymbol.Name));
                hasErrors = true;
            }

            return hasErrors;
        }

        private static bool ReportLessAccessibleTargets(SourceProductionContext context, PatchDtoModel model)
        {
            var hasErrors = false;
            var patchDtoVisibility = GetEffectiveVisibility(model.TypeSymbol);

            foreach (var target in model.Targets)
            {
                if (GetEffectiveVisibility(target.TargetType) >= patchDtoVisibility)
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.PatchTargetLessAccessibleThanPatchDto,
                    target.Location,
                    ToDiagnosticTypeName(target.TargetType),
                    ToDiagnosticTypeName(model.TypeSymbol)));
                hasErrors = true;
            }

            return hasErrors;
        }

        private static bool ReportGeneratedPublicMemberConflicts(SourceProductionContext context, PatchDtoModel model)
        {
            var hasErrors = false;

            foreach (var member in model.TypeSymbol.GetMembers())
            {
                if (member.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (!IsGeneratedPublicMemberName(model, member.Name))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.GeneratedPublicMemberConflict,
                    member.Locations.FirstOrDefault() ?? model.Location,
                    model.TypeSymbol.Name,
                    member.Name));
                hasErrors = true;
            }

            return hasErrors;
        }

        private static bool ReportDuplicatePatchPropertyNames(SourceProductionContext context, PatchDtoModel model)
        {
            var hasErrors = false;
            var seenNames = new System.Collections.Generic.Dictionary<string, PatchPropertyModel>(System.StringComparer.Ordinal);

            foreach (var property in model.Properties)
            {
                if (seenNames.TryGetValue(property.Name, out var existingProperty))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.DuplicatePatchPropertyName,
                        property.Location,
                        property.Name,
                        ToDiagnosticTypeName(property.Symbol.ContainingType),
                        existingProperty.Name,
                        ToDiagnosticTypeName(existingProperty.Symbol.ContainingType)));
                    hasErrors = true;
                    continue;
                }

                seenNames.Add(property.Name, property);
            }

            return hasErrors;
        }

        private static bool ReportDuplicateExplicitJsonPropertyNames(SourceProductionContext context, PatchDtoModel model)
        {
            var hasErrors = false;
            var seenNames = new System.Collections.Generic.Dictionary<string, PatchPropertyModel>(System.StringComparer.Ordinal);

            foreach (var property in model.Properties)
            {
                if (property.ExplicitJsonName == null)
                {
                    continue;
                }

                if (seenNames.TryGetValue(property.ExplicitJsonName, out var existingProperty))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.DuplicateExplicitJsonPropertyName,
                        property.Location,
                        existingProperty.Name,
                        property.Name,
                        property.ExplicitJsonName));
                    hasErrors = true;
                    continue;
                }

                seenNames.Add(property.ExplicitJsonName, property);
            }

            return hasErrors;
        }

        private static bool IsGeneratedPublicMemberName(PatchDtoModel model, string memberName)
        {
            if (memberName == "Has" || memberName == "ProvidedFields")
            {
                return true;
            }

            return model.Targets.Length > 0 && memberName == "ApplyTo";
        }

        private static bool HasAccessibleParameterlessConstructor(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.InstanceConstructors.Any(constructor =>
                constructor.Parameters.Length == 0 &&
                !constructor.IsStatic &&
                IsConstructorAccessibleFromGeneratedConverter(constructor));
        }

        private static bool IsConstructorAccessibleFromGeneratedConverter(IMethodSymbol constructor)
        {
            switch (constructor.DeclaredAccessibility)
            {
                case Accessibility.Public:
                case Accessibility.Internal:
                case Accessibility.ProtectedOrInternal:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsPatchDtoCandidate(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax || node is RecordDeclarationSyntax;
        }

        private static EffectiveVisibility GetEffectiveVisibility(ITypeSymbol typeSymbol)
        {
            var visibility = GetDeclaredVisibility(typeSymbol.DeclaredAccessibility);

            if (typeSymbol is INamedTypeSymbol namedType)
            {
                if (namedType.ContainingType != null)
                {
                    visibility = Min(visibility, GetEffectiveVisibility(namedType.ContainingType));
                }

                foreach (var typeArgument in namedType.TypeArguments)
                {
                    visibility = Min(visibility, GetEffectiveVisibility(typeArgument));
                }
            }
            else if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                visibility = Min(visibility, GetEffectiveVisibility(arrayType.ElementType));
            }
            else if (typeSymbol is IPointerTypeSymbol pointerType)
            {
                visibility = Min(visibility, GetEffectiveVisibility(pointerType.PointedAtType));
            }

            return visibility;
        }

        private static EffectiveVisibility GetDeclaredVisibility(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Public:
                    return EffectiveVisibility.Public;
                case Accessibility.Internal:
                case Accessibility.ProtectedOrInternal:
                    return EffectiveVisibility.Internal;
                default:
                    return EffectiveVisibility.Private;
            }
        }

        private static EffectiveVisibility Min(EffectiveVisibility left, EffectiveVisibility right)
        {
            return left < right ? left : right;
        }

        private static PatchDtoModel? BuildModel(GeneratorAttributeSyntaxContext context)
        {
            var typeSymbol = context.TargetSymbol as INamedTypeSymbol;
            var typeDeclaration = context.TargetNode as TypeDeclarationSyntax;
            if (typeSymbol == null || typeDeclaration == null || typeSymbol.TypeKind != TypeKind.Class)
            {
                return null;
            }

            var properties = BuildPropertyModels(typeSymbol);

            var patchDtoAttribute = context.Attributes.FirstOrDefault(attribute => IsAttribute(attribute, MergePatchAttributeName));

            var targetAttributes = typeSymbol
                .GetAttributes()
                .Where(attribute => IsAttribute(attribute, MergePatchTargetAttributeName))
                .ToArray();

            var targetDeclarations = targetAttributes
                .Concat(patchDtoAttribute == null ? Enumerable.Empty<AttributeData>() : new[] { patchDtoAttribute })
                .ToArray();

            var targets = targetDeclarations
                .Select(BuildTargetModel)
                .Where(target => target != null)
                .Cast<PatchTargetModel>()
                .GroupBy(target => target.TargetType, SymbolEqualityComparer.Default)
                .Select(group => group.First())
                .ToImmutableArray();

            var openGenericTargets = targetDeclarations
                .Select(BuildOpenGenericTargetModel)
                .Where(target => target != null)
                .Cast<PatchTargetModel>()
                .ToImmutableArray();

            var unresolvedTargets = targetDeclarations
                .Where(IsUnresolvedTarget)
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
                typeDeclaration is RecordDeclarationSyntax,
                IsPartial(typeSymbol, typeDeclaration),
                HasRejectUnknownPropertyHandling(patchDtoAttribute),
                properties,
                targets,
                openGenericTargets,
                unresolvedTargets,
                typeDeclaration.Identifier.GetLocation());
        }

        private static ImmutableArray<PatchPropertyModel> BuildPropertyModels(INamedTypeSymbol typeSymbol)
        {
            return EnumeratePatchPropertyTypes(typeSymbol)
                .SelectMany(type => type
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(IsPatchProperty)
                    .Where(property => !JsonNameResolver.IsIgnoredOnRead(property)))
                .Select(BuildPropertyModel)
                .ToImmutableArray();
        }

        private static IEnumerable<INamedTypeSymbol> EnumeratePatchPropertyTypes(INamedTypeSymbol typeSymbol)
        {
            var hierarchy = new Stack<INamedTypeSymbol>();
            for (var current = typeSymbol; current != null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
            {
                if (current.TypeKind == TypeKind.Error)
                {
                    continue;
                }

                hierarchy.Push(current);
            }

            while (hierarchy.Count > 0)
            {
                yield return hierarchy.Pop();
            }
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
                HasJsonConverterAttribute(property),
                GetJsonNumberHandling(property),
                hasPatchIgnore,
                patchToTargetName,
                patchUsingMethodName,
                property.Locations.FirstOrDefault());
        }

        private static bool HasJsonConverterAttribute(IPropertySymbol property)
        {
            foreach (var attribute in property.GetAttributes())
            {
                if (IsJsonConverterAttribute(attribute))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsJsonConverterAttribute(AttributeData attribute)
        {
            var attributeClass = attribute.AttributeClass;
            while (attributeClass != null)
            {
                if (attributeClass.ToDisplayString() == JsonNameResolver.JsonConverterAttributeName)
                {
                    return true;
                }

                attributeClass = attributeClass.BaseType;
            }

            return false;
        }

        private static int? GetJsonNumberHandling(IPropertySymbol property)
        {
            foreach (var attribute in property.GetAttributes())
            {
                if (!JsonNameResolver.IsAttribute(attribute, JsonNameResolver.JsonNumberHandlingAttributeName))
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length == 1 &&
                    attribute.ConstructorArguments[0].Value is int value)
                {
                    return value;
                }
            }

            return null;
        }

        private static PatchTargetModel? BuildTargetModel(AttributeData attribute)
        {
            var targetType = GetTargetType(attribute);
            if (targetType == null ||
                targetType.TypeKind == TypeKind.Error ||
                IsOpenGenericTarget(targetType))
            {
                return null;
            }

            return new PatchTargetModel(targetType, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());
        }

        private static PatchTargetModel? BuildOpenGenericTargetModel(AttributeData attribute)
        {
            var targetType = GetTargetType(attribute);
            if (targetType == null ||
                targetType.TypeKind == TypeKind.Error ||
                !IsOpenGenericTarget(targetType))
            {
                return null;
            }

            return new PatchTargetModel(targetType, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());
        }

        private static bool IsUnresolvedTarget(AttributeData attribute)
        {
            if (!HasTargetConstructorArgument(attribute))
            {
                return false;
            }

            var targetType = GetTargetType(attribute);
            return targetType == null || targetType.TypeKind == TypeKind.Error;
        }

        private static INamedTypeSymbol? GetTargetType(AttributeData attribute)
        {
            if (!HasTargetConstructorArgument(attribute))
            {
                return null;
            }

            return attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
        }

        private static bool IsOpenGenericTarget(INamedTypeSymbol targetType)
        {
            return targetType.IsUnboundGenericType || targetType.TypeArguments.Any(ContainsTypeParameter);
        }

        private static bool ContainsTypeParameter(ITypeSymbol type)
        {
            if (type.TypeKind == TypeKind.TypeParameter)
            {
                return true;
            }

            if (type is INamedTypeSymbol namedType)
            {
                return namedType.IsUnboundGenericType || namedType.TypeArguments.Any(ContainsTypeParameter);
            }

            if (type is IArrayTypeSymbol arrayType)
            {
                return ContainsTypeParameter(arrayType.ElementType);
            }

            if (type is IPointerTypeSymbol pointerType)
            {
                return ContainsTypeParameter(pointerType.PointedAtType);
            }

            return false;
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

        private static bool IsPartial(INamedTypeSymbol symbol, TypeDeclarationSyntax currentDeclaration)
        {
            if (currentDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return true;
            }

            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax() as TypeDeclarationSyntax;
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

        private static string ToDiagnosticTypeName(ITypeSymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        }

        private enum EffectiveVisibility
        {
            Private = 0,
            Internal = 1,
            Public = 2
        }
    }
}
