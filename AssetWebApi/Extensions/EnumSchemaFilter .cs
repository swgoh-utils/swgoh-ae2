using Microsoft.OpenApi;
using System.Text.Json.Nodes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AssetWebApi.Extensions
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum || schema is not OpenApiSchema model)
            {
                return;
            }

            model.Enum?.Clear();

            foreach (var name in Enum.GetNames(context.Type))
            {
                var value = Convert.ToInt64(Enum.Parse(context.Type, name));

                model.Enum?.Add(JsonValue.Create($"{value} = {name}")!);
            }
        }
    }
}