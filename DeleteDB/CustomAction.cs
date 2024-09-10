using System;
using System.Data.SqlClient;
using System.Web;
using WixToolset.Dtf.WindowsInstaller;

namespace DeleteDB
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult DeleteDatabase(Session session)
        {
            session.Log("Begin DeleteDatabase Custom Action.");

            // Retrieve the connection string and database name from session properties or CustomActionData
            string connectionString = null;
            string DBName = null;

            try
            {
                connectionString = session["CONNECTIONSTRING"];
                DBName = session["DBNAME"];
                session.Log($"Using session properties: CONNECTIONSTRING={connectionString}, DBNAME={DBName}");
                // Try to get from session properties first
            
            }
            catch (Exception ex)
            {
                session.Log("Error retrieving properties: " + ex.Message);
            }
            
            

            if (string.IsNullOrWhiteSpace(DBName) || string.IsNullOrWhiteSpace(connectionString))
            {
                try
                {
                    // If session properties are not available, get from CustomActionData
                    session.Log("Session properties not set, falling back to CustomActionData.");
                    connectionString = session.CustomActionData["CONNECTIONSTRING"];
                    connectionString = HttpUtility.UrlDecode(connectionString);
                    DBName = session.CustomActionData["DBNAME"];
                    session.Log($"Using CustomActionData: CONNECTIONSTRING={connectionString}, DBNAME={DBName}");

                }
                catch (Exception ex)
                {
                    session.Log("Error retrieving properties: " + ex.Message);
                    session.Log("Database name or Connection String is empty or not provided: " +
                            $"DBName={DBName}, ConnectionString={connectionString}");
                    return ActionResult.Failure;
                }
            }

            // Safely handle database names
            string formattedDBName = $"[{DBName}]";

            // Query to check if the database exists and drop it if it does
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
