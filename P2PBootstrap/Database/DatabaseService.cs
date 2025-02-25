using Microsoft.Data.Sqlite;
using P2PBootstrap.Database.Tables;
namespace P2PBootstrap.Database
{
    public static class DatabaseService
    {
        private static string DbFilePath => Path.Combine(AppContext.BaseDirectory, AppSettings["Database:DbFilename"]);
        private static string ConnectionString = $"Data Source={DbFilePath}";
        private static SqliteConnection connection;

        // some useful public properties for reference
        public static string AdminConsoleLog => "StrCommand";

        public static bool DbRunning { get; set; } = true;

        public static Table_LogsCLI LogsCLI_table { get; set; } = new Table_LogsCLI();

        public static void InitializeDatabase()
        {

            // open database connection
            connection = new SqliteConnection(ConnectionString);
            connection.Open();

            try
            {
                connection.Open();
            }
            catch(Exception ex)
            {
                DebugMessage($"Unknown error opening database connection: {ex.Message}", MessageType.Critical);
                DbRunning = false;
                return;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    DebugMessage("Successfully opened database connection.", MessageType.General);
                    InitializeTables();
                }
            }
        }

        private static void InitializeTables()
        {
            // InitializeDatabase tables
            using (var command = new SqliteCommand(LogsCLI_table.GetCreateTableCommand(), connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void ExecuteTableCommand(string command)
        {
            DebugMessage($"Executing database command: {command}", MessageType.General);
            using (var cmd = new SqliteCommand(command, connection))
            {
                    cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateMostRecentLogProcessed(bool processed)
        {
            // Find the most recent entry by ID
            string findMostRecentEntryQuery = "SELECT MAX(ID) FROM LogsCLI";
            int mostRecentEntryId = -1;
            try
            {
                using (var findCommand = new SqliteCommand(findMostRecentEntryQuery, connection))
                {
                    var result = findCommand.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int id))
                    {
                        mostRecentEntryId = id;
                    }
                    else
                    {
                        DebugMessage("Error finding most recent entry in LogsCLI.", MessageType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugMessage($"Error finding most recent entry in LogsCLI: {ex.Message}", MessageType.Warning);
            }

            if (mostRecentEntryId != -1)
            {
                // Update the Processed column for the most recent entry
                string updateQuery = $"UPDATE LogsCLI SET Processed = @Processed WHERE ID = @ID";
                using (var updateCommand = new SqliteCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@Processed", 1);
                    updateCommand.Parameters.AddWithValue("@ID", mostRecentEntryId);
                    updateCommand.ExecuteNonQuery();
                }
            }
            else
            {
                DebugMessage("No entries found in LogsCLI_Table.", MessageType.Warning);
            }
        }

    }
}
