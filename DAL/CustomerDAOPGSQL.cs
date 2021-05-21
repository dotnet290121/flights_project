using Domain.Entities;
using Domain.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DAL
{
    public class CustomerDAOPGSQL : BasicDB<Customer>, ICustomerDAO
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override long Add(Customer customer)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            long result = 0;

            result = Execute(() =>
            {
                string procedure = "sp_add_customer";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("_first_name", customer.FirstName),
                    new NpgsqlParameter("_last_name", customer.LastName),
                    new NpgsqlParameter("_address", (object)customer.Address??DBNull.Value),
                    new NpgsqlParameter("_phone_number", customer.PhoneNumber),
                    new NpgsqlParameter("_credit_card_number", (object)customer.CreditCardNumber ?? DBNull.Value),
                    new NpgsqlParameter("_user_id", customer.User.Id)
                });
                command.CommandType = System.Data.CommandType.StoredProcedure;

                result = (long)command.ExecuteScalar();

                return result;
            }, new { Customer = customer }, conn, _logger);

            return result;
        }

        public override Customer Get(long id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Customer result = null;

            result = Execute(() =>
            {
                List<Customer> customers = Run_Generic_SP("sp_get_customer", new { _id = id }, conn);

                if (customers.Count > 0)
                    result = customers[0];

                return result;
            }, new { Id = id }, conn, _logger);

            return result;
        }

        public override IList<Customer> GetAll()
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Customer> result = new List<Customer>();

            result = Execute(() => Run_Generic_SP("sp_get_all_customers", new { }, conn), new { }, conn, _logger);

            return result;
        }

        public Customer GetCustomerByPhone(string phone)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Customer result = null;

            result = Execute(() =>
            {
                List<Customer> customers = Run_Generic_SP("sp_get_customer_by_phone_number", new { _phone_number = phone }, conn);

                if (customers.Count > 0)
                    result = customers[0];

                return result;
            }, new { PhoneNumber = phone }, conn, _logger);

            return result;
        }

        public Customer GetCustomerByUserId(long user_id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Customer result = null;

            result = Execute(() =>
            {
                List<Customer> customers = Run_Generic_SP("sp_get_customer_by_user_id", new { _user_id = user_id }, conn);

                if (customers.Count > 0)
                    result = customers[0];

                return result;
            }, new { UserId = user_id }, conn, _logger);

            return result;
        }

        public Customer GetCustomerByUsername(string username)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Customer result = null;

            result = Execute(() =>
            {
                List<Customer> customers = Run_Generic_SP("sp_get_customer_by_username", new { _username = username }, conn);

                if (customers.Count > 0)
                    result = customers[0];

                return result;
            }, new { Username = username }, conn, _logger);

            return result;
        }

        public override void Remove(Customer customer)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_remove_customer(@_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("@_id", customer.Id));

                command.ExecuteNonQuery();
            }, new { Customer = customer }, conn, _logger);
        }

        public override void Update(Customer customer)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_update_customer(@_id, @_first_name, @_last_name, @_address, @_phone_number, @_credit_card_number)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@_id", customer.Id),
                    new NpgsqlParameter("@_first_name", customer.FirstName),
                    new NpgsqlParameter("@_last_name", customer.LastName),
                    new NpgsqlParameter("@_address", (object)customer.Address??DBNull.Value),
                    new NpgsqlParameter("@_phone_number", customer.PhoneNumber),
                    new NpgsqlParameter("@_credit_card_number", (object)customer.CreditCardNumber??DBNull.Value)
                });

                command.ExecuteNonQuery();
            }, new { Customer = customer }, conn, _logger);
        }
    }
}
