using JsonizeIt.Lib;

namespace JsonizeIT.API.Models;

public class ParseRequest
{
    public string DataToConvertInBase64 { get; set; } = string.Empty;
    public bool IsCamelCase { get; set; } = false;
    public bool IsMinify { get; set; } = false;
    public DataType DataType { get; set; } = DataType.CSharp;
}