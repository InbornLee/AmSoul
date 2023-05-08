using AmSoul.Core.Interfaces;
using MongoDB.Driver;
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