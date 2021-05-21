using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Flight : IPoco
    {
        [Column("flight_id")]
        public long Id { get; set; }
        public AirlineCompany AirlineCompany { get; set; }
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

        public Flight()
        {

        }

        public Flight(AirlineCompany airlineCompany, int originCountryId, int destinationCountryId, DateTime departureTime, DateTime landingTime, int remainingTickets, long id = 0)
        {
            AirlineCompany = airlineCompany;
            OriginCountryId = originCountryId;
            DestinationCountryId = destinationCountryId;
            DepartureTime = departureTime;
            LandingTime = landingTime;
            RemainingTickets = remainingTickets;
            Id = id;
        }

        public static bool operator ==(Flight flight1, Flight flight2)
        {
            if (ReferenceEquals(flight1, null) && ReferenceEquals(flight2, null))
                return true;
            if (ReferenceEquals(flight1, null) || ReferenceEquals(flight2, null))
                return false;

            return flight1.Id == flight2.Id;
        }
        public static bool operator !=(Flight flight1, Flight flight2)
        {
            return !(flight1 == flight2);
        }
        public override bool Equals(object obj)
        {
            Flight flight = obj as Flight;
            return this == flight;
        }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
