using ConfigurationService;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DAL
{
    /// <summary>
    /// Singleton class that provides a pool of NpgsqlConnection
    /// </summary>
    public class DbConnectionPool
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //Queue that holds all the connections
        private Queue<NpgsqlConnection> _connections;
        //The single instance
        private static DbConnectionPool _instance;
        //The key that used for lock in the initialization of the singleton class- that is the reason why this key is static 
        private static object key = new object();
        //The key that used for lock in the methods that gets and returns connection from the pool (GetConnection), there are multiple instances on the connection so for each of them should be their own key
        private object conn_key = new object();
        //Max connections configured
        private static int MAX_CONN = FlightsManagmentSystemConfig.Instance.MaxConnections;
        //Connection string configured
        private static string conn_string = FlightsManagmentSystemConfig.Instance.ConnectionString;

        private DbConnectionPool()
        {
            _connections = new Queue<NpgsqlConnection>(MAX_CONN);
        }

        public static DbConnectionPool Instance
        {
            get
            {
                //if the class already assigned return the exisiting instance if not lock the following code from other threads in order to initialize only one instance
                if (_instance == null)
                {
                    lock (key)
                    {
                        //second check after the lock in case that there was another thread that already passed the first if statement and waited for the lock to end. If there was a situation like this then
                        //the instance is already initialized when the lock is removed so we can return this instance and avoid creating second instance of this class.
                        if (_instance == null)
                        {
                            _instance = new DbConnectionPool();
                            _instance.Init(MAX_CONN);
                            return _instance;
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Create all the NpgsqlConnection as the max number of collection in order to save the tome later of creating each connection (the connection are initialized at the beggining
        /// </summary>
        /// <param name="max_connections">Max number of connections to be initialized</param>
        private void Init(int max_connections)
        {
            for (int i = 0; i < max_connections; i++)
            {
                //Add new connection to the end of the queue
                _connections.Enqueue(new NpgsqlConnection(conn_string));
            }
        }

        /// <summary>
        /// Returns an open connection from the pool
        /// </summary>
        /// <returns>NpgsqlConnection from the connection pool</returns>
        public NpgsqlConnection GetConnection()
        {
            //lock the following code from other threads (prevent cases when there is one connection left and two or more threads request a connection at the same time, it will return one connection and  the other threads will wait
            lock (conn_key)
            {
                //The while loop ensures that when the thread return from the "waiting room" it will check if there are still available connections.
                //That is good for cases when one connection is returned and the thread is awaken but in the same time another new thread that wasn't in the waiting room already pass the lock and takes that connection.
                while (_connections.Count == 0)
                {
                    //The wait is used in order to allow ReturnConnection when there are no connections left available and in the same time the the conn_key is locking GetConnection 
                    //When the thread goes to the "waiting room" due to no connections are available it releases the ley that locking and it's possible to return connections
                    Monitor.Wait(conn_key);
                }
                //Return the first connection from the connection queue
                var conn = _connections.Dequeue();
                conn.Open();
                return conn;
            }
        }

        /// <summary>
        /// Return a connection to the pool
        /// </summary>
        /// <param name="conn">Connection that will be returned to the pull</param>
        public void ReturnConnection(NpgsqlConnection conn)
        {
            //lock so that enqueue and dequeue won't happen at the same moment
            lock (conn_key)
            {
                //In order to prevent situtaion where there are more connections than allowed.
                //If we will use the RestartPool and after the method will finish, another old connection will return there will be more connections than allowed.
                if (_connections.Count < MAX_CONN)
                {
                    if (conn != null)
                    {
                        conn.Close();
                    }

                    _connections.Enqueue(conn);
                }
                //The pulse will notify other threads that are waiting to the key to return from the "waiting room". one of them will leave the "waiting room"
                Monitor.Pulse(conn_key);
            }
        }

        /// <summary>
        /// Reastarts the connections in the pool
        /// </summary>
        public void RestartPool()
        {
            lock (conn_key)
            {
                //Clear all the queue of connections
                _connections.Clear();
                for (int i = 0; i < MAX_CONN; i++)
                {
                    //Add new connection to the end of the queue
                    _connections.Enqueue(new NpgsqlConnection(conn_string));
                }

                //Awakes all the threads that are in the "waiting room"
                Monitor.PulseAll(conn_key);
            }
        }

        /// <summary>
        /// Test if the DB can be connected
        /// </summary>
        /// <returns>true if there is connection</returns>
        public bool TestDbConnection()
        {
            _logger.Debug("Testing db access");
            try
            {
                using (var conn = new NpgsqlConnection(conn_string))
                {
                    conn.Open();
                    _logger.Debug("Testing db access. succeed!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Testing db access. Failed!. Error: {ex}");
                return false;
            }
        }
    }
}
