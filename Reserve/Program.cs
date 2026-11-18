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
    normalizedFlightsList = latamFlightsList
        .Where(flight => flight.DepartureAirport == origem &&
        flight.ArrivalAirport == destino &&
        flight.FlightStart.Date == data.ToDateTime(TimeOnly.MinValue).Date)
        .Select(flight => new NormalizedFlightDataDto
        {
            Voo = flight.FlightID,
            Companhia = flight.Airline,
            Origem = flight.DepartureAirport,
            Destino = flight.ArrivalAirport,
            Partida = flight.FlightStart,
            Chegada = flight.FlightEnd,
            Tarifa = (decimal)flight.TotalFare
        })
        .Concat(golFlightsList
            .Where(flight => flight.OriginAirport == origem &&
            flight.DestinationAirport == destino &&
            flight.DepartureDate.Date == data.ToDateTime(TimeOnly.MinValue))
            .Select(flight => new NormalizedFlightDataDto{
                Voo = flight.FlightNumber,
                Companhia = flight.Carrier,
                Origem = flight.OriginAirport,
                Destino = flight.DestinationAirport,
                Partida = flight.DepartureDate,
                Chegada = flight.ArrivalDate,
                Tarifa = (decimal)flight.FarePrice
            }))
            .ToList();


    var cheapestFlightByAirline = normalizedFlightsList
        .GroupBy(flight => flight.Companhia)
        .Select(group => group.MinBy(flight => flight.Tarifa))
        .Where(flight => flight is not null)
        .Cast<NormalizedFlightDataDto>();

    return Results.Ok(cheapestFlightByAirline);
});


app.Run();
