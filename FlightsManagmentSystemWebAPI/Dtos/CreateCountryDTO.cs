using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class CreateCountryDTO
    {
        [Required]
        [StringLength(30)]
        public string Name { get; set; }
    }
}
