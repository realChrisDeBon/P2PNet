using P2PBootstrap.Database.Tables.TableComponents;

namespace P2PBootstrap.Database.Tables
{
    public class Table_LogsCLI : TableBase
    {
        public Table_LogsCLI()
        {
            TableName = "LogsCLI";

            // Define the columns
            var idColumn = new Column<int>(new ColumnOptions
            {
                IsPrimaryKey = true,
                Autoincrement = true
            }, "ID");

            var strCommandColumn = new Column<string>(new ColumnOptions
            {
                NotNull = true
            }, "StrCommand");

            var processedColumn = new Column<bool>(new ColumnOptions
            {
                HasDefault = true,
                DefaultValue = false
            }, "Processed");

            var timestampColumn = new Column<DateTime>(new ColumnOptions
            {
                HasDefault = true,
                DefaultValue = "CURRENT_TIMESTAMP"
            }, "Timestamp");

            // Add columns to the table
            AddColumn(idColumn);
            AddColumn(strCommandColumn);
            AddColumn(processedColumn);
            AddColumn(timestampColumn);
        }
    }
}
