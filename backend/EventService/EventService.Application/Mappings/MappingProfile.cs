using AutoMapper;
using EventService.Domain.Entities;
using EventService.Application.DTOs;

namespace EventService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventResponse>()
            .ForMember(dest => dest.Zones, opt => opt.MapFrom(src => src.Zones));
        
        CreateMap<Zone, ZoneResponse>();
        
        CreateMap<CreateEventRequest, Event>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Published"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Zones, opt => opt.Ignore());
        
        CreateMap<ZoneRequest, Zone>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Event, opt => opt.Ignore());
    }
}
