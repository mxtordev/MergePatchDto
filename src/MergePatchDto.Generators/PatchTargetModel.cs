using Microsoft.CodeAnalysis;

namespace MergePatchDto.Generators
{
    internal sealed class PatchTargetModel
    {
        public PatchTargetModel(INamedTypeSymbol targetType, Location? location)
        {
            TargetType = targetType;
            Location = location;
        }

        public INamedTypeSymbol TargetType { get; }

        public Location? Location { get; }
    }
}
