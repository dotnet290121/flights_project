using FlightsManagmentSystemWebAPI.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class CreateFlightDTO : IValidatableObject
    {
        [Range(1, int.MaxValue)]
        public int OriginCountryId { get; set; }
        [Range(1, int.MaxValue)]
        public int DestinationCountryId { get; set; }
        [FutureDate(ErrorMessage = "Departure date can't be in the past")]
        public DateTime DepartureTime { get; set; }
        [FutureDate(ErrorMessage = "Landing date can't be in the past")]
        public DateTime LandingTime { get; set; }
        [Range(0, int.MaxValue)]
        public int RemainingTickets { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DepartureTime >= LandingTime)
            {
                yield return new ValidationResult(
                    $"Departure time: {DepartureTime} can't be before landing time: {LandingTime}.",
                    new[] { nameof(DepartureTime), nameof(LandingTime) });
            }
        }
    }
}
