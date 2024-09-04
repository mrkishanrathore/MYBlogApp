
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

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string script = File.ReadAllText(scriptPath);
                    using (SqlCommand command = new SqlCommand(script, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Unable to get Connection string and path "+ connectionString + scriptPath);
                session.Log(e.Message);
                return ActionResult.Success;
            }
            
        }
    }
}
