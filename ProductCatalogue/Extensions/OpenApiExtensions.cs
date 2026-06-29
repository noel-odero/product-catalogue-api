using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace ProductCatalogue.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiWithAuth(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            // 1. Declare the Bearer scheme once
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Servers = new List<OpenApiServer> { new() { Url = "/" } };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                };
                return Task.CompletedTask;
            });

            // 2. Apply Bearer only to endpoints that require auth
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
                    .OfType<IAuthorizeData>().Any()
                    && !context.Description.ActionDescriptor.EndpointMetadata
                        .OfType<IAllowAnonymous>().Any();

                if (requiresAuth)
                {
                    operation.Security ??= new List<OpenApiSecurityRequirement>();
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = new List<string>()
                    });
                }

                return Task.CompletedTask;
            });

            // 3. Describe IFormFile endpoints as multipart/form-data so Scalar shows a file picker
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var hasFileParam = context.Description.ParameterDescriptions
                    .Any(p => p.Type == typeof(IFormFile));

                if (!hasFileParam)
                    return Task.CompletedTask;

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, IOpenApiSchema>
                                {
                                    ["File"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.String,
                                        Format = "binary"
                                    },
                                    ["AssetType"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.String
                                    },
                                    ["Title"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.String
                                    },
                                    ["Description"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.String
                                    },
                                    ["VariantId"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.String,
                                        Format = "uuid"
                                    },
                                    ["Tags"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.Array,
                                        Items = new OpenApiSchema
                                        {
                                            Type = JsonSchemaType.String
                                        }
                                    },
                                },
                                Required = new HashSet<string> { "File", "AssetType", "Title" }
                            }
                        }
                    }
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }
}