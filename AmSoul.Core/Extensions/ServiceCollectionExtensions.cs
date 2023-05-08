using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace AmSoul.Core;

/// <summary>
/// Service Collection Extensions
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Jwt Swagger
    /// </summary>
    /// <param name="services"></param>
    /// <param name="title"></param>
    /// <param name="version"></param>
    /// <param name="description"></param>
    public static void AddJwtSwaggerGen(this IServiceCollection services, string title, string version, string description)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version, Description = description });
            options.DocInclusionPredicate((docName, description) => true);

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var baseXmlPath = Path.Combine(baseDirectory, "AmSoul.Core.xml");
            var xmlPath = Path.Combine(baseDirectory, AppDomain.CurrentDomain.FriendlyName + ".xml");
            options.IncludeXmlComments(baseXmlPath);
            options.IncludeXmlComments(xmlPath);

            options.OrderActionsBy(o => o.RelativePath);

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "输入 JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
        });

    }
    public static void RegisterSwaggerXmlFile(IServiceCollection services, params string[] swaggerXmlFiles)
    {
        services.ConfigureSwaggerGen(options =>
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var xmlfile in swaggerXmlFiles)
            {
                var xmlPath = Path.Combine(baseDirectory, xmlfile);
                options.IncludeXmlComments(xmlPath);
            }
        });

    }
}
