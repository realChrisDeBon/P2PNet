using Microsoft.Data.Sqlite;
using P2PBootstrap.Database.Tables;
namespace P2PBootstrap.Database
{
    public static class DatabaseService
    {
        private static string DbFilePath => Path.Combine(AppContext.BaseDirectory, AppSettings["Database:DbFilename"]);
        private static string ConnectionString = $"Data Source={DbFilePath}";
        private static SqliteConnection connection;

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

        public static void InitializeTables()
        {
            // InitializeDatabase tables
            using (var command = new SqliteCommand(LogsCLI_table.GetCreateTableCommand(), connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
