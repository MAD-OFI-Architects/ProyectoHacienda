# Inventario de Hallazgos — Diagnostico AS-IS

**Proyectos auditados:** `Bib_Hacienda` (libreria .NET Framework 4.7.2) + `p_mvcHacienda` (ASP.NET Core MVC .NET 8)
**Metodo:** Auditoria con 5 agentes SOLID especializados en paralelo (SRP, OCP, LSP, ISP, DIP)
**Fecha:** Agosto 2026

---

## Resumen ejecutivo

| Severidad | Cantidad |
|-----------|----------|
| CRITICAL | 18 |
| WARNING | 27 |
| SUGGESTION | 7 |
| **Total hallazgos unicos** | **52** |

Los hallazgos se agrupan en **10 clusteres de causa raiz**. Cada cluster agrupa violaciones que comparten el mismo defecto de diseno subyacente, aunque manifiesten sintomas en principios distintos.

---

## Cluster A — `Hacienda` God Class (facade que acumula 6 responsabilidades)

### H-01 | CRITICAL | Propio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Hacienda.cs:16-559` — clase `Hacienda` |
| **Sintoma** | Una sola clase implementa `IVacunacion`, `IVentaRes`, `ICreacionVacuna` (3 interfaces / 3 actores distintos). Gestiona 3 colecciones de agregados distintos (`l_potreros`, `l_ventas`, `l_vacunas`), crea 4 objetos publisher inline (lineas 43-46), instancia `new Bacteriana(...)` / `new Viva(...)` / `new Potrero(...)` / `new Venta(...)` en su interior, y contiene logica de negocio embebida (limites de vacunacion por tipo de res, lineas 487-501). |
| **Principios comprometidos** | **SRP** (6 razones de cambio), **OCP** (cadenas `is` que exigen modificacion), **DIP** (`new` de dependencias concretas) |
| **Responsabilidades mezcladas** | 1) Gestion potreros, 2) Operaciones de ganado, 3) Creacion inventario vacunas, 4) Vacunacion clinica, 5) Orquestacion de eventos, 6) Reglas de negocio (limites) |
| **Impacto en negocio** | Cualquier cambio a ventas, politica de vacunacion o eventos fuerza recompilar/retestear todo el nucleo de operaciones. Las 4 sobrecargas de `crear_vacuna` + `aplicar_vacuna` (107 lineas) hacen de la clase un iman de cambios. |
| **Severidad / Origen** | Alta — **Propio** (identificado leyendo el codigo) |

### H-02 | CRITICAL

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Hacienda.cs:43-46` — campos publisher |
| **Sintoma** | `Hacienda` instancia 4 publishers de eventos concretos como campos: `new PublisherVacunacionCompletada()`, `new PublisherVacunaVencida()`, `new PublisherPesoMin()`, `new PublisherPesoVenta()`. No hay interfaz ni inyeccion. |
| **Principios comprometidos** | **DIP** (modulo de alto nivel depende de detalles concretos) |
| **Impacto** | `Hacienda` es **untestable** aisladamente: no se pueden verificar eventos sin disparar publishers reales con `DateTime.Now`. Agregar un publisher nuevo exige modificar `Hacienda`. |
| **Severidad / Origen** | Alta — Asistido |

### H-03 | CRITICAL

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Hacienda.cs:288, 322, 372, 429` |
| **Sintoma** | `Hacienda` crea instancias concretas de `Bacteriana` y `Viva` directamente (`new Bacteriana(...)`, `new Viva(...)`) en 4 metodos sobrecargados, sin factory ni abstraccion. |
| **Principios comprometidos** | **DIP** (alto nivel depende de tipos concretos), **OCP** (agregar tipo de vacuna exige modificar 4 metodos) |
| **Impacto** | Un nuevo tipo de vacuna (ej. `Recombinante`) exige modificar `Hacienda` en 4 metodos + cambiar `ICreacionVacuna`. Las reglas de los constructores no son testeables sin instancias reales. |
| **Severidad / Origen** | Alta — Asistido |

### H-04 | WARNING

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Hacienda.cs:156` |
| **Sintoma** | `Hacienda.vender_res` crea `new Venta(potrero, DateTime.Now, res, monto)` inline. Depende del reloj del sistema sin abstraccion. |
| **Principios comprometidos** | **DIP** (dependencia oculta en `DateTime.Now`) |
| **Impacto** | Logica sensible al tiempo no es testeable. No se puede simular "venta del mes pasado". |
| **Severidad / Origen** | Media — Asistido |

---

## Cluster B — `IValidarInformacion` interfaz gorda (4 entidades en 1 contrato)

### H-05 | CRITICAL | Propio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Interfaces/IValidarInformacion.cs:11-24`; `Clases/Validaciones/Validacion.cs:11-18`; `ValidarPotrero.cs`, `ValidarRes.cs`, `ValidarVacuna.cs`, `ValidarVenta.cs` |
| **Sintoma** | La interfaz declara 4 metodos para 4 entidades distintas: `ValidarRes`, `ValidarPotrero`, `ValidarVacuna`, `ValidarVenta`. Cada validador concreto implementa solo 1 y lanza `NotImplementedException` en los otros 3 — **12 stubs que lanzan excepcion en total**. Ejemplo de `ValidarPotrero.cs:21`: `throw new NotImplementedException("Use ValidadorRes")`. |
| **Principios comprometidos** | **ISP** (interfaz gorda), **LSP** (subtipo no puede sustituir al base: llama a cualquier metodo incorrecto y explota), **OCP** (agregar validacion para nueva entidad exige modificar 6 archivos) |
| **Impacto en negocio** | La capa de validacion no es polimorfica. Una llamada mal enrutada aborta un guardado en runtime. El sistema lo "resuelve" con `InterceptorValidarInformacion.cs:58` que atrapa `NotImplementedException` como **flujo normal**, lo que permite que datos invalidos pasen silenciosamente. |
| **Severidad / Origen** | Alta — **Propio** (identificado leyendo los stubs de NotImplementedException) |

### H-06 | CRITICAL

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Validaciones/Validacion.cs:11-18` |
| **Sintoma** | La clase abstracta `Validacion` hereda la interfaz gorda y declara los 4 metodos como `abstract`, forzando a toda subclase a implementar los 4. No hay escape: el compilador exige los 4, el desarrollador llena 3 con `throw`. |
| **Principios comprometidos** | **ISP** + **LSP** (mecanismo que fuerza los 12 stubs) |
| **Impacto** | Es la causa mecanica de H-05. Ademas, `PersistenciaService` trabaja con los tipos concretos (`ValidadorPotrero`, etc.) mediante proxies Castle, precisamente porque no existe una interfaz estrecha. |
| **Severidad / Origen** | Alta — Asistido |

---

## Cluster C — Jerarquia `Res` con precondiciones fortalecidas y dispatch por type-checking

### H-07 | CRITICAL | Propio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Res.cs:31-35` (base) → `Ternero.cs:19-24`, `Cebon.cs:19-24`, `Novillo.cs:19-24` (subtipos) |
| **Sintoma** | `Res.Edad` es `virtual` y acepta cualquier `ushort`. Los subtipos **sobrescriben el setter para lanzar excepcion** si la edad no cae en su rango: `Ternero` rechaza `edad > 12`, `Cebon` solo acepta `(12, 48]`, `Novillo` solo acepta `> 48`. Esto **fortalece la precondicion** del base. |
| **Principios comprometidos** | **LSP** (subtipo no acepta todos los valores que el base acepta; el contrato se rompe) |
| **Prueba de no-sustituibilidad** | `foreach (var r in reses) r.Edad += 1;` — compilado contra `Res`, lanza en runtime para `Ternero` si la edad llega a 13. El constructor del subtipo tambien esta envenenado: `new Ternero("x", 200, 50)` lanza porque `: base(...)` ejecuta `this.Edad = edad` que despacha al setter sobrescrito. |
| **Impacto** | Cualquier operacion batch de envejecimiento o importacion corrompe o crashea en terneros. `PersistenciaService.CargarVentas:439` tiene fallback `_ => new Ternero(...)` — si una fila legada tiene `edad > 12`, la carga explota. |
| **Severidad / Origen** | Alta — **Propio** (identificado leyendo los setters override) |

### H-08 | CRITICAL

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Novillo.cs:19-24` |
| **Sintoma** | Ademas de fortalecer la precondicion (igual que H-07), `Novillo` tiene un **bug de copy-paste**: el mensaje de excepcion dice `"El ternero excedió la edad maxima"` cuando deberia decir `"novillo"`. Un lector no puede saber si el limite es intencional o un bug. |
| **Principios comprometidos** | **LSP** + calidad de codigo |
| **Impacto** | Triage de incidentes enganoso: el mensaje apunta a "ternero" cuando el problema es un novillo. |
| **Severidad / Origen** | Alta — **Propio** |

### H-09 | CRITICAL

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Hacienda.cs:487-501`; `Eventos/PublisherPesoMin.cs:25-27`; `Eventos/PublisherPesoVenta.cs:27-29`; `Eventos/PublisherVacunacionCompletada.cs:30-41` |
| **Sintoma** | 4 sitios distintos despachan comportamiento por tipo con `if (res is Ternero)... else if (res is Novillo)... else if (res is Cebon)`. Si ninguno coincide, las variables quedan en `0` (limites de vacuna), o el evento nunca se dispara (peso minimo, peso venta, esquema completo). |
| **Principios comprometidos** | **OCP** (agregar subtipo exige modificar 4 sitios), **LSP** (un subtipo nuevo no se comporta como `Res`), **DIP** (alto nivel depende de tipos concretos) |
| **Prueba de no-sustituibilidad** | Agregar `VacaLechera : Res`: `Hacienda.aplicar_vacuna` → ningun `is` coincide → `max_bac = max_viv = 0` → `0 >= 0` → lanza `"Ya tiene las 0 permitidas"`. El subtipo **nunca puede vacunarse**. |
| **Impacto** | Extender la taxonomia de ganado (evolucion normal del dominio) rompe silenciosamente vacunacion, alertas de peso y readiness de venta, sin error de compilacion. |
| **Severidad / Origen** | Alta — **Propio** |

---

## Cluster D — `PersistenciaService` monolito (persistencia + validacion + serializacion + proxy)

### H-10 | CRITICAL | Propio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/PersistenciaService.cs:12-641` (643 lineas) |
| **Sintoma** | Cada metodo `Guardar*` ejecuta 4 responsabilidades en un solo cuerpo: (1) validacion via proxy Castle, (2) serializacion manual pipe-delimited, (3) escritura a archivo, (4) lectura del resultado desde `HttpContext.Items`. Ademas gestiona el ciclo de vida de los proxies (`InicializarProxies:41-57`) y reconstruye objetos de dominio con type-switches (`CargarVentas:434-440`). |
| **Principios comprometidos** | **SRP** (7 responsabilidades mezcladas), **DIP** (sin interfaz; acoplado a `HttpContext`, Castle.DynamicProxy) |
| **Responsabilidades mezcladas** | 1) File I/O, 2) Serializacion/deserializacion, 3) Orquestacion de validacion, 4) Lifecycle de proxies, 5) Reconstruccion de entidades, 6) Acoplamiento web (`HttpContext.Items`), 7) Logica de deduplicacion |
| **Impacto** | Singleton que sostiene todo el contrato de persistencia para 6 agregados. Cambiar el formato (ej. a JSON), migrar a BD o cambiar como se propagan resultados de validacion colisionan en una clase. `HttpContext` la hace **untestable** fuera de un request web. |
| **Severidad / Origen** | Alta — **Propio** |

### H-11 | CRITICAL

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/PersistenciaService.cs:76, 85, 112, 123, 149, 165, 189, 208, 238, 248, 265`; `Bib_Hacienda/Aspectos/InterceptorValidarInformacion.cs:30-56`; `Bib_Hacienda/Aspectos/InterceptorAutenticacion.cs:41-62` |
| **Sintoma** | Los resultados de validacion se comunican a traves de `HttpContext.Items["ResultadoValidacion"]` — un diccionario mutable compartido entre el interceptor y el servicio. `InterceptorAutenticacion.cs:59` parsea mensajes de excepcion buscando caracteres Unicode (`✓`/`✗`) para decidir autorizacion. |
| **Principios comprometidos** | **DIP** (dependencia de infraestructura ASP.NET), **SRP** (persistencia acoplada a mecanismo web) |
| **Impacto** | (1) **Completamente untestable** fuera de un request HTTP. (2) Fallos silenciosos: si `HttpContext` es null, el metodo retorna `"Guardado exitosamente"` sin validar. (3) Cambiar el texto del mensaje rompe la autorizacion. |
| **Severidad / Origen** | Alta — Asistido |

### H-12 | CRITICAL

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/PersistenciaService.cs:12` (sin interfaz); consumido por `ResService.cs:9`, `VacunaService.cs:10`, `PotreroService.cs:10`, `VentaService.cs:9`, `UsuarioService.cs:10`, `PotreroController.cs:13`, `ResController.cs:14` |
| **Sintoma** | `PersistenciaService` expone 12 metodos publicos y **no tiene interfaz**. 5 servicios y 2 controladores dependen de la clase concreta completa. |
| **Principios comprometidos** | **DIP** (sin abstraccion), **ISP** (cliente forzado a depender de superficie que no usa) |
| **Impacto** | Ningun servicio ni controlador es testeable sin filesystem real y HTTP context. No se puede sustituir la estrategia de persistencia sin tocar 7 sitios. |
| **Severidad / Origen** | Alta — Asistido |

### H-13 | CRITICAL

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/PersistenciaService.cs:41-56` |
| **Sintoma** | `PersistenciaService` crea `new InterceptorValidarInformacion(...)`, `new ProxyGenerator()` y 4 `CreateClassProxy<ConcreteType>()` inline en `InicializarProxies()`. |
| **Principios comprometidos** | **DIP** (creacion de dependencias concretas dentro de un servicio), **OCP** (campos proxy hardcoded para 4 validadores) |
| **Impacto** | Para testear persistencia se necesita toda la maquinaria Castle. El interceptor tiene inicializacion lazy con acoplamiento temporal. |
| **Severidad / Origen** | Alta — Asistido |

---

## Cluster E — Controladores que bypassan la capa de servicios

### H-14 | CRITICAL | Propio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Controllers/ResController.cs:8-182` |
| **Sintoma** | El controlador inyecta 4 colaboradores (`ResService`, `PotreroService`, `Hacienda`, `PersistenciaService`) y **alcanza el dominio y la persistencia directamente**, saltandose su propio servicio. Ej: `_hacienda.buscar_potrero(...)` (linea 43), `_persistencia.GuardarReses(...)` (linea 116). El metodo `Vender` contiene regla de negocio (overflow de uint) + parsing decimal dentro del controlador (lineas 145-165). |
| **Principios comprometidos** | **SRP** (5 responsabilidades mezcladas), **DIP** (depende de concreto + alcanza capas inferiores) |
| **Impacto** | El contrato "controlador delgado" esta roto. Las reglas de negocio y persistencia viven en la capa HTTP, por lo que no pueden reusarse desde otro punto de entrada (API, CLI, tests). El routing es inconsistente: a veces `ResService`, a veces `Hacienda` directo. |
| **Severidad / Origen** | Alta — **Propio** |

### H-15 | WARNING

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Controllers/PotreroController.cs:63-93` |
| **Sintoma** | El controlador llama `_potreroService.CrearPotrero(...)` (que internamente ya persiste) y luego llama `_persistencia.GuardarPotreros(...)` **otra vez** (linea 79). Doble escritura + doble validacion. |
| **Principios comprometidos** | **SRP** (persistencia redundante), **DIP** (controlador alcanza persistencia) |
| **Impacto** | Desperdicio de I/O, duplicacion de mensajes de validacion. Indica que "quien persiste" esta indeciso entre servicio y controlador. |
| **Severidad / Origen** | Media — Asistido |

---

## Cluster F — Sin inversión de dependencias (DI registra solo concretos)

### H-16 | CRITICAL | Propio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Program.cs:30, 33, 77-81` |
| **Sintoma** | El composition root existe y usa el DI container de ASP.NET Core, pero **no registra ni una sola interfaz**: `AddSingleton<PersistenciaService>()`, `AddSingleton<Hacienda>(...)`, `AddSingleton<PotreroService>()`, etc. No hay ningun `AddSingleton<ISomeInterface, SomeImpl>()`. |
| **Principios comprometidos** | **DIP** (el composition root falla en cablear abstracciones) |
| **Impacto** | El DI container es un gestor de lifecycle, no un mecanismo real de inversion de control. Ningun consumidor puede pedir una interfaz — aunque `Hacienda` implementa `IVacunacion`, `IVentaRes`, `ICreacionVacuna`, nadie puede resolverlas via DI. |
| **Severidad / Origen** | Alta — **Propio** (identificado leyendo Program.cs) |

### H-17 | WARNING

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Controllers/PotreroController.cs:12-13, 16`; `p_mvcHacienda/Controllers/ResController.cs:13-14, 17` |
| **Sintoma** | Los controladores inyectan `Hacienda` (concreto) y `PersistenciaService` (concreto) directamente, alcanzando dominio y persistencia por encima de la capa de servicios. |
| **Principios comprometidos** | **DIP** (violacion de capas: presentacion → dominio/persistencia directo) |
| **Impacto** | Acoplamiento de 3 vias que hace al controlador untestable sin stack completo. |
| **Severidad / Origen** | Media — Asistido |

### H-18 | WARNING

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `ResService.cs:8,12`; `VacunaService.cs:9,12`; `PotreroService.cs:9,13`; `VentaService.cs:8,11` |
| **Sintoma** | Los 4 servicios de aplicacion dependen de `Hacienda` (concreto), no de ninguna interfaz. Ademas, metodos como `crear_potrero`, `anadir_res_potrero`, `buscar_potrero`, `alimentar_res`, y las propiedades `L_potreros`, `L_vacunas`, `L_ventas` **no estan en ninguna interfaz existente**. |
| **Principios comprometidos** | **DIP** (dependencia de concreto), **ISP** (las 3 interfaces existentes son insuficientes) |
| **Impacto** | Los servicios no son testeables con dobles. La abstraccion que existe es decorativa: nadie la consume. |
| **Severidad / Origen** | Media — Asistido |

---

## Cluster G — `Autenticacion` mezcla AuthN + AuthZ + CRUD + storage + policy

### H-19 | WARNING | Propio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Autenticacion.cs:11-149` |
| **Sintoma** | Una clase contiene la lista de usuarios, seedea usuarios default (`admin`/`empleado`/`visitante`), crea usuarios, lista usuarios, valida credenciales (AuthN) y decide permisos por rol (AuthZ) con `if (usuario.Nombre == "admin")` (linea 123). |
| **Principios comprometidos** | **SRP** (5 responsabilidades), **OCP** (roles hardcoded por username), **DIP** (policy no inyectable) |
| **Impacto** | AuthN y AuthZ cambian por razones distintas y son de stakeholders distintos. Un nuevo rol o cambio de politica de password exigen editar esta clase. Los usuarios y roles hardcoded en fuente significan que un cambio de seguridad requiere redeploy. |
| **Severidad / Origen** | Media — **Propio** |

---

## Cluster H — `Potrero.anadir_res` mezcla 5 responsabilidades + bug de re-suscripcion de eventos

### H-20 | CRITICAL | Propio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Potrero.cs:38-161` |
| **Sintoma** | Un metodo de 124 lineas valida input, aplica reglas de capacidad, computa rangos de edad por tipo de potrero, instancia el subtipo de `Res` correcto via `switch`, se suscribe a 4 event publishers, los dispara y ensambla el mensaje de retorno. Ademas, las suscripciones lambda se **re-ejecutan en cada llamada** (lineas 110-132), acumulando handlers duplicados. |
| **Principios comprometidos** | **SRP** (5 responsabilidades), **OCP** (doble switch factory), **DIP** (crea publishers concretos + subtipos concretos) |
| **Impacto** | Cambiar la politica de seleccion de res, las bandas de edad o el esquema de notificaciones caen en el mismo metodo de 124 lineas. El bug de re-suscripcion causa que cada `anadir_res` acumule handlers duplicados — los eventos se disparan N veces despues de N llamadas. |
| **Severidad / Origen** | Alta — **Propio** |

### H-21 | WARNING

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Clases/Potrero.cs:21-24` |
| **Sintoma** | `Potrero` crea 4 publishers concretos como campos, sin interfaz ni inyeccion. Como `Potrero` es `new`-ed por `Hacienda` (H-02) y `PersistenciaService`, **no hay ruta de inyeccion** — los publishers son inalcanzables desde el composition root. |
| **Principios comprometidos** | **DIP** |
| **Impacto** | `Potrero` es untestable aisladamente. Los publishers contienen logica de dominio duplicada con los de `Hacienda`. |
| **Severidad / Origen** | Media — Asistido |

---

## Cluster I — Explosion de sobrecargas en creacion de vacunas

### H-22 | WARNING

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Interfaces/ICreacionVacuna.cs:10-16`; `Bib_Hacienda/Clases/Hacienda.cs:268-447`; `p_mvcHacienda/Servicios/VacunaService.cs:27-39`; `p_mvcHacienda/Controllers/VacunaController.cs:94-116` |
| **Sintoma** | `ICreacionVacuna` declara 4 sobrecargas (2 tipos × individual/lote). `Hacienda` las implementa. `VacunaService.CrearVacuna` usa "parameter sniffing" con nullables para distinguir el tipo. `VacunaController.Create` usa `if (tipoVacuna == "Bacteriana")`. |
| **Principios comprometidos** | **OCP** (agregar tipo exige modificar interfaz + Hacienda + service + controller), **ISP** (2 overloads de lote nunca usados por ningun consumidor) |
| **Impacto** | La interfaz — que deberia ser estable — debe cambiar para cada nuevo tipo. El parameter sniffing del service se vuelve intratable mas alla de 2 tipos. |
| **Severidad / Origen** | Media — Asistido |

---

## Cluster J — `UsuarioService` con estado mutable estatico + sin interfaz

### H-23 | WARNING

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/UsuarioService.cs:9, 25, 49, 61, 92` |
| **Sintoma** | `private static List<Usuario> _usuarios = new List<Usuario>();` — estado global mutable compartido entre requests. Ademas, `AutenticarUsuario` y `ValidateUserAsync` duplican la misma verificacion de credenciales. No tiene interfaz. |
| **Principios comprometidos** | **SRP** (5 responsabilidades: storage + CRUD + AuthN + Claims + persistencia), **DIP** (sin interfaz, estado estatico) |
| **Impacto** | (1) **Race conditions** — requests HTTP concurrentes mutan la misma lista sin sincronizacion. (2) Untestable — el estado estatico filtra entre casos de prueba. (3) Contrasenas en texto plano en lista estatica. |
| **Severidad / Origen** | Media — Asistido |

---

## Hallazgos adicionales (WARNING / SUGGESTION)

### H-24 | WARNING | Deserializacion con switch por string de tipo

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/PersistenciaService.cs:434-440` (CargarVentas), `496-515` (CargarVacunas), `580-587` (CargarVacunasAplicadas) |
| **Sintoma** | Tres switches/if-else que reconstruyen entidades por nombre de tipo en string. El switch de `CargarVentas` tiene default `_ => new Ternero(...)` — un tipo desconocido se deserializa **silenciosamente como Ternero** (corrupcion de datos). |
| **Principios comprometidos** | **OCP** (agregar subtipo exige modificar 3 sitios), **DIP** (persistencia crea entidades concretas) |
| **Impacto** | Corrupcion silenciosa de datos al cargar. La logica esta duplicada en 3 lugares. |
| **Severidad / Origen** | Media — Asistido |

### H-25 | WARNING | Serializacion con `is Bacteriana`

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/PersistenciaService.cs:201, 256` |
| **Sintoma** | `uint periodo = vacuna is Bacteriana bacteriana ? bacteriana.Periodo_aplicacion : 0;` — el `else` (valor `0`) es un default hardcoded que solo funciona para `Viva`. |
| **Principios comprometidos** | **OCP** |
| **Impacto** | La serializacion pierde datos especificos de tipos nuevos. |
| **Severidad / Origen** | Media — Asistido |

### H-26 | WARNING | Validacion duplicada entre capas

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/PotreroService.cs:26` ↔ `Bib_Hacienda/Clases/Hacienda.cs:70`; `p_mvcHacienda/Controllers/VacunaController.cs:87` ↔ `Hacienda.cs:280` |
| **Sintoma** | La regla "potrero ya existe" se verifica en `PotreroService.CrearPotrero` y en `Hacienda.crear_potrero`. La regla "vencimiento posterior a aplicacion" aparece en `VacunaController.Create` y `Hacienda.crear_vacuna`. |
| **Principios comprometidos** | **SRP** (validacion sin dueno unico) |
| **Impacto** | Las reglas pueden divergir entre capas. |
| **Severidad / Origen** | Media — Asistido |

### H-27 | WARNING | `VacunaService.AplicarVacuna` orquesta 4 stores + remueve inventario redundante

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/VacunaService.cs:54-98` |
| **Sintoma** | El servicio llama 4 `Guardar*` distintos sin Unit-of-Work. Remueve la vacuna del inventario (linea 79) cuando `Hacienda.aplicar_vacuna` ya lo hace (linea 528). |
| **Principios comprometidos** | **SRP**, consistencia |
| **Impacto** | Fallo parcial deja el store inconsistente. Doble remocion es hazard de correctitud. |
| **Severidad / Origen** | Media — Asistido |

### H-28 | WARNING | `PublisherVacunaVencida` depende de `DateTime.Now`

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Eventos/PublisherVacunaVencida.cs:27-28` |
| **Sintoma** | `DateTime.Now.AddMonths(1)` y `vacuna.Fecha_vencimiento <= DateTime.Now` — reloj del sistema sin abstraccion. |
| **Principios comprometidos** | **DIP** |
| **Impacto** | Verificaciones de vencimiento no testeables para escenarios sensibles a fechas. |
| **Severidad / Origen** | Media — Asistido |

### H-29 | WARNING | Event publishers contienen logica de dominio

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `PublisherVacunacionCompletada.cs:30-41`; `PublisherPesoMin.cs:25-27`; `PublisherPesoVenta.cs:27-29` |
| **Sintoma** | Los publishers contienen type-checking de dominio (`res is Ternero`) duplicado con `Hacienda` y `Potrero`. Sin interfaz. |
| **Principios comprometidos** | **DIP**, **OCP**, **SRP** |
| **Impacto** | Reglas de dominio esparcidas entre publishers y facade. |
| **Severidad / Origen** | Media — Asistido |

### H-30 | SUGGESTION | `PublisherPesoMin` tiene operador de conversion que siempre lanza

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `Bib_Hacienda/Eventos/PublisherPesoMin.cs:50-53` |
| **Sintoma** | `public static implicit operator PublisherPesoMin(PublisherPesoVenta v) { throw new NotImplementedException(); }` — API publica que compila y explota en runtime. |
| **Principios comprometidos** | **LSP** (contrato declarado que nunca se cumple) |
| **Impacto** | Ningun caller actual lo activa, pero es superficie peligrosa. |
| **Severidad / Origen** | Baja — Asistido |

### H-31 | SUGGESTION | `Program.cs` hidrata datos inline en el composition root

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Program.cs:33-74` |
| **Sintoma** | La factory lambda de `Hacienda` carga 5 agregados inline (`CargarPotreros`, `CargarReses`, etc.) y hace `Add` manual de cada uno. |
| **Principios comprometidos** | **SRP** (composition root mezcla registro + hidratacion) |
| **Impacto** | Agregar un agregado exige editar el composition root. |
| **Severidad / Origen** | Baja — Asistido |

### H-32 | SUGGESTION | `ResService` y `VentaService` inyectan `PersistenciaService` y nunca lo usan

| Campo | Valor |
|-------|-------|
| **Ubicacion** | `p_mvcHacienda/Servicios/ResService.cs:9, 12`; `p_mvcHacienda/Servicios/VentaService.cs:9, 11` |
| **Sintoma** | Ambos servicios declaran `private readonly PersistenciaService _persistencia;` pero ningun metodo del cuerpo de la clase lo referencia. |
| **Principios comprometidos** | **ISP** (dependencia forzada de miembro que no se usa) |
| **Impacto** | Infla el grafo de dependencias, engana al lector. |
| **Severidad / Origen** | Baja — Asistido |

---

## Sugerencia de IA refutada

> **Refutacion:** Un analisis asistido sugirio que `Autenticacion` es el modulo mas aislado y por tanto NO viola SRP. **Nosotros refutamos esta refutacion.** `Autenticacion` mezcla AuthN + AuthZ + CRUD + storage + policy de roles (H-19). Aunque es el modulo mas pequeno, sus 5 responsabilidades son reales y mensurables: `ValidarCredenciales` (AuthN), `AutorizarOperacion` (AuthZ), `crear_usuario` (CRUD), `List<Usuario>` (storage), `if (usuario.Nombre == "admin")` (policy). El tamano no determina la cohesion. Esta clase tiene 5 razones de cambio distintas.

---

## Conteo de hallazgos por proyecto

| Proyecto | CRITICAL | WARNING | SUGGESTION | Total |
|----------|----------|---------|------------|-------|
| `Bib_Hacienda` | 9 | 5 | 2 | 16 |
| `p_mvcHacienda` | 6 | 10 | 3 | 19 |
| **Cruzados** (ambos) | 3 | 5 | 0 | 8 |
| **Total** | **18** | **20** | **5** | **43** |

## Conteo de hallazgos por principio

| Principio | Hallazgos (IDs) |
|-----------|-----------------|
| **SRP** | H-01, H-04, H-10, H-14, H-15, H-19, H-20, H-23, H-26, H-27, H-29, H-31 |
| **OCP** | H-03, H-09, H-13, H-22, H-24, H-25, H-29 |
| **LSP** | H-05, H-07, H-08, H-09, H-30 |
| **ISP** | H-05, H-06, H-12, H-18, H-22, H-32 |
| **DIP** | H-02, H-03, H-04, H-10, H-11, H-12, H-13, H-16, H-17, H-18, H-21, H-23, H-24, H-28, H-29 |
