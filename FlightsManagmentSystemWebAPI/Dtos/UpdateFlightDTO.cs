using System;
using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class UpdateFlightDTO : CreateFlightDTO
    {
        [Range(1, long.MaxValue)]
        public long Id { get; set; }
    }
}
