namespace Hacienda.Application.DTOs;

public record PotreroDto(string Identificacion, string Tipo, int CantidadReses);
public record ResDto(string Nombre, string Tipo, uint Peso, ushort Edad, string PotreroId);
public record VacunaDto(string Nombre, string Lote, string Categoria, DateTime FechaVencimiento);
public record VentaDto(DateTime Fecha, string NombreRes, string TipoRes, decimal Monto);