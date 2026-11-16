using AutoMapper;
using ERP.Application.Auth.Dtos;
using ERP.Domain.Entities;

namespace ERP.Application.Commons.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<RegisterUserDto, AppUser>()
            .ForMember(dest=>dest.UserName , opt => opt.MapFrom(src => src.Email));
    }
}
