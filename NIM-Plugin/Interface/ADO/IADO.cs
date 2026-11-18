using System.Collections.Generic;

namespace NIM_Plugin.ADO
{
    /// <summary>
    /// Executes a SQL query and returns the result rows.
    /// </summary>
    public delegate List<Dictionary<string, object>> DB_ExecuteQuery(string SQL);

    /// <summary>
    /// Executes a SQL command and returns the number of affected rows.
    /// </summary>
    public delegate int DB_ExecuteNonQuery(string SQL);

    /// <summary>
    /// Executes a SQL query and returns the first column of the first row.
    /// </summary>
    public delegate object DB_ExecuteScalar(string SQL);


    /// <summary>
    /// Provides access to the database of the host engine.
    /// The plugin must receive the three database method delegates provided by the engine
    /// before it can perform database operations.
    /// </summary>
    public interface I_Database
    {
        /// <summary>
        /// Receives the three database method delegates from the host engine.
        /// </summary>
        /// <param name="ExecuteQuery">Executes a SQL query and returns the result rows.</param>
        /// <param name="ExecuteNonQuery">Executes a SQL command and returns the number of affected rows.</param>
        /// <param name="ExecuteScalar">Executes a SQL query and returns the first column of the first row.</param>
        /// <returns>True if the delegates are successfully bound; otherwise, false.</returns>
        bool Bind(
            DB_ExecuteQuery ExecuteQuery,
            DB_ExecuteNonQuery ExecuteNonQuery,
            DB_ExecuteScalar ExecuteScalar
        );

        /// <summary>
        /// Notifies the plugin that database delegate binding has been completed.
        /// The plugin can now use the database methods provided by the host engine.
        /// </summary>
        void OnBound();
    }
}