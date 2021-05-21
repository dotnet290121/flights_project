using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class FlightDetailsDTO:IDetailsDTO
    {
        public long Id { get; set; }
        public AirlineCompanyDetailsDTO AirlineCompany { get; set; }
        public int OriginCountryId { get; set; }
        public string OriginCountryName { get; set; }
        public int DestinationCountryId { get; set; }
        public string DestinationCountryName { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime LandingTime { get; set; }
        public int RemainingTickets { get; set; }
    }
}
