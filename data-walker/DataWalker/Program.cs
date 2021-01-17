using System.Threading.Tasks;
using DataWalker.Configurations;
using DataWalker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataWalker
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();
                    configuration
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true,
                            true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services
                        .Configure<DataWalkerOptions>(
                            hostingContext.Configuration.GetSection(nameof(DataWalkerOptions)))
                        .AddHostedService<DataWalkerService>()
                        .AddSingleton<IDataWalker, SimpleWalker>()
                        .AddSingleton<IExcelValidator, DefaultExcelValidator>()
                        .AddSingleton<ITableMappingLoader, ExcelTableMappingLoader>()
                        .AddSingleton<ITableHunter, DefaultTableHunter>()
                        .AddSingleton<ITableConverter, DefaultTableConverter>()
                        .AddSingleton<TableCombineStrategyProvider>();
                });
        }
    }
}