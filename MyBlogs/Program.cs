using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using MyBlogs.Models.Contexts;
using System.Net;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Read ports and certificate details from configuration
var httpPort = builder.Configuration.GetValue<int>("Kestrel:HttpPort");
var httpsPort = builder.Configuration["Kestrel:HttpsPort"];
var certPath = builder.Configuration["Kestrel:CertPath"];
var certPassword = builder.Configuration["Kestrel:CertPassword"];
var hostName = builder.Configuration["Kestrel:HostName"];

builder.WebHost.ConfigureKestrel(options =>
{
    IPAddress address = string.IsNullOrEmpty(hostName)
            ? Dns.GetHostEntry("localhost").AddressList[0]
            : Dns.GetHostEntry(hostName).AddressList.Length > 0
                ? Dns.GetHostEntry(hostName).AddressList[0]
                : Dns.GetHostEntry("localhost").AddressList[0];
    options.Listen(address, httpPort);
    if(!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword) && !string.IsNullOrEmpty(httpsPort))
    {
        int port = int.Parse(httpsPort);
        options.Listen(address, port, listenOptions =>
        {
            var certificate = new X509Certificate2(fileName: certPath,
                                                   password: certPassword,
                                                   X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            listenOptions.UseHttps(certificate);
        });
    }
    
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserCredentialsContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("BlodDbConnection"));
});

builder.Host.UseWindowsService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

/*app.UseHttpsRedirection();*/

app.UseAuthorization();

app.MapControllers();

app.Run();
