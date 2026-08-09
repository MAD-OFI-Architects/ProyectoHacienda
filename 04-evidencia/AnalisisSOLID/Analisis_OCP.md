# Analisis OCP — Open/Closed Principle

**Proyectos:** `Bib_Hacienda` + `p_mvcHacienda`
**Hallazgos:** 3 CRITICAL, 15 WARNING, 2 SUGGESTION = 20 hallazgos

---

## Analisis de blast-radius (el costo de extension)

### Agregar un nuevo subtipo de `Res` (ej. `Toro`) — 11 modificaciones en 8 archivos:

| # | Archivo | Que cambia |
|---|---------|-----------|
| 1 | `Potrero.cs:15` | Agregar valor al enum `l_tipos_potreros` |
| 2 | `Potrero.cs:62-83` | Agregar case al switch #1 (rango de edad + string tag) |
| 3 | `Potrero.cs:88-102` | Agregar case al switch #2 (dispatch de constructor) |
| 4 | `ReglaRes.cs:12-21` | Agregar constantes `peso_min_*`, `peso_recom_venta_*` |
| 5 | `ReglaVacuna.cs:13-18` | Agregar constantes `max_bac_*`, `max_viv_*` |
| 6 | `Hacienda.cs:487-501` | Agregar `else if (res is Toro)` para limites de vacuna |
| 7 | `PublisherPesoMin.cs:25-27` | Agregar `else if (res is Toro)` para peso minimo |
| 8 | `PublisherPesoVenta.cs:27-29` | Agregar `else if (res is Toro)` para peso venta |
| 9 | `PublisherVacunacionCompletada.cs:30-41` | Agregar `else if (res is Toro)` para esquema |
| 10 | `ResService.cs:66-68` | Agregar estadistica `Count(r => r.Res is Toro)` |
| 11 | `PersistenciaService.cs:434-440` | Agregar case al switch de deserializacion |

### Agregar un nuevo subtipo de `Vacuna` (ej. `Toxoide`) — 12 modificaciones en 7 archivos:

| # | Archivo | Que cambia |
|---|---------|-----------|
| 1 | `ICreacionVacuna.cs:10-16` | Agregar sobrecarga(s) |
| 2 | `Hacienda.cs:268-447` | Agregar sobrecarga(s) de `crear_vacuna` |
| 3 | `Hacienda.cs:474-484` | Agregar `else if (vac is Toxoide)` al conteo |
| 4 | `Hacienda.cs:504-508` | Agregar branch de validacion de limite |
| 5 | `Hacienda.cs:531-538` | Agregar branch de actualizacion de contador |
| 6 | `VacunaService.cs:27-39` | Agregar branch de creacion |
| 7 | `VacunaController.cs:94-116` | Agregar `if (tipoVacuna == "...")` |
| 8 | `VacunaService.cs:142-143` | Agregar estadistica |
| 9 | `PersistenciaService.cs:496-515` | Agregar branch de deserializacion |
| 10 | `PersistenciaService.cs:580-587` | Agregar branch de deserializacion (vacunas aplicadas) |
| 11 | `PersistenciaService.cs:201,256` | Modificar `is Bacteriana` en serializacion |
| 12 | `ReglaVacuna.cs` | Posibles constantes nuevas de periodo |

---

## Violaciones CRITICAL

### OCP-1 — `Hacienda.aplicar_vacuna`: dispatch de limites de vacuna por cadena `is`

**Ubicacion:** `Bib_Hacienda/Clases/Hacienda.cs:487-501`

```csharp
if (res is Ternero) { max_bac = ReglaVacuna.max_bac_ternero; max_viv = ReglaVacuna.max_viv_ternero; }
else if (res is Novillo) { max_bac = ReglaVacuna.max_bac_novillo; max_viv = ReglaVacuna.max_viv_novillo; }
else if (res is Cebon) { max_bac = ReglaVacuna.max_bac_cebon; max_viv = ReglaVacuna.max_viv_cebon; }
// → si ninguno coincide, max_bac y max_viv quedan en 0
```

Si se olvida el branch, `max_bac = max_viv = 0` → `0 >= 0` → lanza `"Ya tiene las 0 permitidas"`. El subtipo nuevo **nunca puede vacunarse**.

---

### OCP-2 — `Potrero.anadir_res`: doble switch factory

**Ubicacion:** `Bib_Hacienda/Clases/Potrero.cs:62-102`

```csharp
// Switch #1: enum → rango edad + string tag (62-83)
switch (tipo_potrero) { case l_tipos_potreros.ternero: ... tipo_vaca = "ternero"; break; ... }
// Switch #2: string tag → constructor (88-102)
switch (tipo_vaca) { case "ternero": res = new Ternero(...); break; ... }
```

Si se agrega el enum pero se olvida un switch, `res` queda `null` → `NullReferenceException`.

---

### OCP-3 — `PublisherVacunacionCompletada`: dispatch de esquema completo por cadena `is`

**Ubicacion:** `Bib_Hacienda/Eventos/PublisherVacunacionCompletada.cs:30-41`

```csharp
if (res is Ternero && contador_bacterianas >= ReglaVacuna.max_bac_ternero && ...) esquema_completo = true;
else if (res is Novillo && ...) esquema_completo = true;
else if (res is Cebon && ...) esquema_completo = true;
```

Si se olvida, el nuevo tipo **nunca completa el esquema** — el evento nunca se dispara, `esquema_completo` queda `false` sin error.

---

## Violaciones WARNING

### OCP-4 — `PublisherPesoMin`: dispatch de peso minimo por cadena `is`

**Ubicacion:** `Bib_Hacienda/Eventos/PublisherPesoMin.cs:25-27`

```csharp
if (res is Ternero) { peso_minimo = ReglaRes.peso_min_ternero; }
else if (res is Cebon) { peso_minimo = ReglaRes.peso_min_cebon; }
else if (res is Novillo) { peso_minimo = ReglaRes.peso_min_novillo; }
```

Si se olvida, `peso_minimo` queda `0` → **alertas de desnutricion nunca se disparan** para el tipo nuevo.

### OCP-5 — `PublisherPesoVenta`: dispatch de peso de venta por cadena `is`

**Ubicacion:** `Bib_Hacienda/Eventos/PublisherPesoVenta.cs:27-29`

Si se olvida, `peso_apto` queda `0` → **todo animal del tipo nuevo se reporta "apto para venta" inmediatamente**.

### OCP-6 — `Hacienda.aplicar_vacuna`: conteo de vacunas por cadena `is`

**Ubicacion:** `Bib_Hacienda/Clases/Hacienda.cs:474-484, 504-508, 531-538` — Tres bloques `if/else-if` separados.

### OCP-7 — `ReglaRes`: explosion de constantes por tipo

**Ubicacion:** `Bib_Hacienda/Reglas/ReglaRes.cs:12-21` — 8 constantes para 3 tipos. Crecimiento lineal con el numero de tipos.

### OCP-8 — `ReglaVacuna`: explosion de constantes por tipo

**Ubicacion:** `Bib_Hacienda/Reglas/ReglaVacuna.cs:13-18` — 6 constantes para 3 tipos.

### OCP-9 — `ResService.ObtenerEstadisticas`: estadisticas por `is`

**Ubicacion:** `p_mvcHacienda/Servicios/ResService.cs:66-68`

```csharp
{ "Terneros", todasLasReses.Count(r => r.Res is Ternero) },
{ "Cebones", todasLasReses.Count(r => r.Res is Cebon) },
{ "Novillos", todasLasReses.Count(r => r.Res is Novillo) },
```

### OCP-10 — `PersistenciaService.CargarVentas`: switch de deserializacion con default silencioso

**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:434-440`

```csharp
Res res = resTipo switch {
    "Ternero" => new Ternero(resNombre, resPeso, resEdad),
    "Novillo" => new Novillo(resNombre, resPeso, resEdad),
    "Cebon"   => new Cebon(resNombre, resPeso, resEdad),
    _         => new Ternero(resNombre, resPeso, resEdad)  // default silencioso: corrupcion de datos
};
```

### OCP-11 — `PersistenciaService.CargarVacunas`: deserializacion if/else por tipo string

**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:496-515`

### OCP-12 — `PersistenciaService.CargarVacunasAplicadas`: deserializacion duplicada

**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:580-587` — Logica duplicada de OCP-11.

### OCP-13 — `PersistenciaService.GuardarVacunas`: serializacion con `is Bacteriana`

**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:201, 256`

### OCP-14 — `ICreacionVacuna`: explosion de sobrecargas

**Ubicacion:** `Bib_Hacienda/Interfaces/ICreacionVacuna.cs:10-16` — 4 sobrecargas, 2 de las cuales (lote) nunca se usan.

### OCP-15 — `VacunaService.CrearVacuna`: branch por parameter-sniffing

**Ubicacion:** `p_mvcHacienda/Servicios/VacunaService.cs:27-39`

```csharp
if (periodoAplicacion.HasValue && !atenuacion.HasValue) // Bacteriana
else if (!periodoAplicacion.HasValue && atenuacion.HasValue) // Viva
else return "Error: parametros invalidos...";
```

### OCP-16 — `VacunaController.Create`: branch por string compare

**Ubicacion:** `p_mvcHacienda/Controllers/VacunaController.cs:94-116`

```csharp
if (tipoVacuna == "Bacteriana") { ... } else { /* Viva */ }
```

### OCP-17 — `VacunaService.ObtenerEstadisticas`: estadisticas por `is`

**Ubicacion:** `p_mvcHacienda/Servicios/VacunaService.cs:142-143`

### OCP-18 — `Autenticacion.AutorizarOperacion`: roles hardcoded por username

**Ubicacion:** `Bib_Hacienda/Clases/Autenticacion.cs:123-137`

```csharp
if (usuario.Nombre == "admin") tienePermiso = true;
else if (usuario.Nombre == "empleado") tienePermiso = !operacion.Contains("Eliminar");
else if (usuario.Nombre == "visitante") tienePermiso = operacion.Contains("Consultar");
```

### OCP-19 — `IValidarInformacion` / `Validacion`: shotgun surgery para nueva entidad

**Ubicacion:** `IValidarInformacion.cs:11-24` + `Validacion.cs:11-18` + 4 validadores

Agregar validacion para una nueva entidad exige: modificar la interfaz + la clase base + los 4 validadores = **6 archivos**.

## SUGGESTION

### OCP-20 — `PersistenciaService`: campos proxy hardcoded para 4 validadores

**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:20-23, 41-56`

---

## Patron recomendado

**Mover datos especificos de tipo a los subtipos** (corrige OCP-1, 3, 4, 5, 7, 8 de un golpe):

```csharp
// En Res (base):
public abstract (byte maxBac, byte maxViv) LimitesVacunacion { get; }
public abstract ushort PesoMinimo { get; }
public abstract ushort PesoRecomendadoVenta { get; }
public abstract bool EsquemaCompleto(int bac, int viv);
```

Cada subtipo devuelve sus propios valores. Los consumidores llaman al miembro polimorfico en vez de hacer `is`-checking. **Cerrado a modificacion, abierto a extension.**
