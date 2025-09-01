using FluentAssertions;
using JsonizeIt.Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonizeIt.Tests;

public class CSharpClassParserTests
{
    private readonly CSharpClassParser _csharpClassParser;
    private readonly CSharpParser2 _csharpClassParser2;
    
    public CSharpClassParserTests()
    {
        _csharpClassParser = new CSharpClassParser();
        _csharpClassParser2 = new CSharpParser2();
    }
    
    [Fact]
    public void Parse_EmptyInput_ShouldReturnEmptyJsonOrThrow()
    {
        // Arrange
        var input = "";

        // Act & Assert
        var result = _csharpClassParser2.Parse(input);
            
        // Should either return empty object or handle gracefully
        Assert.NotNull(result);
        Assert.Equal("{}", result);
    }
    
    [Fact]
    public void Parse_InvalidSyntax_ShouldHandleGracefully()
    {
        // Arrange
        var input = @"
                    public class Person
                    {
                        public string Name { get; set; }
                        // Missing closing brace intentionally
                    ";

        // Act
        var ex = Assert.Throws<FormatException>(() => _csharpClassParser2.Parse(input));
     
        // Assert
        Assert.Equal("The input data is not in the correct format.", ex.Message);
    }
    
    [Fact]
    public void Parse_NonNestedClassWithValidSyntax_ShouldHandleGracefully()
    {
        // Arrange
        var input = """
                    public class Address
                    {
                        public string Street { get; set; } = "4627 Sunset Ave";
                        public decimal Rent { get; set; } = 900.6m;
                    }
                    """;

        // Act
        var result = _csharpClassParser2.Parse(input);
        var data = JsonConvert.DeserializeObject<JObject>(result);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(data);
    }
    
    [Fact]
    public void Parse_WithNestedClassIncludingJsonProperty_ShouldGenerateValidJson()
    {
            // Arrange
            var input = @"
                        public class Person
                        {
                            public string Name { get; set; }
                            public int Age { get; set; }
                            public double Score { get; set; } = 0.0;
                            public bool IsActive { get; set; }
                            public Address Location { get; set; }
                            public List<string> Hobbies { get; set; } = new List<string> { ""Reading"", ""Gaming"" };
                            public string[] Keywords { get; set; } = new[] { ""Developer"", ""Engineer"" };
                            public Dictionary<string, int> Scores { get; set; }
                            public string PostalCode = ""99999"";

                            [JsonIgnore]
                            public string Ignored { get; set; }

                            [JsonProperty(""full_name"")]
                            public string NewName { get; set; } = ""Custom Name"";

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
                        }";

            // Act
            var result = _csharpClassParser2.Parse(input);
            var data = JsonConvert.DeserializeObject<JObject>(result);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(data);
            
            // Check constructor values take precedence
            Assert.Equal("Olayinka", data["name"].ToString());
            Assert.Equal(25, data["age"].ToObject<int>());
            
            // Check default values
            Assert.Equal(0.0, data["score"].ToObject<double>());
            Assert.Equal("99999", data["postalCode"].ToString());
            
            // Check collections with default values
            var hobbies = data["hobbies"].ToObject<string[]>();
            Assert.Equal(2, hobbies.Length);
            Assert.Contains("Reading", hobbies);
            Assert.Contains("Gaming", hobbies);
            
            var keywords = data["keywords"].ToObject<string[]>();
            Assert.Equal(2, keywords.Length);
            Assert.Contains("Developer", keywords);
            Assert.Contains("Engineer", keywords);
            
            // Check nested object
            Assert.NotNull(data["location"]);
            var location = data["location"] as JObject;
            Assert.Equal("4627 Sunset Ave", location["street"].ToString());
            Assert.Equal(900.6m, location["rent"].ToObject<decimal>());
            
            // Check custom JSON property name
            Assert.True(data.ContainsKey("full_name"));
            Assert.Equal("Custom Name", data["full_name"].ToString());
            
            // Check ignored property is not included
            Assert.False(data.ContainsKey("ignored"));
            
            // Check empty collections and objects
            Assert.IsType<JObject>(data["scores"]);
            Assert.False(data["isActive"].ToObject<bool>()); // Default bool value
    }
    
    [Fact]
    public void Parse_WithNestedClassIncludingJsonPropertyName_ShouldGenerateValidJson()
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

        //var resultInJson = _csharpClassParser.Parse(classCodeStringInput, false, false);
        var resultInJson = _csharpClassParser2.Parse(classCodeStringInput, false, false);
        
        resultInJson.Should().NotBeNull();
    }
    
    [Fact]
    public void InitializedCollections_ShouldBeFilled(){}
}