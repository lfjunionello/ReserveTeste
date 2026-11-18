namespace Reserve.DTOs;

public class GolAirlineDto
{
    public string FlightNumber { get; set; }
    public string Carrier { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public string OriginAirport { get; set; }
    public string DestinationAirport { get; set; }
    public double FarePrice { get; set; }
}