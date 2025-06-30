namespace JsonizeIt.Lib;

public class Utility
{
    public static object? GetTypeDefaultValue(string type)
    {
        type = type.ToLower();

        return type switch
        {
            "string" => "string",
            "int" or "int32" => 0,
            "long" or "int64" => 0L,
            "double" => 0.0,
            "decimal" => 0.0m,
            "float" => 0.0f,
            "bool" or "boolean" => false,
            "datetime" => DateTime.UtcNow.ToString("o"),
            _ when type.EndsWith("[]") || type.StartsWith("List<") => Array.Empty<object>(),
            _ => null
        };
    }
}