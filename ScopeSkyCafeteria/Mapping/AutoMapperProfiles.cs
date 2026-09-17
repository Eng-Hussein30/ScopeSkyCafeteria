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
                .ForMember(dest => dest.ProductName,opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));

            CreateMap<AddOrderItemDTO, OrderItem>();

            // User
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<AddUserDTO, User>();


            // Wallet
            CreateMap<Wallet, WalletDTO>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.FirstName,
                    opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName,
                    opt => opt.MapFrom(src => src.User.LastName));

            CreateMap<WalletTransaction, WalletTransactionDTO>()
                .ForMember(dest => dest.PerformedByUserName,
                    opt => opt.MapFrom(src =>
                        src.PerformedByUser != null
                            ? src.PerformedByUser.UserName
                            : null));
        }
    }
}