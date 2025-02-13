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
