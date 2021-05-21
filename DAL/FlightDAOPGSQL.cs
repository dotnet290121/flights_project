using Domain.Entities;
using Domain.Interfaces;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DAL
{
    public class FlightDAOPGSQL : BasicDB<Flight>, IFlightDAO
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override long Add(Flight flight)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            long result = 0;

            result = Execute(() =>
            {
                string procedure = "sp_add_flight";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("_airline_company_id", flight.AirlineCompany.Id),
                    new NpgsqlParameter("_origin_country_id", flight.OriginCountryId),
                    new NpgsqlParameter("_destination_country_id", flight.DestinationCountryId),
                    new NpgsqlParameter("_departure_time", flight.DepartureTime),
                    new NpgsqlParameter("_landing_time", flight.LandingTime),
                    new NpgsqlParameter("_remaining_tickets", flight.RemainingTickets)
                });
                command.CommandType = System.Data.CommandType.StoredProcedure;

                long id = (long)command.ExecuteScalar();

                return id;
            }, new { Flight = flight }, conn, _logger);

            return result;
        }

        public override Flight Get(long id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Flight result = null;

            result = Execute(() =>
            {
                List<Flight> flights = Run_Generic_SP("sp_get_flight", new { _id = id }, conn, true);

                if (flights.Count > 0)
                    result = flights[0];

                return result;
            }, new { Id = id }, conn, _logger);

            return result;
        }

        public override IList<Flight> GetAll()
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Flight> result = new List<Flight>();

            result = Execute(() => Run_Generic_SP("sp_get_all_flights", new { }, conn, true), new { }, conn, _logger);

            return result;
        }

        public IList<Flight> Search(int originCountryId = 0, int destinationCountryId = 0, DateTime? departureDate = null, DateTime? landingDate = null)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Flight> result = new List<Flight>();

            result = Execute(() =>
            {
                string procedure = "sp_search_flights";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                if (originCountryId != 0)
                    command.Parameters.Add(new NpgsqlParameter("_origin_country_id", originCountryId));

                if (destinationCountryId != 0)
                    command.Parameters.Add(new NpgsqlParameter("_destination_country_id", destinationCountryId));

                if (departureDate != null)
                    command.Parameters.Add(new NpgsqlParameter("_departure_date", (NpgsqlDate)departureDate));
        
                if (landingDate != null)
                    command.Parameters.Add(new NpgsqlParameter("_landing_date", (NpgsqlDate)landingDate));

                command.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(
                        new Flight
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
                        });
                }

                return result;
            }, new { }, conn, _logger);
            return result;
        }

        public Dictionary<Flight, int> GetAllFlightsVacancy()
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Dictionary<Flight, int> result = new Dictionary<Flight, int>();

            result = Execute(() =>
            {
                string procedure = "sp_get_all_flights";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(
                        new Flight
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
                        }, (int)reader["remaining_tickets"]);
                }

                return result;
            }, new { }, conn, _logger);

            return result;
        }

        public IList<Flight> GetFlightsByAirlineCompany(AirlineCompany airlineCompany)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Flight> result = new List<Flight>();

            result = Execute(() => Run_Generic_SP("sp_get_flights_by_airline_company", new { _airline_company_id = airlineCompany.Id }, conn, true),
                new { AirlineCompany = airlineCompany }, conn, _logger);

            return result;
        }

        public IList<Flight> GetFlightsByCustomer(Customer customer)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Flight> result = new List<Flight>();

            result = Execute(() => Run_Generic_SP("sp_get_flights_by_customer", new { _customer_id = customer.Id }, conn, true),
                new { Customer = customer }, conn, _logger);

            return result;
        }

        //public IList<Flight> GetFlightsByDepatrureDate(DateTime departureDate)
        //{
        //    NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
        //    List<Flight> result = new List<Flight>();

        //    result = Execute(() => Run_Generic_SP("sp_get_flights_by_departure_date", new { _departure_date = (NpgsqlDate)departureDate }, conn, true),
        //        new { DepartureDate = departureDate }, conn, _logger);

        //    return result;
        //}

        //public IList<Flight> GetFlightsByDestinationCountry(int countryId)
        //{
        //    NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
        //    List<Flight> result = new List<Flight>();

        //    result = Execute(() => Run_Generic_SP("sp_get_flights_by_destination_country", new { _destination_country_id = countryId }, conn, true),
        //        new { CountryId = countryId }, conn, _logger);

        //    return result;
        //}

        //public IList<Flight> GetFlightsByLandingDate(DateTime landingDate)
        //{
        //    NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
        //    List<Flight> result = new List<Flight>();

        //    result = Execute(() => Run_Generic_SP("sp_get_flights_by_landing_date", new { _landing_date = (NpgsqlDate)landingDate }, conn, true),
        //        new { LandingDate = landingDate }, conn, _logger);

        //    return result;
        //}

        //public IList<Flight> GetFlightsByOriginCountry(int countryId)
        //{
        //    NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
        //    List<Flight> result = new List<Flight>();

        //    result = Execute(() => Run_Generic_SP("sp_get_flights_by_origin_country", new { _origin_country_id = countryId }, conn, true),
        //      new { CountryId = countryId }, conn, _logger);

        //    return result;
        //}

        public IDictionary<Flight, List<Ticket>> GetFlightsWithTicketsAfterLanding(long seconds_after_landing)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Dictionary<Flight, List<Ticket>> result = new Dictionary<Flight, List<Ticket>>();

            result = Execute(() =>
            {
                string procedure = "sp_get_flights_with_tickets_that_landed";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("_seconds", seconds_after_landing));
                command.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = command.ExecuteReader();//The sp will return record for each flight and ticket togther, if the flight has no tickets, some of the fields will be null
                while (reader.Read())
                {
                    Flight flight = new Flight
                    {
                        Id = (long)reader["flight_id"]
                    };
                    if (!result.ContainsKey(flight))//Check if the flight already added as key
                    {
                        result.Add(//If not add the flight as key
                            new Flight
                            {
                                Id = flight.Id,
                                AirlineCompany = new AirlineCompany
                                {
                                    Id = (long)reader["airline_company_id"],
                                    Name = (string)reader["airline_company_name"]
                                },
                                OriginCountryId = (int)reader["origin_country_id"],
                                DestinationCountryId = (int)reader["destination_country_id"],
                                DepartureTime = (DateTime)reader["departure_time"],
                                LandingTime = (DateTime)reader["landing_time"],
                                RemainingTickets = (int)reader["remaining_tickets"]
                            },
                            new List<Ticket>()//Creates new list to add as value to the dictionary
                            {
                                new Ticket//Create new ticket
                                {
                                    Id=(reader["ticket_id"]==DBNull.Value? 0:(long)reader["ticket_id"]),//If there are no tickets for the flight the sp will return null and the id will become  
                                    Customer=new Customer
                                    {
                                        Id=(reader["customer_id"]==DBNull.Value? 0:(long)reader["customer_id"]),//Same as the id above
                                        FirstName=(reader["first_name"]==DBNull.Value? null:(string)reader["first_name"]),
                                        LastName=(reader["last_name"]==DBNull.Value? null:(string)reader["last_name"]),
                                        User=new User
                                        {
                                            UserName=(reader["username"]==DBNull.Value? null:(string)reader["username"]),
                                        }
                                    },
                                    Flight=new Flight
                                    {
                                        Id = flight.Id
                                    }
                                }
                            });
                    }
                    else//If the flight is already a key in the dictionary
                    {
                        result[flight].Add(//Add new ticket to the list in the value
                            new Ticket
                            {
                                Id = (long)reader["ticket_id"],
                                Customer = new Customer
                                {
                                    Id = (long)reader["customer_id"],
                                    FirstName = (reader["first_name"] == DBNull.Value ? null : (string)reader["first_name"]),
                                    LastName = (reader["last_name"] == DBNull.Value ? null : (string)reader["last_name"]),
                                    User = new User
                                    {
                                        UserName = (reader["username"] == DBNull.Value ? null : (string)reader["username"]),
                                    }
                                },
                                Flight = new Flight
                                {
                                    Id = flight.Id
                                }
                            });
                    }
                }

                return result;
            }, new { Seconds_After_Landing = seconds_after_landing }, conn, _logger);

            return result;
        }


        public override void Remove(Flight flight)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_remove_flight(@_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("@_id", flight.Id));

                command.ExecuteNonQuery();
            }, new { Flight = flight }, conn, _logger);
        }

        public override void Update(Flight flight)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_update_flight(@_id, @_origin_country_id, @_destination_country_id, @_departure_time, @_landing_time, @_remaining_tickets)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@_id", flight.Id),
                    new NpgsqlParameter("@_origin_country_id", flight.OriginCountryId),
                    new NpgsqlParameter("@_destination_country_id", flight.DestinationCountryId),
                    new NpgsqlParameter("@_departure_time", flight.DepartureTime),
                    new NpgsqlParameter("@_landing_time", flight.LandingTime),
                    new NpgsqlParameter("@_remaining_tickets", flight.RemainingTickets)
                });

                command.ExecuteNonQuery();
            }, new { Flight = flight }, conn, _logger);
        }

        public IList<Flight> GetFutureDepartures(int departureHoursPeriod)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Flight> result = new List<Flight>();

            result = Execute(() => Run_Generic_SP("sp_get_future_departures", new { _hours= departureHoursPeriod }, conn, true), new { DepartureHoursPeriod= departureHoursPeriod}, conn, _logger);

            return result;
        }
    }
}
