using API.Request;
using API.Response.UserRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper {
    public class UserProfile : Profile{
        public UserProfile() {
            CreateMap<RegisterRequest, User>().ForMember(dest => dest.Name, opt => opt.MapFrom(p => p.Name));
            CreateMap<User, UserResponse>();
            CreateMap<Seller, SellerResponse>();
            CreateMap<User, UserDetailResponse>();
            CreateMap<User, StaffResponse>();
        }
    }
}
