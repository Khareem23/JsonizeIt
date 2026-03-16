using System.Text.RegularExpressions;
using System.Text.Json;

namespace JsonizeIt.Lib;

public class TypeScriptParser : IJsonConverter
{
    public string Parse(string dataToConvert, bool isCamelCase = false, bool isMinify = false)
    {
        if (string.IsNullOrWhiteSpace(dataToConvert))
            return "{}";

        var classes = ParseClasses(dataToConvert);

        if (classes.Count == 0)
            return "{}";

        var mainClass = SelectMainClass(classes);
        var sampleData = GenerateSampleData(mainClass, classes);

        var options = new JsonSerializerOptions { WriteIndented = !isMinify };
        return JsonSerializer.Serialize(sampleData, options);
    }

    public List<TsClassInfo> ParseClasses(string code)
    {
        var classes = new List<TsClassInfo>();

        // Normalize line endings and strip comments
        code = Regex.Replace(code, @"\r\n|\r", "\n");
        code = Regex.Replace(code, @"//.*$", "", RegexOptions.Multiline);
        code = Regex.Replace(code, @"/\*[\s\S]*?\*/", "");

        // Match: [export] [abstract] (class|interface) Name [<Generics>] [extends/implements ...] {
        var classPattern = @"(?:export\s+)?(?:abstract\s+)?(class|interface)\s+(\w+)(?:<[^>]*>)?(?:\s+(?:extends|implements)\s+[\w,\s<>]+)?\s*\{";
        var matches = Regex.Matches(code, classPattern);

        foreach (Match match in matches)
        {
            var kind = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var startIndex = match.Index + match.Length;

            // Find matching closing brace
            int braceCount = 1;
            int endIndex = startIndex;

            while (endIndex < code.Length && braceCount > 0)
            {
                if (code[endIndex] == '{') braceCount++;
                else if (code[endIndex] == '}') braceCount--;
                endIndex++;
            }

            if (braceCount != 0)
                throw new FormatException("The input data is not in the correct format.");

            var body = code.Substring(startIndex, endIndex - startIndex - 1);
            var classInfo = new TsClassInfo { Name = name, IsInterface = kind == "interface" };

            ParseProperties(body, classInfo);

            if (!classInfo.IsInterface)
                ParseConstructorShorthand(body, classInfo);

            classes.Add(classInfo);
        }

        return classes;
    }

    // Select the root class: the one not referenced as a property type by any other class.
    // Falls back to the last class if all are referenced (circular refs).
    private TsClassInfo SelectMainClass(List<TsClassInfo> classes)
    {
        var referencedNames = classes
            .SelectMany(c => c.Properties)
            .Select(p => p.Type.Trim())
            .ToHashSet();

        return classes.LastOrDefault(c => !referencedNames.Contains(c.Name))
               ?? classes.Last();
    }

    // Remove constructor declarations (params + body) from the class body so that
    // constructor parameter lines are not mistaken for class property declarations.
    private string RemoveConstructors(string body)
    {
        var sb = new System.Text.StringBuilder();
        int i = 0;
        const string keyword = "constructor";

        while (i < body.Length)
        {
            // Detect 'constructor' keyword
            if (i <= body.Length - keyword.Length &&
                body.Substring(i, keyword.Length) == keyword &&
                (i == 0 || !char.IsLetterOrDigit(body[i - 1])))
            {
                int j = i + keyword.Length;

                // Skip past the parameter list  constructor(...)
                int parenDepth = 0;
                bool seenParen = false;
                while (j < body.Length)
                {
                    if (body[j] == '(') { parenDepth++; seenParen = true; }
                    else if (body[j] == ')') { parenDepth--; if (parenDepth == 0 && seenParen) { j++; break; } }
                    j++;
                }

                // Skip whitespace then look for the constructor body '{'
                while (j < body.Length && char.IsWhiteSpace(body[j])) j++;

                if (j < body.Length && body[j] == '{')
                {
                    int braceDepth = 1;
                    j++;
                    while (j < body.Length && braceDepth > 0)
                    {
                        if (body[j] == '{') braceDepth++;
                        else if (body[j] == '}') braceDepth--;
                        j++;
                    }
                    i = j; // advance past the entire constructor
                }
                else
                {
                    // No body found — emit 'constructor' literally and move on
                    sb.Append(keyword);
                    i += keyword.Length;
                }
            }
            else
            {
                sb.Append(body[i]);
                i++;
            }
        }

        return sb.ToString();
    }

    private void ParseProperties(string body, TsClassInfo classInfo)
    {
        // Strip decorators (e.g. @Expose(), @Transform(...), @IsString)
        body = Regex.Replace(body, @"@\w+\s*\([^)]*\)\s*", "");
        body = Regex.Replace(body, @"@\w+\s*", "");

        // Remove constructor declarations so parameter lines aren't parsed as properties
        body = RemoveConstructors(body);

        var lines = body.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("//") || line.StartsWith("*") || line.StartsWith("/*")) continue;
            if (line.StartsWith("constructor")) continue;

            // Find first colon at depth 0 (not inside brackets or strings)
            var colonIdx = FindPropertyColon(line);
            if (colonIdx < 0) continue;

            var beforeColon = line[..colonIdx].Trim();
            var afterColon = line[(colonIdx + 1)..].Trim().TrimEnd(';').Trim();

            // Parse the part before ':': optional access modifier, optional readonly, name, optional '?'
            var beforeMatch = Regex.Match(beforeColon,
                @"^(?:(?:public|private|protected|static)\s+)?(?:(readonly)\s+)?(\w+)(\?)?$");

            if (!beforeMatch.Success) continue;

            var isReadonly = beforeMatch.Groups[1].Success;
            var propName = beforeMatch.Groups[2].Value;
            var isOptional = beforeMatch.Groups[3].Success;

            if (IsReservedWord(propName)) continue;

            var (type, defaultValueStr) = ExtractTypeAndDefault(afterColon);

            classInfo.Properties.Add(new TsPropertyInfo
            {
                Name = propName,
                Type = type,
                IsOptional = isOptional,
                IsReadonly = isReadonly,
                DefaultValueStr = defaultValueStr
            });
        }
    }

    private void ParseConstructorShorthand(string body, TsClassInfo classInfo)
    {
        // Match constructor parameter list (may span multiple lines)
        var ctorMatch = Regex.Match(body, @"constructor\s*\(([^)]*)\)", RegexOptions.Singleline);
        if (!ctorMatch.Success) return;

        var paramStr = ctorMatch.Groups[1].Value;
        if (string.IsNullOrWhiteSpace(paramStr)) return;

        var paramList = SplitByComma(paramStr);

        foreach (var param in paramList)
        {
            var p = param.Trim();

            // Only shorthand params with access modifier are class properties
            if (!Regex.IsMatch(p, @"^(?:public|private|protected|readonly)\b")) continue;

            var colonIdx = FindPropertyColon(p);
            if (colonIdx < 0) continue;

            var beforeColon = p[..colonIdx].Trim();
            var afterColon = p[(colonIdx + 1)..].Trim();

            var beforeMatch = Regex.Match(beforeColon,
                @"(?:public|private|protected)\s+(?:(readonly)\s+)?(\w+)(\?)?$");

            if (!beforeMatch.Success) continue;

            var propName = beforeMatch.Groups[2].Value;
            var isOptional = beforeMatch.Groups[3].Success;

            // Avoid duplicating a property already parsed from the class body
            if (classInfo.Properties.Any(prop => prop.Name == propName)) continue;

            var (type, defaultValueStr) = ExtractTypeAndDefault(afterColon);

            classInfo.Properties.Add(new TsPropertyInfo
            {
                Name = propName,
                Type = type,
                IsOptional = isOptional,
                DefaultValueStr = defaultValueStr
            });
        }
    }

    // Find first ':' at bracket-depth 0, not inside strings, not part of '::'
    private int FindPropertyColon(string line)
    {
        int depth = 0;
        bool inString = false;
        char stringChar = '"';

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inString)
            {
                if (c == stringChar && (i == 0 || line[i - 1] != '\\'))
                    inString = false;
                continue;
            }

            if (c == '"' || c == '\'') { inString = true; stringChar = c; continue; }
            if (c is '<' or '[' or '{' or '(') { depth++; continue; }
            if (c is '>' or ']' or '}' or ')') { depth--; continue; }

            if (c == ':' && depth == 0)
            {
                // Skip '::' (scope resolution, rare in TS but guard anyway)
                if (i + 1 < line.Length && line[i + 1] == ':') { i++; continue; }
                return i;
            }
        }

        return -1;
    }

    // Split 'type [= default]' respecting bracket depth; handles arrow functions in types
    private (string type, string? defaultValue) ExtractTypeAndDefault(string afterColon)
    {
        int depth = 0;

        for (int i = 0; i < afterColon.Length; i++)
        {
            char c = afterColon[i];

            if (c is '<' or '[' or '{' or '(') depth++;
            else if (c is '>' or ']' or '}' or ')') depth--;
            else if (c == '=' && depth == 0)
            {
                // Skip '=>' (arrow function) and '!=' / '=='
                if (i + 1 < afterColon.Length && afterColon[i + 1] == '>') continue;
                if (i > 0 && afterColon[i - 1] is '!' or '=') continue;

                var type = afterColon[..i].Trim();
                var defaultVal = afterColon[(i + 1)..].Trim().TrimEnd(';').Trim();
                return (type, string.IsNullOrEmpty(defaultVal) ? null : defaultVal);
            }
        }

        return (afterColon.TrimEnd(';').Trim(), null);
    }

    // Comma-split respecting bracket depth (for generic types like Map<string, number>)
    private List<string> SplitByComma(string input)
    {
        var result = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c is '<' or '[' or '{' or '(') depth++;
            else if (c is '>' or ']' or '}' or ')') depth--;
            else if (c == ',' && depth == 0)
            {
                result.Add(input[start..i]);
                start = i + 1;
            }
        }

        if (start < input.Length)
            result.Add(input[start..]);

        return result;
    }

    // Pipe-split respecting bracket depth (for union types: A | B | C)
    private List<string> SplitByPipe(string input)
    {
        var result = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c is '<' or '[' or '{' or '(') depth++;
            else if (c is '>' or ']' or '}' or ')') depth--;
            else if (c == '|' && depth == 0)
            {
                result.Add(input[start..i]);
                start = i + 1;
            }
        }

        if (start < input.Length)
            result.Add(input[start..]);

        return result;
    }

    private Dictionary<string, object?> GenerateSampleData(TsClassInfo classInfo, List<TsClassInfo> allClasses)
    {
        var data = new Dictionary<string, object?>();

        foreach (var prop in classInfo.Properties)
            data[prop.Name] = ResolveValue(prop, allClasses);

        return data;
    }

    private object? ResolveValue(TsPropertyInfo prop, List<TsClassInfo> allClasses)
    {
        if (!string.IsNullOrEmpty(prop.DefaultValueStr))
        {
            var parsed = ParseDefaultValue(prop.DefaultValueStr);
            if (parsed is not null) return parsed;
        }

        return GetDefaultForType(prop.Type, allClasses);
    }

    private object? ParseDefaultValue(string valueStr)
    {
        valueStr = valueStr.Trim();

        // String literals (single or double quotes)
        if ((valueStr.StartsWith('"') && valueStr.EndsWith('"')) ||
            (valueStr.StartsWith('\'') && valueStr.EndsWith('\'')))
            return valueStr[1..^1];

        // Booleans
        if (valueStr == "true") return true;
        if (valueStr == "false") return false;

        // Null / undefined
        if (valueStr is "null" or "undefined") return null;

        // Numbers
        if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double num))
            return num % 1 == 0 ? (object)(int)num : num;

        // Array and object literals — return empty containers
        if (valueStr.StartsWith('[')) return new List<object?>();
        if (valueStr.StartsWith('{')) return new Dictionary<string, object?>();

        return null;
    }

    private object? GetDefaultForType(string type, List<TsClassInfo> allClasses)
    {
        type = type.Trim();

        // Union type: pick first non-null / non-undefined member
        if (type.Contains('|'))
        {
            var parts = SplitByPipe(type)
                .Select(t => t.Trim())
                .Where(t => t is not "null" and not "undefined");
            type = parts.FirstOrDefault() ?? "null";
            if (type == "null") return null;
        }

        // Array types: T[] or Array<T> or ReadonlyArray<T>
        if (type.EndsWith("[]") || type.StartsWith("Array<") || type.StartsWith("ReadonlyArray<"))
            return new List<object?>();

        // Map / record-like types and utility types
        if (type.StartsWith("Record<") || type.StartsWith("Map<") ||
            type.StartsWith("Partial<") || type.StartsWith("Required<") ||
            type.StartsWith("Pick<") || type.StartsWith("Omit<") ||
            type is "object" or "Object")
            return new Dictionary<string, object?>();

        // Nested class / interface
        var nested = allClasses.FirstOrDefault(c => c.Name == type);
        if (nested != null)
            return GenerateSampleData(nested, allClasses);

        return type switch
        {
            "string" => (object?)"",
            "number" => 0,
            "boolean" => false,
            "Date" => DateTime.UtcNow.ToString("o"),
            _ => null   // any, unknown, never, void, custom types not in scope
        };
    }

    private static bool IsReservedWord(string word) =>
        word is "return" or "new" or "if" or "else" or "for" or "while"
             or "switch" or "case" or "break" or "continue" or "throw"
             or "try" or "catch" or "finally" or "const" or "let" or "var"
             or "import" or "export" or "default" or "from" or "of" or "in"
             or "typeof" or "instanceof" or "void" or "delete" or "super" or "this";
}

public class TsClassInfo
{
    public string Name { get; set; } = "";
    public bool IsInterface { get; set; }
    public List<TsPropertyInfo> Properties { get; set; } = new();
}

public class TsPropertyInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public bool IsOptional { get; set; }
    public bool IsReadonly { get; set; }
    public string? DefaultValueStr { get; set; }
}
