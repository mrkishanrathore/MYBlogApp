using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq; // Add Newtonsoft.Json via NuGet

try
{
    // Get the directory where the MyBlogsRunner.exe is located
    string currentDirectory = @"C:\Program Files\MyBlogs";

    // Build the path to MyBlogs.exe and appsettings.json
    string myBlogsExePath = Path.Combine(currentDirectory, "MyBlogs.exe");
    string appSettingsPath = Path.Combine(currentDirectory, "appsettings.json");

    // Check if MyBlogs.exe and appsettings.json exist
    if (File.Exists(myBlogsExePath) && File.Exists(appSettingsPath))
    {
        Console.WriteLine("Starting MyBlogs.exe...");

        // Read the appsettings.json file
        string json = File.ReadAllText(appSettingsPath);
        var settings = JObject.Parse(json);

        // Extract Kestrel settings
        var kestrel = settings["Kestrel"];
        int httpPort = kestrel["HttpPort"].ToObject<int>();
        int httpsPort = kestrel["HttpsPort"].ToObject<int>();
        string hostName = kestrel["HostName"].ToString();

        if (string.IsNullOrEmpty(hostName))
        {
            hostName = "localhost";
        }

        // Start MyBlogs.exe
        Process.Start(new ProcessStartInfo
        {
            FileName = myBlogsExePath,
            UseShellExecute = true // UseShellExecute must be true to run the process with a graphical user interface
        });

        // Display the URLs
        Console.WriteLine("MyBlogs.exe started successfully.");
        Console.WriteLine($"Application is running on:");
        Console.WriteLine($"HTTP: http://{hostName}:{httpPort}/index.html");
        Console.WriteLine($"HTTPS: https://{hostName}/index.html");

        // Prevent the console from closing automatically
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
    else
    {
        Console.WriteLine($"Error: MyBlogs.exe or appsettings.json not found in {currentDirectory}");
    }
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred while trying to start MyBlogs.exe:");
    Console.WriteLine(ex.Message);
}

