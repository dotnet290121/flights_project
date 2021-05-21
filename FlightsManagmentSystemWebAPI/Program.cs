using BL;
using ConfigurationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlightsManagmentSystemWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices((hostContext, services) =>
                {
                    if (hostContext.HostingEnvironment.EnvironmentName == "Test")
                        FlightsManagmentSystemConfig.Instance.Init("FlightsManagmentSystemTests.Config.json");
                    else
                        FlightsManagmentSystemConfig.Instance.Init();

                    services.AddSingleton(FlightsManagmentSystemConfig.Instance);
                    services.AddSingleton<IFlightCenterSystem>(FlightCenterSystem.GetInstance());
                })
                .ConfigureLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Debug);
                    builder.AddLog4Net("Log4Net.config");
                });
    }
}
