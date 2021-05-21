using Domain.Entities;
using Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace BL_Tests
{
    internal static class TestData
    {
        internal static Administrator[] Get_Administrators_Data()
        {
            return new Administrator[]
            {
                new Administrator("admin_1", "admin_1", AdminLevel.Junior_Admin, new User("admin_level_1", "11111", "admin1@admin.com", UserRoles.Administrator)),
                new Administrator("admin_2", "admin_2", AdminLevel.Mid_Level_Admin, new User("admin_level_2", "22222", "admin2@admin.com", UserRoles.Administrator)),
                new Administrator("admin_3", "admin_3", AdminLevel.Senior_Admin, new User("admin_level_3", "33333", "admin3@admin.com", UserRoles.Administrator)),
                new Administrator("admin_4", "admin_4", AdminLevel.Junior_Admin, new User("admin_level_1", "111", "admin4@admin.com", UserRoles.Administrator)),
                new Administrator("admin_4", "admin_4", AdminLevel.Junior_Admin, new User("admin_level_4", "111", "admin1@admin.com", UserRoles.Administrator))
            };
        }

        internal static Country[] Get_Countries_Data()
        {
            return new Country[]
            {
                new Country("Israel"),
                new Country("USA"),
                new Country("Russia"),
                new Country("France"),
                new Country("UK"),
                new Country("Germany")
            };
        }
        internal static Customer[] Get_Customers_Data()
        {
            return new Customer[]
            {
                new Customer("Cutomer_a","cust","Hamasger London","0506817521","41584121587211", new User("customer_1", "111_11", "customer1@cust.com", UserRoles.Customer)),
                new Customer("Cutomer_b","custo","Haarba Barcelona","0542786593","59613215873741", new User("customer_2", "222_22", "customer2@cust.com", UserRoles.Customer)),
                new Customer("Cutomer_c","custom","Eifel Paris","0587824469","1548734148741", new User("customer_3", "333_33", "customer3@cust.com", UserRoles.Customer)),
                new Customer("Cutomer_d","custome","Some city","0506817521","4512354940180", new User("customer_4", "444_44", "customer4@cust.com", UserRoles.Customer)),
                new Customer("Cutomer_e","customer","Some other city","0587824469","41584121587211", new User("customer_5", "555_55", "customer5@cust.com", UserRoles.Customer))
            };
        }
        internal static AirlineCompany[] Get_AirlineCompanies_Data()
        {
            return new AirlineCompany[]
            {
               new AirlineCompany("El-Al",0,new User("elal","elalelal","elal@elal.com",UserRoles.Airline_Company)),
               new AirlineCompany("Airoflot",0,new User("airoflot","airoflot19","airoflot@airoflot.com",UserRoles.Airline_Company)),
               new AirlineCompany("Lufthansa",0,new User("lufthansa","lufthansa77","lufthansa@lufthansa.com",UserRoles.Airline_Company))
            };
        }

        internal static Flight[] Get_Flights_Data()
        {
            return new Flight[]
            {
                new Flight(null,2,3,DateTime.Now.AddDays(1),DateTime.Now.AddDays(1).AddHours(4),50),
                new Flight(null,1,3,DateTime.Now.AddHours(1),DateTime.Now.AddHours(11),120),
                new Flight(null,3,2,DateTime.Now.AddHours(3),DateTime.Now.AddHours(7),78),
                new Flight(null,1,2,DateTime.Now.AddHours(2),DateTime.Now.AddHours(8),25),//Used in the initializtion of the LoggedInAirlineFacadeTests
                new Flight(null,2,2,DateTime.Now.AddHours(5),DateTime.Now.AddHours(9),11),//Used in the initializtion of the LoggedInAirlineFacadeTests
                new Flight(null,1,2,DateTime.Now.AddHours(3),DateTime.Now.AddHours(9),0),//Used in the initializtion of the LoggedInAirlineFacadeTests
            };
        }
        internal static Flight[] Get_Flights_Data_For_Anonymous_Tests()
        {
            return new Flight[]
            {
                new Flight(null,2,3,DateTime.Now.AddDays(1),DateTime.Now.AddDays(3),50),
                new Flight(null,1,3,DateTime.Now.AddDays(1),DateTime.Now.AddDays(2),120),
                new Flight(null,3,2,DateTime.Now.AddDays(2),DateTime.Now.AddDays(3),78),
                new Flight(null,1,2,DateTime.Now.AddDays(4),DateTime.Now.AddDays(5),25),//Used in the initializtion of the LoggedInAirlineFacadeTests
                new Flight(null,2,2,DateTime.Now.AddDays(1),DateTime.Now.AddDays(2),11),//Used in the initializtion of the LoggedInAirlineFacadeTests
                new Flight(null,1,2,DateTime.Now.AddDays(3),DateTime.Now.AddDays(5),0),//Used in the initializtion of the LoggedInAirlineFacadeTests
            };
        }

        internal static void CompareProps(object object_a, object object_b, bool ignore_user = false)
        {

            Type type_a = object_a.GetType();
            Type type_b = object_b.GetType();
            Assert.AreEqual(type_a, type_b);
            PropertyInfo[] props_a = type_a.GetProperties();
            PropertyInfo[] props_b = type_a.GetProperties();

            for (int i = 0; i < props_a.Length; i++)
            {
                Type prop_type = props_a[i].PropertyType;
                if (ignore_user && prop_type == typeof(User))
                    continue;

                if (prop_type.GetInterfaces().Contains(typeof(IPoco)))
                {
                    if (props_a[i].GetValue(object_a) == null && props_a[i].GetValue(object_b) == null)
                        continue;

                    CompareProps(props_a[i].GetValue(object_a), props_a[i].GetValue(object_b), ignore_user);
                }
                else if (prop_type == typeof(DateTime))
                {
                    CompareDates((DateTime)props_a[i].GetValue(object_a), (DateTime)props_b[i].GetValue(object_b));
                }
                else
                {
                    Assert.AreEqual(props_a[i].GetValue(object_a), props_b[i].GetValue(object_b));
                }
            }
        }

        internal static void CompareDates(DateTime object_a, DateTime object_b)
        {
            Assert.AreEqual(object_a.Year, object_b.Year);
            Assert.AreEqual(object_a.Month, object_b.Month);
            Assert.AreEqual(object_a.Day, object_b.Day);
            Assert.AreEqual(object_a.Hour, object_b.Hour);
            Assert.AreEqual(object_a.Minute, object_b.Minute);
        }
    }
}
