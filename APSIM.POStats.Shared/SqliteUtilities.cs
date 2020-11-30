using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace APSIM.POStats.Shared
{
    public class SqliteUtilities
    {
        /// <summary>Create a new table</summary>
        public static void CreateTable(SqliteConnection database, DataTable table)
        {
            StringBuilder sql = new StringBuilder();
            var columnNames = new List<string>();
            foreach (DataColumn column in table.Columns)
            {
                columnNames.Add(column.ColumnName);
                if (sql.Length > 0)
                    sql.Append(',');

                sql.Append("\"");
                sql.Append(column.ColumnName);
                sql.Append("\" ");
                if (column.DataType == null)
                    sql.Append("integer");
                else
                    sql.Append(GetDBDataTypeName(column.DataType));
            }

            sql.Insert(0, "CREATE TABLE [" + table.TableName + "] (");
            sql.Append(')');

            using (var command = database.CreateCommand())
            {
                command.CommandText = sql.ToString();
                command.ExecuteNonQuery();
            }

            using (var command = database.CreateCommand())
            {
                command.CommandText = CreateInsertSQL(table.TableName, columnNames);

                List<object[]> rowValues = new List<object[]>();
                foreach (DataRow row in table.Rows)
                {
                    command.Parameters.Clear();
                    for (int i = 0; i < table.Columns.Count; i++)
                        command.Parameters.AddWithValue(i.ToString(), row.ItemArray[i]);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <returns></returns>
        public static string CreateInsertSQL(string tableName, IList<string> columnNames)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO [");
            sql.Append(tableName);
            sql.Append("](");

            for (int i = 0; i < columnNames.Count; i++)
            {
                string columnName = columnNames[i];
                if (i > 0)
                    sql.Append(',');
                sql.Append('[');
                sql.Append(columnName);
                sql.Append(']');
            }
            sql.Append(") VALUES (");

            for (int i = 0; i < columnNames.Count; i++)
            {
                if (i > 0)
                    sql.Append(',');
                sql.Append("$");
                sql.Append(i);
            }

            sql.Append(')');

            return sql.ToString();
        }

        /// <summary>Convert .NET type into an SQLite type</summary>
        public static string GetDBDataTypeName(Type type)
        {
            if (type == null)
                return "integer";
            else if (type.ToString() == "System.DateTime")
                return "date";
            else if (type.ToString() == "System.Int32")
                return "integer";
            else if (type.ToString() == "System.Single")
                return "real";
            else if (type.ToString() == "System.Double")
                return "real";
            else if (type.ToString() == "System.Boolean")
                return "integer";
            else
                return "text";
        }

        /// <summary>Get an enumerable collection of table names from an Sqlite connection.</summary>
        /// <param name="connection">The Sqlite connection.</param>
        public static IEnumerable<string> GetTableNames(SqliteConnection connection)
        {
            var tableNames = new List<string>();
            using (var command = new SqliteCommand("SELECT * FROM sqlite_master", connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        tableNames.Add(reader[1].ToString());
            return tableNames;
        }

        /// <summary>Get an enumerable collection of table names from an Sqlite connection.</summary>
        /// <param name="connection">The Sqlite connection.</param>
        public static IEnumerable<string> GetColumnNames(SqliteConnection connection, string tableName)
        {
            List<string> columnNames = new List<string>();

            string sql = $"PRAGMA table_info({tableName})";
            using (var command = new SqliteCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                DataTable result = new DataTable();
                result.Load(reader);
                return result.AsEnumerable().Select(r => r["name"].ToString()).ToList();
            }
        }
    }
}
