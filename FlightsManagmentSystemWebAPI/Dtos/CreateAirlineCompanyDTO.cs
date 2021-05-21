using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class CreateAirlineCompanyDTO
    {
        [Required]
        [StringLength(50,MinimumLength =2)]
        public string Name { get; set; }
        public int CountryId { get; set; }
        public CreateUserDTO User { get; set; }
    }
}
