namespace AmSoul.SQL;
/// <summary>
/// Default MySql DatabaseSetting
/// </summary>
public class MySqlDatabaseSetting : IMysqlDatabaseSetting
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? DatabaseName { get; set; }
    public string ConnectionString => $"Server={Server};Port={Port};Database={DatabaseName};User={Username};Password={Password};";
}
/// <summary>
/// Default SqlServer DatabaseSetting
/// </summary>
public class SqlServerDatabaseSetting : ISqlServerDatabaseSetting
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 1433;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? DatabaseName { get; set; }
    public string ConnectionString => $"server={Server};uid={Username};pwd={Password};database={DatabaseName};";
}
/// <summary>
/// Default Oracle DatabaseSetting
/// </summary>
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
/// <summary>
/// Default PostgreSQL DatabaseSetting
/// </summary>
public class PostgreSQLDatabaseSetting : IPostgreSQLDatabaseSetting
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? DatabaseName { get; set; }
    public string ConnectionString => $"Host={Server};Port={Port};Database={DatabaseName};Username={Username};Password={Password};";
}
