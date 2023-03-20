namespace CarDealer;

using Data;
using Models;
using Utilities;
using DTOs.Import;
using DTOs.Export;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using Microsoft.EntityFrameworkCore;

public class StartUp
{
    public static void Main()
    {
        using CarDealerContext context = new CarDealerContext();
    }

    //Problem 1

    public static string ImportSuppliers(CarDealerContext context, string inputXml)
    {
        XmlHelper serializer = new XmlHelper();

        ImportSupplierDto[] supplierDtos = serializer.Deserialize<ImportSupplierDto[]>(inputXml, "Suppliers");

        IMapper mapper = CreateMapper();

        Supplier[] suppliers = mapper.Map<Supplier[]>(supplierDtos);

        context.Suppliers?.AddRange(suppliers);
        context.SaveChanges();

        return $"Successfully imported {suppliers.Length}";
    }

    //Problem 2

    public static string ImportParts(CarDealerContext context, string inputXml)
    {
        XmlHelper serializer = new XmlHelper();

        ImportPartDto[] partDtos = serializer.Deserialize<ImportPartDto[]>(inputXml, "Parts");

        IMapper mapper = CreateMapper();

        ICollection<Part> validParts = new HashSet<Part>();

        foreach (ImportPartDto partDto in partDtos)
        {
            if (context.Suppliers!.AsNoTracking().Any(s => s.Id == partDto.SupplierId))
            {
                Part validPart = mapper.Map<Part>(partDto);

                validParts.Add(validPart);
            }
        }

        context.Parts?.AddRange(validParts);
        context.SaveChanges();

        return $"Successfully imported {validParts.Count}";
    }

    //Problem 3

    public static string ImportCars(CarDealerContext context, string inputXml)
    {
        XmlHelper serializer = new XmlHelper();

        ImportCarDto[] carDtos = serializer.Deserialize<ImportCarDto[]>(inputXml, "Cars");

        ICollection<Car> validCars = new HashSet<Car>();

        ICollection<PartCar> validPartsCars = new HashSet<PartCar>();

        IMapper mapper = CreateMapper();

        int carCounter = 0;

        foreach (ImportCarDto carDto in carDtos)
        {
            carCounter++;

            Car validCar = mapper.Map<Car>(carDto);

            validCars.Add(validCar);

            foreach (int partId in carDto.Parts!.Select(p => p.Id).Distinct()!)
            {
                if (context.Parts!.AsNoTracking().Any(p => p.Id == partId))
                {
                    PartCar validPartCar = new PartCar()
                    {
                        PartId = partId,
                        CarId = carCounter
                    };

                    validPartsCars.Add(validPartCar);
                }
            }
        }

        context.Cars?.AddRange(validCars);
        context.PartsCars?.AddRange(validPartsCars);
        context.SaveChanges();

        return $"Successfully imported {validCars.Count}";
    }

    //Problem 4

    public static string ImportCustomers(CarDealerContext context, string inputXml)
    {
        XmlHelper serializer = new XmlHelper();

        ImportCustomerDto[] customerDtos = serializer.Deserialize<ImportCustomerDto[]>(inputXml, "Customers");

        IMapper mapper = CreateMapper();

        Customer[] customers = mapper.Map<Customer[]>(customerDtos);

        context.Customers?.AddRange(customers);
        context.SaveChanges();

        return $"Successfully imported {customers.Length}";
    }

    //Problem 5

    public static string ImportSales(CarDealerContext context, string inputXml)
    {
        XmlHelper serializer = new XmlHelper();

        ImportSaleDto[] saleDtos = serializer.Deserialize<ImportSaleDto[]>(inputXml, "Sales");

        IMapper mapper = CreateMapper();

        ICollection<Sale> validSales = new HashSet<Sale>();

        foreach (ImportSaleDto saleDto in saleDtos)
        {
            if (context.Cars!.AsNoTracking().Any(c => c.Id == saleDto.CarId))
            {
                Sale validSale = mapper.Map<Sale>(saleDto);

                validSales.Add(validSale);
            }
        }

        context.Sales?.AddRange(validSales);
        context.SaveChanges();

        return $"Successfully imported {validSales.Count}";
    }

    //Problem 6

    public static string GetCarsWithDistance(CarDealerContext context)
    {
        IMapper mapper = CreateMapper();

        var cars = context.Cars?
            .AsNoTracking()
            .Where(c => c.TraveledDistance > 2000000)
            .OrderBy(c => c.Make)
            .ThenBy(c => c.Model)
            .Take(10)
            .ProjectTo<ExportCarDto>(mapper.ConfigurationProvider)
            .ToArray();

        XmlHelper serializer = new XmlHelper();

        string carsXml = serializer.Serialize(cars!, "cars");

        return carsXml;
    }

    //Problem 7

    public static string GetCarsFromMakeBmw(CarDealerContext context)
    {
        IMapper mapper = CreateMapper();

        var bmwCars = context.Cars?
            .AsNoTracking()
            .Where(c => c.Make == "BMW")
            .OrderBy(c => c.Model)
            .ThenByDescending(c => c.TraveledDistance)
            .ProjectTo<ExportCarAttributesDto>(mapper.ConfigurationProvider)
            .ToArray();

        XmlHelper serializer = new XmlHelper();

        string bmwCarsXml = serializer.Serialize(bmwCars!, "cars");

        return bmwCarsXml;
    }

    //Problem 8

    public static string GetLocalSuppliers(CarDealerContext context)
    {
        IMapper mapper = CreateMapper();

        var localSuppliers = context.Suppliers?
            .AsNoTracking()
            .Where(s => !s.IsImporter)
            .ProjectTo<ExportSupplierDto>(mapper.ConfigurationProvider)
            .ToArray();

        XmlHelper serializer = new XmlHelper();

        string localSuppliersXml = serializer.Serialize(localSuppliers!, "suppliers");

        return localSuppliersXml;
    }

    //Problem 9

    public static string GetCarsWithTheirListOfParts(CarDealerContext context)
    {
        IMapper mapper = CreateMapper();

        var carsParts = context.Cars?
            .AsNoTracking()
            .Include(c => c.PartsCars)
            .OrderByDescending(c => c.TraveledDistance)
            .ThenBy(c => c.Model)
            .Take(5)
            .ProjectTo<ExportCarPartsDto>(mapper.ConfigurationProvider)
            .ToArray();

        XmlHelper serializer = new XmlHelper();

        string carsPartsXml = serializer.Serialize(carsParts!, "cars");

        return carsPartsXml;
    }

    //Problem 10 

    public static string GetTotalSalesByCustomer(CarDealerContext context)
    {
        var tempCustomerDtos = context.Customers?
            .AsNoTracking()
            .Where(c => c.Sales.Count >= 1)
            .Select(c => new
            {
                FullName = c.Name,
                BoughtCars = c.Sales.Count,
                Sales = c.Sales
                    .Select(s => new
                    {
                        CarPrices = c.IsYoungDriver
                            ? s.Car.PartsCars.Sum(ps => Math.Round((double)ps.Part.Price * 0.95, 2))
                            : s.Car.PartsCars.Sum(ps => (double)ps.Part.Price)
                    })
                    .ToArray()
            })
            .ToArray();

        ExportCustomerSaleDto[]? customerSales = tempCustomerDtos?
            .OrderByDescending(t => t.Sales.Sum(s => s.CarPrices))
            .Select(t => new ExportCustomerSaleDto()
            {
                FullName = t.FullName,
                BoughtCars = t.BoughtCars,
                SpentMoney = t.Sales.Sum(s => s.CarPrices).ToString("f2")
            })
            .ToArray();

        XmlHelper serializer = new XmlHelper();

        string customerSalesXml = serializer.Serialize(customerSales, "customers");

        return customerSalesXml;
    }

    //Problem 11

    public static string GetSalesWithAppliedDiscount(CarDealerContext context)
    {
        IMapper mapper = CreateMapper();

        var sales = context.Sales?
            .AsNoTracking()
            .ProjectTo<ExportSaleDto>(mapper.ConfigurationProvider)
            .ToArray();

        XmlHelper serializer = new XmlHelper();

        string salesXml = serializer.Serialize(sales, "sales");

        return salesXml;
    }

    private static IMapper CreateMapper()
    {
        IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CarDealerProfile>();
        }));

        return mapper;
    }
}