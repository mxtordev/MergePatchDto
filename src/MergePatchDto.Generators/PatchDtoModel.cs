using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MergePatchDto.Generators
{
    internal sealed class PatchDtoModel
    {
        public PatchDtoModel(
            INamedTypeSymbol typeSymbol,
            string name,
            string namespaceName,
            string typeName,
            string accessibility,
            bool isRecordClass,
            bool isPartial,
            bool rejectUnknownProperties,
            ImmutableArray<PatchPropertyModel> properties,
            ImmutableArray<PatchTargetModel> targets,
            ImmutableArray<PatchTargetModel> openGenericTargets,
            ImmutableArray<Location> unresolvedTargetLocations,
            Location? location)
        {
            TypeSymbol = typeSymbol;
            Name = name;
            NamespaceName = namespaceName;
            TypeName = typeName;
            Accessibility = accessibility;
            IsRecordClass = isRecordClass;
            IsPartial = isPartial;
            RejectUnknownProperties = rejectUnknownProperties;
            Properties = properties;
            Targets = targets;
            OpenGenericTargets = openGenericTargets;
            UnresolvedTargetLocations = unresolvedTargetLocations;
            Location = location;
        }

        public INamedTypeSymbol TypeSymbol { get; }

        public string Name { get; }

        public string NamespaceName { get; }

        public string TypeName { get; }

        public string Accessibility { get; }

        public bool IsRecordClass { get; }

        public bool IsPartial { get; }

        public bool RejectUnknownProperties { get; }

        public ImmutableArray<PatchPropertyModel> Properties { get; }

        public ImmutableArray<PatchTargetModel> Targets { get; }

        public ImmutableArray<PatchTargetModel> OpenGenericTargets { get; }

        public ImmutableArray<Location> UnresolvedTargetLocations { get; }

        public Location? Location { get; }
    }
}
