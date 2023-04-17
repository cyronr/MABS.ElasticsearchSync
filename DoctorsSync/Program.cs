using DoctorsSync.Database;
using DoctorsSync.Database.DataAccess;
using DoctorsSync.Elasticsearch;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using Serilog;
using Topshelf;
using Host = Microsoft.Extensions.Hosting.Host;

namespace DoctorsSync
{
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            var config = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Logger.Information("Application Starting");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IDoctorRepository, DoctorRepository>();
                    services.AddSingleton<IElasticsearchClient, ElasticsearchClient>();

                    services.AddTransient<DoctorsSyncService>();
                    services.AddDbContext<DataContext>();
                    services.AddSingleton<IElasticClient>(sp =>
                    {
                        var pool = new SingleNodeConnectionPool(new Uri(config.GetValue<string>("Elasticsearch:Server")));
                        var settings = new ConnectionSettings(pool)
                            .EnableApiVersioningHeader();

                        return new ElasticClient(settings);
                    });
                })
                .UseSerilog(Log.Logger)
                .Build();

            var service = ActivatorUtilities.CreateInstance<DoctorsSyncService>(host.Services);
            Run(service, config);
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            var assembly = typeof(Program).Assembly;

            builder.SetBasePath(assembly.Location.Replace(assembly.ManifestModule.Name, string.Empty))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }

        static void Run(DoctorsSyncService svc, IConfigurationRoot config)
        {
            try
            {
                var exitCode = HostFactory.Run(x =>
                {
                    x.Service<DoctorsSyncService>(s =>
                    {
                        s.ConstructUsing(service => svc);
                        s.WhenStarted(service => service.Start());
                        s.WhenStopped(service => service.Stop());
                    });

                    x.SetServiceName(config.GetValue<string>("AppSettings:SerivceName"));
                    x.SetDisplayName(config.GetValue<string>("AppSettings:ServiceDisplayName"));
                    x.SetDescription(config.GetValue<string>("AppSettings:SerivceDesc"));

                });

                int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
                Environment.ExitCode = exitCodeValue;
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal("Exception occured: {exMessage}", ex.Message);
            }
        }
    }
}