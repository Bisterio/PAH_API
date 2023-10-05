using API.Request;
using API.Response.AddressRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper {
    public class AddressProfile : Profile{
        public AddressProfile() {
            CreateMap<AddressRequest, Address>();
            CreateMap<Address, AddressResponse>();
        }
    }
}
