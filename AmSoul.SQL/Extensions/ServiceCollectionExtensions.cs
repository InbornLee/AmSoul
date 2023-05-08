using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace AmSoul.SQL;
public static partial class ServiceCollectionExtensions
{
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
        Core.ServiceCollectionExtensions.RegisterSwaggerXmlFile(services, "AmSoul.SQL.xml");
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
        Core.ServiceCollectionExtensions.RegisterSwaggerXmlFile(services, "AmSoul.SQL.xml");
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
        Core.ServiceCollectionExtensions.RegisterSwaggerXmlFile(services, "AmSoul.SQL.xml");
    }

}
