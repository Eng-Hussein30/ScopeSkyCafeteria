using AutoMapper;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;

namespace ScopeSkyCafeteria.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Product
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductsDTO, Product>();
            // Category
            CreateMap<Category, CategoriesDTO>().ReverseMap();
            CreateMap<AddCategoriesDTO, Category>();
            CreateMap<UpdateCategoriesDTO, Category>();

            // Order
            CreateMap<Order, OrdersDTO>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.AdminName,
                    opt => opt.MapFrom(src => src.Admin != null ? src.Admin.UserName : null));

            CreateMap<AddOrdersDTO, Order>();

            CreateMap<UpdateOrdersDTO, Order>();

            // OrderItem
            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name));

            CreateMap<AddOrderItemDTO, OrderItem>();
        }
    }
}