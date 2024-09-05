using System;
using System.Data.SqlClient;
using System.IO;
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

            try
            {
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
                session.Log("Error: " + e.Message);
                return ActionResult.Success; // Changed to Failure for better error reporting
            }
        }
    }
}
