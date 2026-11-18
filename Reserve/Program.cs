using Reserve.DTOs;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

List<NormalizedFlightDataDto> normalizedFlightsList = new List<NormalizedFlightDataDto>();
List<LatamAirlineDto> latamFlightsList = new List<LatamAirlineDto>();
List<LatamAirlineDto> latamFlightsFilteredList = new List<LatamAirlineDto>();

List<GolAirlineDto> golFlightsList = new List<GolAirlineDto>();
List<GolAirlineDto> golFlightsFilteredList = new List<GolAirlineDto>();

using var flightsHttpClient = new HttpClient();

const string latamFlightsUrl =
    "https://dev.reserve.com.br/airapi/latam/flights?departureCity=Rio%20de%20Janeiro&arrivalCity=S%C3%A3o%20Paulo&departureDate=2027-06-20";

const string golFlightsUrl =
    "https://dev.reserve.com.br/airapi/gol/getavailability?origin=Rio%20de%20Janeiro&destination=S%C3%A3o%20Paulo&date=2027-06-20";


var latamFlightsResponse = await flightsHttpClient.GetAsync(latamFlightsUrl);
latamFlightsResponse.EnsureSuccessStatusCode();

var latamFlights = await latamFlightsResponse.Content.ReadFromJsonAsync<List<LatamAirlineDto>>();
if (latamFlights is not null)
{
    latamFlightsList.AddRange(latamFlights);
}


var golFlightsResponse = await flightsHttpClient.GetAsync(golFlightsUrl);
golFlightsResponse.EnsureSuccessStatusCode();

var golFlights = await golFlightsResponse.Content.ReadFromJsonAsync<List<GolAirlineDto>>();
if (golFlights is not null)
{
    golFlightsList.AddRange(golFlights);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/disponibilidade", (string origem, string destino, DateOnly data) =>
{
    normalizedFlightsList.Clear();

    latamFlightsFilteredList = latamFlightsList.FindAll(flight => 
        flight.DepartureAirport == origem && 
        flight.ArrivalAirport == destino && 
        flight.FlightStart.Date == data.ToDateTime(TimeOnly.MinValue));

    golFlightsFilteredList = golFlightsList.FindAll(flight => 
        flight.OriginAirport == origem && 
        flight.DestinationAirport == destino && 
        flight.DepartureDate.Date == data.ToDateTime(TimeOnly.MinValue));

    foreach (var latamFlight in latamFlightsFilteredList)
    {
        NormalizedFlightDataDto normalizedFlight = new NormalizedFlightDataDto
        {
            Voo = latamFlight.FlightID,
            Companhia = latamFlight.Airline,
            Origem = latamFlight.DepartureAirport,
            Destino = latamFlight.ArrivalAirport,
            Partida = latamFlight.FlightStart,
            Chegada = latamFlight.FlightEnd,
            Tarifa = (decimal)latamFlight.TotalFare
        };

        normalizedFlightsList.Add(normalizedFlight);
    }

    foreach (var golFlight in golFlightsFilteredList)
    {
        NormalizedFlightDataDto normalizedFlight = new NormalizedFlightDataDto
        {
            Voo = golFlight.FlightNumber,
            Companhia = golFlight.Carrier,
            Origem = golFlight.OriginAirport,
            Destino = golFlight.DestinationAirport,
            Partida = golFlight.DepartureDate,
            Chegada = golFlight.ArrivalDate,
            Tarifa = (decimal)golFlight.FarePrice
        };

        normalizedFlightsList.Add(normalizedFlight);
    }

    var cheapestFlightByAirline = normalizedFlightsList
        .GroupBy(flight => flight.Companhia)
        .Select(group => group.MinBy(flight => flight.Tarifa))
        .Where(flight => flight is not null)
        .Cast<NormalizedFlightDataDto>()
        .ToList();

    return Results.Ok(cheapestFlightByAirline);
});


app.Run();
