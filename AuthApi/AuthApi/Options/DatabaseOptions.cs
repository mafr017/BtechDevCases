using System.ComponentModel.DataAnnotations;
using Npgsql;

namespace AuthApi.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string Host { get; set; } = null!;

    [Range(1, 65535)]
    public int Port { get; set; } = 5432;

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    public string ToConnectionString() => new NpgsqlConnectionStringBuilder
    {
        Host = Host,
        Port = Port,
        Database = Name,
        Username = Username,
        Password = Password,
    }.ConnectionString;
}
