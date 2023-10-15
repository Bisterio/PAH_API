using API.Request;
using API.Response.BidRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class BidProfile : Profile
    {
        public BidProfile() 
        { 
            CreateMap<Bid, BidResponse>();
            CreateMap<BidRequest, Bid>();
        }
    }
}
