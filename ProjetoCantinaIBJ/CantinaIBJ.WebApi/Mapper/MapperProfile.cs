using AutoMapper;
using CantinaIBJ.Framework.Helpers;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Core;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model.Orders;
using CantinaIBJ.WebApi.Helpers;
using CantinaIBJ.WebApi.Models.Create;
using CantinaIBJ.WebApi.Models.Create.Core;
using CantinaIBJ.WebApi.Models.Create.Customer;
using CantinaIBJ.WebApi.Models.Create.Order;
using CantinaIBJ.WebApi.Models.Create.Product;
using CantinaIBJ.WebApi.Models.Read;
using CantinaIBJ.WebApi.Models.Read.Core;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Read.Order;
using CantinaIBJ.WebApi.Models.Read.Product;
using CantinaIBJ.WebApi.Models.Update;
using CantinaIBJ.WebApi.Models.Update.Core;
using CantinaIBJ.WebApi.Models.Update.Customer;
using CantinaIBJ.WebApi.Models.Update.Order;
using CantinaIBJ.WebApi.Models.Update.Product;

namespace CantinaIBJ.WebApi.Mapper;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<BaseCreateModel, BaseModel>()
            .AddTransform<string>(s => string.IsNullOrWhiteSpace(s) ? null : s.Trim());

        CreateMap<BaseUpdateModel, BaseModel>()
            .AddTransform<string>(s => string.IsNullOrWhiteSpace(s) ? null : s.Trim());

        CreateMap<BaseModel, BaseReadModel>();

        CreateMap<CustomerPersonCreateModel, CustomerPerson>();
        CreateMap<CustomerPersonUpdateModel, CustomerPerson>();
        CreateMap<CustomerPerson, CustomerPersonReadModel>()
            .IncludeBase<BaseModel, BaseReadModel>();

        CreateMap<ProductCreateModel, Product>();
        CreateMap<ProductUpdateModel, Product>();
        CreateMap<Product, ProductReadModel>()
            .IncludeBase<BaseModel, BaseReadModel>();

        CreateMap<OrderCreateModel, Order>();
        CreateMap<OrderUpdateModel, Order>();
        CreateMap<Order, OrderReadModel>()
            .IncludeBase<BaseModel, BaseReadModel>()
            .ForMember(dst => dst.CustomerPersonDisplay, 
                map => map.MapFrom(src => src.CustomerPerson.Name))
            .ForMember(dst => dst.StatusDisplay, 
                map => map.MapFrom(src => RandomHelpers.GetEnumDescription(src.Status)))
            .ForMember(dst => dst.PaymentOfTypeDisplay, 
                map => map.MapFrom(src => RandomHelpers.GetEnumDescription(src.PaymentOfType)));

        CreateMap<OrderProductCreateModel, OrderProduct>();
        CreateMap<OrderProductUpdateModel, OrderProduct>();
        CreateMap<OrderProduct, OrderProductReadModel>()
            .ForMember(dst => dst.ProductDisplay, map => map.MapFrom(src => src.Product.Name))
            .ForMember(dst => dst.Name, map => map.MapFrom(src => src.Product.Name))
            .ForMember(dst => dst.Description, map => map.MapFrom(src => src.Product.Description));
    }
}