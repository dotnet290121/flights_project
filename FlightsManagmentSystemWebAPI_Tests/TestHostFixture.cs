using System;
using System.Net.Http;
using BL;
using ConfigurationService;
using FlightsManagmentSystemWebAPI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlightsManagmentSystemWebAPI_Tests
{
    /// <summary>
    /// Initializes the webHost
    /// </summary>
    public class TestHostFixture : IDisposable
    {
        public HttpClient Client { get; }//Http client used to send requests to the contoller
        public IServiceProvider ServiceProvider { get; }//Service provider used to provide services that registered in the API

        public TestHostFixture()
        {
            var builder = Program.CreateHostBuilder(null)
                .ConfigureWebHost(webHost =>//Configure the web host to use test server and test environament
                {
                    webHost.UseTestServer();
                    webHost.UseEnvironment("Test");
                })
                //.ConfigureServices((hostContext, services) =>
                //{
                //    FlightsManagmentSystemConfig.Instance.Init("FlightsManagmentSystemTests.Config.json");

                //    services.AddSingleton(FlightsManagmentSystemConfig.Instance);
                //    services.AddSingleton<IFlightCenterSystem>(FlightCenterSystem.GetInstance());
                //})
                ;
            var host = builder.Start();//Start the host
            ServiceProvider = host.Services;//Get the services from the host
            Client = host.GetTestClient();//Get the test client from the host
            //Console.WriteLine("TEST Host Started.");
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
