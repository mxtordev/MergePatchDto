using System.Linq;
using Microsoft.CodeAnalysis;

namespace MergePatchDto.Generators
{
    internal static class JsonNameResolver
    {
        public const string JsonIgnoreAttributeName = "System.Text.Json.Serialization.JsonIgnoreAttribute";
        public const string JsonPropertyNameAttributeName = "System.Text.Json.Serialization.JsonPropertyNameAttribute";

        public static bool HasJsonIgnore(IPropertySymbol property)
        {
            return property.GetAttributes().Any(attribute => IsAttribute(attribute, JsonIgnoreAttributeName));
        }

        public static string? GetJsonPropertyName(IPropertySymbol property)
        {
            foreach (var attribute in property.GetAttributes())
            {
                if (!IsAttribute(attribute, JsonPropertyNameAttributeName))
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length == 1)
                {
                    return attribute.ConstructorArguments[0].Value as string;
                }
            }

            return null;
        }

        public static bool IsAttribute(AttributeData attribute, string fullyQualifiedMetadataName)
        {
            return attribute.AttributeClass?.ToDisplayString() == fullyQualifiedMetadataName;
        }
    }
}
