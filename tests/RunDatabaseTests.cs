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
            RngSeed = 42,
            CreatedAt = now,
            UpdatedAt = now,
        };

        Assert.Equal(15, state.PlayerHealth);
        Assert.Equal(7, state.EnemyHealth);
        Assert.Equal(3, state.TurnCount);
        Assert.Equal(42, state.RngSeed);
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

    [Fact]
    public void Save_ThenLoad_RoundTripsAllFields()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var db = new RunDatabase(path);
            db.Initialize();

            var saved = new RunState
            {
                PlayerHealth = 15,
                EnemyHealth = 7,
                TurnCount = 3,
                RngSeed = 42,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            };

            db.Save(saved);
            var loaded = db.Load();

            Assert.NotNull(loaded);
            Assert.Equal(15, loaded.PlayerHealth);
            Assert.Equal(7, loaded.EnemyHealth);
            Assert.Equal(3, loaded.TurnCount);
            Assert.Equal(42, loaded.RngSeed);
            Assert.Equal(saved.CreatedAt, loaded.CreatedAt);
            Assert.Equal(saved.UpdatedAt, loaded.UpdatedAt);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_ReturnsNull_WhenNoRunExists()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var db = new RunDatabase(path);
            db.Initialize();

            var loaded = db.Load();

            Assert.Null(loaded);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Save_Twice_UpsertsInsteadOfDuplicating()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var db = new RunDatabase(path);
            db.Initialize();

            var state = new RunState
            {
                PlayerHealth = 20,
                EnemyHealth = 10,
                TurnCount = 1,
                RngSeed = 99,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            };
            db.Save(state);

            state.PlayerHealth = 12;
            state.TurnCount = 5;
            state.UpdatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            db.Save(state);

            var loaded = db.Load();
            Assert.NotNull(loaded);
            Assert.Equal(12, loaded.PlayerHealth);
            Assert.Equal(5, loaded.TurnCount);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Delete_RemovesActiveRun()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var db = new RunDatabase(path);
            db.Initialize();

            var state = new RunState
            {
                PlayerHealth = 20,
                EnemyHealth = 10,
                TurnCount = 1,
                RngSeed = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            db.Save(state);
            db.Delete();

            Assert.Null(db.Load());
        }
        finally
        {
            File.Delete(path);
        }
    }
}
