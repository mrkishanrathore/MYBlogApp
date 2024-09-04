using System;
using System.Diagnostics;
using System.IO;
using System.Web.NBitcoin;
using System.Linq;
using WixToolset.Dtf.WindowsInstaller;
using Newtonsoft.Json.Linq;

namespace UpdateAppSettings
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult UpdateAppSettings(Session session)
        {
            try
            {
                // Retrieve properties
                string installDir = session.CustomActionData["INSTALLDIR"];
                string httpPort = session.CustomActionData["HTTPPORT"];
                string httpsPort = session.CustomActionData["HTTPSPORT"];
                string encodedConnectionString = session.CustomActionData["CONNECTIONSTRING"];
                string connectionString = HttpUtility.UrlDecode(encodedConnectionString); // Decode the connection string
                string certPath = session.CustomActionData["CERTPATH"];
                string certPassword = session.CustomActionData["CERTPASSWORD"];
                string hostName = session.CustomActionData["HOSTNAME"];
                string enableHttps = session.CustomActionData["ENABLEHTTPS"];

                if (hostName == "example.io")
                {
                    hostName = "";
                }

                if (string.IsNullOrEmpty(installDir) ||
                    string.IsNullOrEmpty(httpPort) ||
                    string.IsNullOrEmpty(httpsPort) ||
                    string.IsNullOrEmpty(connectionString) ||
                    string.IsNullOrEmpty(certPath) ||
                    string.IsNullOrEmpty(certPassword))
                {
                    session.Log("Error: One or more required properties are missing.");
                    return ActionResult.Failure;
                }

                string appSettingsPath = Path.Combine(installDir, "appsettings.json");
                if (!File.Exists(appSettingsPath))
                {
                    session.Log($"Error: appsettings.json file not found at {appSettingsPath}.");
                    return ActionResult.Failure;
                }

                // Read the JSON file
                string json = File.ReadAllText(appSettingsPath);
                var jsonObject = JObject.Parse(json);

                // Update the JSON object
                if (jsonObject["ConnectionStrings"] != null)
                {
                    jsonObject["ConnectionStrings"]["BlodDbConnection"] = connectionString;
                }

                if (jsonObject["Kestrel"] != null)
                {
                    if (enableHttps == "1")
                    {
                        jsonObject["Kestrel"]["HttpsPort"] = int.Parse(httpsPort);
                        jsonObject["Kestrel"]["CertPath"] = certPath;
                        jsonObject["Kestrel"]["CertPassword"] = certPassword;
                    }
                    else
                    {
                        jsonObject["Kestrel"]["HttpsPort"] = "";
                        jsonObject["Kestrel"]["CertPath"] = "";
                        jsonObject["Kestrel"]["CertPassword"] = "";
                    }
                    jsonObject["Kestrel"]["HttpPort"] = int.Parse(httpPort);
                    jsonObject["Kestrel"]["HostName"] = hostName;
                }

                // Write the updated JSON back to the file
                File.WriteAllText(appSettingsPath, jsonObject.ToString());

                session.Log("Successfully updated appsettings.json.");

                

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("Error: " + ex.Message);
                return ActionResult.Success;
            }
        }


    }
}
