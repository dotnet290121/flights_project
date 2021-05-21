using Domain.Entities;
using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IAirlineDAO : IBasicDB<AirlineCompany>
    {
        AirlineCompany GetAirlineCompanyByUserId(long user_id);
        AirlineCompany GetAirlineCompanyByUsername(string username);
        AirlineCompany GetAirlineCompanyByName(string name);
        IList<AirlineCompany> GetAllAirlinesByCountry(int countryId);
    }
}
