#nullable enable
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

    public void Save(RunState state)
    {
        using var transaction = _connection.BeginTransaction();

        using var delete = _connection.CreateCommand();
        delete.Transaction = transaction;
        delete.CommandText = "DELETE FROM run_state";
        delete.ExecuteNonQuery();

        using var insert = _connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = """
            INSERT INTO run_state (player_health, enemy_health, turn_count, rng_seed, created_at, updated_at)
            VALUES (@ph, @eh, @tc, @rs, @ca, @ua)
            """;
        insert.Parameters.AddWithValue("@ph", state.PlayerHealth);
        insert.Parameters.AddWithValue("@eh", state.EnemyHealth);
        insert.Parameters.AddWithValue("@tc", state.TurnCount);
        insert.Parameters.AddWithValue("@rs", state.RngSeed);
        insert.Parameters.AddWithValue("@ca", state.CreatedAt.ToString("o"));
        insert.Parameters.AddWithValue("@ua", state.UpdatedAt.ToString("o"));
        insert.ExecuteNonQuery();

        transaction.Commit();
    }

    public RunState? Load()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT player_health, enemy_health, turn_count, rng_seed, created_at, updated_at FROM run_state LIMIT 1";
        using var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return null;

        return new RunState
        {
            PlayerHealth = reader.GetInt32(0),
            EnemyHealth = reader.GetInt32(1),
            TurnCount = reader.GetInt32(2),
            RngSeed = reader.GetInt64(3),
            CreatedAt = DateTime.Parse(reader.GetString(4)).ToUniversalTime(),
            UpdatedAt = DateTime.Parse(reader.GetString(5)).ToUniversalTime(),
        };
    }

    public void Delete()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM run_state";
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
