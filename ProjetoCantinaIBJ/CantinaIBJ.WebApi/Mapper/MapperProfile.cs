﻿using AutoMapper;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Core;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.User;
using CantinaIBJ.WebApi.Models.Create.Core;
using CantinaIBJ.WebApi.Models.Create.Customer;
using CantinaIBJ.WebApi.Models.Create.Product;
using CantinaIBJ.WebApi.Models.Create.User;
using CantinaIBJ.WebApi.Models.Read.Core;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Read.Product;
using CantinaIBJ.WebApi.Models.Read.User;
using CantinaIBJ.WebApi.Models.Update.Core;
using CantinaIBJ.WebApi.Models.Update.Customer;
using CantinaIBJ.WebApi.Models.Update.Product;
using CantinaIBJ.WebApi.Models.Update.User;

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

        CreateMap<UserCreateModel, User>()
            .ForMember(dst => dst.PasswordHash, map => map.MapFrom(src => src.Password));
        CreateMap<UserUpdateModel, User>()
            .ForMember(dst => dst.PasswordHash, map => map.MapFrom(src => src.Password));
        CreateMap<User, UserReadModel>()
            .IncludeBase<BaseModel, BaseReadModel>();
    }
}