using System;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Net;
using WixToolset.Dtf.WindowsInstaller;

namespace CheckPortAndConnection
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ValidatePortAndConnection(Session session)
        {
            string portNumber = session["PORTNUMBER"];
            string connectionString = session["CONNECTIONSTRING"];

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

            session["VALIDATIONRESULT"] = validationResult;
            session.Log(validationResult);
            return ActionResult.Success;
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
