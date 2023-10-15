using AutoMapper;
using DataAccess.Models;
using Respon.OrderRes;

namespace API.Mapper {
    public class OrderProfile : Profile {
        public OrderProfile() {
            CreateMap<Order, OrderResponse>();
            CreateMap<OrderItem, OrderItemResponse>().ForMember(dest => dest.ProductName, opt => opt.MapFrom(p => p.Product.Name));
        }
    }
}
