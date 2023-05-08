using AmSoul.MongoDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System.ComponentModel;
using System.Text;

namespace AmSoul.Identity.MongoDB;

/// <summary>
/// Service Collection Extensions
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Jwt Authentication
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="customTokenOptions"></param>
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, JwtTokenOptions? customTokenOptions = default)
    {
        JwtTokenOptions? tokenOptions = customTokenOptions ?? configuration.GetSection(nameof(JwtTokenOptions)).Get<JwtTokenOptions>();
        if (tokenOptions == null) throw new ArgumentNullException(nameof(tokenOptions));
        services.Configure<JwtTokenOptions>(options =>
        {
            options.Audience = tokenOptions.Audience;
            options.Issuer = tokenOptions.Issuer;
            options.SecurityKey = tokenOptions.SecurityKey;
            options.DurationInMinutes = tokenOptions.DurationInMinutes;
        });

        services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<JwtTokenOptions>>().Value);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = tokenOptions!.Issuer,

                    ValidateAudience = true,//验证Audience
                    ValidAudience = tokenOptions.Audience,

                    RequireExpirationTime = true,
                    ValidateLifetime = true,//验证失效时间

                    ValidateIssuerSigningKey = true, //验证SecurityKey
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey ?? string.Empty)), //将密钥添加到JWT加密算法中
                };
            });
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var baseXmlPath = Path.Combine(baseDirectory, "AmSoul.Identity.MongoDB.xml");
        services.AddSwaggerGen(options => options.IncludeXmlComments(baseXmlPath));
    }
    public static IdentityBuilder AddMongoDbIdentityStores(
    this IServiceCollection services,
    Action<IdentityOptions> identitySettingsAction,
    IMongoDbDatabaseSetting? databaseSettings,
    IdentityErrorDescriber? identityErrorDescriber = default)
    {
        var builder = services.AddIdentity<BaseUser, BaseRole>(identitySettingsAction ?? (s => { }));
        return builder.AddMongoDbIdentityStores<BaseUser, BaseRole, ObjectId>(databaseSettings, identityErrorDescriber);
    }

    public static IdentityBuilder AddMongoDbIdentityStores(
        this IServiceCollection services,
        Action<IdentityOptions> identitySettingsAction,
        Action<IMongoDbDatabaseSetting?> databaseSettingsAction,
        IdentityErrorDescriber? identityErrorDescriber = default)
    {
        var builder = services.AddIdentity<BaseUser, BaseRole>(identitySettingsAction ?? (s => { }));
        return builder.AddMongoDbIdentityStores<BaseUser, BaseRole, ObjectId>(databaseSettingsAction, identityErrorDescriber);
    }

    public static IdentityBuilder AddMongoDbIdentityStores<TUser, TRole>(
        this IServiceCollection services,
        Action<IdentityOptions> identitySettingsAction,
        Action<IMongoDbDatabaseSetting?> databaseSettingsAction,
        IdentityErrorDescriber? identityErrorDescriber = default)
        where TUser : BaseUser<ObjectId>
        where TRole : BaseRole<ObjectId>
    {
        var builder = services.AddIdentity<TUser, TRole>(identitySettingsAction ?? (s => { }));
        return builder.AddMongoDbIdentityStores<TUser, TRole, ObjectId>(databaseSettingsAction, identityErrorDescriber);
    }
    public static IdentityBuilder AddMongoDbIdentityStores<TUser, TRole, TKey>(
        this IServiceCollection services,
        Action<IdentityOptions> identitySettingsAction,
        Action<IMongoDbDatabaseSetting?> databaseSettingsAction,
        IdentityErrorDescriber? identityErrorDescriber = default)
        where TKey : IEquatable<TKey>
        where TUser : BaseUser<TKey>
        where TRole : BaseRole<TKey>
    {
        var builder = services.AddIdentity<TUser, TRole>(identitySettingsAction ?? (s => { }));
        return builder.AddMongoDbIdentityStores<TUser, TRole, TKey>(databaseSettingsAction, identityErrorDescriber);
    }
    public static IdentityBuilder AddMongoDbIdentityStores<TUser, TRole>(
    this IServiceCollection services,
    Action<IdentityOptions> identitySettingsAction,
    IMongoDbDatabaseSetting? databaseSettings,
    IdentityErrorDescriber? identityErrorDescriber = default)
    where TUser : BaseUser<ObjectId>
    where TRole : BaseRole<ObjectId>
    {
        var builder = services.AddIdentity<TUser, TRole>(identitySettingsAction ?? (s => { }));
        return builder.AddMongoDbIdentityStores<TUser, TRole, ObjectId>(databaseSettings, identityErrorDescriber);
    }
    public static IdentityBuilder AddMongoDbIdentityStores<TUser, TRole, TKey>(
        this IServiceCollection services,
        Action<IdentityOptions> identitySettingsAction,
        IMongoDbDatabaseSetting? databaseSettings,
        IdentityErrorDescriber? identityErrorDescriber = default)
        where TKey : IEquatable<TKey>
        where TUser : BaseUser<TKey>
        where TRole : BaseRole<TKey>
    {
        var builder = services.AddIdentity<TUser, TRole>(identitySettingsAction ?? (s => { }));
        return builder.AddMongoDbIdentityStores<TUser, TRole, TKey>(databaseSettings, identityErrorDescriber);
    }

    private static IdentityBuilder AddMongoDbIdentityStores<TUser, TRole, TKey>(this IdentityBuilder builder,
        IMongoDbDatabaseSetting? databaseSettings,
        IdentityErrorDescriber? identityErrorDescriber = default)
        where TKey : IEquatable<TKey>
        where TUser : BaseUser<TKey>
        where TRole : BaseRole<TKey>
        => databaseSettings == null
        ? throw new ArgumentNullException(nameof(IMongoDbDatabaseSetting))
        : builder.AddMongoDbIdentityStores<TUser, TRole, TKey>(database =>
        {
            database!.Server = databaseSettings.Server;
            database.Port = databaseSettings.Port;
            database.DatabaseName = databaseSettings.DatabaseName;
            database.Username = databaseSettings.Username;
            database.Password = databaseSettings.Password;
        }, identityErrorDescriber);

    private static IdentityBuilder AddMongoDbIdentityStores<TUser, TRole, TKey>(
        this IdentityBuilder builder,
        Action<IMongoDbDatabaseSetting?> databaseSettingsAction,
        IdentityErrorDescriber? identityErrorDescriber = default)
        where TKey : IEquatable<TKey>
        where TUser : BaseUser<TKey>
        where TRole : BaseRole<TKey>
    {
        MongoDbDatabaseSetting dbOptions = new();
        databaseSettingsAction(dbOptions);

        var database = MongoDbQueryExtensions.GetDatabase(dbOptions);

        var userCollection = database.GetCollection<TUser>(dbOptions.UsersCollection);
        var roleCollection = database.GetCollection<TRole>(dbOptions.RolesCollection);

        builder.AddRoleStore<RoleStore<TRole, TKey>>();
        builder.AddUserStore<UserStore<TUser, TRole, TKey>>();
        builder.AddUserManager<UserManager<TUser>>();
        builder.AddRoleManager<RoleManager<TRole>>();
        builder.AddErrorDescriber<LocalizedIdentityErrorDescriber>();

        builder.Services.TryAddSingleton(x => userCollection);
        builder.Services.TryAddSingleton(x => roleCollection);
        builder.Services.AddAutoMapper(typeof(MapperConfig));
        builder.Services.TryAddScoped<IUserService, UserServiceBase>();

        if (typeof(TKey) == typeof(ObjectId))
        {
            TypeDescriptor.AddAttributes(typeof(ObjectId), new Attribute[1] { new TypeConverterAttribute(typeof(ObjectIdConverter)) });
        }

        Core.ServiceCollectionExtensions.RegisterSwaggerXmlFile(builder.Services, "AmSoul.Identity.MongoDB.xml");
        return builder;
    }
}
