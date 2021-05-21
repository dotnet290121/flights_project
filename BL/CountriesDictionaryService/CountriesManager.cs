using BL.LoginService;
using DAL;
using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;

namespace BL.CountriesDictionaryService
{
    public class CountriesManager
    {

        private static CountriesManager _instance;
        private static readonly object key = new object();
        private readonly object refresh_key = new object();
        private static Dictionary<int, string> _countriesDictionary;
        private static readonly ICountryDAO _countryDAO = new CountryDAOPGSQL();

        private CountriesManager()
        {
            _countriesDictionary = new Dictionary<int, string>();
        }
        public static CountriesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (key)
                    {
                        if (_instance == null)
                        {
                            _instance = new CountriesManager();
                            _instance.LoadDictionary();
                            return _instance;
                        }
                    }
                }

                return _instance;
            }
        }

        private void LoadDictionary()
        {
            _countriesDictionary.Clear();

            var countries = _countryDAO.GetAll();
            foreach (var country in countries)
                _countriesDictionary.TryAdd(country.Id, country.Name);
        }

        public string GetCountryName(int id)
        {
            string name;

            if (!_countriesDictionary.TryGetValue(id, out name))
            {
                lock (refresh_key)
                {
                    if (!_countriesDictionary.TryGetValue(id, out name))
                    {
                        LoadDictionary();
                    }
                }
            }

            return name;
        }

        public void RefreshDictionary(LoginToken<Administrator> loginToken)
        {
            lock (refresh_key)
            {
                LoadDictionary();
            }
        }
    }
}
