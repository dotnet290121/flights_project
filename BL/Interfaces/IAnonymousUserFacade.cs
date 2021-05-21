using Domain.Entities;
using System;
using System.Collections.Generic;

namespace BL.Interfaces
{
    public interface IAnonymousUserFacade
    {
        IList<Flight> GetAllFlights();
        IList<Country> GetAllCountries();
        IList<AirlineCompany> GetAllAirlineCompanies();
        Dictionary<Flight, int> GetAllFlightsVacancy();
        Flight GetFlightById(long id);
        Country GetCountryById(int id);
        AirlineCompany GetAirlineCompanyById(long id);
        //IList<Flight> GetFlightsByOriginCountry(int countryId);
        //IList<Flight> GetFlightsByDestinationCountry(int countryId);
        //IList<Flight> GetFlightsByDepatrureDate(DateTime departureDate);
        //IList<Flight> GetFlightsByLandingDate(DateTime landingDate);
        IList<Flight> SearchFlights(int originCountryId = 0, int destinationCountryId = 0, DateTime? departureDate = null, DateTime? landingDate = null);
        IList<Flight> GetFutureDepartures(int departureHoursPeriod);
    }
}
