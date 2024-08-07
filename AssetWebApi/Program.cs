using AssetWebApi.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System;

string disableHttpsRedirect = Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECT");
string enableSwagger = Environment.GetEnvironmentVariable("ENABLE_SWAGGER");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var openApiContact = new OpenApiContact
    {
        Name = "Commlink Discord",
        Extensions = new Dictionary<string, IOpenApiExtension>(),
        Url = new Uri("https://discord.gg/7Mk7a8zS")
    };

    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "swgoh Asset getter",
            Version = "v1",
            Description = $"swgoh Asset getter can list and download swgoh related assets. (2d Textures only)",
            Contact = openApiContact,
        }
     );

    //c.SchemaFilter<EnumSchemaFilter>();

    var filePath = Path.Combine(System.AppContext.BaseDirectory, "AssetWebApi.xml");
    c.IncludeXmlComments(filePath, true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || enableSwagger == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (disableHttpsRedirect == null || disableHttpsRedirect != "true")
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
