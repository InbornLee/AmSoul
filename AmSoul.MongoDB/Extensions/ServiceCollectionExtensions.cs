using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AmSoul.MongoDB;

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
        Core.ServiceCollectionExtensions.RegisterSwaggerXmlFile(services, "AmSoul.MongoDB.xml");
    }
}
