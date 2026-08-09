# Analisis ISP — Interface Segregation Principle

**Proyectos:** `Bib_Hacienda` + `p_mvcHacienda`
**Hallazgos:** 2 CRITICAL, 1 WARNING, 2 SUGGESTION = 5 hallazgos

---

## Violaciones CRITICAL

### ISP-1 — `IValidarInformacion`: interfaz gorda que fuerza `NotImplementedException`

**Ubicacion:**
- `Bib_Hacienda/Interfaces/IValidarInformacion.cs:11-24` (la interfaz)
- `Bib_Hacienda/Clases/Validaciones/ValidarPotrero.cs:21-34`
- `Bib_Hacienda/Clases/Validaciones/ValidarRes.cs:21-34`
- `Bib_Hacienda/Clases/Validaciones/ValidarVacuna.cs:21-34`
- `Bib_Hacienda/Clases/Validaciones/ValidarVenta.cs:21-34`

**Analisis de la interfaz** — declara **4 metodos no relacionados**, uno por entidad de dominio:
```csharp
public interface IValidarInformacion
{
    bool ValidarRes(Res res);
    bool ValidarPotrero(Potrero potrero);
    bool ValidarVacuna(Vacuna vacuna);
    bool ValidarVenta(Venta venta);
}
```

**Analisis de clientes** — hay **4 roles de cliente distintos**, cada uno necesitando exactamente **un** metodo:

| Cliente (validador) | Necesita | Forzado a tambien implementar |
|---------------------|----------|-------------------------------|
| `ValidadorPotrero` | `ValidarPotrero` | `ValidarRes`, `ValidarVacuna`, `ValidarVenta` |
| `ValidadorRes` | `ValidarRes` | `ValidarPotrero`, `ValidarVacuna`, `ValidarVenta` |
| `ValidadorVacuna` | `ValidarVacuna` | `ValidarRes`, `ValidarPotrero`, `ValidarVenta` |
| `ValidadorVenta` | `ValidarVenta` | `ValidarRes`, `ValidarPotrero`, `ValidarVacuna` |

**Sintoma** — 12 stubs de `NotImplementedException` (3 por validador x 4 validadores):
```csharp
// ValidarPotrero.cs:21-34
public override bool ValidarRes(Res res)
{
    throw new NotImplementedException("Use ValidadorRes");
}
public override bool ValidarVacuna(Vacuna vacuna)
{
    throw new NotImplementedException("Use ValidadorVacuna");
}
public override bool ValidarVenta(Venta venta)
{
    throw new NotImplementedException("Use ValidadorVenta");
}
```

**Mecanismo de compensacion arquitectural:** `InterceptorValidarInformacion.cs:58-69` atrapa `NotImplementedException` como **flujo normal**:
```csharp
catch (NotImplementedException ex)
{
    _httpContextAccessor.HttpContext.Items["ResultadoValidacion"] = $"Metodo no implementado: {ex.Message}";
    _httpContextAccessor.HttpContext.Items["ValidacionExitosa"] = false;
    throw;
}
```

Usar excepciones para comportamiento esperado y conocido en tiempo de diseno es consecuencia directa y observable de la violacion de ISP.

**Fix:** Segregar en una role-interface por entidad, un metodo cada una:
```csharp
public interface IValidarRes       { bool Validar(Res res); }
public interface IValidarPotrero   { bool Validar(Potrero potrero); }
public interface IValidarVacuna    { bool Validar(Vacuna vacuna); }
public interface IValidarVenta     { bool Validar(Venta venta); }
```
`ValidadorPotrero : IValidarPotrero` implementa solo su metodo. El catch de `NotImplementedException` en el interceptor se vuelve codigo muerto.

---

### ISP-2 — `Validacion` (clase abstracta) propaga el contrato gordo

**Ubicacion:** `Bib_Hacienda/Clases/Validaciones/Validacion.cs:11-18`

```csharp
public abstract class Validacion : IValidarInformacion
{
    public abstract bool ValidarRes(Res res);
    public abstract bool ValidarPotrero(Potrero potrero);
    public abstract bool ValidarVacuna(Vacuna vacuna);
    public abstract bool ValidarVenta(Venta venta);
}
```

La clase base declara los 4 metodos como `abstract`, asi que la interfaz gorda se hereda a toda subclase sin escape. El compilador exige los 4; el desarrollador llena 3 con `throw`.

**Fix:** Eliminar `Validacion`. Despues de la segregacion de ISP-1, cada validador concreto implementa su propia role-interface directamente. La base compartida no tiene proposito restante.

---

## Violacion WARNING

### ISP-3 — `ICreacionVacuna` incluye 2 sobrecargas de lote que ningun consumidor usa

**Ubicacion:** `Bib_Hacienda/Interfaces/ICreacionVacuna.cs:10-16`

```csharp
string crear_vacuna(..., uint periodo_aplicacion);                              // single bacteriana
string crear_vacuna(..., Viva.enum_l_atenuaciones grado_atenuacion);            // single viva
string crear_vacuna(..., uint periodo_aplicacion, uint cantidad);               // LOTE bacteriana
string crear_vacuna(..., Viva.enum_l_atenuaciones grado_atenuacion, uint cantidad); // LOTE viva
```

**Analisis de clientes:** hay exactamente **un** consumidor en toda la solucion (`VacunaService.CrearVacuna`) y llama **solo las 2 sobrecargas individuales**. Una busqueda en todo el repositorio (`rg "crear_vacuna\("`) confirma que las 2 de lote **nunca se invocan**.

**Impacto:** Un cliente que solo crea vacunas individuales debe depender de un contrato que tambien anuncia fabricacion por lote. Ademas colapsa 2 responsabilidades (creacion individual vs fabricacion por lote) en una interfaz.

**Fix:** Separar en 2 role-interfaces:
```csharp
public interface ICreacionVacuna        // solo creacion individual
public interface ICreacionLoteVacuna    // fabricacion por lote
```

---

## SUGGESTION

### ISP-4 — Servicios de `p_mvcHacienda` son clases concretas sin interfaz

**Ubicacion:**
- `p_mvcHacienda/Servicios/PersistenciaService.cs:12` (12 metodos publicos, sin interfaz)
- Controllers que inyectan `Hacienda` y `PersistenciaService` concretos directamente

**Analisis:**

| Consumidor | Metodos que realmente llama | Metodos a los que esta acoplado transitivamente |
|------------|---------------------------|------------------------------------------------|
| `PotreroService` | `GuardarPotreros`, `GuardarReses` | los otros 10 |
| `VacunaService` | `GuardarVacunas`, `GuardarVacunasAplicadas`, `GuardarPotreros`, `GuardarReses`, `CargarVacunas` | los otros 7 |
| `UsuarioService` | `GuardarUsuarios`, `CargarUsuarios` | los otros 10 |
| `PotreroController` (directo) | `GuardarPotreros` | los otros 11 |
| `ResController` (directo) | `GuardarReses`, `GuardarVentas` | los otros 10 |

**Fix:** Extraer role-interfaces por entidad y direccion:
```csharp
public interface IGestorPotreros { List<Potrero> Cargar(); string Guardar(List<Potrero> p); }
public interface IGestorReses     { void Cargar(List<Potrero> p); string Guardar(List<Potrero> p); }
// ... etc
```

### ISP-5 — `ResService` y `VentaService` inyectan `PersistenciaService` y nunca lo usan

**Ubicacion:**
- `p_mvcHacienda/Servicios/ResService.cs:9, 12`
- `p_mvcHacienda/Servicios/VentaService.cs:9, 11`

```csharp
private readonly PersistenciaService _persistencia;  // declarado, inyectado...
public ResService(Hacienda hacienda, PersistenciaService persistencia) { _hacienda = hacienda; _persistencia = persistencia; }
// ... nigun metodo del cuerpo de la clase referencia _persistencia
```

Dependencia forzada de un miembro que no se usa. Cualquier fake/mock debe satisfacer una dependencia que nunca se ejercita.

---

## Nota — Interfaces segregadas pero nunca consumidas (brecha de DIP, no ISP)

`IVacunacion`, `IVentaRes`, `IAutenticacion` son individualmente **bien segregadas** (un metodo cada una). Sin embargo, una busqueda en el repositorio muestra que **nunca se usan como tipo de dependencia**:

- `IVacunacion` / `IVentaRes` — referenciadas solo en su declaracion y en `Hacienda.cs:16`. Todos los consumidores dependen del **concreto** `Hacienda`.
- `IAutenticacion` — `Autenticacion` la implementa, pero en `p_mvcHacienda` la auth se maneja con `UsuarioService` + cookie middleware. `IAutenticacion`/`Autenticacion`/`InterceptorAutenticacion` son un subsistema paralelo sin uso.

Esto es una preocupacion de **Dependency Inversion**, no ISP: las interfaces son estrechas pero decorativas porque ningun cliente depende de la abstraccion.
