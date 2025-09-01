using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Text;

namespace JsonizeIt.Lib;

public class CSharpParser2 : IJsonConverter
{
    public string Parse(string dataToConvert, bool isCamelCase = false, bool isMinify = false)
    {
        if (string.IsNullOrEmpty(dataToConvert))
        {
            return "{}";
        }
        // Return Schema 
        string jsonSchema = ConvertToJsonSchema(dataToConvert);
        
        // Return Schema and Json Values
        string sampleJson = GenerateSampleJson(dataToConvert);

        return sampleJson;

    }
    
    // Convert to Json Schema
    public string ConvertToJsonSchema(string classCode)
    {
        var classes = ParseClasses(classCode);
        var jsonSchema = new Dictionary<string, object>();
        
        foreach (var cls in classes)
        {
            jsonSchema[cls.Name] = GenerateClassSchema(cls, classes);
        }
        
        return JsonConvert.SerializeObject(jsonSchema, Formatting.Indented);
    }
    
    
    
    // Parse Classes 
    // Make ParseClasses public for debugging
    public List<ClassInfo> ParseClasses(string classCode)
    {
        var classes = new List<ClassInfo>();
        
        // Clean up the input and normalize whitespace
        classCode = classCode.Trim();
        classCode = Regex.Replace(classCode, @"\r\n|\r|\n", "\n");
        
        // Find all class definitions - handle multiline with proper brace matching
        var classPattern = @"public\s+class\s+(\w+)\s*\{";
        var matches = Regex.Matches(classCode, classPattern, RegexOptions.Multiline);
        
        foreach (Match match in matches)
        {
            var className = match.Groups[1].Value;
            var startIndex = match.Index + match.Length;
            
            // Find the matching closing brace
            int braceCount = 1;
            int endIndex = startIndex;
            
            while (endIndex < classCode.Length && braceCount > 0)
            {
                if (classCode[endIndex] == '{') braceCount++;
                else if (classCode[endIndex] == '}') braceCount--;
                endIndex++;
            }
            
            if (braceCount == 0)
            {
                var classBody = classCode.Substring(startIndex, endIndex - startIndex - 1);
                
                var classInfo = new ClassInfo { Name = className };
                ParseProperties(classBody, classInfo);
                ParseConstructor(classBody, classInfo);
                
                classes.Add(classInfo);
            }
            else
            {
                throw new FormatException("The input data is not in the correct format.");
            }
        }
        
        return classes;
    }
    
    // Generate Class Schema 
    private Dictionary<string, object> GenerateClassSchema(ClassInfo classInfo, List<ClassInfo> allClasses)
    {
        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>()
        };
        
        var properties = (Dictionary<string, object>)schema["properties"];
        
        foreach (var prop in classInfo.Properties.Where(p => !p.IsIgnored))
        {
            var propName = prop.JsonPropertyName ?? ToCamelCase(prop.Name);
            properties[propName] = GetPropertySchema(prop, allClasses);
        }
        
        return schema;
    }
    
    // Parse Properties 
    private void ParseProperties(string classBody, ClassInfo classInfo)
    {
        // Remove comments and normalize whitespace
        classBody = Regex.Replace(classBody, @"//.*$", "", RegexOptions.Multiline);
        classBody = Regex.Replace(classBody, @"/\*.*?\*/", "", RegexOptions.Singleline);
        
        // First, find and mark JsonIgnore properties
        var jsonIgnoreMatches = Regex.Matches(classBody, @"\[JsonIgnore\]\s*public\s+([^{]+?)\s+(\w+)\s*{\s*get;\s*set;\s*}", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        var ignoredProperties = new HashSet<string>();
        foreach (Match match in jsonIgnoreMatches)
        {
            ignoredProperties.Add(match.Groups[2].Value);
        }
        
        // Parse regular properties with get/set
        var propertyPattern = @"(?:\[JsonProperty\([""']([^""']+)[""']\)\]\s*)?(?:\[JsonPropertyName\([""']([^""']+)[""']\)\]\s*)?public\s+([^{]+?)\s+(\w+)\s*{\s*get;\s*set;\s*}(?:\s*=\s*([^;]+);)?";
        var propertyMatches = Regex.Matches(classBody, propertyPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
        foreach (Match match in propertyMatches)
        {
            var prop = new PropertyInfo
            {
                JsonPropertyName = match.Groups[1].Success ? match.Groups[1].Value : 
                                 match.Groups[2].Success ? match.Groups[2].Value : null,
                Type = match.Groups[3].Value.Trim(),
                Name = match.Groups[4].Value,
                DefaultValue = match.Groups[5].Success ? match.Groups[5].Value.Trim() : null,
                IsIgnored = ignoredProperties.Contains(match.Groups[4].Value)
            };
            
            classInfo.Properties.Add(prop);
        }
        
        // Parse fields (public fields with direct assignment)
        var fieldPattern = @"public\s+([^=]+?)\s+(\w+)\s*=\s*([^;]+);";
        var fieldMatches = Regex.Matches(classBody, fieldPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
        foreach (Match match in fieldMatches)
        {
            var prop = new PropertyInfo
            {
                Type = match.Groups[1].Value.Trim(),
                Name = match.Groups[2].Value,
                DefaultValue = match.Groups[3].Value.Trim()
            };
            
            classInfo.Properties.Add(prop);
        }
    }
    
    // Parse Constructors 
    private void ParseConstructor(string classBody, ClassInfo classInfo)
    {
        // Handle multiline constructors
        var constructorPattern = @"public\s+" + Regex.Escape(classInfo.Name) + @"\(\s*\)\s*\{([^}]+)\}";
        var constructorMatch = Regex.Match(classBody, constructorPattern, RegexOptions.Multiline | RegexOptions.Singleline);
        
        if (constructorMatch.Success)
        {
            var constructorBody = constructorMatch.Groups[1].Value;
            var assignments = Regex.Matches(constructorBody, @"(\w+)\s*=\s*([^;]+);", RegexOptions.Multiline);
            
            foreach (Match assignment in assignments)
            {
                var propName = assignment.Groups[1].Value.Trim();
                var value = assignment.Groups[2].Value.Trim();
                
                classInfo.ConstructorAssignments[propName] = value;
            }
        }
    }
    
    // Convert Field to CamelCase
    private string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || str.Length == 1)
            return str?.ToLower();
        
        return char.ToLower(str[0]) + str.Substring(1);
    }
    
    // Return Property Schema 
    private Dictionary<string, object> GetPropertySchema(PropertyInfo prop, List<ClassInfo> allClasses)
    {
        var schema = new Dictionary<string, object>();
        
        if (prop.Type.Contains("List<") || prop.Type.Contains("[]"))
        {
            schema["type"] = "array";
            var itemType = ExtractGenericType(prop.Type);
            schema["items"] = GetTypeSchema(itemType, allClasses);
        }
        else if (prop.Type.Contains("Dictionary<"))
        {
            schema["type"] = "object";
            schema["additionalProperties"] = GetTypeSchema("object", allClasses);
        }
        else if (allClasses.Any(c => c.Name == prop.Type))
        {
            schema["$ref"] = $"#{prop.Type}";
        }
        else
        {
            schema = GetTypeSchema(prop.Type, allClasses);
        }
        
        return schema;
    }
    
    // Extract Generic Types 
    private string ExtractGenericType(string type)
    {
        if (type.Contains("List<"))
        {
            var match = Regex.Match(type, @"List<([^>]+)>");
            return match.Success ? match.Groups[1].Value : "string";
        }
        else if (type.Contains("[]"))
        {
            return type.Replace("[]", "").Trim();
        }
        return "string";
    }
    
    // Return Schema for Types
    private Dictionary<string, object> GetTypeSchema(string type, List<ClassInfo> allClasses)
    {
        var schema = new Dictionary<string, object>();
        
        switch (type.ToLower())
        {
            case "string":
                schema["type"] = "string";
                break;
            case "int":
            case "int32":
                schema["type"] = "integer";
                break;
            case "double":
            case "decimal":
                schema["type"] = "number";
                break;
            case "bool":
            case "boolean":
                schema["type"] = "boolean";
                break;
            default:
                if (allClasses.Any(c => c.Name == type))
                    schema["$ref"] = $"#{type}";
                else
                    schema["type"] = "object";
                break;
        }
        
        return schema;
    }
    
    // Return Json Data
    public string GenerateSampleJson(string classCode)
    {
        var classes = ParseClasses(classCode);
        var mainClass = classes.FirstOrDefault(c => c.Name == "Person") ?? classes.First();
        
        var sampleData = GenerateSampleData(mainClass, classes);
        return JsonConvert.SerializeObject(sampleData, Formatting.Indented);
    }
    
    // Return Json 
    private object GenerateSampleData(ClassInfo classInfo, List<ClassInfo> allClasses)
    {
        var data = new Dictionary<string, object>();
        
        foreach (var prop in classInfo.Properties.Where(p => !p.IsIgnored))
        {
            var propName = prop.JsonPropertyName ?? ToCamelCase(prop.Name);
            
            // Use constructor assignment if available
            if (classInfo.ConstructorAssignments.ContainsKey(prop.Name))
            {
                data[propName] = ParseValue(classInfo.ConstructorAssignments[prop.Name], prop.Type);
            }
            else if (!string.IsNullOrEmpty(prop.DefaultValue))
            {
                data[propName] = ParseValue(prop.DefaultValue, prop.Type);
            }
            else
            {
                data[propName] = GetDefaultValue(prop, allClasses);
            }
        }
        
        return data;
    }
    
    // Parse the Values
    private object ParseValue(string value, string type)
    {
        value = value.Trim().Trim('"');
        
        if (value.StartsWith("new List") && value.Contains("{"))
        {
            var listContent = Regex.Match(value, @"\{([^}]+)\}").Groups[1].Value;
            return listContent.Split(',').Select(s => s.Trim().Trim('"')).ToList();
        }
        else if (value.StartsWith("new[]") && value.Contains("{"))
        {
            var arrayContent = Regex.Match(value, @"\{([^}]+)\}").Groups[1].Value;
            return arrayContent.Split(',').Select(s => s.Trim().Trim('"')).ToList();
        }
        else if (value == "Array.Empty<string>()" || value == "new List<string>()")
        {
            return new List<string>();
        }
        else
        {
            switch (type.ToLower())
            {
                case "int": case "int32": return int.TryParse(value, out int i) ? i : 0;
                case "double": return double.TryParse(value, out double d) ? d : 0.0;
                case "decimal": return decimal.TryParse(value.TrimEnd('m'), out decimal dec) ? dec : 0.0m;
                case "bool": case "boolean": return bool.TryParse(value, out bool b) ? b : false;
                default: return value;
            }
        }
    }
    
    // 
    private object GetDefaultValue(PropertyInfo prop, List<ClassInfo> allClasses)
    {
        if (prop.Type.Contains("List<"))
        {
            return new List<object>();
        }
        else if (prop.Type.Contains("[]"))
        {
            return new List<object>();
        }
        else if (prop.Type.Contains("Dictionary<"))
        {
            return new Dictionary<string, object>();
        }
        else if (allClasses.Any(c => c.Name == prop.Type))
        {
            var nestedClass = allClasses.First(c => c.Name == prop.Type);
            return GenerateSampleData(nestedClass, allClasses);
        }
        else
        {
            switch (prop.Type.ToLower())
            {
                case "string": return "";
                case "int": case "int32": return 0;
                case "double": case "decimal": return 0.0;
                case "bool": case "boolean": return false;
                default: return null;
            }
        }
    }
    
}

public class ClassInfo
{
    public string Name { get; set; }
    public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
    public Dictionary<string, string> ConstructorAssignments { get; set; } = new Dictionary<string, string>();
}

public class PropertyInfo
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? DefaultValue { get; set; }
    public string? JsonPropertyName { get; set; }
    public bool IsIgnored { get; set; }
}



public class ClassToJsonConverter
{
    
}