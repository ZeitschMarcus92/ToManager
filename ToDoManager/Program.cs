using ITTitans.ToDoManager.Data;
using ITTitans.ToDoManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.ToDoManager;

internal static class Program
{
    private static readonly Action<ILogger, Exception?> s_dbInitFailed =
        LoggerMessage.Define(LogLevel.Error, new EventId(3001, "DbInitFailed"), "Failed to initialize database");

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Ensure correct configuration precedence:
        // 1) appsettings.json
        // 2) appsettings.{Environment}.json (overrides appsettings.json)
        // 3) Environment variables (highest priority)
        var env = builder.Environment;
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddApplicationInsights(
                configureTelemetryConfiguration: (config) =>
                {
                    string? applicationInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
                    if (string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
                    {
                        applicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
                    }

                    if (applicationInsightsConnectionString is not null)
                    {
                        config.ConnectionString = applicationInsightsConnectionString;
                    }
                },
                configureApplicationInsightsLoggerOptions: (_) =>
                {
                });
        });
        
        builder.Logging.AddApplicationInsights();

        // Determine storage based on connection string
        string? connectionString = builder.Configuration.GetConnectionString("ToDoDatabase");
        bool usePostgres = !string.IsNullOrWhiteSpace(connectionString);

        if (usePostgres)
        {
            builder.Services.AddDbContext<TodoDbContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddScoped<ITodoService, PostgresTodoService>();
            builder.Services.AddSingleton<IStorageInfo>(new StorageInfo("Postgres"));
        }
        else
        {
            builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();
            builder.Services.AddSingleton<IStorageInfo>(new StorageInfo("In-Memory"));
        }

        WebApplication app = builder.Build();

        // When Postgres is configured, ensure the database and schema exist.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetService<TodoDbContext>();
            if (db != null)
            {
                try
                {
                    // If there are migrations, this applies them; otherwise it will create the schema.
                    // Using EnsureCreated because no compiled migrations are present in this project.
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    s_dbInitFailed(app.Logger, ex);
                    throw;
                }
            }
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
