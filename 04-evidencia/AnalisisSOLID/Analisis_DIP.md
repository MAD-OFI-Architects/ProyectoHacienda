# Analisis DIP — Dependency Inversion Principle

**Proyectos:** `Bib_Hacienda` + `p_mvcHacienda`
**Hallazgos:** 9 CRITICAL, 8 WARNING, 3 SUGGESTION = 20 hallazgos

---

## Analisis del composition root

El composition root esta en `p_mvcHacienda/Program.cs:11-113`. Existe y usa el DI container de ASP.NET Core, pero **cada registracion es clase concreta, nunca interfaz**:

```csharp
builder.Services.AddSingleton<PersistenciaService>();          // concreto, sin interfaz
builder.Services.AddSingleton<Hacienda>(sp => { ... });        // concreto, no IVacunacion/IVentaRes/ICreacionVacuna
builder.Services.AddSingleton<PotreroService>();               // concreto
builder.Services.AddSingleton<ResService>();                   // concreto
builder.Services.AddSingleton<VacunaService>();                // concreto
builder.Services.AddSingleton<VentaService>();                 // concreto
builder.Services.AddSingleton<UsuarioService>(sp => { ... });  // concreto, sin interfaz
```

**No hay ni un solo `AddSingleton<IInterfaz, Implementacion>()`.** El DI es gestor de lifecycle, no de inversion de control.

---

## Violaciones CRITICAL

### DIP-1 — `Hacienda` instancia 4 publishers de eventos concretos como campos

**Modulo alto nivel:** `Hacienda` (facade de dominio)
**Modulo bajo nivel:** `PublisherVacunacionCompletada`, `PublisherVacunaVencida`, `PublisherPesoMin`, `PublisherPesoVenta` (concretos, sin interfaz)
**Abstraccion faltante:** `IDomainEventPublisher` o interfaces por rol
**Ubicacion:** `Bib_Hacienda/Clases/Hacienda.cs:43-46`

```csharp
private PublisherVacunacionCompletada publisher_vacunacion_completa = new PublisherVacunacionCompletada();
private PublisherVacunaVencida publisher_vacuna_vencida = new PublisherVacunaVencida();
private PublisherPesoMin publisher_peso_min = new PublisherPesoMin();
private PublisherPesoVenta publisher_peso_ideal = new PublisherPesoVenta();
```

**Donde se resuelve hoy:** Field initializer — no hay seam. Los publishers no se pueden swap, mockear ni inyectar.

---

### DIP-2 — `Hacienda` crea instancias concretas de `Potrero` inline

**Modulo alto nivel:** `Hacienda` (gestion de potreros)
**Modulo bajo nivel:** `Potrero` (objeto de dominio complejo con 4 publishers y reglas)
**Abstraccion faltante:** `IPotreroFactory`
**Ubicacion:** `Bib_Hacienda/Clases/Hacienda.cs:77`

```csharp
Potrero nuevo_potrero = new Potrero(indentificacion, tipo_potrero);
```

---

### DIP-3 — `Hacienda` crea instancias concretas de `Bacteriana` / `Viva` en 4 sobrecargas

**Modulo alto nivel:** `Hacienda` (implementa `ICreacionVacuna`)
**Modulo bajo nivel:** `Bacteriana`, `Viva` (subtipos concretos)
**Abstraccion faltante:** `IVacunaFactory`
**Ubicacion:** `Bib_Hacienda/Clases/Hacienda.cs:288, 322, 372, 429`

```csharp
Bacteriana nueva_vacuna = new Bacteriana(nombre, lote, fecha_vencimiento, fecha_aplicacion, periodo_aplicacion);
Viva nueva_vacuna = new Viva(nombre, lote, fecha_vencimiento, fecha_aplicacion, grado_atenuacion);
```

---

### DIP-4 — `Potrero` instancia 4 publishers concretos como campos

**Modulo alto nivel:** `Potrero`
**Modulo bajo nivel:** 4 publishers concretos sin interfaz
**Abstraccion faltante:** interfaces de publisher
**Ubicacion:** `Bib_Hacienda/Clases/Potrero.cs:21-24`

```csharp
private PublisherPotreroMitad publisher_potrero_mitad = new PublisherPotreroMitad();
private PublisherPotreroLleno publisher_potrero_lleno = new PublisherPotreroLleno();
private PublisherPesoVenta publisher_peso_venta = new PublisherPesoVenta();
private PublisherPesoMin publisher_peso_min = new PublisherPesoMin();
```

Como `Potrero` es `new`-ed por `Hacienda` (DIP-2) y `PersistenciaService`, **no hay ruta de inyeccion** desde el composition root.

---

### DIP-5 — `Potrero.anadir_res` crea subtipos concretos de `Res` via switch

**Modulo alto nivel:** `Potrero` (asignacion de ganado)
**Modulo bajo nivel:** `Ternero`, `Cebon`, `Novillo` (subtipos concretos)
**Abstraccion faltante:** `IResFactory`
**Ubicacion:** `Bib_Hacienda/Clases/Potrero.cs:88-101`

```csharp
switch (tipo_vaca) {
    case "ternero": res = new Ternero(nombre, peso, edad); l_reses.Add(res); break;
    case "cebon":   res = new Cebon(nombre, peso, edad);   l_reses.Add(res); break;
    case "novillo": res = new Novillo(nombre, peso, edad);  l_reses.Add(res); break;
}
```

---

### DIP-6 — `PersistenciaService` no tiene interfaz; 7 clases dependen del concreto

**Modulo alto nivel:** Todos los servicios + 2 controladores
**Modulo bajo nivel:** `PersistenciaService` (I/O de archivos, Castle DynamicProxy, `HttpContext`)
**Abstraccion faltante:** `IPersistenciaService` o repositorios por agregado
**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:12` + 7 consumidores

```csharp
public class PersistenciaService  // sin interfaz
// ResService.cs:12, VacunaService.cs:12, PotreroService.cs:13, VentaService.cs:11,
// UsuarioService.cs:13, PotreroController.cs:16, ResController.cs:17
```

---

### DIP-7 — `PersistenciaService` crea interceptor + proxy generator + 4 class proxies inline

**Modulo alto nivel:** `PersistenciaService`
**Modulo bajo nivel:** `InterceptorValidarInformacion`, `ProxyGenerator`, 4 `ValidadorX` concretos
**Abstraccion faltante:** Registro via DI container
**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:41-56`

```csharp
_interceptorValidacion = new InterceptorValidarInformacion(_httpContextAccessor);
var proxyGenerator = new ProxyGenerator();
_validadorVacunaProxy = proxyGenerator.CreateClassProxy<ValidadorVacuna>(_interceptorValidacion);
_validadorPotreroProxy = proxyGenerator.CreateClassProxy<ValidadorPotrero>(_interceptorValidacion);
_validadorResProxy = proxyGenerator.CreateClassProxy<ValidadorRes>(_interceptorValidacion);
_validadorVentaProxy = proxyGenerator.CreateClassProxy<ValidadorVenta>(_interceptorValidacion);
```

---

### DIP-8 — `PersistenciaService` e interceptores usan `HttpContext.Items` como canal de datos

**Modulo alto nivel:** `PersistenciaService`, `InterceptorValidarInformacion`, `InterceptorAutenticacion`
**Modulo bajo nivel:** `HttpContext.Items` (infraestructura ASP.NET)
**Abstraccion faltante:** `ValidationResult` como return value
**Ubicacion:** `PersistenciaService.cs:76, 85, 112...`; `InterceptorValidarInformacion.cs:30-56`; `InterceptorAutenticacion.cs:41-62`

```csharp
// PersistenciaService.cs:76 — lee lo que el interceptor escribio
var mensaje = _httpContextAccessor.HttpContext?.Items["ResultadoValidacion"]?.ToString();
// InterceptorValidarInformacion.cs:48 — escribe
_httpContextAccessor.HttpContext.Items["ResultadoValidacion"] = "Datos validos...";
// InterceptorAutenticacion.cs:59 — parsea mensajes de excepcion
bool esExitoso = ex.Message.Contains("✓");
```

---

### DIP-9 — `UsuarioService` usa estado mutable estatico + no tiene interfaz

**Modulo alto nivel:** `UsuarioService`
**Modulo bajo nivel:** `static List<Usuario>` global + `PersistenciaService` concreto
**Abstraccion faltante:** `IUsuarioService` + `IUserStore` inyectado
**Ubicacion:** `p_mvcHacienda/Servicios/UsuarioService.cs:9`

```csharp
private static List<Usuario> _usuarios = new List<Usuario>();
```

---

## Violaciones WARNING

### DIP-10 — Todos los servicios dependen de `Hacienda` concreto

**Ubicacion:** `ResService.cs:8,12`; `VacunaService.cs:9,12`; `PotreroService.cs:9,13`; `VentaService.cs:8,11`

Los servicios usan metodos que **no estan en ninguna interfaz existente** (`crear_potrero`, `anadir_res_potrero`, `buscar_potrero`, `alimentar_res`, `L_potreros`, `L_vacunas`, `L_ventas`). Las 3 interfaces que existen (`IVacunacion`, `IVentaRes`, `ICreacionVacuna`) son insuficientes.

### DIP-11 — `Program.cs` registra `Hacienda` como concreto, ignorando sus 3 interfaces

**Ubicacion:** `p_mvcHacienda/Program.cs:33`

```csharp
builder.Services.AddSingleton<Hacienda>(sp => { ... });  // no registra IVacunacion/IVentaRes/ICreacionVacuna
```

### DIP-12 — `Hacienda.vender_res` crea `new Venta(...)` + dependencia oculta en `DateTime.Now`

**Ubicacion:** `Bib_Hacienda/Clases/Hacienda.cs:156`

```csharp
Venta venta = new Venta(potrero, DateTime.Now, res, monto);
```

### DIP-13 — `PotreroController` inyecta `Hacienda` y `PersistenciaService` concretos directamente

**Ubicacion:** `p_mvcHacienda/Controllers/PotreroController.cs:12-13, 16`

```csharp
public PotreroController(PotreroService potreroService, Hacienda hacienda, PersistenciaService persistencia)
```

### DIP-14 — `ResController` inyecta `Hacienda` y `PersistenciaService` concretos, llama metodos sin interfaz

**Ubicacion:** `p_mvcHacienda/Controllers/ResController.cs:13-14, 17, 113, 165-169`

`alimentar_res` **no esta en ninguna interfaz**, forzando al controlador a depender del concreto `Hacienda`.

### DIP-15 — Ningun servicio de aplicacion tiene interfaz

**Ubicacion:** `Program.cs:77-80`; `VentaController.cs:11`, `VacunaController.cs:17`, `UsuarioController.cs:10`, `AccountController.cs:12`

### DIP-16 — Event publishers sin interfaz con logica de dominio embebida

**Ubicacion:** `PublisherVacunacionCompletada.cs:11, 30-41`; `PublisherVacunaVencida.cs:10, 27-28`; `PublisherPesoMin.cs:11, 25-27`; `PublisherPesoVenta.cs:11, 27-29`

### DIP-17 — `Hacienda.aplicar_vacuna` contiene type-checking extensivo de tipos concretos

**Ubicacion:** `Bib_Hacienda/Clases/Hacienda.cs:474-501`

---

## SUGGESTION

### DIP-18 — `PublisherVacunaVencida` depende de `DateTime.Now` sin abstraccion

**Ubicacion:** `Bib_Hacienda/Eventos/PublisherVacunaVencida.cs:27-28`

### DIP-19 — `Autenticacion` hardcodea usuarios y logica de permisos

**Ubicacion:** `Bib_Hacienda/Clases/Autenticacion.cs:23-25, 123-137`

### DIP-20 — `PersistenciaService` crea entidades de dominio durante la carga (responsabilidad de factory filtrada a persistencia)

**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:332, 431, 436-439, 442, 505, 514, 582, 586, 627`

---

## Composition root propuesto (TO-BE)

```
AddSingleton<TimeProvider>()
AddSingleton<IResFactory, ResFactory>()
AddSingleton<IVacunaFactory, VacunaFactory>()
AddScoped<IPotreroRepository, PotreroRepositoryCsv>()
AddScoped<IResRepository, ResRepositoryCsv>()
AddScoped<IVentaRepository, VentaRepositoryCsv>()
AddScoped<IVacunaRepository, VacunaRepositoryCsv>()
AddScoped<IUsuarioRepository, UsuarioRepositoryCsv>()
AddScoped<IGestorPotreros, GestorPotreros>()
AddScoped<IGestorReses, GestorReses>()
AddScoped<IServicioVacunacion, ServicioVacunacion>()
AddScoped<IServicioVentas, ServicioVentas>()
AddScoped<IValidarRes, ValidadorRes>()
AddScoped<IValidarPotrero, ValidadorPotrero>()
AddScoped<IValidarVacuna, ValidadorVacuna>()
AddScoped<IValidarVenta, ValidadorVenta>()
```

Cada consumidor pide la interfaz mas estrecha que necesita. El composition root resuelve las implementaciones concretas.
