using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Configuration;
using MongoDB.Driver.Core.Misc;
using MyWebApp.Models;
using MyWebApp.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Register MongoDB services
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<ReviewService>();
builder.Services.AddSingleton<ContactService>();


// Load connection string from appsettings.json
string oracleConnectionString = builder.Configuration.GetConnectionString("OracleDb");

// Make sure IConfiguration is properly injected into controllers
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Ensure DatabaseService registration is correct
builder.Services.AddScoped<DatabaseService>(provider => {
    return new DatabaseService(oracleConnectionString);
});

builder.Services.AddScoped<CustomerClass>(provider => {
    var logger = provider.GetRequiredService<ILogger<CustomerClass>>();
    return new CustomerClass(oracleConnectionString, logger);
});

// Register CustomerService with dependency injection
builder.Services.AddScoped<CustomerService>(provider => {
    var logger = provider.GetRequiredService<ILogger<CustomerService>>();
    return new CustomerService(oracleConnectionString, logger);
});

// Ensure DatabaseService registration is correct
builder.Services.AddScoped<DatabaseService1>(provider => {
    return new DatabaseService1(oracleConnectionString);
});

// Add Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole(); // Logs will be printed in the console
    config.AddDebug();   // Add debug logging provider
});

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Enable session handling

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Enable session middleware
app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add additional route to ensure the controller is reachable:
app.MapControllerRoute(
    name: "reviews",
    pattern: "reviews/{action=Index}/{id?}",
    defaults: new { controller = "Review" });

// Initialize database on application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var databaseService = services.GetRequiredService<CustomerClass>();
        databaseService.InitializeDatabase().Wait(); // Run synchronously during startup
        Console.WriteLine("Database initialized successfully.");

        // Verify MongoDB connection
        var mongoSettings = services.GetRequiredService<IOptions<MongoDBSettings>>();
        Console.WriteLine($"MongoDB connection string: {mongoSettings.Value.ConnectionString}");
        Console.WriteLine($"MongoDB database name: {mongoSettings.Value.DatabaseName}");

        // Try to get reviews from MongoDB on startup
        var reviewService = services.GetRequiredService<ReviewService>();
        var reviews = reviewService.GetReviews().Result;
        Console.WriteLine($"Startup: Found {reviews.Count} reviews in database");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();