using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Factories.Vacunas;

/// <summary>
/// Request object (P-02): una sola forma de pedir una vacuna; la categoría decide la fábrica.
/// Mata la interfaz método-por-tipo y el ternario del controller.
/// </summary>
public sealed record DatosVacuna(
    VacunaCategoria Categoria,
    string Nombre,
    string Lote,
    DateTime FechaVencimiento,
    DateTime FechaAplicacion,
    uint? PeriodoAplicacion = null,
    Viva.GradoAtenuacion? Atenuacion = null);
