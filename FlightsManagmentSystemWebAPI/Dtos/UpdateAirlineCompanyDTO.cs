using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class UpdateAirlineCompanyDTO
    {
        [Range(1, long.MaxValue)]
        public long Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }
        public int CountryId { get; set; }
    }
}
