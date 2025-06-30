using System.ComponentModel;

namespace JsonizeIt.Lib;

public enum DataType
{
    [Description("C-Sharp")] CSharp = 1,
    [Description("Java")] Java = 2,
    [Description("Python")] Python = 3,
    [Description("Dart")] Dart = 4,
    [Description("TypeScript")] TypeScript = 5,
    [Description("XML")] Xml = 6,
    [Description("CSV")] Csv = 7
}
    
