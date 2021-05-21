using AutoMapper;
using BL;
using BL.Exceptions;
using BL.Interfaces;
using BL.LoginService;
using DAL.Exceptions;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CustomerFacadeController : LoggedInControllerBase<Customer>
    {
        private readonly IFlightCenterSystem _flightCenterSystem;
        private readonly IMapper _mapper;
        private readonly ILoggedInCustomerFacade _loggedInCustomerFacade;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger<CustomerFacadeController> _logger;

        public CustomerFacadeController(IFlightCenterSystem flightCenterSystem, IMapper mapper, LinkGenerator linkGenerator, ILogger<CustomerFacadeController> logger)
        {
            _flightCenterSystem = flightCenterSystem;
            _mapper = mapper;
            _loggedInCustomerFacade = _flightCenterSystem.GetFacade<LoggedInCustomerFacade>();
            _linkGenerator = linkGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Get list of all the flights belonging to the logged-in customer
        /// </summary>
        /// <returns>List of all the flights</returns>
        /// <response code="200">Returns the list of flights</response>
        /// <response code="204">If the list of flights is empty</response> 
        /// <response code="401">If the user is not authenticated as customer</response> 
        [HttpGet("Customer/Flights")]
        public ActionResult<FlightDetailsDTO> GetAllMyFlights()
        {
            LoginToken<Customer> customer_token = DesirializeToken();

            IList<Flight> flights = _loggedInCustomerFacade.GetAllMyFlights(customer_token);
            if (flights.Count == 0)
                return NoContent();

            List<FlightDetailsDTO> flightDetailsDTOs = new List<FlightDetailsDTO>();
            foreach (var flight in flights)
                flightDetailsDTOs.Add(_mapper.Map<FlightDetailsDTO>(flight));

            return Ok(flightDetailsDTOs);
        }

        /// <summary>
        /// Get list of all the tickets belonging to the logged-in customer
        /// </summary>
        /// <returns>List of all the tickets</returns>
        /// <response code="200">Returns the list of tickets</response>
        /// <response code="204">If the list of tickets is empty</response> 
        /// <response code="401">If the user is not authenticated as customer</response> 
        [HttpGet("Customer/Tickets")]
        public ActionResult<TicketDetailsDTO> GetAllMyTickets()
        {
            LoginToken<Customer> customer_token = DesirializeToken();

            IList<Ticket> tickets = _loggedInCustomerFacade.GetAllMyTickets(customer_token);
            if (tickets.Count == 0)
                return NoContent();

            List<TicketDetailsDTO> ticketDetailsDTOs = new List<TicketDetailsDTO>();
            foreach (var ticket in tickets)
                ticketDetailsDTOs.Add(_mapper.Map<TicketDetailsDTO>(ticket));

            return Ok(tickets);
        }

        /// <summary>
        /// Get specific ticket by id
        /// </summary>
        /// <returns>Ticket</returns>
        /// <param name="id">The id of the ticket that you wish to get</param>
        /// <response code="200">Returns desired ticket</response>
        /// <response code="401">If the user is not authenticated as customer</response> 
        /// <response code="403">If the ticket belong to another customer</response> 
        /// <response code="404">If the desired ticket is not found</response> 
        [HttpGet("Tickets/{id}")]
        public ActionResult<TicketDetailsDTO> GetTicketById(long id)
        {
            LoginToken<Customer> customer_token = DesirializeToken();
            Ticket ticket = null;
            try
            {
                ticket = _loggedInCustomerFacade.GetTicketById(customer_token, id);
                if (ticket == null)
                    return NotFound();
            }
            catch (WrongCustomerException)
            {
                return Forbid();
            }

            TicketDetailsDTO ticketDetailsDTO = _mapper.Map<TicketDetailsDTO>(ticket);

            return Ok(ticketDetailsDTO);
        }

        /// <summary>
        /// Create new ticket
        /// </summary>
        /// <returns>Ticket that has been created</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /
        ///     {
        ///         "flight_id": 1
        ///     }
        ///
        /// </remarks>  
        /// <param name="flight_id">The id of the flight that the ticket belongs to</param>
        /// <response code="201">The ticket has been created successfully</response>
        /// <response code="401">If the user is not authenticated as customer</response> 
        /// <response code="404">If the flight_id doesn't point to existing flight</response> 
        /// <response code="409">If there is another ticket that belongs to the same user</response> 
        /// <response code="410">If there flight departured or will departure within 15 minutes. If there are no tockets left </response> 
        [HttpPost("Tickets")]
        public ActionResult<TicketDetailsDTO> PurchaseTicket([FromBody] long flight_id)
        {
            LoginToken<Customer> customer_token = DesirializeToken();
            Ticket ticket = null;
            string uri = null;

            Flight flight = _loggedInCustomerFacade.GetFlightById(flight_id);
            if (flight == null)
                return NotFound();

            if (flight.RemainingTickets < 1 || flight.DepartureTime <= DateTime.Now.AddMinutes(15))
                return StatusCode(StatusCodes.Status410Gone);

            try
            {
                ticket = _loggedInCustomerFacade.PurchaseTicket(customer_token, flight);

                uri = _linkGenerator.GetPathByAction(nameof(GetTicketById), "customerfacade", new { id = ticket.Id });

            }
            catch (RecordAlreadyExistsException)
            {
                return Conflict();
            }
            catch (TicketPurchaseFailedException)
            {
                return StatusCode(StatusCodes.Status410Gone);

            }

            TicketDetailsDTO ticketDetailsDTO = _mapper.Map<TicketDetailsDTO>(ticket);

            return Created(uri, ticket);
        }

        /// <summary>
        /// Removes ticket
        /// </summary>
        /// <param name="id">The id of the ticket that has to be removed</param>
        /// <response code="204">The ticket has been removed successfully</response>
        /// <response code="401">If the user is not authenticated as customer</response> 
        /// <response code="403">If the ticket belong to another customer. Or if the flight already departured</response> 
        /// <response code="404">If the ticket that has to be removed not exist</response>
        [HttpDelete("Tickets/{id}")]
        public IActionResult CancelTicket(long id)
        {
            LoginToken<Customer> customer_token = DesirializeToken();

            try
            {
                Ticket ticket = _loggedInCustomerFacade.GetTicketById(customer_token, id);
                if (ticket == null)
                    return NotFound();
                if (ticket.Flight.DepartureTime<=DateTime.Now.AddMinutes(-15))
                    return Forbid();

                ticket.Customer.User = customer_token.User.User;

                _loggedInCustomerFacade.CancelTicket(customer_token, ticket);
            }
            catch (WrongCustomerException)
            {
                return Forbid();
            }

            return NoContent();
        }
    }
}
