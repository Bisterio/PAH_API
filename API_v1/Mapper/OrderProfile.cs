using API.Request;
using API.Response.OrderRes;
using AutoMapper;
using DataAccess.Models;

namespace API.Mapper {
    public class OrderProfile : Profile {
        public OrderProfile() {
            CreateMap<Order, OrderResponse>();
            CreateMap<OrderItem, OrderItemResponse>().ForMember(dest => dest.ProductName, opt => opt.MapFrom(p => p.Product.Name));
        }
    }
}
