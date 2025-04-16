using Intranet.Api.Middleware;
using Intranet.Application.Interfaces;
using Intranet.Application.Mappings; // Needed if using AutoMapper registration
using Intranet.Application.Services;
using Intranet.Domain.Interfaces;
using Intranet.Infrastructure.Persistence;
using Intranet.Infrastructure.Persistence.Repositories;
using Serilog; // Add Serilog usings

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Read from appsettings.json Serilog section if present
    .Enrich.FromLogContext()
    .WriteTo.Console()
    // Add file sink (adjust path and rolling interval as needed)
    .WriteTo.File("Logs/intranet_log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Use Serilog for logging throughout the application
builder.Host.UseSerilog();

// Add services to the container.

// Register Global Exception Handler Middleware
builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

// Register Application Services and Repositories
// Use AddScoped for services/repositories that use resources like DbContext or connections per request
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>(); // Assuming you created these
builder.Services.AddScoped<IEmployeeService, EmployeeService>();     // Assuming you created these

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>(); // Assuming you created these
builder.Services.AddScoped<IDepartmentService, DepartmentService>();     // Assuming you created these

// Add registrations for the legacy common service and DAL
builder.Services.AddScoped<ICommonDAL, CommonDAL>();
builder.Services.AddScoped<ICommonService, CommonService>();

// Register AutoMapper (Uncomment if using AutoMapper)
// builder.Services.AddAutoMapper(typeof(ManualMapping).Assembly); // Tell AutoMapper to scan the Application assembly for profiles

// *** Register AutoMapper ***
// Scans the assembly containing MappingProfile (i.e., Intranet.Application) for all Profiles.
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




var app = builder.Build();

// Configure the HTTP request pipeline.

// Use the custom Global Exception Handler Middleware EARLY in the pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Log request details in development for easier debugging
    app.UseSerilogRequestLogging();
}

app.UseDefaultFiles();  // These two lines are for using default.html
app.UseStaticFiles();


app.UseHttpsRedirection();

// Minimal API routing (if you had any) would go here or below UseRouting

app.UseAuthorization(); // Add if you implement authorization later

// This sets up routing for controllers
app.MapControllers();

// Run the application
try
{
    Log.Information("Starting Intranet API web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Intranet API Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // Ensure logs are written before shutdown
}

















//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
