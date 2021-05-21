using Domain.Interfaces;
using Newtonsoft.Json;

namespace Domain.Entities
{
    public class Country : IPoco
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Country()
        {
        }

        public Country(string name, int id = 0)
        {
            Name = name;
            Id = id;
        }

        public static bool operator ==(Country country1, Country country2)
        {
            if (ReferenceEquals(country1, null) && ReferenceEquals(country2, null))
                return true;
            if (ReferenceEquals(country1, null) || ReferenceEquals(country2, null))
                return false;

            return country1.Id == country2.Id;
        }
        public static bool operator !=(Country country1, Country country2)
        {
            return !(country1 == country2);
        }
        public override bool Equals(object obj)
        {
            Country country = obj as Country;
            return this == country;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
