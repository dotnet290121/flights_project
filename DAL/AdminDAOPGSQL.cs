using Domain.Entities;
using Domain.Interfaces;
using log4net;
using Npgsql;
using System.Collections.Generic;
using System.Reflection;


namespace DAL
{
    public class AdminDAOPGSQL : BasicDB<Administrator>, IAdminDAO
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override long Add(Administrator admin)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            long result = 0;

            result = Execute(() =>
             {

                 string procedure = "sp_add_administrator";

                 NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                 command.Parameters.AddRange(new NpgsqlParameter[]
                 {
                        new NpgsqlParameter("_first_name", admin.FirstName),
                        new NpgsqlParameter("_last_name", admin.LastName),
                        new NpgsqlParameter("_level", (int)admin.Level),
                        new NpgsqlParameter("_user_id", admin.User.Id),
                 });
                 command.CommandType = System.Data.CommandType.StoredProcedure;

                 result = (int)command.ExecuteScalar();

                 return result;
             }, new { Administrator = admin }, conn, _logger);

            return result;
        }

        public override Administrator Get(long id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Administrator result = null;

            result = Execute(() =>
            {
                List<Administrator> administrators = Run_Generic_SP("sp_get_administrator", new { _id = (int)id }, conn);

                if (administrators.Count > 0)
                    result = administrators[0];

                return result;
            }, new { Id = id }, conn, _logger);

            return result;
        }

        public Administrator GetAdministratorByUserId(long user_id)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            Administrator result = null;

            result = Execute(() =>
            {
                List<Administrator> administrators = Run_Generic_SP("sp_get_administrator_by_user_id", new { _user_id = user_id }, conn);

                if (administrators.Count > 0)
                    result = administrators[0];

                return result;
            }, new { UserId = user_id }, conn, _logger);

            return result;
        }

        public override IList<Administrator> GetAll()
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();
            List<Administrator> result = new List<Administrator>();
            result = Execute(() =>Run_Generic_SP("sp_get_all_administrators", new { }, conn), new { }, conn, _logger);
            
            return result;
        }

        public override void Remove(Administrator admin)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_remove_administrator(@_id)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.Add(new NpgsqlParameter("@_id", admin.Id));

                command.ExecuteNonQuery();
            }, new { Administrator = admin }, conn, _logger);
        }

        public override void Update(Administrator admin)
        {
            NpgsqlConnection conn = DbConnectionPool.Instance.GetConnection();

            Execute(() =>
            {
                string procedure = "call sp_update_administrator(@_id, @_first_name, @_last_name, @_level)";

                NpgsqlCommand command = new NpgsqlCommand(procedure, conn);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@_id", admin.Id),
                    new NpgsqlParameter("@_first_name", admin.FirstName),
                    new NpgsqlParameter("@_last_name", admin.LastName),
                    new NpgsqlParameter("@_level", (int)admin.Level)
                });

                command.ExecuteNonQuery();
            }, new { Administrator = admin }, conn, _logger);
        }
    }
}
