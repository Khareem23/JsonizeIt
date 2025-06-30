namespace JsonizeIt.Lib;

public interface IJsonConverter
{
    public string ConvertToJson(string dataToConvert, DataType type ,bool isCamelCase = false, bool isMinify = false );
}
