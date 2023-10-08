using API.Response.SellerRes;
using API.Response.UserRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class SellerProfile : Profile
    {
        public SellerProfile() 
        { 
            CreateMap<Seller, SellerWithAddressResponse>();
            CreateMap<Seller, SellerDetailResponse>();
        }
    }
}
