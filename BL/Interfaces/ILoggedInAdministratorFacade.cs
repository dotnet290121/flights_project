using BL.LoginService;
using Domain.Entities;
using System.Collections.Generic;

namespace BL.Interfaces
{
    public interface ILoggedInAdministratorFacade : IAnonymousUserFacade
    {
        IList<Customer> GetAllCustomers(LoginToken<Administrator> token);
        long CreateNewAirlineCompany(LoginToken<Administrator> token, AirlineCompany airline);
        void UpdateAirlineDetails(LoginToken<Administrator> token, AirlineCompany airline);
        void RemoveAirline(LoginToken<Administrator> token, AirlineCompany airline);
        long CreateNewCustomer(LoginToken<Administrator> token, Customer customer);
        void UpdateCustomerDetails(LoginToken<Administrator> token, Customer customer);
        void RemoveCustomer(LoginToken<Administrator> token, Customer customer);
        int CreateNewAdmin(LoginToken<Administrator> token, Administrator admin);
        int CreateNewCountry(LoginToken<Administrator> token, Country country);
        Administrator GetAdminById(LoginToken<Administrator> token, int id);
        Customer GetCustomerById(LoginToken<Administrator> token, long id);
        void UpdateAdminDetails(LoginToken<Administrator> token, Administrator admin);
        void UpdateCountryDetails(LoginToken<Administrator> token, Country country);
        void RemoveAdmin(LoginToken<Administrator> token, Administrator admin);
        void RemoveCountry(LoginToken<Administrator> token, Country country);
        IList<Administrator> GetAllAdministrators(LoginToken<Administrator> token);
    }
}
