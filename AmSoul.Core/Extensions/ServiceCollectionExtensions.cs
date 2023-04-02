using AmSoul.Core.Interfaces;
using AmSoul.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace AmSoul.Core.Extensions;

/// <summary>
/// Service Collection Extensions
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Mongodb Store
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        AddMongoDb<MongoDbDatabaseSetting>(services, configuration);
    }
    /// <summary>
    /// Register Mongodb Store External
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddMongoDb<T>(this IServiceCollection services, IConfiguration configuration) where T : class, IMongoDbDatabaseSetting
    {
        services.Configure<T>(configuration.GetSection(typeof(T).Name));
        services.TryAddScoped(sp => sp.GetRequiredService<IOptions<T>>().Value);
    }

    /// <summary>
    /// Register Mysql Store
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddMySql(this IServiceCollection services, IConfiguration configuration)
    {
        AddMySql<MySqlDatabaseSetting>(services, configuration);
    }
    /// <summary>
    /// Register Mysql Store External
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddMySql<T>(this IServiceCollection services, IConfiguration configuration) where T : class, IMysqlDatabaseSetting
    {
        services.Configure<T>(configuration.GetSection(typeof(T).Name));
        services.TryAddScoped(sp => sp.GetRequiredService<IOptions<T>>().Value);
    }
    /// <summary>
    /// Register Oracle Store
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddOracle(this IServiceCollection services, IConfiguration configuration)
    {
        AddOracle<OracleDatabaseSetting>(services, configuration);
    }
    /// <summary>
    /// Register Oracle Store External
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddOracle<T>(this IServiceCollection services, IConfiguration configuration) where T : class, IOracleDatabaseSetting
    {
        services.Configure<T>(configuration.GetSection(typeof(T).Name));
        services.TryAddScoped(sp => sp.GetRequiredService<IOptions<T>>().Value);
    }
    /// <summary>
    /// Register SqlServer Store
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        AddSqlServer<SqlServerDatabaseSetting>(services, configuration);
    }
    /// <summary>
    /// Register SqlServer Store External
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddSqlServer<T>(this IServiceCollection services, IConfiguration configuration) where T : class, ISqlServerDatabaseSetting
    {
        services.Configure<T>(configuration.GetSection(typeof(T).Name));
        services.TryAddScoped(sp => sp.GetRequiredService<IOptions<T>>().Value);
    }
    /// <summary>
    /// Register Jwt Swagger
    /// </summary>
    /// <param name="services"></param>
    /// <param name="title"></param>
    /// <param name="version"></param>
    /// <param name="description"></param>
    /// <param name="externalXmlFileName"></param>
    public static void AddJwtSwaggerGen(this IServiceCollection services, string title, string version, string description, string? externalXmlFileName = default)
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
            if (!string.IsNullOrWhiteSpace(externalXmlFileName))
            {
                var externalXmlPath = Path.Combine(baseDirectory, externalXmlFileName);
                options.IncludeXmlComments(externalXmlPath);
            }

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
}
