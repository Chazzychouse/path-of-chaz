using PathOfChaz.Core;

namespace PathOfChaz.Tests;

public class RunDatabaseTests
{
    [Fact]
    public void RunState_RoundTripsProperties()
    {
        var now = DateTime.UtcNow;
        var state = new RunState
        {
            PlayerHealth = 15,
            EnemyHealth = 7,
            TurnCount = 3,
            RngSeed = 42L,
            CreatedAt = now,
            UpdatedAt = now,
        };

        Assert.Equal(15, state.PlayerHealth);
        Assert.Equal(7, state.EnemyHealth);
        Assert.Equal(3, state.TurnCount);
        Assert.Equal(42L, state.RngSeed);
        Assert.Equal(now, state.CreatedAt);
        Assert.Equal(now, state.UpdatedAt);
    }

    [Fact]
    public void Initialize_CreatesTablesAndSetsSchemaVersion()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var db = new RunDatabase(path);
            db.Initialize();

            // Verify schema_version table exists and has version 1
            using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT version FROM schema_version";
            var version = Convert.ToInt32(cmd.ExecuteScalar());
            Assert.Equal(1, version);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
