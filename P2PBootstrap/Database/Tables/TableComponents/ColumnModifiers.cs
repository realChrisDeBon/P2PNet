namespace P2PBootstrap.Database.Tables.TableComponents
{
    public static class ColumnModifiers
    {
        public static readonly List<string> Modifiers = new List<string>
        {
            "PRIMARY KEY",
            "FOREIGN KEY",
            "AUTOINCREMENT",
            "NOT NULL",
            "UNIQUE",
            "DEFAULT"
        };
    }
}
