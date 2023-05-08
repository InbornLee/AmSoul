namespace AmSoul.Core;

public interface IDatabaseSetting
{
    string Server { get; set; }
    int Port { get; set; }
    string? Username { get; set; }
    string? Password { get; set; }
    string? DatabaseName { get; set; }
    string ConnectionString { get; }

}