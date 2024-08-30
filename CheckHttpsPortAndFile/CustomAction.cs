using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using WixToolset.Dtf.WindowsInstaller;

namespace CheckHttpsPortAndFile
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CheckHttpsPort_CertFile(Session session)
        {
            try
            {
                // Retrieve the properties
                string httpsPort = session["HTTPSPORT"];
                string certPath = session["CERTPATH"];

                // Check if HTTPSPORT is a valid port and if it is free
                if (!int.TryParse(httpsPort, out int port) || !IsPortAvailable(port))
                {
                    session["VALIDATIONRESULT2"] = "Port is not available"; // Port is not available
                    return ActionResult.Success;
                }

                // Check if the file at CERTPATH exists
                if (!File.Exists(certPath))
                {
                    session["VALIDATIONRESULT2"] = "File does not exist"; // File does not exist
                    return ActionResult.Success;
                }

                // setting exit msg
                string hostname = session["HOSTNAME"];
                if (string.IsNullOrEmpty(hostname) || hostname=="example.io")
                {
                    hostname = "localhost";
                }
                session["WIXUI_EXITDIALOGOPTIONALTEXT"] = "Visit to Site : https://" + hostname + "/index.html or http://" + hostname + ":" + session["HTTPPORT"] + "/index.html";

                // Both checks passed
                session["VALIDATIONRESULT2"] = "success";
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("Error in CheckPortAndFile: " + ex.Message);
                session["VALIDATIONRESULT2"] = "0"; // Set to failure
                return ActionResult.Success;
            }
        }

        private static bool IsPortAvailable(int port)
        {
            bool isAvailable = true;

            TcpListener tcpListener = null;
            try
            {
                tcpListener = new TcpListener(System.Net.IPAddress.Loopback, port);
                tcpListener.Start();
            }
            catch (SocketException)
            {
                isAvailable = false;
            }
            finally
            {
                tcpListener?.Stop();
            }

            return isAvailable;
        }
    }
}
