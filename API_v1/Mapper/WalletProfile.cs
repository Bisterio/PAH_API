using API.Response.WalletRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class WalletProfile : Profile
    {
        public WalletProfile() 
        { 
            CreateMap<Wallet, WalletCurrentUserResponse>();
        }
    }
}
