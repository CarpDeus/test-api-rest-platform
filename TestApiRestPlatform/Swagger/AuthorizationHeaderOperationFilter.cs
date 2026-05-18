using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestApiRestPlatform.Swagger;

public class AuthorizationHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorizationHeader = context.ApiDescription.ParameterDescriptions
            .Any(parameter =>
                string.Equals(parameter.Name, "authorization", StringComparison.OrdinalIgnoreCase) &&
                parameter.Source?.Id == "Header");

        if (!hasAuthorizationHeader)
        {
            return;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                []
            }
        });
    }
}
