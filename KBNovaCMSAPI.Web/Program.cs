using GTDC.Model;
using KBNovaCMS.Common;
using KBNovaCMS.Common.Enums;
using KBNovaCMS.CustomMiddleWare;
using KBNovaCMS.Model;
using KBNovaCMSAPI.Web.CustomMiddleWare;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // ------------------------------
            // Core Framework Registrations
            // ------------------------------
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
    
            // Load the application configuration from appsettings.json
            IConfigurationRoot configuration = LoadConfiguration();

            // Configure the database connection string using the environment-specific configuration
            ConfigureDatabaseConnection(configuration);

            // Add application-level services
            AddServices(builder);

            // Build the application
            var app = builder.Build();

            // Initialize service manager
            WebUtility.InitializeServiceManager(app.Services);

            // ------------------------------
            // Middleware Pipeline
            // ------------------------------

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            // Force HTTPS
            app.UseHttpsRedirection();

            // Enable static files (if serving docs or Swagger assets)
            app.UseStaticFiles();

            // Add caching for static files
            ConfigureStaticFileCaching(app);

            // Enable response compression
            app.UseResponseCompression();

            // Custom security and logging middlewares
            app.UseAntiXssMiddleware();
            app.UseMiddleware<SqlInjectionPreventionMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<EncryptionMiddleware>();
            
            app.UseRouting();  
            app.UseAuthorization();  
            app.UseCors("CorsPolicy");
            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapAreaControllerRoute(
                        name: "CMS",
                        areaName: "CMS",
                        pattern: "CMS/{controller=Home}/{action=Index}/{id?}");
                    endpoints.MapAreaControllerRoute(
                        name: "Web",
                        areaName: "Web",
                        pattern: "Web/{controller=Home}/{action=Index}/{id?}");
               });

    await app.RunAsync();
        }
        catch (Exception e)
        {
            await NLogger.LogAsync(NLogType.Error, "Error in Program.cs", e);
        }

    #region Helper  
     static IConfigurationRoot LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

     static void ConfigureDatabaseConnection(IConfiguration configuration)
       {
            MDbConnection.IsDevlopment = true;
            MDbConnection.IsDB_Postgresql = true;

            // Set connection string based on environment (Development, Staging, Production)
            MDbConnection.ConnectionString = MDbConnection.IsDevlopment
                ? configuration.GetConnectionString("ApplicationDataBaseConnection_Devlopment") ?? string.Empty
                    : configuration.GetConnectionString("ApplicationDataBaseConnection_Production") ?? string.Empty;
        }

     static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();

        // Configure Kestrel limits
        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = long.MaxValue;
        });

        // CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        // Response caching & compression
        builder.Services.AddResponseCaching();
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        // Register HttpClient & Context accessor
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

        // Configure controllers and JSON
        builder.Services.AddControllers(options =>
        {
            options.CacheProfiles.Add("Default", new Microsoft.AspNetCore.Mvc.CacheProfile
            {
                Duration = 0,
                Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any,
                NoStore = true
            });
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        });

        // Custom service registration from KBNovaCMS.Common
        WebUtility.ConfigureServices(builder.Services);
    }

     static void ConfigureStaticFileCaching(WebApplication app)
{
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? string.Empty; // Convert PathString to string safely

        if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers.Append("Cache-Control", "public,max-age=604800"); // 7 days
        }
        else
        {
            context.Response.Headers.Append("Cache-Control", "no-cache, private");
        }

        await next();
    });
}

#endregion