using DAL.ExtentionMethods;
using Domain.Entities;
using Domain.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DAL
{
    public class TicketDAOPGSQL : BasicDB<Ticket>, ITicketDAO
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override long Add(Ticket ticket)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            long result = 0;

            result = Execute(() =>
            {
                string procedure = "sp_add_ticket";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("_flight_id", ticket.Flight.Id),
                    new NpgsqlParameter("_customer_id", ticket.Customer.Id)
                });
                command.CommandType = System.Data.CommandType.StoredProcedure;

                result = (long)command.ExecuteScalar();

                return result;
            }, new { Ticket = ticket }, conn, _logger);

            return result;
        }

        public override Ticket Get(long id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Ticket result = null;

            result = Execute(() =>
            {
                List<Ticket> tickets = Run_Generic_SP("sp_get_ticket", new { _id = id }, conn, true);
                if (tickets.Count > 0)
                    result = tickets[0];     

                return result;
            }, new { Id = id }, conn, _logger);

            return result;
        }

        public override IList<Ticket> GetAll()
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Ticket> result = new List<Ticket>();
            result = Execute(() => Run_Generic_SP("sp_get_all_tickets", new {  }, conn, true), new { }, conn, _logger);
           
            return result;
        }

        //Won't work with Run_Generic_SP - there are 2 different users and only one is read from the db
        public IList<Ticket> GetTicketsByAirlineCompany(AirlineCompany airlineCompany)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Ticket> result = new List<Ticket>();

            result = Execute(() =>
            {
                string procedure = "sp_get_tickets_by_airline_company";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("_airline_company_id", airlineCompany.Id));
                command.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(
                        new Ticket
                        {
                            Id = (long)reader["ticket_id"],
                            Flight = new Flight
                            {
                                Id = (long)reader["flight_id"],
                                AirlineCompany = new AirlineCompany
                                {
                                    Id = (long)reader["airline_company_id"],
                                    Name = (string)reader["airline_company_name"],
                                    CountryId = (int)reader["airline_company_country_id"]
                                },
                                OriginCountryId = (int)reader["origin_country_id"],
                                DestinationCountryId = (int)reader["destination_country_id"],
                                DepartureTime = (DateTime)reader["departure_time"],
                                LandingTime = (DateTime)reader["landing_time"],
                                RemainingTickets = (int)reader["remaining_tickets"]
                            },
                            Customer = new Customer
                            {
                                Id = (long)reader["customer_id"],
                                FirstName = (string)reader["first_name"],
                                LastName = (string)reader["last_name"],
                                Address = reader.GetSafeString(reader.GetOrdinal("address")),
                                PhoneNumber = (string)reader["phone_number"],
                                CreditCardNumber = reader.GetSafeString(reader.GetOrdinal("credit_card_number")),
                                User = new User
                                {
                                    Id = (long)reader["user_id"],
                                    UserName = (string)reader["username"],
                                    Password = (string)reader["password"],
                                    Email = (string)reader["email"],
                                    UserRole = (UserRoles)reader["user_role_id"]
                                }
                            }
                        });
                }

                return result;
            }, new { }, conn, _logger);

            return result;
        }

        //Won't work with Run_Generic_SP - there are 2 different users and only one is read from the db
        public IList<Ticket> GetTicketsByCustomer(Customer customer)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Ticket> result = new List<Ticket>();

            result = Execute(() =>
            {
                string procedure = "sp_get_tickets_by_customer";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("_customer_id", customer.Id));
                command.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(
                        new Ticket
                        {
                            Id = (long)reader["ticket_id"],
                            Flight = new Flight
                            {
                                Id = (long)reader["flight_id"],
                                AirlineCompany = new AirlineCompany
                                {
                                    Id = (long)reader["airline_company_id"],
                                    Name = (string)reader["airline_company_name"],
                                    CountryId = (int)reader["airline_company_country_id"]
                                },
                                OriginCountryId = (int)reader["origin_country_id"],
                                DestinationCountryId = (int)reader["destination_country_id"],
                                DepartureTime = (DateTime)reader["departure_time"],
                                LandingTime = (DateTime)reader["landing_time"],
                                RemainingTickets = (int)reader["remaining_tickets"]
                            },
                            Customer = new Customer
                            {
                                Id = (long)reader["customer_id"],
                                FirstName = (string)reader["first_name"],
                                LastName = (string)reader["last_name"],
                                Address = reader.GetSafeString(reader.GetOrdinal("address")),
                                PhoneNumber = (string)reader["phone_number"],
                                CreditCardNumber = reader.GetSafeString(reader.GetOrdinal("credit_card_number")),
                                User = new User
                                {
                                    Id = (long)reader["user_id"],
                                    UserName = (string)reader["username"],
                                    Password = (string)reader["password"],
                                    Email = (string)reader["email"],
                                    UserRole = (UserRoles)reader["user_role_id"]
                                }
                            }
                        });
                }

                return result;
            }, new { }, conn, _logger);

            return result;
        }

        //Won't work with Run_Generic_SP - there are 2 different users and only one is read from the db
        public IList<Ticket> GetTicketsByFlight(Flight flight)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Ticket> result = new List<Ticket>();

            result = Execute(() =>
            {
                string procedure = "sp_get_tickets_by_flight";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("_flight_id", flight.Id));
                command.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(
                        new Ticket
                        {
                            Id = (long)reader["ticket_id"],
                            Flight = new Flight
                            {
                                Id = (long)reader["flight_id"],
                                AirlineCompany = new AirlineCompany
                                {
                                    Id = (long)reader["airline_company_id"],
                                    Name = (string)reader["airline_company_name"],
                                    CountryId = (int)reader["airline_company_country_id"]
                                },
                                OriginCountryId = (int)reader["origin_country_id"],
                                DestinationCountryId = (int)reader["destination_country_id"],
                                DepartureTime = (DateTime)reader["departure_time"],
                                LandingTime = (DateTime)reader["landing_time"],
                                RemainingTickets = (int)reader["remaining_tickets"]
                            },
                            Customer = new Customer
                            {
                                Id = (long)reader["customer_id"],
                                FirstName = (string)reader["first_name"],
                                LastName = (string)reader["last_name"],
                                Address = reader.GetSafeString(reader.GetOrdinal("address")),
                                PhoneNumber = (string)reader["phone_number"],
                                CreditCardNumber = reader.GetSafeString(reader.GetOrdinal("credit_card_number")),
                                User = new User
                                {
                                    Id = (long)reader["user_id"],
                                    UserName = (string)reader["username"],
                                    Password = (string)reader["password"],
                                    Email = (string)reader["email"],
                                    UserRole = (UserRoles)reader["user_role_id"]
                                }
                            }
                        });
                }

                return result;
            }, new { }, conn, _logger);

            return result;
        }

        public override void Remove(Ticket ticket)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_remove_ticket(@_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("@_id", ticket.Id));

                command.ExecuteNonQuery();
            }, new { Ticket = ticket }, conn, _logger);
        }

        public override void Update(Ticket ticket)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_update_ticket(@_id, @_flight_id, @_customer_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@_id", ticket.Id),
                    new NpgsqlParameter("@_flight_id", ticket.Flight.Id),
                    new NpgsqlParameter("@_customer_id", ticket.Customer.Id)
                });

                command.ExecuteNonQuery();
            }, new { Ticket = ticket }, conn, _logger);
        }
    }
}
