using System;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Net;
using WixToolset.Dtf.WindowsInstaller;
using System.Web.NBitcoin;
using System.IO;

namespace CheckPortAndConnection
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ValidatePortAndConnection(Session session)
        {
            try
            {
                string portNumber = session["HTTPPORT"];
                string connectionString = session["CONNECTIONSTRING"];
                string scriptPath = session["SQLSCRIPTPATH"];
                string dbName = ExtractDatabaseNameFromScript();
                string validationResult = "Validation successful!";

                // Validate Port Number
                if (!IsPortAvailable(int.Parse(portNumber)))
                {
                    validationResult = "Port number is already in use.";
                    session["VALIDATIONRESULT"] = validationResult;
                    session.Log(validationResult);
                    return ActionResult.Success;
                }

                // Validate Connection String
                if (!IsConnectionStringValid(connectionString))
                {
                    validationResult = "Invalid connection string.";
                    session["VALIDATIONRESULT"] = validationResult;
                    session.Log(validationResult);
                    return ActionResult.Success;
                }

                session.Log("Checking For Existing Database " + dbName);
                session["VALIDATIONRESULT"] = CheckDBExists(connectionString, dbName);
                session["DBNAME"] = dbName;
                session["ENCODEDCONNECTIONSTRING"] = HttpUtility.UrlEncode(connectionString);

                // setting exit msg
                string hostname = session["HOSTNAME"];
                if (string.IsNullOrEmpty(hostname) || hostname == "example.io")
                {
                    hostname = "localhost";
                }
                session["WIXUI_EXITDIALOGOPTIONALTEXT"] = "Visit to Site : http://" + hostname + ":" + session["HTTPPORT"] + "/index.html";

                session.Log(validationResult);
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session["VALIDATIONRESULT"] = ex.Message;
                return ActionResult.Success;
            }
            
        }

        private static string CheckDBExists(string connStr, string dbName)
        {
            try
            {

                if (dbName == null)
                {
                    return "Unable to determine the database name from the script.";
                }

                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    connection.Open();

                    // Check if the database exists
                    var checkDbCmdText = $"SELECT database_id FROM sys.databases WHERE Name = '{dbName}'";
                    using (var checkDbCmd = new SqlCommand(checkDbCmdText, connection))
                    {
                        var dbId = checkDbCmd.ExecuteScalar();
                        if (dbId != null)
                        {
                            // Database already exists
                            return $"{dbName} already exists. Please rename or delete it to move forward.";
                        }
                    }
                }

                // If database does not exist, validation is successful
                return "Validation successful!";
            }
            catch (Exception e)
            {
                // Handle and return error message
                return $"Error: {e.Message}";
            }
        }

        private static string ExtractDatabaseNameFromScript()
        {
            // Get the directory of the executing assembly
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Build the full path to the SQL script
            string scriptPath = Path.Combine(baseDirectory, "SQL_Script.sql");

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("The SQL script file could not be found.", scriptPath);
            }

            string script = File.ReadAllText(scriptPath);

            // A simple example of extracting the database name from a CREATE DATABASE statement
            // This assumes the script contains a statement like: CREATE DATABASE [DatabaseName]
            string pattern = @"CREATE\s+DATABASE\s+(\w+)";
            var match = System.Text.RegularExpressions.Regex.Match(script, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success)
            {

                return match.Groups[1].Value;
            }

            return null;
        }



        private static bool IsPortAvailable(int port)
        {
            bool isAvailable = true;

            // Check if the port is available
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.Stop();
            }
            catch (SocketException)
            {
                isAvailable = false;
            }

            return isAvailable;
        }

        private static bool IsConnectionStringValid(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    connection.Close();
                }
                return true;
            }  
            catch (Exception)
            {
                return false;
            }
        }
    }
}
