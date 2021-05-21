using Domain.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public enum UserRoles
    { 
        Anonymous=0,
        Administrator=1,
        Customer=2,
        Airline_Company=3
    }

    public class User : IPoco
    {
        [Column("user_id")]
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        [Column("user_role_id")]
        public UserRoles UserRole { get; set; }

        public User()
        {

        }

        public User(string userName, string password, string email, UserRoles userRole, long id = 0)
        {
            UserName = userName;
            Password = password;
            Email = email;
            UserRole = userRole;
            Id = id;
        }

        public static bool operator ==(User user1, User user2)
        {
            if (ReferenceEquals(user1, null) && ReferenceEquals(user2, null))
                return true;
            if (ReferenceEquals(user1, null) || ReferenceEquals(user2, null))
                return false;

            return user1.Id == user2.Id;
        }
        public static bool operator !=(User user1, User user2)
        {
            return !(user1 == user2);
        }
        public override bool Equals(object obj)
        {
            User user = obj as User;
            return this == user;
        }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}