using P2PBootstrap.Database.Tables.TableComponents;

namespace P2PBootstrap.Database.Tables
{
    public class Table_SigningHistory : TableBase
    {
        public Table_SigningHistory()
        {
            TableName = "SigningHistory";

            // Define the columns
            var idColumn = new Column<int>(new ColumnOptions
            {
                IsPrimaryKey = true,
                Autoincrement = true
            }, "ID");

            var objHash = new Column<string>(new ColumnOptions
            {
                NotNull = true
            }, "Hash");

            var signature = new Column<string>(new ColumnOptions
            {
                NotNull = true
            }, "Signature");

            var timestampColumn = new Column<DateTime>(new ColumnOptions
            {
                HasDefault = true,
                DefaultValue = "CURRENT_TIMESTAMP"
            }, "Timestamp");

            // Add columns to the table
            AddColumn(idColumn);
            AddColumn(objHash);
            AddColumn(signature);
            AddColumn(timestampColumn);
        }
    }
}
