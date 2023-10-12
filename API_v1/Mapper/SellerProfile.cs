using API.Request;
using API.Response.SellerRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class SellerProfile : Profile
    {
        public SellerProfile() 
        {
            CreateMap<SellerRequest, Seller>();
            CreateMap<Seller, SellerWithAddressResponse>();
            CreateMap<Seller, SellerDetailResponse>();
        }
    }
}
