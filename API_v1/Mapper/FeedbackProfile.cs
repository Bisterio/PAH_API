using API.Request;
using API.Response.FeedbackRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            CreateMap<FeedbackRequest, Feedback>();
            CreateMap<Feedback, FeedbackResponse>();
        }
    }
}
