using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=leveling_system;Username=postgres;Password=postgres";

using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

var sql = @"
ALTER TABLE ""QuestDefinitions"" 
ADD COLUMN IF NOT EXISTS ""CompletionCount"" INTEGER NOT NULL DEFAULT 0;
";

using var command = new NpgsqlCommand(sql, connection);
await command.ExecuteNonQueryAsync();

Console.WriteLine("CompletionCount column added successfully!");
