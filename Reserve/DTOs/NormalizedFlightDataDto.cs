namespace Reserve.DTOs;
public class NormalizedFlightDataDto
{
    public string Voo { get; set; }
    public string Companhia { get; set; }
    public string Origem { get; set; }
    public string Destino { get; set; }
    public DateTime Partida { get; set; }
    public DateTime Chegada { get; set; }
    public decimal Tarifa { get; set; }
}