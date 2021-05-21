using Domain.Entities;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class AdministratorDetailsDTO: IDetailsDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AdminLevel Level { get; set; }
        public UserDetailsDTO User { get; set; }
    }
}
