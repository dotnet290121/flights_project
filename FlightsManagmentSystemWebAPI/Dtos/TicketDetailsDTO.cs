
namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class TicketDetailsDTO : IDetailsDTO
    {
        public long Id { get; set; }
        public FlightDetailsDTO Flight { get; set; }
        public CustomerDetailsDTO Customer { get; set; }
    }
}
