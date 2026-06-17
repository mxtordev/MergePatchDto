using Microsoft.CodeAnalysis;

namespace MergePatchDto.Generators
{
    internal sealed class PatchPropertyModel
    {
        public PatchPropertyModel(
            IPropertySymbol symbol,
            string name,
            string typeName,
            bool isInitOnly,
            string? explicitJsonName,
            string? jsonConverterTypeName,
            int? jsonNumberHandling,
            bool hasPatchIgnore,
            string? patchToTargetName,
            string? patchUsingMethodName,
            Location? location)
        {
            Symbol = symbol;
            Name = name;
            TypeName = typeName;
            IsInitOnly = isInitOnly;
            ExplicitJsonName = explicitJsonName;
            JsonConverterTypeName = jsonConverterTypeName;
            JsonNumberHandling = jsonNumberHandling;
            HasPatchIgnore = hasPatchIgnore;
            PatchToTargetName = patchToTargetName;
            PatchUsingMethodName = patchUsingMethodName;
            Location = location;
        }

        public IPropertySymbol Symbol { get; }

        public string Name { get; }

        public string TypeName { get; }

        public bool IsInitOnly { get; }

        public string? ExplicitJsonName { get; }

        public string? JsonConverterTypeName { get; }

        public int? JsonNumberHandling { get; }

        public bool HasPropertyJsonOptions => JsonConverterTypeName != null || JsonNumberHandling.HasValue;

        public bool HasPatchIgnore { get; }

        public string? PatchToTargetName { get; }

        public string? PatchUsingMethodName { get; }

        public Location? Location { get; }
    }
}
