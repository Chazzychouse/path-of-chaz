using System;
using Microsoft.Data.Sqlite;

namespace PathOfChaz.Core;

public class RunDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public RunDatabase(string dbPath)
    {
        _connection = new SqliteConnection($"Data Source={dbPath}");
        _connection.Open();
    }

    public void Initialize()
    {
        using var transaction = _connection.BeginTransaction();

        using var createSchema = _connection.CreateCommand();
        createSchema.Transaction = transaction;
        createSchema.CommandText = """
            CREATE TABLE IF NOT EXISTS schema_version (
                version INTEGER NOT NULL
            );
            """;
        createSchema.ExecuteNonQuery();

        using var checkVersion = _connection.CreateCommand();
        checkVersion.Transaction = transaction;
        checkVersion.CommandText = "SELECT COUNT(*) FROM schema_version";
        var count = Convert.ToInt32(checkVersion.ExecuteScalar());

        if (count == 0)
        {
            using var insertVersion = _connection.CreateCommand();
            insertVersion.Transaction = transaction;
            insertVersion.CommandText = "INSERT INTO schema_version VALUES (1)";
            insertVersion.ExecuteNonQuery();
        }

        using var createRunState = _connection.CreateCommand();
        createRunState.Transaction = transaction;
        createRunState.CommandText = """
            CREATE TABLE IF NOT EXISTS run_state (
                player_health INTEGER NOT NULL,
                enemy_health  INTEGER NOT NULL,
                turn_count    INTEGER NOT NULL,
                rng_seed      INTEGER NOT NULL,
                created_at    TEXT NOT NULL,
                updated_at    TEXT NOT NULL
            );
            """;
        createRunState.ExecuteNonQuery();

        transaction.Commit();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
