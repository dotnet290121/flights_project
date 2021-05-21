using Domain.Entities;
using Domain.Interfaces;
using Npgsql;
using System.Collections.Generic;
using System.Reflection;

namespace DAL
{
    public class UserDAOPGSQL : BasicDB<User>, IUserDAO
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override long Add(User user)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            long result = 0;

            result = Execute(() =>
            {
                string procedure = "sp_add_user";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("_username", user.UserName),
                    new NpgsqlParameter("_password", user.Password),
                    new NpgsqlParameter("_email", user.Email),
                    new NpgsqlParameter("_user_role_id", (int)user.UserRole),
                });
                command.CommandType = System.Data.CommandType.StoredProcedure;

                result = (long)command.ExecuteScalar();

                return result;
            }, new { User = user }, conn, _logger);

            return result;
        }

        public override User Get(long id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            User result = null;

            result = Execute(() =>
            {
                List<User> users = Run_Generic_SP("sp_get_user", new { _id = id }, conn);

                if (users.Count > 0)
                    result = users[0];

                return result;
            }, new { Id = id }, conn, _logger);

            return result;
        }

        public override IList<User> GetAll()
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<User> result = new List<User>();

            result = Execute(() => Run_Generic_SP("sp_get_all_users", new { }, conn), new { }, conn, _logger);

            return result;
        }

        public User GetUserByUserName(string username)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            User result = null;

            result = Execute(() =>
            {
                List<User> users = Run_Generic_SP("sp_get_user_by_username", new { _username = username }, conn);
                if (users.Count > 0)
                    result = users[0];

                return result;
            }, new { Username = username }, conn, _logger);

            return result;
        }

        public User GetUserByUserNameAndPassword(string username, string password)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            User result = null;

            result = Execute(() =>
            {
                List<User> users = Run_Generic_SP("sp_get_user_by_username_and_password", new { _username = username, _password = password }, conn);
                if (users.Count > 0)
                    result = users[0];

                return result;
            }, new { Username = username, Password = password }, conn, _logger);

            return result;
        }

        public override void Remove(User user)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_remove_user(@_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("@_id", user.Id));

                command.ExecuteNonQuery();
            }, new { User = user }, conn, _logger);
        }

        public override void Update(User user)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_update_user(@_id, @_username, @_password, @_email, @_user_role_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@_id", user.Id),
                    new NpgsqlParameter("@_username", user.UserName),
                    new NpgsqlParameter("@_password", user.Password),
                    new NpgsqlParameter("@_email", user.Email),
                    new NpgsqlParameter("@_user_role_id", (int)user.UserRole)
                });

                command.ExecuteNonQuery();
            }, new { User = user }, conn, _logger);
        }
    }
}
