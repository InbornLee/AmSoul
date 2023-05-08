using AutoMapper;

namespace AmSoul.Identity.MongoDB;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<BaseUser, UserDto>().ReverseMap();
        CreateMap<BaseRole, RoleDto>().ReverseMap();
    }
}
