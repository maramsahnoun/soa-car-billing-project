using Microsoft.EntityFrameworkCore;
using SoapCore;
// InMemory provider methods are available via Microsoft.EntityFrameworkCore
// (the package provides the implementation). Do not import a non-existent namespace.
using MaintenanceService.Services;

var builder = WebApplication.CreateBuilder(args);

// Load connection string (env var preferred)
var connString = builder.Configuration.GetConnectionString("maintenance_db")
                 ?? Environment.GetEnvironmentVariable("MAINTENANCE_CONN");

// DbContext - Use InMemory for testing if database unavailable
builder.Services.AddDbContext<MaintenanceDbContext>(options =>
{
    if (string.IsNullOrEmpty(connString))
    {
        options.UseInMemoryDatabase("maintenance_db");
    }
    else
    {
        try
        {
            options.UseMySql(connString, ServerVersion.AutoDetect(connString));
        }
        catch
        {
            options.UseInMemoryDatabase("maintenance_db");
        }
    }
});

// Register SOAP service implementation
builder.Services.AddScoped<IMaintenanceService, MaintenanceServiceImpl>();

// HttpClient for Car-Service
builder.Services.AddHttpClient("carservice", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["CarService:BaseUrl"]
        ?? Environment.GetEnvironmentVariable("CAR_SERVICE_BASEURL")
        ?? "http://localhost:3000"
    );
});

// SOAP support
builder.Services.AddSoapCore();
builder.Services.AddControllers();

var app = builder.Build();

// Developer exception page for debugging
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// CORRECT ORDER: UseRouting() MUST come BEFORE UseEndpoints()
app.UseRouting();

// SOAP endpoint registration
app.UseEndpoints(endpoints =>
{
    // Register SOAP endpoint
    endpoints.UseSoapEndpoint<IMaintenanceService>(
        "/ws/maintenance",
        new SoapEncoderOptions(),
        SoapSerializer.DataContractSerializer
    );

    // Map controllers
    endpoints.MapControllers();
});

app.Run();
