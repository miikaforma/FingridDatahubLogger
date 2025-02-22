namespace FingridDatahubLogger.Services.DatabaseModels;

public class DbMeteringPoint
{
    public string MeteringPointEan { get; set; }
    public string Type { get; set; }
    public string? StreetName { get; set; }
    public string? BuildingNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? PostOffice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}