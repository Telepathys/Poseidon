using System.Data;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Poseidon
{
    public class DatabaseConfig
    {
        /// <summary>
        /// Postgresql Database Connection
        /// </summary>
        /// <returns>
        /// NpgsqlConnection
        /// </returns>
        public IDbConnection PostgreSql()  
        {  
            var database = Environment.GetEnvironmentVariable("DATABASE");
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
            var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            NpgsqlLoggingConfiguration.InitializeLogging(loggerFactory);
            var conn = new NpgsqlConnection($"Host={host}:5432; Database=postgres; Username={username}; Password={password}; Database={database};");
            conn.Open();
            return conn;
        } 
    }
}
