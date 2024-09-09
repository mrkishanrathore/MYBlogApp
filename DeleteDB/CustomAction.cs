using System;
using System.Data.SqlClient;
using WixToolset.Dtf.WindowsInstaller;

namespace DeleteDB
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult DeleteDatabase(Session session)
        {
            session.Log("Begin DeleteDatabase Custom Action.");
            // Retrieve the connection string and database name from the installer properties
            string connectionString = session["CONNECTIONSTRING"];
            string DBName = session["DBNAME"];
            session.Log(connectionString + DBName);

            if (string.IsNullOrWhiteSpace(DBName) || string.IsNullOrWhiteSpace(connectionString))
            {
                session.Log("Database name or Connection String is empty or not provided." + DBName + connectionString);
                return ActionResult.Success;
            }

            // Safely handle database names
            string formattedDBName = $"[{DBName}]";

            string checkDbExistsQuery = $@"
                IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{DBName}')
                BEGIN
                    DROP DATABASE {formattedDBName}
                END";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(checkDbExistsQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                session.Log("Database " + DBName + " deleted successfully.");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log($"Error deleting database: {ex.Message}");
                return ActionResult.Success;
            }
        }
    }
}
