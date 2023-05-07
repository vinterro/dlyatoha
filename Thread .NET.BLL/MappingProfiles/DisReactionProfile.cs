using AutoMapper;
using Thread_.NET.Common.DTO.Dis;
using Thread_.NET.DAL.Entities.Abstract;

namespace Thread_.NET.BLL.MappingProfiles
{
    public sealed class DisreactionProfile : Profile
    {
        public DisreactionProfile()
        {
            CreateMap<Disreaction, DisreactionDTO>();
            CreateMap<DisreactionDTO, Disreaction>();
        }
    }
}
