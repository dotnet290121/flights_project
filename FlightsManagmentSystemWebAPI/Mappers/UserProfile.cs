using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;

namespace FlightsManagmentSystemWebAPI.Mappers
{
    public class UserProfile:Profile
    {
        public UserProfile()
        {
            CreateMap<CreateUserDTO, User>();
            CreateMap<User, UserDetailsDTO>();
        }
    }
}
