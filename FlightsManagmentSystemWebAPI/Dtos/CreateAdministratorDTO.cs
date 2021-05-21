using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class CreateAdministratorDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public AdminLevel Level { get; set; }
        public CreateUserDTO User { get; set; }
    }
}
