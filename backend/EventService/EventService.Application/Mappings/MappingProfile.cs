using AutoMapper;
using EventService.Domain.Entities;
using EventService.Application.DTOs;
using EventService.Application.Commands;

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
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => 
                src.Date.Kind == DateTimeKind.Utc 
                    ? src.Date 
                    : src.Date.ToUniversalTime()))
            .ForMember(dest => dest.Zones, opt => opt.Ignore());
        
        // Mapeo de CreateEventCommand a Event (usado en el handler)
        CreateMap<CreateEventCommand, Event>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Published"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => 
                src.Date.Kind == DateTimeKind.Utc 
                    ? src.Date 
                    : src.Date.ToUniversalTime()))
            .ForMember(dest => dest.Zones, opt => opt.Ignore());
        
        CreateMap<ZoneRequest, Zone>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Event, opt => opt.Ignore());
    }
}
