using FluentAssertions;
using JsonizeIt.Lib;

namespace JsonizeIt.Tests;

public class CSharpClassParserTests
{
    private readonly CSharpClassParser _csharpClassParser;
    public CSharpClassParserTests()
    {
        _csharpClassParser = new CSharpClassParser();
    }
    
    [Fact]
    public void UninitializedProperties_AreFilledWithDefaults()
    {
        string classCodeStringInput = @"
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public double Score { get; set; } = 0.0;
            public bool IsActive { get; set; }
            public Address Location { get; set; }
            public List<string> Hobbies { get; set; } = new List<string> { ""Reading"", ""Gaming"" };
            public string[] Keywords { get; set; } = new[] { ""Developer"", ""Engineer"" };
            public List<string> NewHobbies { get; set; } = new List<string>();
            public string[] NewKeywords { get; set; } = Array.Empty<string>();
            public Dictionary<string, int> Scores { get; set; }
            public string PostalCode = ""99999"";

            [JsonIgnore]
            public string Ignored { get; set; }

            [JsonProperty(""full_name"")]
            [JsonPropertyName(""fullName"")]
            public string NewName { get; set; }

            [JsonProperty(""years_old"")]
            [JsonPropertyName(""age"")]
            public int NewAge { get; set; }

            public Person()
            {
                Name = ""Olayinka"";
                Age = 25;
            }
        }

        public class Address
        {
            public string Street { get; set; } = ""4627 Sunset Ave"";
            public decimal Rent { get; set; } = 900.6m;
        }
        ";

        var resultInJson = _csharpClassParser.Parse(classCodeStringInput, false, false);
        
        resultInJson.Should().NotBeNull();
    }
    
    [Fact]
    public void InitializedCollections_ShouldBeFilled(){}
}