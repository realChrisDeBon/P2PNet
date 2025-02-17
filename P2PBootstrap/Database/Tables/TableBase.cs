using P2PBootstrap.Database.Tables.TableComponents;
using System.Text;

namespace P2PBootstrap.Database.Tables
{
    public class TableBase
    {
        public string TableName { get; set; }
        List<IColumn> Columns = new List<IColumn>();

        public void AddColumn(IColumn column)
        {
            Columns.Add(column);
        }

        public string GetInsertCommand(string value, string targetColumnName)
        {
            var targetColumn = Columns.FirstOrDefault(c => c.ColumnName == targetColumnName);
            if (targetColumn == null)
            {
                throw new ArgumentException($"Column '{targetColumnName}' does not exist in the table '{TableName}'.");
            }

            // should prevent SQL injection
            string escapedValue = value.Replace("'", "''");

            // SQL insert command
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append($"INSERT INTO {TableName} ({targetColumnName}) VALUES ('{escapedValue}');");

            return commandBuilder.ToString();
        }


        public string GetCreateTableCommand()
        {
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append($"CREATE TABLE IF NOT EXISTS {TableName} (");

            List<string> columnDefinitions = new List<string>();
            foreach (var column in Columns)
            {
                columnDefinitions.Add(column.GetColumnDefinition());
            }

            commandBuilder.Append(string.Join(", ", columnDefinitions));
            commandBuilder.Append(");");

            return commandBuilder.ToString();
        }

    }
}
