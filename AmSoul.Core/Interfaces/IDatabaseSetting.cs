namespace AmSoul.Core.Interfaces;

public interface IDatabaseSetting
{
    string Server { get; set; }
    int Port { get; set; }
    string? Username { get; set; }
    string? Password { get; set; }
    string? DatabaseName { get; set; }
    string ConnectionString { get; }

}
public interface IMysqlDatabaseSetting : IDatabaseSetting { }
public interface IMongoDbDatabaseSetting : IDatabaseSetting { }
public interface IOracleDatabaseSetting : IDatabaseSetting { }
public interface ISqlServerDatabaseSetting : IDatabaseSetting { }
