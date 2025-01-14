using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Filters;
using Smartstore.Core.Logging.Serilog;
using Smartstore.Engine.Initialization;
using MsHost = Microsoft.Extensions.Hosting.Host;

namespace Smartstore.Web
{
    public class Program
    {
        private static readonly Regex _rgSystemSource = new Regex("^File|^System|^Microsoft|^Serilog|^Autofac|^Castle|^MiniProfiler|^Newtonsoft|^Pipelines|^StackExchange|^Superpower", RegexOptions.Compiled);

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true)
                .AddJsonFile("Config/connections.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"Config/connections.{EnvironmentName}.json", optional: true)
                .AddJsonFile("Config/usersettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"Config/usersettings.{EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private readonly static string EnvironmentName 
            = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        private readonly static IConfiguration Configuration 
            = BuildConfiguration();

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args);
            await host.InitAsync();
            await host.RunAsync();
        }

        public static IHost CreateHostBuilder(string[] args) 
        {
            Log.Logger = SetupSerilog(Configuration);

            return MsHost.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging(SetupLogging)
                .UseSerilog(dispose: true)
                .ConfigureWebHostDefaults(wb => wb
                    .UseStartup<Startup>(hostingContext => 
                    {
                        hostingContext.Configuration = Configuration;
                        var startupLogger = new SerilogLoggerFactory(Log.Logger).CreateLogger("File");
                        return new Startup(hostingContext, startupLogger); 
                    }))
                .Build();
        }

        private static void SetupLogging(HostBuilderContext hostingContext, ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        }

        private static Logger SetupSerilog(IConfiguration configuration)
        {
            var builder = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext();

            // Build DEBUG logger
            if (EnvironmentName == "Development")
            {
                builder.WriteTo.Debug();
            }

            builder
                // Build INSTALL logger
                .WriteTo.Conditional(Matching.FromSource("Install"), a => a.Async(logger =>
                {
                    logger.File("App_Data/Logs/install-.log",
                        //restrictedToMinimumLevel: LogEventLevel.Debug,
                        outputTemplate: "{Timestamp:G} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        fileSizeLimitBytes: 100000000,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        rollingInterval: RollingInterval.Day,
                        flushToDiskInterval: TimeSpan.FromSeconds(5));
                }))
                // Build FILE logger (also replaces the Smartstore classic "TraceLogger")
                .WriteTo.Logger(logger =>
                {
                    logger
                        .Enrich.FromLogContext()
                        // Allow only "File[/{path}]" sources
                        .Filter.ByIncludingOnly(IsFileSource)
                        // Extracts path from source and adds it as log event property name.
                        .Enrich.With<LogFilePathEnricher>()
                        .WriteTo.Map(LogFilePathEnricher.LogFilePathPropertyName, (logFilePath, wt) => 
                        {
                            wt.Async(c => c.File($"{logFilePath}",
                                //restrictedToMinimumLevel: LogEventLevel.Debug,
                                outputTemplate: "{Timestamp:G} [{Level:u3}] {Message:lj} {RequestPath} (UserId: {CustomerId}, Username: {UserName}){NewLine}{Exception}",
                                fileSizeLimitBytes: 100000000,
                                rollOnFileSizeLimit: true,
                                shared: true,
                                rollingInterval: RollingInterval.Day,
                                flushToDiskInterval: TimeSpan.FromSeconds(5)));
                        }, sinkMapCountLimit: 10);
                })
                // Build "SmartDbContext" logger
                .WriteTo.Logger(logger =>
                {
                    logger
                        .Enrich.FromLogContext()
                        // Do not allow system/3rdParty noise less than WRN level
                        .Filter.ByIncludingOnly(IsDbSource)
                        .WriteTo.DbContext(period: TimeSpan.FromSeconds(5), batchSize: 50, eagerlyEmitFirstEvent: false, queueLimit: 1000);
                }, restrictedToMinimumLevel: LogEventLevel.Information, levelSwitch: null);

            return builder.CreateLogger();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDbSource(LogEvent e)
        {
            // Allow only app logs >= INFO or system logs >= WARNING
            return e.Level >= LogEventLevel.Warning || !_rgSystemSource.IsMatch(e.GetSourceContext());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFileSource(LogEvent e)
        {
            var source = e.GetSourceContext();
            return source != null && (source.Equals("File", StringComparison.OrdinalIgnoreCase) || source.StartsWith("File/", StringComparison.OrdinalIgnoreCase));
        }
    }
}
