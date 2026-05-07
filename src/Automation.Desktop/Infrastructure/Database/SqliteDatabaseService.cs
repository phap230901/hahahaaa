using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models;
using Microsoft.Data.Sqlite;

namespace Automation.Desktop.Infrastructure.Database;

public sealed class SqliteDatabaseService : IDatabaseService
{
    private readonly string _connectionString = "Data Source=automation.db";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS devices (id INTEGER PRIMARY KEY, device_id TEXT UNIQUE, status TEXT, model TEXT, updated_at TEXT);
CREATE TABLE IF NOT EXISTS logs (id INTEGER PRIMARY KEY, level TEXT, message TEXT, created_at TEXT);
CREATE TABLE IF NOT EXISTS workflows (id INTEGER PRIMARY KEY, name TEXT, json_definition TEXT, updated_at TEXT);
CREATE TABLE IF NOT EXISTS profiles (id INTEGER PRIMARY KEY, name TEXT, payload TEXT, updated_at TEXT);";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertDevicesAsync(IEnumerable<DeviceInfo> devices, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var device in devices)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO devices (device_id, status, model, updated_at)
VALUES ($deviceId, $status, $model, $updatedAt)
ON CONFLICT(device_id) DO UPDATE SET status=excluded.status, model=excluded.model, updated_at=excluded.updated_at;";
            cmd.Parameters.AddWithValue("$deviceId", device.DeviceId);
            cmd.Parameters.AddWithValue("$status", device.Status);
            cmd.Parameters.AddWithValue("$model", device.Model);
            cmd.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("O"));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
