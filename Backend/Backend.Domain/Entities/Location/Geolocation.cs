namespace Backend.Domain.Entities.Location;

public sealed class Geolocation
{
    public int ZipCodePrefix { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
}
