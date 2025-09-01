using JsonizeIT.API.Models;
using JsonizeIt.Lib;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IJsonConverter, CSharpParser2>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// POST endpoint
app.MapPost("/parse", (ParseRequest request, IJsonConverter parser) =>
{
    try
    {
        var bytes = Convert.FromBase64String(request.DataToConvertInBase64);
        var decoded = System.Text.Encoding.UTF8.GetString(bytes);
    
        var result = parser.Parse(decoded, request.IsCamelCase, request.IsMinify);

        // Automatically returns JSON
        return Results.Ok(new { data = result });
    }
    catch (Exception e)
    {
        // Log the Exception
        return Results.BadRequest( new { data = e.Message, error = e.ToString(), isError = true });
    }
    
})
.WithName("ParseInput")
.WithOpenApi();

app.Run();
