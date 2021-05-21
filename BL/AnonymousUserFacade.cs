using BL.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;

namespace BL
{
    public class AnonymousUserFacade : FacadeBase, IAnonymousUserFacade
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AnonymousUserFacade() : base()
        {
        }

        public AirlineCompany GetAirlineCompanyById(long id)
        {
            AirlineCompany result = null;

            result = Execute(() => _airlineDAO.Get(id), new { Id = id }, _logger);

            return result;

        }

        public IList<AirlineCompany> GetAllAirlineCompanies()
        {
            IList<AirlineCompany> result = null;

            result = Execute(() => _airlineDAO.GetAll(), new { }, _logger);

            return result;

        }

        public IList<Country> GetAllCountries()
        {
            IList<Country> result = null;

            result = Execute(() => _countryDAO.GetAll(), new { }, _logger);

            return result;
        }

        public IList<Flight> GetAllFlights()
        {
            IList<Flight> result = null;

            result = Execute(() => _flightDAO.GetAll(), new { }, _logger);

            return result;
        }

        public IList<Flight> SearchFlights(int originCountryId = 0, int destinationCountryId = 0, DateTime? departureDate = null, DateTime? landingDate = null)
        {
            IList<Flight> result = null;

            result = Execute(() => _flightDAO.Search(originCountryId, destinationCountryId, departureDate, landingDate), new { }, _logger);

            return result;
        }

        public Dictionary<Flight, int> GetAllFlightsVacancy()
        {
            Dictionary<Flight, int> result = null;

            result = Execute(() => _flightDAO.GetAllFlightsVacancy(), new { }, _logger);

            return result;
        }

        public Country GetCountryById(int id)
        {
            Country result = null;

            result = Execute(() => _countryDAO.Get(id), new { Id = id }, _logger);

            return result;
        }

        public Flight GetFlightById(long id)
        {
            Flight result = null;

            result = Execute(() => _flightDAO.Get(id), new { Id = id }, _logger);

            return result;
        }

        public IList<Flight> GetFutureDepartures(int departureHoursPeriod)
        {
            IList<Flight> result = null;

            result = Execute(() => _flightDAO.GetFutureDepartures(departureHoursPeriod), new { DepartureHoursPeriod= departureHoursPeriod }, _logger);

            return result;
        }

        //public IList<Flight> GetFlightsByDepatrureDate(DateTime departureDate)
        //{
        //    IList<Flight> result = null;

        //    result = Execute(() => _flightDAO.GetFlightsByDepatrureDate(departureDate), new { DepartureDate = departureDate }, _logger);

        //    return result;
        //}

        //public IList<Flight> GetFlightsByDestinationCountry(int countryId)
        //{
        //    IList<Flight> result = null;

        //    result = Execute(() => _flightDAO.GetFlightsByDestinationCountry(countryId), new { CountryId = countryId }, _logger);

        //    return result;
        //}

        //public IList<Flight> GetFlightsByLandingDate(DateTime landingDate)
        //{
        //    IList<Flight> result = null;

        //    result = Execute(() => _flightDAO.GetFlightsByLandingDate(landingDate), new { LandingDate = landingDate }, _logger);

        //    return result;
        //}

        //public IList<Flight> GetFlightsByOriginCountry(int countryId)
        //{
        //    IList<Flight> result = null;

        //    result = Execute(() => _flightDAO.GetFlightsByOriginCountry(countryId), new { CountryId = countryId }, _logger);

        //    return result;
        //}
    }
}
