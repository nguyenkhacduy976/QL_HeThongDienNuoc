using AutoMapper;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;

namespace QL_HethongDiennuoc.Helpers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Customer mappings
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Meter mappings
        CreateMap<Meter, MeterDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName));
        CreateMap<CreateMeterDto, Meter>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (MeterType)src.Type))
            .ForMember(dest => dest.InstallDate, opt => opt.MapFrom(src => src.InstallDate ?? DateTime.Now));

        // Reading mappings
        CreateMap<Reading, ReadingDto>()
            .ForMember(dest => dest.MeterNumber, opt => opt.MapFrom(src => src.Meter.MeterNumber));
        CreateMap<CreateReadingDto, Reading>();

        // Bill mappings
        CreateMap<Bill, BillDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
            .ForMember(dest => dest.Consumption, opt => opt.MapFrom(src => src.Reading.Consumption))
            .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => src.Reading.Meter.Type.ToString()));

        // Payment mappings
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.ToString()))
            .ForMember(dest => dest.BillNumber, opt => opt.MapFrom(src => src.Bill.BillNumber));
        CreateMap<CreatePaymentDto, Payment>()
            .ForMember(dest => dest.Method, opt => opt.MapFrom(src => (PaymentMethod)src.Method));

        // Service mappings
        CreateMap<Service, ServiceStatusDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName));
    }
}
