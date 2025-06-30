namespace JsonizeIt.Lib.Models;

public class ClassProperty
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? JsonAlias { get; set; }
    public object? DefaultValue { get; set; }
    public bool Ignore { get; set; } = false;
}