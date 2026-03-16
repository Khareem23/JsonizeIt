using FluentAssertions;
using JsonizeIt.Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonizeIt.Tests;

public class TypeScriptParserTests
{
    private readonly TypeScriptParser _parser = new();

    // ────────────────────────────────────────────────────────────────────────
    // Edge cases
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_EmptyInput_ReturnsEmptyObject()
    {
        _parser.Parse("").Should().Be("{}");
        _parser.Parse("   ").Should().Be("{}");
    }

    [Fact]
    public void Parse_MissingClosingBrace_ThrowsFormatException()
    {
        var input = """
            interface User {
              name: string;
            """;

        var act = () => _parser.Parse(input);
        act.Should().Throw<FormatException>()
           .WithMessage("The input data is not in the correct format.");
    }

    // ────────────────────────────────────────────────────────────────────────
    // Interface — primitive types
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_SimpleInterface_GeneratesCorrectDefaults()
    {
        var input = """
            interface User {
              name: string;
              age: number;
              isActive: boolean;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["name"]!.Value<string>().Should().Be("");
        obj["age"]!.Value<int>().Should().Be(0);
        obj["isActive"]!.Value<bool>().Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Class — primitive types with export keyword
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_ExportedClass_ParsesCorrectly()
    {
        var input = """
            export class Product {
              id: number;
              title: string;
              inStock: boolean;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["id"]!.Value<int>().Should().Be(0);
        obj["title"]!.Value<string>().Should().Be("");
        obj["inStock"]!.Value<bool>().Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Optional properties
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_OptionalProperties_AreIncluded()
    {
        var input = """
            interface Profile {
              username: string;
              bio?: string;
              avatarUrl?: string;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj.ContainsKey("username").Should().BeTrue();
        obj.ContainsKey("bio").Should().BeTrue();
        obj.ContainsKey("avatarUrl").Should().BeTrue();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Default values
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_ClassWithDefaultValues_UsesProvidedDefaults()
    {
        var input = """
            class Config {
              host: string = "localhost";
              port: number = 8080;
              debug: boolean = true;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["host"]!.Value<string>().Should().Be("localhost");
        obj["port"]!.Value<int>().Should().Be(8080);
        obj["debug"]!.Value<bool>().Should().BeTrue();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Array types
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_ArrayProperties_ReturnEmptyArrays()
    {
        var input = """
            interface Post {
              tags: string[];
              scores: number[];
              comments: Array<string>;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["tags"]!.Type.Should().Be(JTokenType.Array);
        obj["scores"]!.Type.Should().Be(JTokenType.Array);
        obj["comments"]!.Type.Should().Be(JTokenType.Array);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Record / Map types
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_RecordAndMapProperties_ReturnEmptyObjects()
    {
        var input = """
            interface Store {
              metadata: Record<string, any>;
              cache: Map<string, number>;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["metadata"]!.Type.Should().Be(JTokenType.Object);
        obj["cache"]!.Type.Should().Be(JTokenType.Object);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Union types
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_UnionTypes_UsesFirstNonNullMember()
    {
        var input = """
            interface Item {
              label: string | null;
              count: number | undefined;
              value: string | number;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["label"]!.Value<string>().Should().Be("");   // string chosen over null
        obj["count"]!.Value<int>().Should().Be(0);       // number chosen over undefined
        obj["value"]!.Value<string>().Should().Be("");   // string chosen over number
    }

    [Fact]
    public void Parse_NullOnlyUnion_ReturnsNull()
    {
        var input = """
            interface Item {
              deleted: null | undefined;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["deleted"]!.Type.Should().Be(JTokenType.Null);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Nested classes / interfaces
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_NestedInterfaces_GeneratesNestedObjects()
    {
        var input = """
            interface Address {
              street: string;
              city: string;
            }

            interface Person {
              name: string;
              address: Address;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["name"]!.Value<string>().Should().Be("");
        obj["address"]!.Type.Should().Be(JTokenType.Object);

        var address = (JObject)obj["address"]!;
        address["street"]!.Value<string>().Should().Be("");
        address["city"]!.Value<string>().Should().Be("");
    }

    // ────────────────────────────────────────────────────────────────────────
    // Access modifiers & readonly
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_ClassWithAccessModifiers_ParsesAllPublicAndPrivate()
    {
        var input = """
            class Vehicle {
              public make: string;
              public year: number;
              readonly vin: string;
              private _engineCode: string;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj.ContainsKey("make").Should().BeTrue();
        obj.ContainsKey("year").Should().BeTrue();
        obj.ContainsKey("vin").Should().BeTrue();
        obj.ContainsKey("_engineCode").Should().BeTrue();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Constructor shorthand
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_ConstructorShorthand_IncludesParamsAsProperties()
    {
        var input = """
            class Point {
              constructor(public x: number, public y: number) {}
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["x"]!.Value<int>().Should().Be(0);
        obj["y"]!.Value<int>().Should().Be(0);
    }

    [Fact]
    public void Parse_ConstructorShorthandWithDefaults_UsesDefaultValues()
    {
        var input = """
            class Server {
              constructor(
                public host: string = "localhost",
                public port: number = 3000
              ) {}
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["host"]!.Value<string>().Should().Be("localhost");
        obj["port"]!.Value<int>().Should().Be(3000);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Decorators (class-transformer style)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_DecoratedProperties_StripsDecoratorsAndParses()
    {
        var input = """
            class UserEntity {
              @Expose()
              name: string;

              @IsNumber()
              age: number;
            }
            """;

        var result = _parser.Parse(input);
        var obj = JObject.Parse(result);

        obj["name"]!.Value<string>().Should().Be("");
        obj["age"]!.Value<int>().Should().Be(0);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Date type
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_DateProperty_ReturnsIsoString()
    {
        var input = """
            interface Event {
              name: string;
              createdAt: Date;
            }
            """;

        var result = _parser.Parse(input);

        // Parse without Newtonsoft's automatic date conversion so the raw ISO string is preserved
        var obj = JObject.Load(new JsonTextReader(new System.IO.StringReader(result))
        {
            DateParseHandling = DateParseHandling.None
        });

        var dateStr = obj["createdAt"]!.Value<string>();
        dateStr.Should().NotBeNullOrEmpty();
        DateTimeOffset.TryParse(dateStr, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out _).Should().BeTrue();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Isminify option
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithMinify_ReturnsCompactJson()
    {
        var input = """
            interface User { name: string; }
            """;

        var result = _parser.Parse(input, isMinify: true);

        result.Should().NotContain("\n");
        result.Should().NotContain("  ");
    }

    // ────────────────────────────────────────────────────────────────────────
    // Factory wiring
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Factory_TypeScriptDataType_ReturnsTypeScriptParser()
    {
        var factory = new JsonConverterFactory();
        var converter = factory.GetConverter(DataType.TypeScript);

        converter.Should().BeOfType<TypeScriptParser>();
    }
}
