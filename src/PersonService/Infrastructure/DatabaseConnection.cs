using Microsoft.AspNetCore.WebUtilities;
using Npgsql;

namespace PersonService.Infrastructure;

public static class DatabaseConnection
{
    private const string ConnectionStringName = "PersonsDb";

    public static string Resolve(IConfiguration configuration)
    {
        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return FromUrl(databaseUrl);
        }

        return configuration.GetConnectionString(ConnectionStringName)
               ?? throw new InvalidOperationException(
                   $"Neither DATABASE_URL nor ConnectionStrings:{ConnectionStringName} is configured");
    }

    public static string FromUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var credentials = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port,
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty,
            Database = uri.AbsolutePath.Trim('/'),
            SslMode = DefaultSslMode(uri.Host)
        };

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("sslmode", out var sslMode)
            && Enum.TryParse<SslMode>(sslMode.ToString(), ignoreCase: true, out var parsedSslMode))
        {
            builder.SslMode = parsedSslMode;
        }

        return builder.ConnectionString;
    }

    private static SslMode DefaultSslMode(string host) =>
        host is "localhost" or "127.0.0.1" or "postgres" ? SslMode.Disable : SslMode.Require;
}
