using System.Text.Json;

namespace MergePatchDto.Tests;

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
