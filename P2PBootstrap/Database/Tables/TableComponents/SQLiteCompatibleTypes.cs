namespace P2PBootstrap.Database.Tables.TableComponents
{
    public static class SQLiteCompatibleTypes
        {
            private static readonly Dictionary<Type, string> TypeToSQLiteStringMap = new Dictionary<Type, string>
            {
                { typeof(bool), "BOOLEAN" },
                { typeof(int), "INTEGER" },
                { typeof(long), "INTEGER" },
                { typeof(short), "INTEGER" },
                { typeof(byte), "INTEGER" },
                { typeof(float), "REAL" },
                { typeof(double), "REAL" },
                { typeof(decimal), "REAL" },
                { typeof(string), "TEXT" },
                { typeof(byte[]), "BLOB" },
                { typeof(DBNull), "NULL" },
                { typeof(DateTime), "TIMESTAMP" }
            };

            public static void EnsureSQLiteCompatible<T>()
            {
                if (!TypeToSQLiteStringMap.ContainsKey(typeof(T)))
                {
                    throw new InvalidOperationException($"{typeof(T).Name} is not a valid SQLite type.");
                }
            }

            public static string GetSQLiteType<T>()
            {
                if (TypeToSQLiteStringMap.TryGetValue(typeof(T), out var sqliteType))
                {
                    return sqliteType;
                }
                throw new InvalidOperationException($"{typeof(T).Name} is not a valid SQLite type.");
            }
    }
    

}
