using System;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using WixToolset.Dtf.WindowsInstaller;

namespace UpdateAppSettings
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult UpdateAppSettings(Session session)
        {
            try
            {
                // Retrieve installation directory and other settings
                string installDir = session.CustomActionData["INSTALLDIR"];
                string portNumber = session.CustomActionData["PORTNUMBER"];
                string connectionString = session.CustomActionData["CONNECTIONSTRING"];

                /*string installDir = "D:\\inetpub\\wwwroot\\MyBlogs";
                string portNumber = "8080";
                string connectionString = "Demo string";*/

                if (string.IsNullOrEmpty(installDir) || string.IsNullOrEmpty(portNumber) || string.IsNullOrEmpty(connectionString))
                {
                    session.Log("Error: One or more required properties are missing."+installDir+ " "+ portNumber +" "+ connectionString);
                    return ActionResult.Failure;
                }

                string appSettingsPath = Path.Combine(installDir, "appsettings.json");

                if (!File.Exists(appSettingsPath))
                {
                    session.Log($"Error: appsettings.json file not found at {appSettingsPath}.");
                    return ActionResult.Success;
                }

                // Read the JSON file
                string json = File.ReadAllText(appSettingsPath);
                var jsonObject = JObject.Parse(json);

                // Update the JSON object
                if (jsonObject["ConnectionStrings"] != null)
                {
                    foreach (var connString in jsonObject["ConnectionStrings"])
                    {
                        connString.First.Replace(connectionString);
                    }
                }

                if (jsonObject["PortNumber"] != null)
                {
                    jsonObject["PortNumber"] = portNumber;
                }

                // Write the updated JSON back to the file
                File.WriteAllText(appSettingsPath, jsonObject.ToString());

                session.Log("Successfully updated appsettings.json.");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("Error: " + ex.Message);
                return ActionResult.Failure;
            }
        }
    }
}
