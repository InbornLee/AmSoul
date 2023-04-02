using AmSoul.Identity.MongoDB.Models;
using AutoMapper;

namespace AmSoul.Identity.MongoDB.Extensions;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<BaseUser, UserDto>().ReverseMap();
        CreateMap<BaseRole, RoleDto>().ReverseMap();
    }
}
