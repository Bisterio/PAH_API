using API.Request;
using API.Response.ProductRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper
{
    public class ProductProfile : Profile
    {
        public ProductProfile() 
        {
            CreateMap<ProductRequest, Product>();
            CreateMap<Product, ProductResponse>();
        }
    }
}
