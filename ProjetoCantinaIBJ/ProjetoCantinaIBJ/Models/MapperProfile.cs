using AutoMapper;
using CantinaIBJ.Model.Core;
using CantinaIBJ.Model.CustomerPerson;
using CantinaIBJ.WebApi.Models.Create.Core;
using CantinaIBJ.WebApi.Models.Create.Customer;
using CantinaIBJ.WebApi.Models.Read.Core;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Update.Core;
using CantinaIBJ.WebApi.Models.Update.Customer;

namespace CantinaIBJ.WebApi.Models;

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
    }
}