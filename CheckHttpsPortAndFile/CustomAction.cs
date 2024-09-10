using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
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
                string certPass = session["CERTPASSWORD"];

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

                if (!(ValidateSSLCertificate(certPath, certPass) == ""))
                {
                    session["VALIDATIONRESULT2"] = ValidateSSLCertificate(certPath, certPass);
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

        public static string ValidateSSLCertificate(string filePath, string password)
        {
            try
            {
                // Attempt to load the certificate using the provided file path and password
                X509Certificate2 certificate = new X509Certificate2(filePath, password);

                // Check if the certificate is valid and has a private key
                if (certificate.HasPrivateKey)
                {
                    return "";
                }
                else
                {
                    return "The certificate is valid but does not contain a private key.";
                }
            }
            catch (CryptographicException ex)
            {
                // This exception is thrown if the password is incorrect or the file is invalid
                if (ex.Message.Contains("The specified network password is not correct"))
                {
                    return "The password provided for the certificate is incorrect.";
                }
                else
                {
                    return "The file provided is not a valid SSL certificate.";
                }
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that may occur
                return $"An error occurred: {ex.Message}";
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
