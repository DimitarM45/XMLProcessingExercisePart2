namespace CarDealer;

using Models;
using DTOs.Import;
using DTOs.Export;

using AutoMapper;

public class CarDealerProfile : Profile
{
    public CarDealerProfile()
    {
        CreateMap<ImportSupplierDto, Supplier>();

        CreateMap<ImportPartDto, Part>();

        CreateMap<ImportCarDto, Car>();

        CreateMap<ImportCustomerDto, Customer>();

        CreateMap<ImportSaleDto, Sale>();

        CreateMap<Car, ExportCarDto>();

        CreateMap<Car, ExportCarAttributesDto>();

        CreateMap<Supplier, ExportSupplierDto>();

        CreateMap<Car, ExportCarPartsDto>()
            .ForMember(cdto => cdto.Parts,
                otp => otp.MapFrom(src => src.PartsCars.Select(ps => ps.Part).OrderByDescending(p => p.Price)));

        CreateMap<Part, ExportPartAttributesDto>();

        CreateMap<Car, ExportCarSaleDto>();

        CreateMap<Sale, ExportSaleDto>()
            .ForMember(sdto => sdto.Discount,
                otp => otp.MapFrom(src => src.Discount))
            .ForMember(sdto => sdto.Price,
                otp => otp.MapFrom(src => src.Car.PartsCars.Sum(ps => ps.Part.Price)))
            .ForMember(sdto => sdto.PriceWithDiscount,
                otp => otp.MapFrom(src => Math.Round((double)(src.Car.PartsCars.Sum(p => p.Part.Price) * (1 - (src.Discount / 100))), 4)));
    }
}
