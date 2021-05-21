using ConfigurationService;
using DAL.Exceptions;
using DAL.ExtentionMethods;
using Domain.Entities;
using Domain.ExtentionMethods;
using Domain.Interfaces;
using log4net;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DAL
{
    public abstract class BasicDB<T> : IBasicDB<T> where T : IPoco, new()
    {
        protected string conn_string = FlightsManagmentSystemConfig.Instance.ConnectionString;

        public abstract long Add(T t);

        public abstract T Get(long id);

        public abstract IList<T> GetAll();

        public abstract void Remove(T t);

        /// <summary>
        /// Generates array of NpgsqlParameter from the dataObject parameters
        /// </summary>
        /// <param name="dataObject">Object from which we want to build the parameters array</param>
        /// <returns></returns>
        private NpgsqlParameter[] GetParametersFromDataHolder(object dataObject)
        {
            List<NpgsqlParameter> paraResult = new List<NpgsqlParameter>();//Create new list
            var props_in_dataObject = dataObject.GetType().GetProperties();//Get all the properties contained in the dataObject type
            foreach (var prop in props_in_dataObject)//Run over all those properties
            {
                paraResult.Add(new NpgsqlParameter(prop.Name, prop.GetValue(dataObject)));//For each prop generate new NpgsqlParameter and add it to the list
            }
            return paraResult.ToArray();
        }

        /// <summary>
        /// Generates instance of the model with the data read from DB
        /// </summary>
        /// <param name="reader">NpgsqlDataReader to read data from DB</param>
        /// <param name="type">The type from which create instance</param>
        /// <param name="ignore_user">If the sp don't return a user, but the type has a User property set as true</param>
        /// <returns></returns>
        private object GetParametersFromReader(NpgsqlDataReader reader, Type type, bool ignore_user)
        {
            var instance = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())//Run over all the properties of the recieved entity
            {
                if (ignore_user && prop.PropertyType == typeof(User))//In case that the sp don't return the user details and one of the properties is indeed User
                    continue;//Continue to the next prop


                if (prop.PropertyType.GetInterfaces().Contains(typeof(IPoco)))//Check if the property is implementing the IPoco interface
                {
                    var instance_prop = GetParametersFromReader(reader, prop.PropertyType, ignore_user);//Recursion
                    prop.SetValue(instance, instance_prop);//After recursion ends set the prop value to the value that was read
                    continue;//Continue to the next prop
                }

                string column_name = prop.Name;//Set the column name to read as the property name

                var custom_attr_column_name =
                    (ColumnAttribute[])prop.GetCustomAttributes(typeof(ColumnAttribute), true);//Get all column attributes of the property
                if (custom_attr_column_name.Length > 0)//If there is at least one attribute (in this program there won't be 2 column attributes on same prop)
                    column_name = custom_attr_column_name[0].Name;//Set the column name as the column attribute value


                if (prop.PropertyType == typeof(string))//Check if property is a string (some of the strings might be nulls);
                {
                    var value = reader.GetSafeString(reader.GetOrdinal(column_name));//Get the string safely, if there is not string return null
                    prop.SetValue(instance, value);//Set the prop value as the value that was read (might be null)
                }
                else
                {
                    var value = reader[column_name];//Get the value from DB
                    prop.SetValue(instance, value);//Set the prop value as the value that was read
                }

            }

            return instance;
        }

        /// <summary>
        /// Generic method to invoke different stored procedures
        /// </summary>
        /// <param name="sp_name">The name of the stored procedure</param>
        /// <param name="dataHolder">Object that holds all the data that need to be passed to the sp</param>
        /// <param name="ignore_user">If the sp don't return a user, but the type has a User property set as true</param>
        /// <returns></returns>
        protected List<T> Run_Generic_SP(string sp_name, object dataHolder, NpgsqlConnection conn, bool ignore_user = false)
        {
            List<T> result = new List<T>();//Create a list of object from type T
            NpgsqlParameter[] param = null;

            NpgsqlCommand command = new NpgsqlCommand(sp_name, conn);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            param = GetParametersFromDataHolder(dataHolder);//Get array of NpgsqlParameter

            command.Parameters.AddRange(param);//Add all the NpgsqlParameter to the command

            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                T one_row = (T)GetParametersFromReader(reader, typeof(T), ignore_user);//Get one record from the DB

                result.Add(one_row);//Add the record to the list
            }

            return result;
        }

        public abstract void Update(T t);

        protected TResult Execute<TResult>(Func<TResult> func, object props_holder, NpgsqlConnection conn, ILog _logger, [CallerMemberName] string callerName = "")
        {
            _logger.Debug($"Enter {callerName}({props_holder.GenerateString()})");

            TResult result = default;

            try
            {
                result = func.Invoke();
            }
            catch (PostgresException ex)
            {
                switch (ex.SqlState)
                {
                    case "23503"://When adding record that has foreign key that points to a record that not exist
                        throw new RelatedRecordNotExistsException(ex, ex.TableName, ex.Statement.ToString(), ex.ConstraintName);
                    case "23505":
                        throw new RecordAlreadyExistsException(ex, ex.TableName, ex.Statement.ToString(), ex.ConstraintName);
                    default:
                        throw ex;
                }
                throw ex;
            }
            finally
            {
                DbConnectionPool.Instance.ReturnConnection(conn);

                if (result == null)
                    _logger.Debug($"Exit {callerName}. Result: null");

                else if (result.GetType().GetInterfaces().Contains(typeof(IEnumerable)))
                    _logger.Debug($"Exit {callerName}. Result: {((IEnumerable)result).BuildString()}");

                else
                    _logger.Debug($"Exit {callerName}. Result: {result}");
            }
            return result;
        }

        protected void Execute(Action action, object props_holder, NpgsqlConnection conn, ILog _logger, [CallerMemberName] string callerName = "")
        {
            _logger.Debug($"Enter {callerName}({props_holder.GenerateString()})");

            try
            {
                action.Invoke();
            }
            catch (PostgresException ex)
            {
                switch (ex.SqlState)
                {
                    case "23503":
                        {
                            if (ex.MessageText.StartsWith("update or delete"))//When removing record that has related records
                            {
                                throw new DeleteTargetHasRelatedRecordsException(ex, ex.TableName, ex.Statement.ToString(), ex.ConstraintName);
                            }
                            else if (ex.MessageText.StartsWith("insert or update"))//When updating record with foreign key that points to a record that not exist
                            {
                                throw new RelatedRecordNotExistsException(ex, ex.TableName, ex.Statement.ToString(), ex.ConstraintName);
                            }
                        };
                        break;
                    case "23505":
                        throw new RecordAlreadyExistsException(ex, ex.TableName, ex.Statement.ToString(), ex.ConstraintName);
                    default:
                        throw ex;
                }
            }
            finally
            {
                DbConnectionPool.Instance.ReturnConnection(conn);

                _logger.Debug($"Exit {callerName}.");
            }
        }
    }
}
