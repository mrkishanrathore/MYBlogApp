using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using MyBlogs.Models.Contexts;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var httpPort = builder.Configuration.GetValue<int>("Kestrel:HttpPort");
var httpsPort = builder.Configuration["Kestrel:HttpsPort"];
var certPath = builder.Configuration["Kestrel:CertPath"];
var certPassword = builder.Configuration["Kestrel:CertPassword"];
var hostName = builder.Configuration["Kestrel:HostName"];

Console.WriteLine($"Read configuration - HttpPort: {httpPort}, HttpsPort: {httpsPort}, CertPath: {certPath}, CertPassword: {certPassword}, HostName: {hostName}");


builder.WebHost.ConfigureKestrel(options =>
{
    IPAddress address = string.IsNullOrEmpty(hostName)
            ? Dns.GetHostEntry("localhost").AddressList[0]
            : Dns.GetHostEntry(hostName).AddressList.Length > 0
                ? Dns.GetHostEntry(hostName).AddressList[0]
                : Dns.GetHostEntry("localhost").AddressList[0];
    Console.WriteLine($"Using address: {address}");
    options.Listen(address, httpPort);
    Console.WriteLine($"Listening on HTTP port: {httpPort}");

    if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword) && !string.IsNullOrEmpty(httpsPort))
    {
        int port = int.Parse(httpsPort);
        Console.WriteLine($"Listening on HTTPS port: {port}");
        options.Listen(address, port, listenOptions =>
        {
            var certificate = new X509Certificate2(fileName: certPath,
                                                   password: certPassword,
                                                   X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            listenOptions.UseHttps(certificate);
            Console.WriteLine($"Using HTTPS certificate from path: {certPath}");
        });
    }
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
Console.WriteLine("Added controllers with NewtonsoftJson");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
Console.WriteLine("Added Swagger");

builder.Services.AddDbContext<UserCredentialsContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("BlodDbConnection"));
});
Console.WriteLine("Added DbContext for UserCredentialsContext");

builder.Services.AddSingleton<IRestartService, RestartService>();
builder.Services.AddHostedService<CpuMonitorService>();
Console.WriteLine("Added services: RestartService, CpuMonitorService");

/*builder.Host.UseWindowsService();*/
Console.WriteLine("Configured to run as Windows Service");

var app = builder.Build();
Console.WriteLine("Built the application");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Console.WriteLine("Configured Swagger UI for Development");
}

app.UseDefaultFiles();
app.UseStaticFiles();
Console.WriteLine("Configured static files middleware");

app.UseAuthorization();
Console.WriteLine("Configured authorization middleware");

app.MapControllers();
Console.WriteLine("Mapped controllers");

app.Run();
Console.WriteLine($"Application running on HTTP port: {httpPort}, HTTPS port: {httpsPort}");
Console.WriteLine("Press Enter to exit...");
Console.ReadLine();
