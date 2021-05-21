using AutoMapper;
using BL;
using BL.Exceptions;
using BL.Interfaces;
using BL.LoginService;
using DAL.Exceptions;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FlightsManagmentSystemWebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize(Roles = "Airline_Company")]
    public class AirlineCompanyFacadeController : LoggedInControllerBase<AirlineCompany>
    {
        private readonly IFlightCenterSystem _flightCenterSystem;
        private readonly IMapper _mapper;
        private readonly ILoggedInAirlineFacade _loggedInAirlineFacade;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger<AirlineCompanyFacadeController> _logger;

        public AirlineCompanyFacadeController(IFlightCenterSystem flightCenterSystem, IMapper mapper, LinkGenerator linkGenerator, ILogger<AirlineCompanyFacadeController> logger)
        {
            _flightCenterSystem = flightCenterSystem;
            _mapper = mapper;
            _loggedInAirlineFacade = _flightCenterSystem.GetFacade<LoggedInAirlineFacade>();
            _linkGenerator = linkGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Get list of all the tickets belonging to the logged-in airline company
        /// </summary>
        /// <returns>List of all the tickets</returns>
        /// <response code="200">Returns the list of tickets</response>
        /// <response code="204">If the list of tickets is empty</response> 
        /// <response code="401">If the user is not authenticated as airline company</response> 
        [HttpGet("Airline-Company/Tickets")]
        public ActionResult<IList<TicketDetailsDTO>> GetAllTickets()
        {
            LoginToken<AirlineCompany> airline_token = DesirializeToken();

            IList<Ticket> tickets = _loggedInAirlineFacade.GetAllTickets(airline_token);
            if (tickets.Count == 0)
                return NoContent();

            List<TicketDetailsDTO> ticketDetailsDTOs = new List<TicketDetailsDTO>();
            foreach (var ticket in tickets)
                ticketDetailsDTOs.Add(_mapper.Map<TicketDetailsDTO>(ticket));

            return Ok(ticketDetailsDTOs);
        }

        /// <summary>
        /// Get list of all the tickets belonging to the logged-in airline company
        /// </summary>
        /// <returns>List of all the tickets</returns>
        /// <param name="flight_id">The id of the flight</param>
        /// <response code="200">Returns the list of tickets</response>
        /// <response code="204">If the list of tickets is empty</response> 
        /// <response code="401">If the user is not authenticated as airline company</response> 
        /// <response code="404">If the flight is not found</response> 
        [HttpGet("Airline-Company/Flights/{flight_id}/Tickets")]
        public ActionResult<IList<TicketDetailsDTO>> GetAllTicketsByFlight(long flight_id)
        {
            LoginToken<AirlineCompany> airline_token = DesirializeToken();
            IList<Ticket> tickets;

            Flight flight = _loggedInAirlineFacade.GetFlightById(flight_id);
            if (flight == null)
                return NotFound();

            tickets = _loggedInAirlineFacade.GetAllTicketsByFlight(airline_token, flight);
            if (tickets.Count == 0)
                return NoContent();


            List<TicketDetailsDTO> ticketDetailsDTOs = new List<TicketDetailsDTO>();
            foreach (var ticket in tickets)
                ticketDetailsDTOs.Add(_mapper.Map<TicketDetailsDTO>(ticket));

            return Ok(ticketDetailsDTOs);
        }

        /// <summary>
        /// Get list of all the flights belonging to the logged-in airline company
        /// </summary>
        /// <returns>List of all the flights</returns>
        /// <response code="200">Returns the list of flights</response>
        /// <response code="204">If the list of flights is empty</response> 
        /// <response code="401">If the user is not authenticated as airline company</response> 
        [HttpGet("Airline-Company/Flights")]
        public ActionResult<IList<FlightDetailsDTO>> GetAllFlights()
        {
            LoginToken<AirlineCompany> airline_token = DesirializeToken();

            IList<Flight> flights = _loggedInAirlineFacade.GetAllFlights(airline_token);
            if (flights.Count == 0)
                return NoContent();

            List<FlightDetailsDTO> flightsDetailsDTOs = new List<FlightDetailsDTO>();
            foreach (var flight in flights)
                flightsDetailsDTOs.Add(_mapper.Map<FlightDetailsDTO>(flight));

            return Ok(flightsDetailsDTOs);
        }

        /// <summary>
        /// Cancel flight and associated tickets
        /// </summary>
        /// <param name="id">The id of the flight that has to be removed</param>
        /// <response code="204">The flight has been removed successfully</response>
        /// <response code="401">If the user is not authenticated as airline company</response> 
        /// <response code="403">If the flight doesn't belong to the logged-in airline company</response>
        /// <response code="404">If the flight that has to be removed not exist</response>
        [HttpDelete("Flights/{id}")]
        public IActionResult CancelFlight(long id)
        {
            LoginToken<AirlineCompany> airline_token = DesirializeToken();

            Flight flight = _loggedInAirlineFacade.GetFlightById(id);
            if (flight == null)
                return NotFound();

            try
            {
                _loggedInAirlineFacade.CancelFlight(airline_token, flight);
            }
            catch (NotAllowedAirlineActionException)
            {
                return Forbid();
            }

            return NoContent();
        }

        /// <summary>
        /// Create new flight
        /// </summary>
        /// <returns>Airline company that has been created</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /CreateFlightDTO
        ///     {  
        ///         "originCountryId": 1,
        ///         "destinationCountryId": 1,
        ///         "departureTime": 2021-10-10 18:00:00,
        ///         "landingTime": 2021-10-10 21:00:00,
        ///         "remainingTickets": 10,
        ///     }
        /// </remarks>  
        /// <param name="createFlightDTO">DTO that holds the data for creating flight</param>
        /// <response code="201">The flight has been created successfully</response>
        /// <response code="401">If the user is not authenticated as airline company</response> 
        /// <response code="404">If the country id doesn't point to existing country</response> 
        [HttpPost("Flights")]
        public ActionResult<Flight> CreateFlight(CreateFlightDTO createFlightDTO)
        {
            LoginToken<AirlineCompany> airline_token = DesirializeToken();

            Flight flight = _mapper.Map<Flight>(createFlightDTO);

            string uri = null;

            try
            {
                flight.Id = _loggedInAirlineFacade.CreateFlight(airline_token, flight);

                uri = _linkGenerator.GetPathByAction(nameof(AnonymousFacadeController.GetFlightById), "AnonymousFacade", new { id = flight.Id });
            }
            catch (RelatedRecordNotExistsException)
            {
                return NotFound();
            }

            return Created(uri, flight);
        }

        /// <summary>
        /// Update existing flight details
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /UpdateFlightDTO
        ///     {
        ///         "id": 1,
        ///         "originCountryId": 1,
        ///         "destinationCountryId": 1,
        ///         "departureTime": 2021-10-10 18:00:00,
        ///         "landingTime": 2021-10-10 21:00:00,
        ///         "remainingTickets": 10,
        ///     }
        /// </remarks>  
        /// <param name="id">The id of the flight that will be updated</param>
        /// <param name="updateFlightDTO">DTO that holds the data for updating flight</param>
        /// <response code="204">The flight has been updated successfully</response>
        /// <response code="400">If the airline company id is different between the url and the body</response> 
        /// <response code="401">If the user is not authenticated as airline company</response> 
        /// <response code="403">If the flight doesn't belong to the logged-in airline company</response>
        /// <response code="404">If the flight has not been found or the country id doesn't point to existing country</response> 
        [HttpPut("Flights/{id}")]
        public IActionResult UpdateFlight(long id, UpdateFlightDTO updateFlightDTO)
        {
            LoginToken<AirlineCompany> airline_token = DesirializeToken();

            if (id == 0)
                return NotFound();

            if (id != updateFlightDTO.Id)
                return BadRequest();

            Flight flight = _mapper.Map<Flight>(updateFlightDTO);

            Flight original_flight = _loggedInAirlineFacade.GetFlightById(flight.Id);

            if (original_flight == null)
                return NotFound();

            if (original_flight.AirlineCompany != airline_token.User)
                return Forbid();

            flight.AirlineCompany = airline_token.User;

            try
            {
                _loggedInAirlineFacade.UpdateFlight(airline_token, flight);
            }
            catch (NotAllowedAirlineActionException)//might be irrelevant, we are checking it here also
            {
                return Forbid();
            }
            catch (RelatedRecordNotExistsException)
            {
                return NotFound();
            }

            return NoContent();
        }


        /// <summary>
        /// Update the logged-in airline company details
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /updateAirlineCompanyDTO
        ///     {
        ///         "id": 1,
        ///         "name": "Arkia",
        ///         "countryId": 1,
        ///     }
        /// </remarks>  
        /// <param name="updateAirlineCompanyDTO">DTO that holds the data for updating the airline company</param>
        /// <response code="204">The airline company has been updated successfully</response>
        /// <response code="401">If the user is not authenticated as airline company</response> 
        /// <response code="403">If the id of the logged-in airline company is different then the id in the request DTO</response>
        /// <response code="404">If the country id doesn't point to existing country</response> 
        [HttpPut("Airline-Companies")]
        public IActionResult MofidyAirlineDetails(UpdateAirlineCompanyDTO updateAirlineCompanyDTO)
        {
            LoginToken<AirlineCompany> airline_token = DesirializeToken();

            if (airline_token.User.Id != updateAirlineCompanyDTO.Id)
                return Forbid();

            AirlineCompany airline = _mapper.Map<AirlineCompany>(updateAirlineCompanyDTO);

            try
            {
                _loggedInAirlineFacade.MofidyAirlineDetails(airline_token, airline);
            }
            catch (NotAllowedAirlineActionException)
            {
                return Forbid();
            }
            catch (RelatedRecordNotExistsException)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Get specific flight history by id
        /// </summary>
        /// <returns>Flight History</returns>
        /// <param name="id">The id of the flight history that you wish to get</param>
        /// <response code="200">Returns desired flight history</response>
        /// <response code="401">If the user is not authenticated as airline company</response> 
        /// <response code="403">If the flight history doesn't belong to the logged-in airline company</response>
        /// <response code="404">If the desired flight history is not found</response> 
        [HttpGet("history/flights/{id}")]
        public ActionResult<FlightHistory> GetFlightHistoryByOriginalId(long id)
        {
            LoginToken<AirlineCompany> airline_token = DesirializeToken();
            FlightHistory flightHistory = null;

            try
            {
                flightHistory = _loggedInAirlineFacade.GetFlightHistoryByOriginalId(airline_token, id);
                if (flightHistory == null)
                    return NotFound();

                if (flightHistory.AirlineCompanyId != airline_token.User.Id)
                    return Forbid();

            }
            catch (NotAllowedAirlineActionException)
            {
                return Forbid();
            }

            return Ok(flightHistory);
        }
    }
}