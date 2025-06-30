namespace JsonizeIt.Lib;

public class JsonConverterFactory
{
    public IJsonConverter GetConverter(DataType dataType)
    {
        //  _ => throw new NotSupportedException("Unsupported input type.")
        switch (dataType)
        {
            case DataType.CSharp:
                return new CSharpClassParser();
            case DataType.Java:
                throw new NotImplementedException();
            default:
                return new CSharpClassParser();
        }
    }
}