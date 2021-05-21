using AutoMapper;
using BL;
using BL.Interfaces;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Configuration;
using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FlightsManagmentSystemWebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class AnonymousFacadeController : ControllerBase
    {
        private readonly IFlightCenterSystem _flightCenterSystem;
        private readonly IMapper _mapper;
        private readonly IAnonymousUserFacade _anonymousUserFacade;
        private readonly ILogger<AnonymousFacadeController> _logger;
        private readonly DeparturesAndLandingConfig _departuresAndLandingConfig;

        public AnonymousFacadeController(IFlightCenterSystem flightCenterSystem, IMapper mapper, ILogger<AnonymousFacadeController> logger, DeparturesAndLandingConfig departuresAndLandingConfig)
        {
            _flightCenterSystem = flightCenterSystem;
            _mapper = mapper;
            _anonymousUserFacade = _flightCenterSystem.GetFacade<AnonymousUserFacade>();
            _logger = logger;
            _departuresAndLandingConfig = departuresAndLandingConfig;
        }

        [HttpGet("Flights/Departures")]
        public ActionResult<IList<FlightDetailsDTO>> GetFutureDepartures()
        {
            IList<Flight> flights = _anonymousUserFacade.GetFutureDepartures(_departuresAndLandingConfig.DepartureHoursPeriod);
            if (flights.Count == 0)
                return NoContent();

            List<FlightDetailsDTO> flightDetailsDTOs = new List<FlightDetailsDTO>();

            foreach (var flight in flights)
                flightDetailsDTOs.Add(_mapper.Map<FlightDetailsDTO>(flight));

            return Ok(flightDetailsDTOs);
        }

        /// <summary>
        /// Get list of all the flights
        /// </summary>
        /// <returns>List of all the flights</returns>
        /// <response code="200">Returns the list of all flights</response>
        /// <response code="204">If the list of flights is empty</response> 
        [HttpGet("Flights")]
        public ActionResult<IList<FlightDetailsDTO>> GetAllFlights()
        {
            IList<Flight> flights = _anonymousUserFacade.GetAllFlights();
            if (flights.Count == 0)
                return NoContent();

            List<FlightDetailsDTO> flightDetailsDTOs = new List<FlightDetailsDTO>();
            foreach (var flight in flights)
                flightDetailsDTOs.Add(_mapper.Map<FlightDetailsDTO>(flight));

            return Ok(flightDetailsDTOs);
        }

        /// <summary>
        /// Get list of all the flights by search parameters
        /// </summary>
        /// <returns>List of all the flights that answering the parameters</returns>
        /// <param name="originCountryId">The id of the origin country</param>
        /// <param name="detinationCountryId">The id of the destination country</param>
        /// <param name="departureDate">The deprature date of the flight</param>
        /// <param name="landingDate">The landing date of the flight</param>
        /// <response code="200">Returns the list of the flights</response>
        /// <response code="204">If the list of the flights is empty</response> 
        [HttpGet("Flights/Search")]
        public ActionResult<IList<FlightDetailsDTO>> SearchFlights([FromQuery] int originCountryId, [FromQuery] int detinationCountryId, [FromQuery] DateTime? departureDate, [FromQuery] DateTime? landingDate)
        {
            IList<Flight> flights = _anonymousUserFacade.SearchFlights(originCountryId, detinationCountryId, departureDate, landingDate);
            if (flights.Count == 0)
                return NoContent();

            List<FlightDetailsDTO> flightDetailsDTOs = new List<FlightDetailsDTO>();
            foreach (var flight in flights)
                flightDetailsDTOs.Add(_mapper.Map<FlightDetailsDTO>(flight));

            return Ok(flightDetailsDTOs);
        }

        /// <summary>
        /// Get list of all the airline companies
        /// </summary>
        /// <returns>List of all the airline companies</returns>
        /// <response code="200">Returns the list of all airline companies</response>
        /// <response code="204">If the list of airline companies is empty</response> 
        [HttpGet("Airline-Companies")]
        public ActionResult<IList<AirlineCompanyDetailsDTO>> GetAllAirlineCompanies()
        {
            IList<AirlineCompany> airlineCompanies = _anonymousUserFacade.GetAllAirlineCompanies();
            if (airlineCompanies.Count == 0)
                return NoContent();

            List<AirlineCompanyDetailsDTO> airlineCompanyDetailsDTOs = new List<AirlineCompanyDetailsDTO>();
            foreach (var airlineCompany in airlineCompanies)
                airlineCompanyDetailsDTOs.Add(_mapper.Map<AirlineCompanyDetailsDTO>(airlineCompany));

            return Ok(airlineCompanyDetailsDTOs);
        }

        /// <summary>
        /// Get dictionary of all the flights and remaining tickets
        /// </summary>
        /// <returns>Dictionary of all the flights and remaining tickets</returns>
        /// <response code="200">Returns the dictionary of the flights and remaining tickets</response>
        /// <response code="204">If the dictionary of the flights and remaining tickets is empty</response> 
        [HttpGet("Flights-Vacancy")]
        public ActionResult<IDictionary<FlightDetailsDTO, int>> GetAllFlightsVacancy()
        {
            IDictionary<Flight, int> flights_vacancy = _anonymousUserFacade.GetAllFlightsVacancy();
            if (flights_vacancy.Count == 0)
                return NoContent();

            IDictionary<FlightDetailsDTO, int> flights_vacancy_DTO = new Dictionary<FlightDetailsDTO, int>();
            foreach (var flight in flights_vacancy.Keys)
                flights_vacancy_DTO.Add(_mapper.Map<FlightDetailsDTO>(flight), flights_vacancy[flight]);

            return Ok(flights_vacancy_DTO);
        }

        /// <summary>
        /// Get list of all the countries
        /// </summary>
        /// <returns>List of all the countries</returns>
        /// <response code="200">Returns the list of all countries</response>
        /// <response code="204">If the list of countries is empty</response> 
        [HttpGet("Countries")]
        public ActionResult<IList<Country>> GetAllCountries()
        {
            IList<Country> countries = _anonymousUserFacade.GetAllCountries();
            if (countries.Count == 0)
                return NoContent();

            return Ok(countries);
        }

        /// <summary>
        /// Get specific airline company by id
        /// </summary>
        /// <returns>Airline Company</returns>
        /// <param name="id">The id of the airline company that you wish to get</param>
        /// <response code="200">Returns desired airline company</response>
        /// <response code="404">If the desired airline company is not found</response> 
        [HttpGet("Airline-Companies/{id?}")]
        public ActionResult<AirlineCompanyDetailsDTO> GetAirlineCompanyById(long id)
        {
            AirlineCompany airlineCompany = _anonymousUserFacade.GetAirlineCompanyById(id);
            if (airlineCompany == null)
                return NotFound();

            AirlineCompanyDetailsDTO airlineCompanyDetailsDTO = _mapper.Map<AirlineCompanyDetailsDTO>(airlineCompany);

            return Ok(airlineCompanyDetailsDTO);
        }

        /// <summary>
        /// Get specific flight by id
        /// </summary>
        /// <returns>Flight</returns>
        /// <param name="id">The id of the flight that you wish to get</param>
        /// <response code="200">Returns desired flight</response>
        /// <response code="404">If the desired flight is not found</response> 
        [HttpGet("Flights/{id?}")]
        public ActionResult<FlightDetailsDTO> GetFlightById(long id)
        {
            Flight flight = _anonymousUserFacade.GetFlightById(id);
            if (flight == null)
                return NotFound();

            FlightDetailsDTO flightDetailsDTO = _mapper.Map<FlightDetailsDTO>(flight);

            return Ok(flightDetailsDTO);
        }

        /// <summary>
        /// Get specific country by id
        /// </summary>
        /// <returns>Country</returns>
        /// <param name="id">The id of the country that you wish to get</param>
        /// <response code="200">Returns desired country</response>
        /// <response code="404">If the desired country is not found</response> 
        [HttpGet("Countries/{id?}")]
        public ActionResult<Country> GetCountryById(int id)
        {
            Country country = _anonymousUserFacade.GetCountryById(id);
            if (country == null)
                return NotFound();

            return Ok(country);
        }

        //[HttpGet("GetFlightsByOriginCountry")]
        //public ActionResult<IList<Flight>> GetFlightsByOriginCountry([FromQuery]int countryId)
        //{
        //    IList<Flight> flights = _anonymousUserFacade.GetFlightsByOriginCountry(countryId);
        //    if (flights.Count == 0)
        //        return NoContent();

        //    return Ok(flights);
        //}

        //[HttpGet("GetFlightsByDestinationCountry")]
        //public ActionResult<IList<Flight>> GetFlightsByDestinationCountry(int countryId)
        //{
        //    IList<Flight> flights = _anonymousUserFacade.GetFlightsByDestinationCountry(countryId);
        //    if (flights.Count == 0)
        //        return NoContent();

        //    return Ok(flights);
        //}

        //[HttpGet("GetFlightsByDepatrureDate")]
        //public ActionResult<IList<Flight>> GetFlightsByDepatrureDate(DateTime departureDate)
        //{
        //    IList<Flight> flights = _anonymousUserFacade.GetFlightsByDepatrureDate(departureDate);
        //    if (flights.Count == 0)
        //        return NoContent();

        //    return Ok(flights);
        //}

        //[HttpGet("GetFlightsByLandingDate")]
        //public ActionResult<IList<Flight>> GetFlightsByLandingDate(DateTime landingDate)
        //{
        //    IList<Flight> flights = _anonymousUserFacade.GetFlightsByLandingDate(landingDate);
        //    if (flights.Count == 0)
        //        return NoContent();

        //    return Ok(flights);
        //}
    }
}
