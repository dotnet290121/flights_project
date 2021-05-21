using Domain.Entities;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class UserDetailsDTO:IDetailsDTO
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public UserRoles UserRole { get; set; }
    }
}
