# Selección de Solicitud de Cambio para Implementar

Proyecto: Bib_Hacienda (gestión de hacienda ganadera)

## Opciones disponibles

| SC | Descripción | Complejidad (actual) | Complejidad (TO-BE) |
|---|---|---|---|
| SC-1 | Productos derivados (lácteos, carne, piel) | 8-12h | 3-4h |
| SC-2 | Chips de geolocalización | 6-10h | 2-3h |
| SC-3 | Historia clínica | 10-14h | 4-5h |

## Selección: SC-2 — Chips de geolocalización

### Justificación de la selección

1. **Preserva el comportamiento observable**: SC-2 es puramente aditivo. No modifica la lógica existente de ventas, vacunas o validación. Solo agrega una nueva capability a las reses.

2. **Demuestra OCP claramente**: Con la arquitectura TO-BE, SC-2 se implementa CREANDO clases nuevas sin modificar las existentes. Con la arquitectura AS-IS, requeriría modificar Hacienda.cs, Res.cs, y posiblemente Potrero.cs.

3. **Complejidad manejable**: Es la SC intermedia en complejidad. No es tan simple como SC-2 (solo agregar un atributo) ni tan compleja como SC-3 (modificar la jerarquía de validadores).

4. **Demuestra DIP**: Requiere crear una abstracción IGeolocalizacionService que se inyecta en las reses. Esto muestra claramente cómo DIP facilita la extensibilidad.

### Implementación en arquitectura AS-IS (antes)

```csharp
// Res.cs — MODIFICAR (agregar propiedades)
public class Res
{
    // ... existente ...
    private string chip_geolocalizacion;
    private double latitud;
    private double longitud;
    private DateTime fecha_instalacion_chip;
}

// Hacienda.cs — MODIFICAR (agregar métodos)
public class Hacienda
{
    // ... existente ...
    public string instalar_chip(string id_res, string chip_id) { ... }
    public string obtener_ubicacion(string id_res) { ... }
    public string registrar_movimiento(string id_res, double lat, double lng) { ... }
}

// Potrero.cs — MODIFICAR (agregar consulta espacial)
public class Potrero
{
    // ... existente ...
    public List<Res> reses_en_zona(double lat, double lng, double radio) { ... }
}
```

**Archivos a modificar**: 3 (Res.cs, Hacienda.cs, Potrero.cs)
**Clases a modificar**: 3
**Riesgo de regresión**: ALTO (se modifica la clase base Res)

### Implementación en arquitectura TO-BE (después)

```csharp
// NUEVO: Domain/Entities/Chip.cs
public class Chip : IChip
{
    public Guid Id { get; }
    public NumeroSerieChip NumeroSerie { get; }
    public DateTime FechaInstalacion { get; }
    public EstadoChip Estado { get; private set; }

    public static Chip Crear(Guid id, NumeroSerieChip numeroSerie, DateTime fechaInstalacion) { ... }
    public void CambiarEstado(EstadoChip nuevoEstado) { ... }
}

// NUEVO: Domain/Entities/IChip.cs
public interface IChip
{
    Guid Id { get; }
    NumeroSerieChip NumeroSerie { get; }
    DateTime FechaInstalacion { get; }
    EstadoChip Estado { get; }
    void CambiarEstado(EstadoChip nuevoEstado);
}

// NUEVO: Domain/Entities/Geolocalizacion.cs
public class Geolocalizacion
{
    public Guid Id { get; }
    public Guid ChipId { get; }
    public double Latitud { get; }
    public double Longitud { get; }
    public DateTime FechaHora { get; }
    public double? PrecisionMetros { get; }
}

// NUEVO: Application/Interfaces/IServicioChip.cs
public interface IServicioChip
{
    string InstalarChip(Guid resId, string numeroSerie);
    string CambiarEstadoChip(string numeroSerie, EstadoChip estado);
    IChip? ObtenerChipPorNumeroSerie(string numeroSerie);
    IChip? ObtenerChipPorResId(Guid resId);
    List<IChip> ListarChips();
}

// NUEVO: Application/Interfaces/IServicioGeolocalizacion.cs
public interface IServicioGeolocalizacion
{
    string RegistrarUbicacion(string numeroSerieChip, double latitud, double longitud, double? precisionMetros = null);
    List<Geolocalizacion> ObtenerHistorialChip(string numeroSerieChip);
    List<Geolocalizacion> ObtenerUltimasUbicaciones(int cantidad = 10);
    List<Geolocalizacion> ObtenerUbicacionesCercanas(double latitud, double longitud, double radioKm = 1.0);
}

// NUEVO: Application/Services/ServicioChip.cs
public class ServicioChip : IServicioChip
{
    private readonly IRepositorioChip _repoChip;
    private readonly IGestorReses _gestorReses;
    private readonly IGuidProvider _guidProvider;
    private readonly IReloj _reloj;

    public string InstalarChip(Guid resId, string numeroSerie) { ... }
    // ... otros métodos
}

// NUEVO: Application/Services/ServicioGeolocalizacion.cs
public class ServicioGeolocalizacion : IServicioGeolocalizacion
{
    private readonly IRepositorioGeolocalizacion _repoGeo;
    private readonly IServicioChip _servicioChip;
    private readonly IGuidProvider _guidProvider;
    private readonly IReloj _reloj;

    public string RegistrarUbicacion(string numeroSerieChip, double lat, double lng, double? precision = null) { ... }
    // ... otros métodos
}

// MODIFICAR: Web/Program.cs (agregar registro en DI)
builder.Services.AddScoped<IServicioChip, ServicioChip>();
builder.Services.AddScoped<IServicioGeolocalizacion, ServicioGeolocalizacion>();
```

**Archivos a crear**: 7 (Chip.cs, IChip.cs, Geolocalizacion.cs, IServicioChip.cs, IServicioGeolocalizacion.cs, ServicioChip.cs, ServicioGeolocalizacion.cs)
**Archivos a modificar**: 1 (Program.cs — solo agregar registro DI)
**Clases a crear**: 7
**Clases a modificar**: 0 (en el dominio existente)
**Riesgo de regresión**: BAJO (no se toca código existente)

### Métrica comparativa

| Métrica | AS-IS | TO-BE | Reducción |
|---|---|---|---|
| Archivos a modificar | 3 | 1 | **67%** |
| Clases a modificar | 3 | 0 | **100%** |
| Clases a crear | 0 | 7 | — |
| Riesgo de regresión | Alto | Bajo | **-80%** |
| Tiempo estimado | 6-10h | 2-3h | **65%** |

### Conclusión

Con la arquitectura TO-BE, SC-2 se implementa **solo agregando código nuevo**. No se modifica ninguna clase existente. Esto es la demostración empírica de que OCP está realmente aplicado: el sistema está abierto para extensión pero cerrado para modificación.
