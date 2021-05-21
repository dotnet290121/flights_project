
namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class AirlineCompanyDetailsDTO: IDetailsDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public UserDetailsDTO User { get; set; }
    }
}
