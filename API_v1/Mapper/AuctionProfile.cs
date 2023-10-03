using API.Request;
using API.Response.AuctionRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class AuctionProfile : Profile
    {
        public AuctionProfile() 
        {
            CreateMap<AuctionRequest, Auction>();
            CreateMap<Auction, AuctionResponse>();
        }
    }
}
