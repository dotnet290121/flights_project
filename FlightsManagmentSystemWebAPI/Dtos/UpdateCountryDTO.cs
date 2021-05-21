using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class UpdateCountryDTO
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string Name { get; set; }
    }
}
