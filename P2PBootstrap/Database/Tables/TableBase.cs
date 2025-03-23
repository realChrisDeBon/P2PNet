using P2PBootstrap.Database.Tables.TableComponents;
using System.Text;

namespace P2PBootstrap.Database.Tables
{
    public enum EqualityCheck
    {
        EqualTo,
        NotEqualTo
    }


    public class TableBase
    {
        public string TableName { get; set; }
        List<IColumn> Columns = new List<IColumn>();

        public void AddColumn(IColumn column)
        {
            Columns.Add(column);
        }

        /// <summary>
        /// Generates an SQL insert command to insert a single value into a target column.
        /// </summary>
        /// <param name="value">The value to be inserted into the target column.</param>
        /// <param name="targetColumn">The name of the column to insert the value into.</param>
        /// <returns>A string representing the SQL insert command.</returns>
        /// <exception cref="ArgumentException">Thrown if the target column does not exist in the table.</exception>
        public string RunInsertCommand(string value, string targetColumn)
        {
            var targetColumn_ = Columns.FirstOrDefault(c => c.ColumnName == targetColumn);
            if (targetColumn_ == null)
            {
                throw new ArgumentException($"Column '{targetColumn}' does not exist in the table '{TableName}'.");
            }

            // should prevent SQL injection
            string escapedValue = value.Replace("'", "''");

            // SQL insert command
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append($"INSERT INTO {TableName} ({targetColumn}) VALUES ('{escapedValue}');");

            return commandBuilder.ToString();
        }
        /// <summary>
        /// Generates an SQL insert command to insert multiple values into multiple target columns.
        /// </summary>
        /// <param name="values">A dictionary where the keys are the column names and the values are the corresponding values to be inserted.</param>
        /// <returns>A string representing the SQL insert command.</returns>
        /// <exception cref="ArgumentException">Thrown if any of the target columns do not exist in the table.</exception>
        public string RunInsertCommand(Dictionary<string, string> values)
        {
            // ensure all columns exist
            foreach (var column in values.Keys)
            {
                var targetColumn = Columns.FirstOrDefault(c => c.ColumnName == column);
                if (targetColumn == null)
                {
                    throw new ArgumentException($"Column '{column}' does not exist in the table '{TableName}'.");
                }
            }

            // escape single quotes in the values to prevent SQL injection
            var escapedValues = values.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Replace("'", "''")
            );

            // dynamically build the SQL INSERT command
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append($"INSERT INTO {TableName} (");
            commandBuilder.Append(string.Join(", ", escapedValues.Keys));
            commandBuilder.Append(") VALUES (");
            commandBuilder.Append(string.Join(", ", escapedValues.Values.Select(v => $"'{v}'")));
            commandBuilder.Append(");");

            return commandBuilder.ToString();
        }

        /// <summary>
        /// Generates an SQL update command to modify a value in the target column based on a condition involving another column and an equality check.
        /// </summary>
        /// <param name="tableName">The name of the table to update.</param>
        /// <param name="targetColumn">The name of the column to modify.</param>
        /// <param name="checkColumn">The name of the column to check the value against.</param>
        /// <param name="value">The value to set in the target column and to check against in the check column.</param>
        /// <param name="equalityCheck">The type of equality check to perform (EqualTo or NotEqualTo).</param>
        /// <returns>A string representing the SQL update command.</returns>
        /// <exception cref="ArgumentException">Thrown if the target column or check column does not exist in the table.</exception>
        public string RunUpdateCommand(string tableName, string targetColumn, string checkColumn, string value, EqualityCheck equalityCheck)
        {
            var targetColumn_ = Columns.FirstOrDefault(c => c.ColumnName == targetColumn);
            if (targetColumn_ == null)
            {
                throw new ArgumentException($"Column '{targetColumn}' does not exist in the table '{TableName}'.");
            }

            var checkColumn_ = Columns.FirstOrDefault(c => c.ColumnName == checkColumn);
            if (checkColumn_ == null)
            {
                throw new ArgumentException($"Column '{checkColumn}' does not exist in the table '{TableName}'.");
            }

            // Escape single quotes in the value to prevent SQL injection
            string escapedValue = value.Replace("'", "''");

            // Determine the equality operator
            string equalityOperator = equalityCheck == EqualityCheck.EqualTo ? "=" : "!=";

            // Build the SQL update command
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append($"UPDATE {tableName} SET {targetColumn} = '{escapedValue}' WHERE {checkColumn} {equalityOperator} '{escapedValue}';");

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
