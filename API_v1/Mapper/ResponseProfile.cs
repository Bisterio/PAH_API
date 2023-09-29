using API.Request;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class ResponseProfile : Profile
    {
        public ResponseProfile()
        {

            CreateMap<ResponseRequest, DataAccess.Models.Response>();
        }
    }
}
