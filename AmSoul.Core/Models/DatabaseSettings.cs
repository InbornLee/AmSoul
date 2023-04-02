using AmSoul.Core.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.Configuration;

namespace AmSoul.Core.Models;

public class MongoDbDatabaseSetting : IMongoDbDatabaseSetting
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 27017;
    public string? Username { get; set; }
    public string? Password { get; set; }

    public string? DatabaseName { get; set; }
    public Action<ClusterBuilder>? ClusterConfigurator { get; set; }
    public MongoClientSettings? MongoClientSettings { get; set; }
    public string UsersCollection { get; set; } = "users";

    public string RolesCollection { get; set; } = "roles";

    public string MigrationCollection { get; set; } = "_migrations";

    public string ConnectionString => new MongoUrlBuilder()
    {
        Server = new MongoServerAddress(Server, Port),
        Username = Username,
        Password = Password,
        //DatabaseName = DatabaseName,
        AuthenticationSource = DatabaseName
    }.ToString();
}
public class MySqlDatabaseSetting : IMysqlDatabaseSetting
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? DatabaseName { get; set; }
    public string ConnectionString => $"Server={Server};Port={Port};Database={DatabaseName};User={Username};Password={Password};";
}
public class SqlServerDatabaseSetting : ISqlServerDatabaseSetting
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? DatabaseName { get; set; }
    public string ConnectionString => $"server={Server};uid={Username};pwd={Password};database={DatabaseName};";
}
public class OracleDatabaseSetting : IOracleDatabaseSetting
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 1521;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? DatabaseName { get; set; }
    public string ConnectionString => $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={Server}) (PORT={Port})))(CONNECT_DATA=(SERVICE_NAME= {DatabaseName})));User Id={Username}; Password={Password};";
    //public string ConnectionString => $"Data Source={Server}:{Port}/{DatabaseName};User ID={Username};Password={Password};";
}
