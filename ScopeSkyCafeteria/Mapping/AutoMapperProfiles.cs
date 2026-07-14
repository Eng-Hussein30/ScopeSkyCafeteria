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
            // =========================
            // Order
            // =========================
            CreateMap<Order, OrdersDTO>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));
            CreateMap<AddOrdersDTO, Order>().ReverseMap();
            CreateMap<UpdateOrdersDTO, Order>().ReverseMap();


            // =========================
            // OrderItem
            // =========================

            CreateMap<OrderItem, OrderItemDTO>().ReverseMap();
                CreateMap<AddOrderItemDTO, OrderItem>().ReverseMap();
                CreateMap<UpdateOrderItemDTO, OrderItem>().ReverseMap();


            // =========================
            // Product
            // =========================

            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<CreateProductDto, Product>().ReverseMap();
            CreateMap<UpdateProductsDTO , Product>();

            // =========================
            // Category
            // =========================

            CreateMap<Category, CategoriesDTO>().ReverseMap();

            CreateMap<AddCategoriesDTO, Category>().ReverseMap();
            CreateMap<UpdateCategoriesDTO, Category>().ReverseMap();


            // =========================
            // User
            // =========================

                CreateMap<User, UserDTO>().ReverseMap();
                CreateMap<AddUserDTO, User>().ReverseMap();


        }
    }
}