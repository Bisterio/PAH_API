using API.Request;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper {
    public class UserProfile : Profile{
        public UserProfile() {
            CreateMap<RegisterRequest, User>().ForMember(dest => dest.Name, opt => opt.MapFrom(p => p.Name));
        }
    }
}
