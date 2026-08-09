# Analisis SRP — Single Responsibility Principle

**Proyectos:** `Bib_Hacienda` + `p_mvcHacienda`
**Hallazgos:** 5 CRITICAL, 6 WARNING, 2 SUGGESTION = 13 hallazgos

---

## Violaciones CRITICAL

### SRP-1 — `Hacienda` es una God Class con 6 responsabilidades

**Ubicacion:** `Bib_Hacienda/Clases/Hacienda.cs:16-559`

```csharp
public class Hacienda : IVacunacion, IVentaRes, ICreacionVacuna  // 3 interfaces, 3 actores
{
    private List<Potrero> l_potreros;   // gestion potreros
    private List<Venta> l_ventas;       // ventas / facturacion
    private List<Vacuna> l_vacunas;     // inventario vacunas
    // + 4 publishers de eventos (lineas 43-46)
    // + reglas de negocio inline (lineas 487-501)
}
```

**Responsabilidades mezcladas:**
1. Gestion de potreros (`crear_potrero:61`, `buscar_potrero:91`) — actor: administrador de potreros
2. Operaciones de ganado (`anadir_res_potrero:128`, `alimentar_res:171/220`, `vender_res:143`) — actor: operador de ganado
3. Creacion de inventario de vacunas (`crear_vacuna` x4 sobrecargas `268/302/336/393`) — actor: administrador de inventario
4. Vacunacion clinica (`aplicar_vacuna:451`) — actor: veterinario
5. Orquestacion de eventos (suscribe lambdas + dispara 4 publishers) — actor: sistema de notificacion
6. Reglas de negocio (limites de vacunacion por tipo de res) — actor: dueno de politica de negocio

**Impacto:** Cualquier cambio a ventas, politica de vacunacion o como se disparan eventos fuerza recompilar/retestear todo el nucleo. Las 4 sobrecargas de `crear_vacuna` + `aplicar_vacuna` (107 lineas) hacen de la clase un iman de cambios.

---

### SRP-2 — `PersistenciaService` fusiona persistencia + serializacion + validacion + lifecycle de proxy

**Ubicacion:** `p_mvcHacienda/Servicios/PersistenciaService.cs:12-641` (643 lineas)

```csharp
esValida = _validadorResProxy!.ValidarRes(res);          // validacion (108)
lineas.Add($"{potrero.Identificacion}|{res.Nombre}...");  // serializacion (117)
File.WriteAllLines(..., "Reses.txt", lineas);             // I/O (121)
return _httpContextAccessor.HttpContext?.Items["ResultadoValidacion"]...; // web-coupling (123)
```

**Responsabilidades mezcladas:**
1. File I/O (`File.WriteAllLines`/`ReadAllLines` en 6 entidades)
2. Serializacion/deserializacion (formato `|`-delimited, distinto por entidad)
3. Orquestacion de validacion (creacion de proxy + llamadas `ValidarX` + extraccion de resultado)
4. Lifecycle de proxy/DI (`InicializarProxies`, Castle DynamicProxy wiring)
5. Reconstruccion de entidades de dominio (type-switches en `CargarVentas:434-440`)
6. Acoplamiento web (`HttpContext.Items["ResultadoValidacion"]`)
7. Logica de deduplicacion (`CargarPotreros:329-331`)

**Impacto:** Singleton que sostiene todo el contrato de persistencia para 6 agregados. Cambiar el formato, migrar a BD o cambiar como se propaga el resultado de validacion colisionan en una clase. El acoplamiento a `HttpContext` la hace untestable fuera de un request web.

---

### SRP-3 — `Autenticacion` mezcla autenticacion + autorizacion + CRUD + storage + policy

**Ubicacion:** `Bib_Hacienda/Clases/Autenticacion.cs:11-149`

```csharp
public bool ValidarCredenciales(...)   // line 69 — autenticacion
public void AutorizarOperacion(...)    // line 104 — autorizacion
public string crear_usuario(...)       // line 29 — CRUD
private List<Usuario> usuarios_registrados;  // line 14 — storage
if (usuario.Nombre == "admin") { tienePermiso = true; }  // line 123 — policy
```

**Impacto:** AuthN y AuthZ cambian por razones distintas y son de stakeholders distintos. Un nuevo rol o cambio de politica de password exigen editar esta clase. Los usuarios y roles hardcoded en fuente significan que un cambio de seguridad requiere redeploy.

---

### SRP-4 — `ResController` bypassa la capa de servicios y mezcla HTTP + dominio + persistencia + reglas

**Ubicacion:** `p_mvcHacienda/Controllers/ResController.cs:8-182`

```csharp
private readonly Hacienda _hacienda;                 // alcanza dominio directo
private readonly PersistenciaService _persistencia;  // alcanza persistencia directo
var potrero = _hacienda.buscar_potrero(potreroId);   // query de dominio en controller
_persistencia.GuardarReses(_hacienda.L_potreros);    // persistencia en controller
if (montoDec < 0 || montoDec > uint.MaxValue) { ... } // regla de negocio en controller
```

**Impacto:** El contrato "controlador delgado" esta roto. Las reglas de negocio (overflow) y persistencia viven en la capa HTTP, por lo que no pueden reusarse desde otro punto de entrada (API, CLI, tests).

---

### SRP-5 — `Potrero.anadir_res` mezcla validacion + reglas + factory + eventos + mensajeria

**Ubicacion:** `Bib_Hacienda/Clases/Potrero.cs:38-161` (124 lineas)

```csharp
if (l_reses.Count() == ReglaPotrero.max_reses_potrero) { throw ... }  // capacidad (55)
switch (tipo_potrero) { case l_tipos_potreros.ternero: ... }          // reglas (62)
switch (tipo_vaca) { case "ternero": res = new Ternero(...); ... }    // factory (88)
publisher_potrero_mitad.evt_potrero_mitad += mensaje => { ... };       // eventos (122)
```

**Impacto:** Las lambdas se re-suscriben en cada llamada (lineas 110-132), acumulando handlers duplicados. Cambiar la politica de seleccion, las bandas de edad o el esquema de notificaciones caen en el mismo metodo de 124 lineas.

---

## Violaciones WARNING

### SRP-6 — `VacunaController.Create` hace parsing + regla de fecha + deteccion fragil de exito

**Ubicacion:** `p_mvcHacienda/Controllers/VacunaController.cs:56-137`

```csharp
if (!DateTime.TryParseExact(fechaVencimiento, "yyyy-MM-dd", ...))   // parsing (73)
if (fechaAplic > fechaVenc) { ... }                                // regla (87)
if (resultado.Contains("x")) { ...exito... }                       // control flow fragil (118)
```

El exito/fallo se codifica como substring de un mensaje localizado — cambiar el texto rompe el flujo silenciosamente.

---

### SRP-7 — `VacunaService.AplicarVacuna` orquesta 4 stores + remueve inventario redundante

**Ubicacion:** `p_mvcHacienda/Servicios/VacunaService.cs:54-98`

```csharp
_hacienda.L_vacunas.Remove(existente);            // remocion duplicada (79) — Hacienda:528 ya lo hace
_persistencia.GuardarVacunasAplicadas(...);       // 4 stores sin Unit-of-Work (83)
_persistencia.GuardarVacunas(...); _persistencia.GuardarPotreros(...); _persistencia.GuardarReses(...);
```

---

### SRP-8 — `UsuarioService` con estado mutable estatico + AuthN + Claims + CRUD + persistencia

**Ubicacion:** `p_mvcHacienda/Servicios/UsuarioService.cs:9-108`

```csharp
private static List<Usuario> _usuarios = new List<Usuario>();   // estado global mutable
public string CrearUsuario(...) { ...; _persistencia.GuardarUsuarios(_usuarios); }  // CRUD + persist
public bool AutenticarUsuario(...)                               // AuthN (61)
public async Task<(bool, IEnumerable<Claim>)> ValidateUserAsync(...)  // ASP.NET identity (92)
```

`AutenticarUsuario` y `ValidateUserAsync` duplican la misma verificacion de credenciales.

---

### SRP-9 — `PotreroService` duplica validacion de `Hacienda` y traga excepciones

**Ubicacion:** `p_mvcHacienda/Servicios/PotreroService.cs:20-49, 71-108`

```csharp
if (_hacienda.L_potreros.Any(p => p.Identificacion == identificacion)) { throw ... }  // dup de Hacienda:70
catch (InvalidOperationException) { throw new InvalidOperationException("Validacion fallida..."); }  // pierde detalle
```

---

### SRP-10 — `PotreroController.Create` dispara persistencia dos veces (service + controller)

**Ubicacion:** `p_mvcHacienda/Controllers/PotreroController.cs:63-93`

```csharp
string exitoso = _potreroService.CrearPotrero(identificacion, tipo);  // service ya persiste (76)
_persistencia.GuardarPotreros(_hacienda.L_potreros);                  // controller persiste OTRA VEZ (79)
```

---

### SRP-11 — `ResController.Alimentar` llama persistencia directo + codigo muerto

**Ubicacion:** `p_mvcHacienda/Controllers/ResController.cs:106-129`

```csharp
mensaje = _hacienda.alimentar_res(potreroId, nombreRes, cantidadAlimento);  // dominio directo (113)
_persistencia.GuardarReses(_hacienda.L_potreros);                           // persistencia en controller (116)
string mensajeAlimento = cantidadAlimento == 1 ? "vez" : "veces";           // variable muerta (118)
```

---

## SUGGESTION

### SRP-12 — `Program.cs` hidrata datos inline en el composition root

**Ubicacion:** `p_mvcHacienda/Program.cs:33-74` — La factory lambda carga 5 agregados y hace `Add` manual.

### SRP-13 — Validacion duplicada entre capas (cruzada)

**Ubicacion:** `PotreroService.cs:26` ↔ `Hacienda.cs:70`; `VacunaController.cs:87` ↔ `Hacienda.cs:280`

La regla "potrero ya existe" se verifica en service y dominio. La regla "vencimiento posterior a aplicacion" en controller y dominio. Las reglas pueden divergir.

---

## Lo que NO es violacion SRP

- Jerarquias `Res`/`Vacuna` — entidades cohesivas pequenas
- Publishers — cada uno tiene una sola responsabilidad de notificacion
- `Reglas/*`, `Venta`, `Usuario` — limpios, un solo proposito
- `HomeController` — delgado y correcto
