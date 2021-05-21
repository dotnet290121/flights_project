using ConfigurationService;
using DAL;
using Domain.Entities;
using Domain.Interfaces;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("Log4Net.config"));

            FlightsManagmentSystemConfig.Instance.Init("FlightsManagmentSystem.Config.json");

            IFlightDAO flightDAO = new FlightDAOPGSQL();
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 1, 3, DateTime.Now.AddHours(5),DateTime.Now.AddHours(9),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 2, 3, DateTime.Now.AddHours(20),DateTime.Now.AddHours(30),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 2, 3, DateTime.Now.AddHours(10),DateTime.Now.AddHours(16),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 3, 1, DateTime.Now.AddHours(2),DateTime.Now.AddHours(9),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 2, 2, DateTime.Now.AddHours(3),DateTime.Now.AddHours(7),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 1, 3, DateTime.Now.AddHours(25),DateTime.Now.AddHours(29),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 1, 1, DateTime.Now.AddHours(15),DateTime.Now.AddHours(19),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 2, 1, DateTime.Now.AddHours(11),DateTime.Now.AddHours(13),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 3, 3, DateTime.Now.AddHours(7),DateTime.Now.AddHours(10),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 2, 2, DateTime.Now.AddHours(1),DateTime.Now.AddHours(4),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 1, 3, DateTime.Now.AddHours(3),DateTime.Now.AddHours(14),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 3, 1, DateTime.Now.AddHours(8),DateTime.Now.AddHours(20),10));
            flightDAO.Add(new Flight(new AirlineCompany { Id = 1 }, 2, 1, DateTime.Now.AddHours(50),DateTime.Now.AddHours(60),10));
        }
    }
}
