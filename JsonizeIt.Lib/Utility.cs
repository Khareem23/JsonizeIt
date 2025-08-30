using System.Text.RegularExpressions;

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

    public static object? ExtractPresetValue(string line, string type)
    {
        
        // Check if line contains an assignment (=)
        if (!line.Contains("="))
            return null;

        // Split by = and get the value part
        var parts = line.Split('=', 2);
        if (parts.Length != 2)
            return null;

        var valueStr = parts[1].Trim();
        
        // Remove trailing semicolon if present
        if (valueStr.EndsWith(";"))
            valueStr = valueStr.Substring(0, valueStr.Length - 1).Trim();

        // Handle different value types
        try
        {
            // String literals
            if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
            {
                return valueStr.Substring(1, valueStr.Length - 2); // Remove quotes
            }

            // Numeric literals
            if (type == "int" && int.TryParse(valueStr, out int intVal))
                return intVal;
            
            if (type == "double" && double.TryParse(valueStr, out double doubleVal))
                return doubleVal;
            
            if (type == "decimal" && decimal.TryParse(valueStr.TrimEnd('m', 'M'), out decimal decimalVal))
                return decimalVal;

            // Boolean literals
            if (type == "bool" && bool.TryParse(valueStr, out bool boolVal))
                return boolVal;

            // List initializations - handle multiline
            if (valueStr.StartsWith("new List<") || valueStr.Contains("new List<"))
            {
                // Simple empty list
                if (valueStr.Contains("new List<string>()"))
                    return new List<string>();
                
                // List with items - extract items between braces
                var listMatch = Regex.Match(valueStr, @"new List<[^>]+>\s*\{\s*([^}]*)\s*\}");
                if (listMatch.Success)
                {
                    var itemsStr = listMatch.Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(itemsStr))
                        return new List<string>(); // Empty list
                    
                    var items = itemsStr.Split(',')
                        .Select(item => item.Trim().Trim('"'))
                        .Where(item => !string.IsNullOrEmpty(item))
                        .ToList();
                    return items;
                }
            }

            // Array initializations
            if (valueStr.StartsWith("new[]") || valueStr.StartsWith("new string[]"))
            {
                // Handle new[] { "item1", "item2" }
                var arrayMatch = Regex.Match(valueStr, @"new\s*(?:string)?\[\]\s*\{\s*([^}]*)\s*\}");
                if (arrayMatch.Success)
                {
                    var itemsStr = arrayMatch.Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(itemsStr))
                        return new string[0]; // Empty array
                    
                    var items = itemsStr.Split(',')
                        .Select(item => item.Trim().Trim('"'))
                        .Where(item => !string.IsNullOrEmpty(item))
                        .ToArray();
                    return items;
                }
            }

            // Array.Empty<string>()
            if (valueStr.Contains("Array.Empty<"))
            {
                return new string[0];
            }

            // Default numeric values
            if (valueStr == "0.0")
                return 0.0;

            // For complex objects or unrecognized patterns, return the raw string
            // This allows for custom handling later if needed
            return valueStr;
        }
        catch
        {
            // If parsing fails, return the raw string value
            return valueStr;
        }
    }
}