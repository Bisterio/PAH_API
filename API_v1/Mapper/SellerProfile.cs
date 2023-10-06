using API.Response.SellerRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class SellerProfile : Profile
    {
        public SellerProfile() 
        { 
            CreateMap<Seller, SellerResponse>();
        }
    }
}
