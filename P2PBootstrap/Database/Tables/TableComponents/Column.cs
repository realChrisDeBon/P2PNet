using Org.BouncyCastle.Asn1.X509.Qualified;

namespace P2PBootstrap.Database.Tables.TableComponents
{
    public interface IColumn
    {
        public List<string> Modifiers { get; set; }
        string GetColumnDefinition();
        void ValidateType();
        string ColumnName { get; set; }
    }

    public interface IColumn<T> : IColumn
    {
    }

    public class Column<T> : IColumn<T>
    {
        public List<string> Modifiers { get; set; }
        private string foreignKeyConstraint;
        public string ColumnName { get; set; }

        public Column(string columnName)
        {
            Modifiers = new List<string>();
            ValidateType();
            ColumnName = columnName;
        }
        public Column(ColumnOptions options, string columnName)
        {
            Modifiers = new List<string>();
            ApplyOptions(options);
            ColumnName = columnName;
            ValidateType();
        }

        public void ValidateType()
        {
            SQLiteCompatibleTypes.EnsureSQLiteCompatible<T>();
        }

        public string GetColumnDefinition()
        {
            List<string> columnDefinition = new List<string>
            {
                ColumnName,
                SQLiteCompatibleTypes.GetSQLiteType<T>()
            };

            // Add modifiers in the correct order
            columnDefinition.AddRange(Modifiers);

            return string.Join(" ", columnDefinition);
        }

        private void ApplyOptions(ColumnOptions options)
        {
            if (options.IsPrimaryKey)
            {
                Modifiers.Add("PRIMARY KEY");
            }
            if (options.IsForeignKey && !string.IsNullOrEmpty(options.ForeignKeyReferences))
            {
                foreignKeyConstraint = $"FOREIGN KEY ({options.ForeignKey}) REFERENCES {options.ForeignKeyReferences}";
            }
            if (options.Autoincrement)
            {
                Modifiers.Add("AUTOINCREMENT");
            }
            if (options.NotNull)
            {
                Modifiers.Add("NOT NULL");
            }
            if (options.Unique)
            {
                Modifiers.Add("UNIQUE");
            }
            if (options.HasDefault && options.DefaultValue != null)
            {
                Modifiers.Add($"DEFAULT {options.DefaultValue}");
            }
        }

    }

    public class ColumnOptions
    {
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsForeignKey { get; set; } = false;
        public string ForeignKey { get; set; } = null;
        public string ForeignKeyReferences { get; set; } = null;
        public bool HasDefault { get; set; } = false;
        public object DefaultValue { get; set; } = null;
        public bool Autoincrement { get; set; } = false;
        public bool NotNull { get; set; } = false;
        public bool Unique { get; set; } = false;
    }

}
