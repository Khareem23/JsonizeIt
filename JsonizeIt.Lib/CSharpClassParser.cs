using System.Text.RegularExpressions;
using JsonizeIt.Lib.Models;

namespace JsonizeIt.Lib;

public class CSharpClassParser : IJsonConverter
{
    public string ConvertToJson(string dataToConvert, DataType type, bool isCamelCase = false, bool isMinify = false)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Convert Input String to Object
    /// </summary>
    /// <param name="classString"></param>
    /// <returns></returns>
    private static object Parse(string classString)
    {
        // Extracting Data type and field names in the input string
        var propertyPattern = @"public\s+(?<type>\S+)\s+(?<name>\w+)\s*\{\s*get;\s*set;\s*\}";
        var matches = Regex.Matches(classString, propertyPattern);
        
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        
        foreach (Match match in matches)
        {
            // Types: int, string etc
            var type = match.Groups["type"].Value;
            
            // Fields : Id, Name etc
            var name = match.Groups["name"].Value;

            result[name] = Utility.GetTypeDefaultValue(type) ;
        }
        return result;
    }
    
    private static Dictionary<string, object?> BuildObject(string className, Dictionary<string, List<ClassProperty>> classMap)
    {
        var result = new Dictionary<string, object?>();

        if (!classMap.ContainsKey(className))
            return result;

        foreach (var prop in classMap[className])
        {
            if (prop.Ignore) continue;

            var key = prop.JsonAlias ?? prop.Name;

            if (classMap.ContainsKey(prop.Type)) // nested object
                result[key] = BuildObject(prop.Type, classMap);
            else
                result[key] = prop.DefaultValue ?? Utility.GetTypeDefaultValue(prop.Type);
        }

        return result;
    }
    
    private static Dictionary<string, List<ClassProperty>> ExtractClasses(string code)
    {
        var classes = new Dictionary<string, List<ClassProperty>>();
        var classPattern = @"public\s+class\s+(\w+)\s*\{([\s\S]*?)\}";
        var matches = Regex.Matches(code, classPattern);

        foreach (Match match in matches)
        {
            string className = match.Groups[1].Value.Trim();
            string classBody = match.Groups[2].Value;

            var properties = ExtractProperties(classBody);
            classes[className] = properties;
        }

        return classes;
    }
    
    private static List<ClassProperty> ExtractProperties(string classBody)
    {
        var properties = new List<ClassProperty>();
        var lines = classBody.Split('\n');
        string? jsonAlias = null;
        bool ignoreNext = false;

        foreach (var raw in lines)
        {
            var line = raw.Trim();

            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("[JsonIgnore")) { ignoreNext = true; continue; }

            var jsonProp = GetJsonProperty(line);
            if (jsonProp != null) { jsonAlias = jsonProp; continue; }

            var match = Regex.Match(line, @"public\s+(?<type>[^\s]+)\s+(?<name>\w+)\s*\{?\s*(get;)?\s*(set;)?\s*\}?.*");
            if (match.Success)
            {
                var prop = new ClassProperty
                {
                    Type = match.Groups["type"].Value,
                    Name = match.Groups["name"].Value,
                    JsonAlias = jsonAlias,
                    Ignore = ignoreNext
                };

                // Default value (simple)
                var defaultMatch = Regex.Match(line, @"=\s*new\s+[^{;]*{([^}]*)}");
                if (defaultMatch.Success)
                {
                    prop.DefaultValue = defaultMatch.Value.Contains("new List") || defaultMatch.Value.Contains("[]")
                        ? new List<object>() : new object();
                }

                jsonAlias = null;
                ignoreNext = false;
                properties.Add(prop);
            }
        }

        return properties;
    }
    
    private static string? GetJsonProperty(string line)
    {
        var jsonPropMatch = Regex.Match(line, @"JsonProperty(?:Name)?\(""(?<alias>[^""]+)""\)");
        return jsonPropMatch.Success ? jsonPropMatch.Groups["alias"].Value : null;
    }
}