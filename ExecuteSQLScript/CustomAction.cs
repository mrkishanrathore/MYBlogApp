using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using WixToolset.Dtf.WindowsInstaller;

namespace ExecuteSQLScript
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ExecuteSQLScript(Session session)
        {
            string connectionString = null;
            string scriptPath = null;
            string databaseName = null;
            bool isDatabaseCreated = false;

            try
            {
                // Retrieve connection string and script path from session data
                string encodedConnectionString = session.CustomActionData["CONNECTIONSTRING"];
                connectionString = HttpUtility.UrlDecode(encodedConnectionString);
                scriptPath = session.CustomActionData["SQLSCRIPTPATH"];

                // Read the SQL script from the file
                string script = File.ReadAllText(scriptPath);

                // Split the script into individual commands by separating statements with semicolons
                string[] commands = script.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var commandText in commands)
                    {
                        // Skip empty commands
                        if (string.IsNullOrWhiteSpace(commandText))
                            continue;

                        // Ensure commands are trimmed and end with a semicolon
                        string trimmedCommandText = commandText.Trim();
                        if (!trimmedCommandText.EndsWith(";"))
                        {
                            trimmedCommandText += ";";
                        }

                        session.Log("Executing :" + trimmedCommandText);
                        // Try to extract database name if it's a CREATE DATABASE command
                        if (!string.IsNullOrEmpty(GetDatabaseName(trimmedCommandText)))
                        {
                            databaseName = GetDatabaseName(trimmedCommandText);
                            isDatabaseCreated = true;
                        }

                        // Execute the command
                        using (SqlCommand command = new SqlCommand(trimmedCommandText, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }

                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Unable to execute SQL script. Connection string: " + connectionString);
                session.Log("Script path: " + scriptPath);
                session.Log("DBNAME: " + databaseName);
                session.Log("DBNAMECreated: " + isDatabaseCreated);
                session.Log("Error: " + e.Message);

                // If a database was created before the error, attempt to delete it
                if (isDatabaseCreated && !string.IsNullOrWhiteSpace(databaseName))
                {
                    try
                    {
                        DeleteDatabase(connectionString, databaseName);
                        session.Log($"Database '{databaseName}' deleted successfully.");
                    }
                    catch (Exception ex)
                    {
                        session.Log($"Failed to delete database '{databaseName}'. Error: " + ex.Message);
                    }
                }

                return ActionResult.Failure;
            }
        }

        // Method to extract database name from a CREATE DATABASE command
        private static string GetDatabaseName(string commandText)
        {
            // Use a regular expression to match the CREATE DATABASE pattern and extract the database name
            string pattern = @"CREATE\s+DATABASE\s+(\w+)";
            var match = Regex.Match(commandText, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {

                return match.Groups[1].Value;
            }

            return null;
        }

        // Method to delete the database if an error occurs
        private static void DeleteDatabase(string connectionString, string databaseName)
        {
            string dropDbQuery = $"DROP DATABASE [{databaseName}]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(dropDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
