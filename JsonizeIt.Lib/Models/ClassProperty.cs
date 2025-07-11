namespace JsonizeIt.Lib.Models;

public class ClassProperty
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? JsonAlias { get; set; }
    public bool Ignore { get; set; } = false;
    public object? DefaultValue { get; set; }    // C# type default (null, 0, false, etc.)
    public object? PresetValue { get; set; }     // Actual initialized value from class definition
    public bool HasPresetValue { get; set; }    // Flag to indicate if a preset value was found
}