namespace JsonizeIt.Lib;

public interface IJsonConverter
{
    public string Parse(string dataToConvert ,bool isCamelCase = false, bool isMinify = false );
}
