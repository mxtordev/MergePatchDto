using System.Linq;
using Microsoft.CodeAnalysis;

namespace MergePatchDto.Generators
{
    internal static class JsonNameResolver
    {
        public const string JsonIgnoreAttributeName = "System.Text.Json.Serialization.JsonIgnoreAttribute";
        public const string JsonConverterAttributeName = "System.Text.Json.Serialization.JsonConverterAttribute";
        public const string JsonNumberHandlingAttributeName = "System.Text.Json.Serialization.JsonNumberHandlingAttribute";
        public const string JsonPropertyNameAttributeName = "System.Text.Json.Serialization.JsonPropertyNameAttribute";

        public static bool IsIgnoredOnRead(IPropertySymbol property)
        {
            foreach (var attribute in property.GetAttributes())
            {
                if (!IsAttribute(attribute, JsonIgnoreAttributeName))
                {
                    continue;
                }

                return IsJsonIgnoreConditionIgnoredOnRead(attribute);
            }

            return false;
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

        private static bool IsJsonIgnoreConditionIgnoredOnRead(AttributeData attribute)
        {
            foreach (var namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Key != "Condition")
                {
                    continue;
                }

                return IsJsonIgnoreConditionIgnoredOnRead(namedArgument.Value);
            }

            return true;
        }

        private static bool IsJsonIgnoreConditionIgnoredOnRead(TypedConstant condition)
        {
            var conditionName = GetEnumConstantName(condition);
            switch (conditionName)
            {
                case "Never":
                case "WhenWriting":
                case "WhenWritingDefault":
                case "WhenWritingNull":
                    return false;
                case "Always":
                case "WhenReading":
                default:
                    return true;
            }
        }

        private static string? GetEnumConstantName(TypedConstant constant)
        {
            var enumType = constant.Type;
            if (enumType == null)
            {
                return null;
            }

            foreach (var field in enumType.GetMembers().OfType<IFieldSymbol>())
            {
                if (!field.HasConstantValue)
                {
                    continue;
                }

                if (Equals(field.ConstantValue, constant.Value))
                {
                    return field.Name;
                }
            }

            return null;
        }
    }
}
