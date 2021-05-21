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
using System.Collections.Generic;

namespace FlightsManagmentSystemWebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class AdministratorFacadeController : LoggedInControllerBase<Administrator>
    {
        private readonly IFlightCenterSystem _flightCenterSystem;
        private readonly IMapper _mapper;
        private readonly ILoggedInAdministratorFacade _loggedInAdministratorFacade;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger<AdministratorFacadeController> _logger;

        public AdministratorFacadeController(IFlightCenterSystem flightCenterSystem, IMapper mapper, LinkGenerator linkGenerator, ILogger<AdministratorFacadeController> logger)
        {
            _flightCenterSystem = flightCenterSystem;
            _mapper = mapper;
            _loggedInAdministratorFacade = _flightCenterSystem.GetFacade<LoggedInAdministratorFacade>();
            _linkGenerator = linkGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Get list of all the customers
        /// </summary>
        /// <returns>List of all the customers</returns>
        /// <response code="200">Returns the list of all customers</response>
        /// <response code="204">If the list of customers is empty</response> 
        /// <response code="401">If the user is not authenticated as administrator</response> 

        [HttpGet("Customers")]
        public ActionResult<IList<CustomerDetailsDTO>> GetAllCustomers()
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            IList<Customer> customers = _loggedInAdministratorFacade.GetAllCustomers(admin_token);
            if (customers.Count == 0)
                return NoContent();

            List<CustomerDetailsDTO> customersDetailsDTOS = new List<CustomerDetailsDTO>();
            foreach (var customer in customers)
                customersDetailsDTOS.Add(_mapper.Map<CustomerDetailsDTO>(customer));

            return Ok(customersDetailsDTOS);
        }

        /// <summary>
        /// Create new airline company with associated user
        /// </summary>
        /// <returns>Airline company that has been created</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /CreateAirlineCompanyDTO
        ///     {
        ///         "name": "Elal",
        ///         "countryId": 1,
        ///         "user": {
        ///             "userName": "ElAlIL",
        ///             "password": "Pass1234",
        ///             "email": "Elal@Elal.com",
        ///         }
        ///     }
        ///
        /// </remarks>  
        /// <param name="createAirlineCompanyDTO">DTO that holds the data for creating airline company</param>
        /// <response code="201">The airline company has been created successfully</response>
        /// <response code="401">If the user is not authenticated as administrator</response> 
        /// <response code="404">If the country id doesn't point to existing country</response> 
        /// <response code="409">If there is another airline company with same username/email/name</response> 
        [HttpPost("Airline-Companies")]
        public ActionResult<AirlineCompany> CreateNewAirlineCompany(CreateAirlineCompanyDTO createAirlineCompanyDTO)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            AirlineCompany airline = _mapper.Map<AirlineCompany>(createAirlineCompanyDTO);

            string uri = null;
            try
            {
                airline.Id = _loggedInAdministratorFacade.CreateNewAirlineCompany(admin_token, airline);
                if (airline.Id == 0)
                    return Conflict();

                uri = _linkGenerator.GetPathByAction(nameof(AnonymousFacadeController.GetAirlineCompanyById), "AnonymousFacade", new { id = airline.Id });

            }
            catch (RecordAlreadyExistsException)
            {
                return Conflict();
            }
            catch (RelatedRecordNotExistsException)
            {
                return NotFound($"Country with id: {createAirlineCompanyDTO.CountryId} doesn't exist");
            }

            return Created(uri, airline);
        }

        /// <summary>
        /// Update existing airline company details
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /UpdateAirlineCompanyDTO
        ///     {
        ///         "id":"1"
        ///         "name":"Arkia",
        ///         "countryId": 1
        ///     }
        /// </remarks>  
        /// <param name="id">The id of the airline company that will be updated</param>
        /// <param name="updateAirlineCompanyDTO">DTO that holds the data for updating airline company</param>
        /// <response code="204">The airline company has been updated successfully</response>
        /// <response code="400">If the airline company id is different between the url and the body</response> 
        /// <response code="401">If the user is not authenticated as administrator</response> 
        /// <response code="404">If the country id doesn't point to existing country</response> 
        /// <response code="409">If there is another airline company with same name</response> 
        [HttpPut("Airline-Companies/{id}")]
        public IActionResult UpdateAirlineDetails(long id, UpdateAirlineCompanyDTO updateAirlineCompanyDTO)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            if (id == 0)
                return NotFound();

            if (id != updateAirlineCompanyDTO.Id)
                return BadRequest();

            AirlineCompany airline = _mapper.Map<AirlineCompany>(updateAirlineCompanyDTO);

            try
            {
                _loggedInAdministratorFacade.UpdateAirlineDetails(admin_token, airline);
            }
            catch (RecordAlreadyExistsException)
            {
                return Conflict();
            }
            catch (RelatedRecordNotExistsException)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Removes airline company and associated user
        /// </summary>
        /// <param name="id">The id of the airline company that has to be removed</param>
        /// <response code="204">The airline company has been removed successfully</response>
        /// <response code="401">If the user is not authenticated as administrator of level 2+</response> 
        /// <response code="404">If the airline company that has to be removed not exist</response>
        [HttpDelete("Airline-Companies/{id}")]
        public IActionResult RemoveAirline(long id)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            AirlineCompany airlineCompany = _loggedInAdministratorFacade.GetAirlineCompanyById(id);
            if (airlineCompany == null)
                return NotFound();

            try
            {
                _loggedInAdministratorFacade.RemoveAirline(admin_token, airlineCompany);
            }
            catch (NotAllowedAdminActionException)
            {
                return Unauthorized();
            }

            return NoContent();
        }

        /// <summary>
        /// Create new customer with associated user
        /// </summary>
        /// <returns>Customer that has been created</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /CreateCustomerDTO
        ///     {
        ///         "firstName": "Customer",
        ///         "lastName": "Custom",
        ///         "address": null,
        ///         "phoneNumber": "052-1234567",
        ///         "creditCardNumber": null,
        ///         "user": {
        ///            "userName": "CustomerA",
        ///            "password": "Pass1234",
        ///            "email": "ACustomer@gmail.com",
        ///         }
        ///     }
        /// </remarks>  
        /// <param name="createCustomerDTO">DTO that holds the data for creating customer</param>
        /// <response code="201">The customer has been created successfully</response>
        /// <response code="401">If the user is not authenticated as administrator</response> 
        /// <response code="409">If there is another customer with same username/email/phone</response> 
        [HttpPost("Customers")]
        public ActionResult<Customer> CreateNewCustomer(CreateCustomerDTO createCustomerDTO)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            Customer customer = _mapper.Map<Customer>(createCustomerDTO);

            string uri = null;
            try
            {
                customer.Id = _loggedInAdministratorFacade.CreateNewCustomer(admin_token, customer);
                if (customer.Id == 0)
                    return Conflict();

                uri = _linkGenerator.GetPathByAction(nameof(AdministratorFacadeController.GetCustomerById), "AdministratorFacade", new { id = customer.Id });
            }
            catch (RecordAlreadyExistsException)
            {
                return Conflict();
            }

            return Created(uri, customer);
        }

        /// <summary>
        /// Update existing customer details
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /UpdateCustomerDTO
        ///     {  
        ///         "id":"1"
        ///         "firstName":"Customer",
        ///         "lastName":"Custom",
        ///         "address":"Ashdod",
        ///         "phoneNumber":"052-1234567",
        ///         "creditCardNumber": null
        ///     }
        /// </remarks>  
        /// <param name="id">The id of the customer that will be updated</param>
        /// <param name="updateCustomerDTO">DTO that holds the data for updating customer</param>
        /// <response code="204">The customer has been updated successfully</response>
        /// <response code="400">If the customer id is different between the url and the body</response> 
        /// <response code="401">If the user is not authenticated as administrator</response> 
        /// <response code="404">If the customer id is 0</response> 
        /// <response code="409">If there is another customer with same phone</response> 
        [HttpPut("Customers/{id}")]
        public IActionResult UpdateCustomerDetails(long id, UpdateCustomerDTO updateCustomerDTO)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            if (id == 0)
                return NotFound();

            if (id != updateCustomerDTO.Id)
                return BadRequest();

            Customer customer = _mapper.Map<Customer>(updateCustomerDTO);

            try
            {
                _loggedInAdministratorFacade.UpdateCustomerDetails(admin_token, customer);
            }
            catch (RecordAlreadyExistsException)
            {
                return Conflict();
            }
            return NoContent();
        }

        /// <summary>
        /// Removes customer and associated user
        /// </summary>
        /// <param name="id">The id of the customer that has to be removed</param>
        /// <response code="204">The customer has been removed successfully</response>
        /// <response code="401">If the user is not authenticated as administrator of level 2+</response> 
        /// <response code="404">If the airline company that has to be removed not exist</response>
        [HttpDelete("Customers/{id}")]
        public IActionResult RemoveCustomer(long id)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            Customer customer = _loggedInAdministratorFacade.GetCustomerById(admin_token, id);
            if (customer == null)
                return NotFound();

            try
            {
                _loggedInAdministratorFacade.RemoveCustomer(admin_token, customer);
            }
            catch (NotAllowedAdminActionException)
            {
                return Unauthorized();
            }

            return NoContent();
        }

        /// <summary>
        /// Create new administrator with associated user
        /// </summary>
        /// <returns>Administrator that has been created</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /CreateAdministratorDTO
        ///     {
        ///         "firstName": "admin1",
        ///         "lastName": "admin1",
        ///         "level": 1,
        ///         "user":{
        ///             "userName": "admin1",
        ///             "password": "Pass1234",
        ///             "email": "admin1@admin.com"
        ///         }
        ///     }
        /// </remarks>  
        /// <param name="createAdministratorDTO">DTO that holds the data for creating administrator</param>
        /// <response code="201">The administrator has been created successfully</response>
        /// <response code="401">If the user is not authenticated as administrator level 3+, in case that the admin that is added is of level 3 the creating administrator must be of level 4</response> 
        /// <response code="409">If there is another administrator with same username/email</response> 
        [HttpPost("Administrators")]
        public ActionResult<Administrator> CreateNewAdmin(CreateAdministratorDTO createAdministratorDTO)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            Administrator admin = _mapper.Map<Administrator>(createAdministratorDTO);

            string uri = null;
            try
            {
                admin.Id = _loggedInAdministratorFacade.CreateNewAdmin(admin_token, admin);
                if (admin.Id == 0)
                    return Conflict();

                uri = _linkGenerator.GetPathByAction(nameof(AdministratorFacadeController.GetAdminById), "AdministratorFacade", new { id = admin.Id });
            }
            catch (RecordAlreadyExistsException)
            {
                return Conflict();
            }
            catch (NotAllowedAdminActionException)
            {
                return Unauthorized();
            }

            return Created(uri, admin);
        }

        /// <summary>
        /// Create new country
        /// </summary>
        /// <returns>Country that has been created</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /CreateCountryDTO
        ///     {
        ///         "name": "Israel"
        ///     }
        /// </remarks>  
        /// <param name="countryDTO">DTO that holds the data for creating country</param>
        /// <response code="201">The country has been created successfully</response>
        /// <response code="401">If the user is not authenticated as admin of level 3+</response> 
        /// <response code="409">If there is another country with same name</response> 
        [HttpPost("Countries")]
        public ActionResult<Country> CreateNewCountry(CreateCountryDTO countryDTO)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();
            string uri = null;

            Country country = _mapper.Map<Country>(countryDTO);

            try
            {
                country.Id = _loggedInAdministratorFacade.CreateNewCountry(admin_token, country);
                if (country.Id == 0)
                    return Conflict();

                uri = _linkGenerator.GetPathByAction(nameof(AnonymousFacadeController.GetCountryById), "AnonymousFacade", new { id = country.Id });
            }
            catch (RecordAlreadyExistsException)
            {
                return Conflict();
            }

            return Created(uri, country);

        }

        /// <summary>
        /// Get specific administrator by id
        /// </summary>
        /// <returns>Administrator</returns>
        /// <param name="id">The id of the administrator that you wish to get</param>
        /// <response code="200">Returns desired administrator</response>
        /// <response code="401">If the user is not authenticated as administrator</response> 
        /// <response code="404">If the desired administrator is not found</response> 
        [HttpGet("Administrators/{id?}")]
        public ActionResult<AdministratorDetailsDTO> GetAdminById(int id)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            Administrator admin = _loggedInAdministratorFacade.GetAdminById(admin_token, id);
            if (admin == null)
                return NotFound();

            AdministratorDetailsDTO administratorDetailsDTO = _mapper.Map<AdministratorDetailsDTO>(admin);

            return Ok(administratorDetailsDTO);
        }

        /// <summary>
        /// Get specific customer by id
        /// </summary>
        /// <returns>Customer</returns>
        /// <param name="id">The id of the customer that you wish to get</param>
        /// <response code="200">Returns desired customer</response>
        /// <response code="401">If the user is not authenticated as administrator</response> 
        /// <response code="404">If the desired customer is not found</response> 
        [HttpGet("Customers/{id?}")]
        public ActionResult<CustomerDetailsDTO> GetCustomerById(long id)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            Customer customer = _loggedInAdministratorFacade.GetCustomerById(admin_token, id);
            if (customer == null)
                return NotFound();

            CustomerDetailsDTO customerDetailsDTO = _mapper.Map<CustomerDetailsDTO>(customer);

            return Ok(customerDetailsDTO);
        }

        /// <summary>
        /// Update existing administrator details
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /UpdateAdministratorDTO
        ///     {  
        ///         "id":"1"
        ///         "firstName": "admin",
        ///         "lastName": "admin",
        ///         "level": 3
        ///     }
        /// </remarks>  
        /// <param name="id">The id of the administrator that will be updated</param>
        /// <param name="updateAdministratorDTO">DTO that holds the data for updating administrator</param>
        /// <response code="204">The administrator has been updated successfully</response>
        /// <response code="400">If the administrator id is different between the url and the body</response> 
        /// <response code="401">If the user is not authenticated as administrator level 3+, in case that the admin that is updated is of level 3 the creating administrator must be of level 4</response> 
        /// <response code="404">If the administrator id is 0</response> 
        [HttpPut("Administrators/{id}")]
        public IActionResult UpdateAdminDetails(int id, UpdateAdministratorDTO updateAdministratorDTO)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            if (id == 0)
                return NotFound();

            if (id != updateAdministratorDTO.Id)
                return BadRequest();

            Administrator admin = _mapper.Map<Administrator>(updateAdministratorDTO);

            try
            {

                _loggedInAdministratorFacade.UpdateAdminDetails(admin_token, admin);
            }
            catch (NotAllowedAdminActionException)
            {
                return Unauthorized();
            }

            return NoContent();
        }

        /// <summary>
        /// Update existing country details
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /UpdateCountryDTO
        ///     {  
        ///         "id": "1"
        ///         "name": "Russia"
        ///     }
        /// </remarks>  
        /// <param name="id">The id of the country that will be updated</param>
        /// <param name="updateCountryDTO">DTO that holds the data for updating country</param>
        /// <response code="204">The country has been updated successfully</response>
        /// <response code="400">If the country id is different between the url and the body</response> 
        /// <response code="401">If the user is not authenticated as administrator</response> 
        /// <response code="404">If the country id is 0</response> 
        /// <response code="409">If there is another country with same name</response> 
        [HttpPut("Countries/{id}")]
        public IActionResult UpdateCountryDetails(int id, UpdateCountryDTO updateCountryDTO)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            if (id == 0)
                return NotFound();

            if (id != updateCountryDTO.Id)
                return BadRequest();

            Country country = _mapper.Map<Country>(updateCountryDTO);

            try
            {
                _loggedInAdministratorFacade.UpdateCountryDetails(admin_token, country);
            }
            catch (RecordAlreadyExistsException)
            {
                return Conflict();
            }

            return NoContent();
        }

        /// <summary>
        /// Removes administrator and associated user
        /// </summary>
        /// <param name="id">The id of the administrator that has to be removed</param>
        /// <response code="204">The administrator has been removed successfully</response>
        /// <response code="401">If the user is not authenticated as administrator of level 3+ or when trying to remove administrator level 3+</response> 
        /// <response code="404">If the administrator that has to be removed is not exist</response>
        [HttpDelete("Administrators/{id}")]
        public IActionResult RemoveAdmin(int id)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            Administrator admin = _loggedInAdministratorFacade.GetAdminById(admin_token, id);
            if (admin == null)
                return NotFound();

            try
            {
                _loggedInAdministratorFacade.RemoveAdmin(admin_token, admin);
            }
            catch (NotAllowedAdminActionException)
            {
                return Unauthorized();
            }

            return NoContent();
        }

        /// <summary>
        /// Removes country
        /// </summary>
        /// <param name="id">The id of the country that has to be removed</param>
        /// <response code="204">The country has been removed successfully</response>
        /// <response code="304">If there are airline companies of flights related to the country, then the country won't be deleted</response>
        /// <response code="401">If the user is not authenticated as administrator of level 3+</response> 
        /// <response code="404">If the country that has to be removed is not exist</response>
        [HttpDelete("Countries/{id}")]
        public IActionResult RemoveCountry(int id)
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            Country country = _loggedInAdministratorFacade.GetCountryById(id);
            if (country == null)
                return NotFound();

            try
            {
                _loggedInAdministratorFacade.RemoveCountry(admin_token, country);
            }
            catch (NotAllowedAdminActionException)
            {
                return Unauthorized();
            }
            catch (DeleteTargetHasRelatedRecordsException)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            return NoContent();
        }

        /// <summary>
        /// Get list of all the administrators
        /// </summary>
        /// <returns>List of all the administrators</returns>
        /// <response code="200">Returns the list of all administrators</response>
        /// <response code="204">If the list of administrators is empty</response> 
        /// <response code="401">If the user is not authenticated as administrator</response> 

        [HttpGet("Administrators")]
        public ActionResult<IList<AdministratorDetailsDTO>> GetAllAdministrators()
        {
            LoginToken<Administrator> admin_token = DesirializeToken();

            IList<Administrator> administrators = _loggedInAdministratorFacade.GetAllAdministrators(admin_token);
            if (administrators.Count == 0)
                return NoContent();

            List<AdministratorDetailsDTO> administratorDetailsDTOs = new List<AdministratorDetailsDTO>();
            foreach (var administrator in administrators)
                administratorDetailsDTOs.Add(_mapper.Map<AdministratorDetailsDTO>(administrator));

            return Ok(administratorDetailsDTOs);
        }
    }
}
