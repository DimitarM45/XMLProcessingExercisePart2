namespace CarDealer.DTOs.Export;

using System.Xml.Serialization;

[XmlType("car")]

public class ExportCarPartsDto
{
    [XmlAttribute("make")]

    public string? Make { get; set; }

    [XmlAttribute("model")]

    public string? Model { get; set; }

    [XmlAttribute("traveled-distance")]

    public long TraveledDistance { get; set; }

    [XmlArray("parts")]

    public ExportPartAttributesDto[]? Parts { get; set; }
}
