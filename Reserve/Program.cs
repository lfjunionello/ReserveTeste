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
    return Results.Ok("Disponibilidade");
});

/* var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi(); */

app.Run();

/* record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
} */
