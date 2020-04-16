using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Service.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DbCommand"/>.
    /// </summary>
    public static class DbExtensions
    {
        /// <summary>
        /// Add a named parameter to the DbCommand.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="value">Value of the paramter.</param>
        public static void AddParamWithValue(this DbCommand command, string name, object value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Create a DbCommand with the specified query text.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="commandText">Command text (SQL).</param>
        public static DbCommand CreateCommand(this DbConnection connection, string commandText)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            return command;
        }
    }
}