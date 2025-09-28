using AutoMapper;
using firstProject.Model;
using Shared;

namespace firstProject.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<DataTransferObject.UserDTO, User>().ReverseMap();
            CreateMap<DataTransferObject.BrokerDTO, User>().ReverseMap();
            CreateMap<DataTransferObject.BrokerDTO, DataTransferObject.UserDTO>().ReverseMap();
            CreateMap<DataTransferObject.BrokerDTO, DataTransferObject.CompanyDTO>().ReverseMap();
        }
    }
}
