using Domain.Entities;
using Domain.Interfaces;
using Npgsql;
using System;

namespace DAL
{
    public class FlightsTicketsHistoryDAOPGSQL : IFlightsTicketsHistoryDAO
    {
        public FlightHistory GetFlightHistory(long original_flight_id)
        {
            NpgsqlConnection conn = null;
            FlightHistory flightHistory = null;

            try
            {
                conn = DbConnectionPool.Instance.GetConnection();

                string procedure = "sp_get_flight_history_by_original_id";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("_original_id", original_flight_id));

                command.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    flightHistory = new FlightHistory
                    {
                        Id = (long)reader["id"],
                        OriginalId = (long)reader["flight_original_id"],
                        AirlineCompanyId = (long)reader["airline_company_id"],
                        AirlineCompanyName = (string)reader["airline_company_name"],
                        OriginCountryId = (int)reader["origin_country_id"],
                        DestinationCountryId = (int)reader["destination_country_id"],
                        DepartureTime = (DateTime)reader["departure_time"],
                        LandingTime = (DateTime)reader["landing_time"],
                        RemainingTickets = (int)reader["remaining_tickets"],
                        FlightStatus = (FlightStatus)reader["flight_status"]
                    };
                }
                return flightHistory;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                DbConnectionPool.Instance.ReturnConnection(conn);
            }
        }

        public TicketHistory GetTicketHistory(long original_ticket_id)
        {
            NpgsqlConnection conn = null;
            TicketHistory ticketHistory = null;

            try
            {
                conn = DbConnectionPool.Instance.GetConnection();

                string procedure = "sp_get_ticket_history_by_original_id";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("_original_id", original_ticket_id));

                command.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    ticketHistory = new TicketHistory
                    {
                        Id = (long)reader["id"],
                        OriginalId = (long)reader["ticket_original_id"],
                        FlightId = (long)reader["flight_id"],
                        CustomerId = (long)reader["customer_id"],
                        CustomerFullName = (string)reader["customer_full_name"],
                        CustomerUserName = (string)reader["customer_username"],
                        TicketStatus = (TicketStatus)reader["ticket_status"]
                    };
                }
                return ticketHistory;
            }
            finally
            {
                DbConnectionPool.Instance.ReturnConnection(conn);
            }
        }

        public void Add(Flight flight, FlightStatus status)
        {
            NpgsqlConnection conn = null;

            try
            {
                conn = DbConnectionPool.Instance.GetConnection();

                string procedure = "sp_add_flight_history";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("_original_id", flight.Id),
                    new NpgsqlParameter("_airline_company_id", flight.AirlineCompany.Id),
                    new NpgsqlParameter("_airline_company_name", flight.AirlineCompany.Name),
                    new NpgsqlParameter("_origin_country_id", flight.OriginCountryId),
                    new NpgsqlParameter("_destination_country_id", flight.DestinationCountryId),
                    new NpgsqlParameter("_departure_time", flight.DepartureTime),
                    new NpgsqlParameter("_landing_time", flight.LandingTime),
                    new NpgsqlParameter("_remaining_tickets", flight.RemainingTickets),
                    new NpgsqlParameter("_flight_status", (int)status)
                });
                command.CommandType = System.Data.CommandType.StoredProcedure;

                var id = command.ExecuteScalar();
            }
            finally
            {
                DbConnectionPool.Instance.ReturnConnection(conn);

            }
        }

        public void Add(Ticket ticket, TicketStatus status)
        {
            NpgsqlConnection conn = null;

            try
            {
                conn = DbConnectionPool.Instance.GetConnection();

                string procedure = "sp_add_ticket_history";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("_original_id", ticket.Id),
                    new NpgsqlParameter("_flight_id", ticket.Flight.Id),
                    new NpgsqlParameter("_customer_id", ticket.Customer.Id),
                    new NpgsqlParameter("_customer_full_name", $"{ticket.Customer.FirstName} {ticket.Customer.LastName}"),
                    new NpgsqlParameter("_customer_username", ticket.Customer.User.UserName),
                    new NpgsqlParameter("_ticket_status", (int)status)
                });
                command.CommandType = System.Data.CommandType.StoredProcedure;

                var id = command.ExecuteScalar();
            }
            finally
            {
                DbConnectionPool.Instance.ReturnConnection(conn);
            }
        }
    }
}
