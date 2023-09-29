using API.Request;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            CreateMap<FeedbackRequest, Feedback>();
        }
    }
}
