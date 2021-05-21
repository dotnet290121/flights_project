using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigurationService
{
    public class FlightsManagmentSystemConfig
    {
        private static readonly log4net.ILog my_logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //Name of the configuration file
        private string m_file_name;
        //Boolean to check if the instance already initialized
        private bool m_init = false;
        //Represent the root object of the configuration file
        private JObject m_configRoot;

        //static instance of the class
        public static readonly FlightsManagmentSystemConfig Instance = new FlightsManagmentSystemConfig();

        // Members according to json file
        public string ConnectionString { get; set; }
        public int MaxConnections { get; set; }
        public string WorkTime { get; set; }

        private FlightsManagmentSystemConfig()
        {

        }

        /// <summary>
        /// Initilize the instance of the class
        /// </summary>
        /// <param name="file_name"></param>
        public void Init(string file_name = null)
        {
            if (m_init)//If already initilized do not continue
                return;

            m_file_name = file_name != null ? file_name : "FlightsManagmentSystem.Config.json";//Check if there is file name provided if not go for the default
            m_init = true;//Set the init to true

            //If configuration file not exists throw error and quit
            if (!File.Exists(m_file_name))
            {
                my_logger.Fatal($"File {m_file_name} does not exist!");
                Environment.Exit(-1);
            }

            var reader = File.OpenText(m_file_name);//Open the file in order to read
            string json_string = reader.ReadToEnd();//Read the whole file

            JObject all = (JObject)JsonConvert.DeserializeObject(json_string);//Convert the string from the file to JSON object
            m_configRoot = (JObject)all["FlightsManagmentSystem"];
            ConnectionString = m_configRoot["ConnectionString"].Value<string>();
            MaxConnections = m_configRoot["MaxConnections"].Value<int>();
            WorkTime = m_configRoot["WorkTime"].Value<string>();
        }
    }
}
