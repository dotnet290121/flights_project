using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public enum FlightStatus
    {
        Landed = 0,
        Cancelled_By_Company = 1,
        Cancelled_By_Administrator = 2
    }

    public class FlightHistory
    {
        public long Id { get; set; }

        [Column("original_flight_id")]
        public long OriginalId { get; set; }

        [Column("airline_company_id")]
        public long AirlineCompanyId { get; set; }

        [Column("airline_company_name")]
        public string AirlineCompanyName { get; set; }

        [Column("origin_country_id")]
        public int OriginCountryId { get; set; }

        [Column("destination_country_id")]
        public int DestinationCountryId { get; set; }

        [Column("departure_time")]
        public DateTime DepartureTime { get; set; }

        [Column("landing_time")]
        public DateTime LandingTime { get; set; }

        [Column("remaining_tickets")]
        public int RemainingTickets { get; set; }

        [Column("flight_status")]
        public FlightStatus FlightStatus { get; set; }
    }
}
