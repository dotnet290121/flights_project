using Domain.Entities;
using Domain.Interfaces;
using log4net;
using Npgsql;
using System.Collections.Generic;
using System.Reflection;

namespace DAL
{
    public class AirlineDAOPGSQL : BasicDB<AirlineCompany>, IAirlineDAO
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override long Add(AirlineCompany airlineCompany)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            long result = 0;

            result = Execute(() =>
            {
                string procedure = "sp_add_airline_company";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("_name", airlineCompany.Name),
                    new NpgsqlParameter("_country_id", airlineCompany.CountryId),
                    new NpgsqlParameter("_user_id", airlineCompany.User.Id)
                });
                command.CommandType = System.Data.CommandType.StoredProcedure;

                result = (long)command.ExecuteScalar();

                return result;
            }, new { AirlineCompany = airlineCompany }, conn, _logger);

            return result;
        }

        public override AirlineCompany Get(long id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            AirlineCompany result = null;
            result = Execute(() =>
            {
                List<AirlineCompany> airlineCompanies = Run_Generic_SP("sp_get_airline_company", new { _id = id }, conn);

                if (airlineCompanies.Count > 0)
                    result = airlineCompanies[0];

                return result;
            }, new { Id = id }, conn, _logger);

            return result;
        }

        public AirlineCompany GetAirlineCompanyByName(string name)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            AirlineCompany result = null;
            result = Execute(() =>
            {
                List<AirlineCompany> airlineCompanies = Run_Generic_SP("sp_get_airline_company_by_name", new { _name = name }, conn);

                if (airlineCompanies.Count > 0)
                    result = airlineCompanies[0];

                return result;
            }, new { Name = name }, conn, _logger);

            return result;
        }

        public AirlineCompany GetAirlineCompanyByUserId(long user_id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            AirlineCompany result = null;

            result = Execute(() =>
            {
                List<AirlineCompany> airlineCompanies = Run_Generic_SP("sp_get_airline_company_by_user_id", new { _user_id = user_id }, conn);

                if (airlineCompanies.Count > 0)
                    result = airlineCompanies[0];

                return result;
            }, new { UserId = user_id }, conn, _logger);

            return result;
        }

        public AirlineCompany GetAirlineCompanyByUsername(string username)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            AirlineCompany result = null;
            result = Execute(() =>
            {
                List<AirlineCompany> airlineCompanies = Run_Generic_SP("sp_get_airline_company_by_username", new { _username = username }, conn);

                if (airlineCompanies.Count > 0)
                    result = airlineCompanies[0];

                return result;
            }, new { Username = username }, conn, _logger);

            return result;
        }

        public override IList<AirlineCompany> GetAll()
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<AirlineCompany> result = new List<AirlineCompany>();
            result = Execute(() => Run_Generic_SP("sp_get_all_airline_companies", new { }, conn), new { }, conn, _logger);

            return result;
        }

        public IList<AirlineCompany> GetAllAirlinesByCountry(int countryId)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<AirlineCompany> result = new List<AirlineCompany>();
            result = Execute(() => Run_Generic_SP("sp_get_all_airline_companies_by_country", new { _country_id = countryId }, conn), new { CountryId = countryId }, conn, _logger);

            return result;
        }

        public override void Remove(AirlineCompany airlineCompany)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_remove_airline_company(@_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("@_id", airlineCompany.Id));

                command.ExecuteNonQuery();
            }, new { AirlineCompany = airlineCompany }, conn, _logger);
        }

        public override void Update(AirlineCompany airlineCompany)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_update_airline_company(@_id, @_name, @_country_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@_id", airlineCompany.Id),
                    new NpgsqlParameter("@_name", airlineCompany.Name),
                    new NpgsqlParameter("@_country_id", airlineCompany.CountryId)
                });

                command.ExecuteNonQuery();
            }, new { AirlineCompany = airlineCompany }, conn, _logger);
        }
    }
}
