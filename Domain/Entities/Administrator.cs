using Domain.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public enum AdminLevel
    {
        Junior_Admin=1,
        Mid_Level_Admin=2,
        Senior_Admin=3,
        Main_Admin=4
    }

    public class Administrator : IPoco, IUser
    {
        [Column("admin_id")]
        public int Id { get; set; }
        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }
        public AdminLevel Level { get; set; }
        public User User { get; set; }

        public Administrator()
        {

        }

        public Administrator(string firstName, string lastName, AdminLevel level, User user, int id = 0)
        {
            FirstName = firstName;
            LastName = lastName;
            Level = level;
            User = user;
            Id = id;
        }

        public static bool operator ==(Administrator admin1, Administrator admin2)
        {
            if (ReferenceEquals(admin1, null) && ReferenceEquals(admin2, null))
                return true;
            if (ReferenceEquals(admin1, null) || ReferenceEquals(admin2, null))
                return false;

            return admin1.Id == admin2.Id;
        }
        public static bool operator !=(Administrator admin1, Administrator admin2)
        {
            return !(admin1 == admin2);
        }

        public override bool Equals(object obj)
        {
            Administrator admin = obj as Administrator;
            return this == admin;
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
