using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class CreateUserDTO
    {
        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string UserName { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
