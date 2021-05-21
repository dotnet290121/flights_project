using Domain.Entities;
using System;
using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IFlightDAO : IBasicDB<Flight>
    {
        Dictionary<Flight, int> GetAllFlightsVacancy();
        IList<Flight> GetFlightsByAirlineCompany(AirlineCompany airlineCompany);
        //IList<Flight> GetFlightsByOriginCountry(int countryId);
        //IList<Flight> GetFlightsByDestinationCountry(int countryId);
        //IList<Flight> GetFlightsByDepatrureDate(DateTime departureDate);
        //IList<Flight> GetFlightsByLandingDate(DateTime landingDate);
        IList<Flight> GetFlightsByCustomer(Customer customer);
        IList<Flight> Search(int originCountryId = 0, int destinationCountryId = 0, DateTime? departureDate = null, DateTime? landingDate = null);
        IDictionary<Flight,List<Ticket>> GetFlightsWithTicketsAfterLanding(long seconds);
        IList<Flight> GetFutureDepartures(int departureHoursPeriod);
    }
}
