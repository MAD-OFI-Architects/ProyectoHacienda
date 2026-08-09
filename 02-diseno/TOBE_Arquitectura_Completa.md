# Arquitectura TO-BE Completa — Hacienda Ganadera

**Proyectos:** `Hacienda.Domain` + `Hacienda.Application` + `Hacienda.Infrastructure` + `Hacienda.Web`
**Paradigma:** Clean Architecture / Hexagonal con DDD táctico
**Target:** .NET 8.0
**Fecha:** Agosto 2026

> **Nota de migración:** La capa de persistencia fue migrada de archivos CSV a **SQLite + Dapper**. Los 7 repositorios CSV y los 7 parsers fueron reemplazados por 7 repositorios SQLite (`Hacienda.Infrastructure/Persistence/Sqlite/`). El esquema usa TPH para `Res` (discriminator `tipo` TEXT) y `Vacuna` (discriminator `categoria` TEXT). Se agregó `chip_id` FK en `reses` para persistir la relación Res↔Chip. Los interfaces de dominio no fueron modificados — DIP respetado: solo se cambió el adapter.

---

## Tabla de Contenidos

1. [Visión General y Regla de Dependencia](#1-visión-general-y-regla-de-dependencia)
2. [Convención de Colores (para diagrama UML)](#2-convención-de-colores-para-diagrama-uml)
3. [Capa Domain — Completa](#3-capa-domain--completa)
4. [Capa Application — Completa](#4-capa-application--completa)
5. [Capa Infrastructure — Completa](#5-capa-infrastructure--completa)
6. [Capa Web — Completa](#6-capa-web--completa)
7. [Herencias Justificadas con Verificación LSP](#7-herencias-justificadas-con-verificación-lsp)
8. [Inversiones de Dependencia (DIP)](#8-inversiones-de-dependencia-dip)
9. [Composition Root Final](#9-composition-root-final)
10. [Resumen de Relaciones](#10-resumen-de-relaciones)
11. [Cumplimiento SOLID por Principio](#11-cumplimiento-solid-por-principio)
12. [Registros de Decisión Arquitectónica (ADR)](#12-registros-de-decisión-arquitectónica-adr)
13. [Estructura de Carpetas Completa](#13-estructura-de-carpetas-completa)

---

## 1. Visión General y Regla de Dependencia

```
┌─────────────────────────────────────────────────────┐
│                Hacienda.Web                           │
│   Controllers · ViewModels · Views · Program.cs      │
│                  (Composition Root)                   │
├─────────────────────────────────────────────────────┤
│             Hacienda.Application                       │
│   Use Cases · DTOs · Validadores · Application Svc   │
├─────────────────────────────────────────────────────┤
│              Hacienda.Domain                           │
│   Entities · Value Objects · Enums · Events          │
│   Factories · Repository Interfaces                   │
├─────────────────────────────────────────────────────┤
│           Hacienda.Infrastructure                      │
│   Persistencia SQLite + Dapper · Seeder · Events     │
└─────────────────────────────────────────────────────┘
```

**Regla de dependencia (DIP):**

| Proyecto | Depende de | NO depende de |
|----------|-----------|---------------|
| `Hacienda.Domain` | Nada (solo BCL .NET) | Application, Infrastructure, Web |
| `Hacienda.Application` | `Hacienda.Domain` | Infrastructure, Web |
| `Hacienda.Infrastructure` | `Hacienda.Domain`, `Hacienda.Application` | Web |
| `Hacienda.Web` | `Hacienda.Application`, `Hacienda.Domain`, `Hacienda.Infrastructure` (solo Program.cs) | — |

> **La flecha de compilación siempre apunta hacia adentro.** Domain no sabe quién la consume. Infrastructure implementa interfaces de Domain/Application. Web solo cablea en Program.cs.

---

## 2. Convención de Colores (para diagrama UML)

**Aplicada en:** `03-diseno/UML_Hacienda_Unificado.dia` — las 108 clases del diagrama están pintadas según esta convención.

| Color | Borde (hex) | Relleno (hex) | Principio | Significado |
|-------|------------|---------------|-----------|-------------|
| **Negro** | `#000000` | `#FFFFFF` | — | Se conserva del diseño original sin cambios |
| **Verde** | `#82B366` | `#D5E8D4` | **SRP** | Clase partida o creada por descomposición de responsabilidades |
| **Azul** | `#6C8EBF` | `#DAE8FC` | **OCP** | Punto de extensión (método abstracto, plugin registry): se extiende sin modificar |
| **Rojo** | `#B85450` | `#F8CECC` | **LSP** | Corrección de contrato (setter puro, pre/postcondición) |
| **Naranja** | `#D6B656` | `#FFF2CC` | **ISP** | Interfaz segregada de una interfaz gorda anterior |
| **Violeta** | `#9673A6` | `#E1D5E7` | **DIP** | Abstracción que invierte una dependencia concreta |

**Regla de pintado en el .dia:** cada clase lleva `fill_color` (background) = relleno del principio y `line_color` (foreground) = borde del principio. El texto permanece negro. Las clases **negras** se ven blancas con borde negro (= sin intervención).

**Regla de doble principio:** cuando un elemento responde a dos principios, el **relleno** codifica el principio principal y el **borde** el secundario. Ejemplo: `Res` lleva relleno rojo (LSP — la corrección del setter) y borde azul (OCP — los abstracts polimórficos).

### 2.1 Mapa clase → color (trazabilidad UML ↔ código ↔ hallazgos)

Cada fila justifica el color con la evidencia que lo motiva (`H-xx` = hallazgo del inventario en `01-diagnostico/Inventario_Hallazgos.md`; ADR = decisión en §12).

#### Domain — Entidades y enums (19)

| Clase | Color (relleno + borde) | Principio(s) | Motivo |
|-------|------------------------|--------------|--------|
| `Res` (abstract) | Rojo + azul | LSP + OCP | Setter `Edad` puro (H-07, H-08); datos de subtipo como abstracts (H-04) |
| `Ternero`, `Novillo`, `Cebon` | Rojo + azul | LSP + OCP | Implementan los abstracts sin fortalecer precondiciones (ADR-03) |
| `Vacuna` (abstract) | Azul | OCP | `Categoria` / `Serializar` / `DetalleVisual` polimórficos (H-21, H-23) |
| `Bacteriana`, `Viva` | Azul | OCP | Subtipo con atributo propio; nuevo tipo = nueva clase |
| `Potrero` | Verde | SRP | Solo el invariante de capacidad; ya no crea reses ni publica eventos (H-20) |
| `Venta` | Negro | — | Conservada: snapshot inmutable de la res vendida |
| `Usuario` | Naranja + verde | ISP + SRP | `RolUsuario` enum en vez de `Nombre == "admin"`; credencial como VO (H-19) |
| `Chip` | Verde + azul | SRP + OCP | SC-2: gestiona solo su estado y serie; estados extensibles |
| `Geolocalizacion` | Verde | SRP | SC-2: solo coordenadas y timestamp |
| `TipoRes` | Azul | OCP | Discriminador de la extensión de subtipos |
| `VacunaCategoria` | Azul | OCP | Discriminador polimórfico de vacunas |
| `TipoPotrero` | Negro | — | Conservado |
| `RolUsuario` | Naranja | ISP | Reemplaza el hardcodeo de roles (H-19) |
| `EstadoVacuna` | Verde | SRP | Estado derivado del cálculo de fechas |
| `EstadoChip` | Azul | OCP | SC-2: nuevos estados sin modificar código |
| `GradoAtenuacion` | Azul | OCP | Anidado en `Viva`; la atenuación ahora persiste en el ciclo de persistencia |

#### Domain — Value Objects y Results (7)

| Clase | Color | Principio | Motivo |
|-------|-------|-----------|--------|
| `Credencial` | Verde | SRP | Reemplaza la contraseña en texto plano; verificación delegada a `IHasher` |
| `Dinero` | Verde | SRP | Reemplaza el `uint` suelto; invariante monto ≥ 0 y moneda única |
| `Identificacion` | Verde | SRP | Reemplaza el `string` suelto con validación |
| `NumeroSerieChip` | Verde | SRP | SC-2: serie validada y normalizada |
| `ValidationResult` | Violeta | DIP | Reemplaza `HttpContext.Items` como canal de validación (H-11) |
| `ResultadoAutenticacion` | Violeta | DIP | Reemplaza las excepciones de auth como flujo de control (H-11) |
| `ResultadoAutorizacion` | Violeta | DIP | Reemplaza el parseo de ✓/✗ en mensajes de excepción (H-11) |

#### Domain — Factories (8)

| Clase | Color (relleno + borde) | Principio(s) | Motivo |
|-------|------------------------|--------------|--------|
| `IResFactory` | Violeta + azul | DIP + OCP | Abstrae la creación; el plugin registry la hace extensible |
| `IVacunaFactory` | Violeta + azul | DIP + OCP | Ídem para vacunas |
| `IPotreroFactory` | Violeta | DIP | Abstrae la creación de `Potrero` (antes `new` inline, H-03) |
| `IVentaFactory` | Violeta | DIP | Abstrae la creación de `Venta` + el reloj (H-02) |
| `FabricaRes` | Azul | OCP | Plugin registry: nuevo subtipo = 1 entrada en el diccionario |
| `FabricaPotrero` | Verde | SRP | Creación extraída de la God Class |
| `FabricaVacuna` | Verde | SRP | Ídem; valida invariantes comunes en el boundary |
| `FabricaVenta` | Verde | SRP | Ídem |

#### Domain — Interfaces de abstracción (12)

| Clase | Color (relleno + borde) | Principio(s) | Motivo |
|-------|------------------------|--------------|--------|
| `IRepositorioPotrero` | Naranja + violeta | ISP + DIP | Segregada de `PersistenciaService` (12 métodos sin interfaz, H-12); invierte la dependencia |
| `IRepositorioRes` | Naranja + violeta | ISP + DIP | Ídem |
| `IRepositorioVacuna` | Naranja + violeta | ISP + DIP | Ídem |
| `IRepositorioVenta` | Naranja + violeta | ISP + DIP | Ídem |
| `IRepositorioUsuario` | Naranja + violeta | ISP + DIP | Ídem |
| `IRepositorioChip` | Violeta | DIP | SC-2: interfaz nueva y mínima (sin `ObtenerPorResId` — ver §3.7) |
| `IRepositorioGeolocalizacion` | Violeta | DIP | SC-2: interfaz nueva y mínima |
| `IGuidProvider` | Violeta | DIP | Reemplaza `Guid.NewGuid()` inline |
| `IHasher` | Violeta | DIP | BCrypt es detalle intercambiable |
| `IChip` | Violeta | DIP | SC-2: `Res` depende de la abstracción, no del `Chip` concreto |
| `IDomainEvent` | Violeta + azul | DIP + OCP | Timestamp como parámetro (no `DateTime.UtcNow` interno) |
| `IDomainEventPublisher` | Violeta + azul | DIP + OCP | Reemplaza los 4 publishers instanciados como campos (H-02, H-20) |

#### Domain — Events (6)

| Clase | Color | Principio | Motivo |
|-------|-------|-----------|--------|
| `VacunacionCompletadaEvent`, `VacunaVencidaEvent`, `PesoMinimoEvent`, `PesoVentaEvent`, `PotreroMitadEvent`, `PotreroLlenoEvent` | Azul | OCP | Nuevo evento = nuevo record; cero modificación de publicadores ni consumers |

#### Application — Interfaces (14)

| Clase | Color (relleno + borde) | Principio(s) | Motivo |
|-------|------------------------|--------------|--------|
| `IGestorPotreros` | Violeta + verde | DIP + SRP | Abstracción que consume Web; un solo actor |
| `IGestorReses` | Violeta + verde | DIP + SRP | Ídem |
| `IServicioVacunacion` | Violeta + verde | DIP + SRP | Ídem |
| `IServicioVentas` | Violeta + verde | DIP + SRP | Ídem |
| `IServicioAutenticacion` | Violeta + verde | DIP + SRP | Solo AuthN (H-19) |
| `IAutorizador` | Violeta + verde | DIP + SRP | Solo AuthZ (H-19) |
| `IPoliticaPermisos` | Naranja + azul | ISP + OCP | Un método por rol; nuevo rol = nueva clase |
| `IDataSeeder` | Verde | SRP | Extrae la carga de datos del composition root |
| `IValidarRes` | Naranja | ISP | Un método; elimina los 12 stubs `NotImplementedException` (H-05, H-06) |
| `IValidarPotrero` | Naranja | ISP | Ídem |
| `IValidarVacuna` | Naranja | ISP | Ídem |
| `IValidarVenta` | Naranja | ISP | Ídem |
| `IServicioChip` | Naranja | ISP | SC-2: 5 operaciones mínimas |
| `IServicioGeolocalizacion` | Naranja | ISP | SC-2: 4 operaciones mínimas |

#### Application — Servicios, validadores y DTOs (16)

| Clase | Color (relleno + borde) | Principio(s) | Motivo |
|-------|------------------------|--------------|--------|
| `GestorPotreros` | Verde | SRP | 1/6 de la God Class `Hacienda` (H-01) |
| `GestorReses` | Verde + azul | SRP + OCP | Orquesta reses; eventos vía publisher inyectado |
| `ServicioVacunacion` | Verde + azul | SRP + OCP | Límites evaluados polimórficamente (sin is-checking) |
| `ServicioVentas` | Verde | SRP | Solo ventas |
| `ServicioAutenticacion` | Verde | SRP | Solo AuthN |
| `AutorizadorRbca` | Azul + violeta | OCP + DIP | Plugin registry de políticas inyectadas |
| `ServicioChip` | Verde | SRP | SC-2 |
| `ServicioGeolocalizacion` | Verde | SRP | SC-2 |
| `ValidadorRes`, `ValidadorPotrero`, `ValidadorVacuna`, `ValidadorVenta` | Naranja | ISP | Cada uno implementa solo su interfaz (H-05) |
| `PotreroDto`, `ResDto`, `VacunaDto`, `VentaDto` | Violeta | DIP | Desacoplan Web de las entidades de Domain |

#### Infrastructure (15)

| Clase | Color | Principio | Motivo |
|-------|-------|-----------|--------|
| `RepositorioPotreroSqlite` | Violeta | DIP | Adapter intercambiable detrás de la interfaz de Domain (ADR-09) |
| `RepositorioResSqlite` | Violeta | DIP | Ídem |
| `RepositorioVacunaSqlite` | Violeta | DIP | Ídem |
| `RepositorioVentaSqlite` | Violeta | DIP | Ídem |
| `RepositorioUsuarioSqlite` | Violeta | DIP | Ídem |
| `RepositorioChipSqlite` | Violeta | DIP | SC-2 |
| `RepositorioGeolocalizacionSqlite` | Violeta | DIP | SC-2 |
| `DatabaseInitializer` | Violeta | DIP | Crea y migra el schema del adapter (TPH) |
| `PoliticaAdmin`, `PoliticaEmpleado`, `PoliticaVisitante` | Azul | OCP | Plugin por rol; elimina el if-else de roles (H-19) |
| `DomainEventPublisherConsola` | Violeta | DIP | Implementación intercambiable del publisher |
| `GuidProviderSistema` | Violeta | DIP | Implementación de `IGuidProvider` |
| `HasherBcrypt` | Violeta | DIP | Implementación de `IHasher` |
| `DataLoader` | Verde | SRP | Seeder extraído del composition root |

#### Web (11)

| Clase | Color | Principio | Motivo |
|-------|-------|-----------|--------|
| `PotreroController`, `ResController`, `VacunaController`, `VentaController`, `AccountController`, `UsuarioController`, `HomeController`, `ChipController` | Verde | SRP | Delgados: solo delegan en servicios de aplicación (H-14) |
| `Controller` | Negro | — | Clase base del framework ASP.NET MVC |
| `ErrorViewModel` | Negro | — | Conservado del template |
| `LoginViewModel` | Negro | — | Conservado |

**Conteo total:** 5 negro · 25 verde · 17 azul · 11 naranja · 25 violeta · 25 con doble principio = **108 clases** (todas las del diagrama, sin excepción).

---

## 3. Capa Domain — Completa

### 3.1 Enums

> **Decisión:** Los enums se conservan para conjuntos cerrados de valores sin comportamiento. Se ubican en `Hacienda.Domain/Enums/`.

```
Hacienda.Domain/Enums/
├── TipoRes.cs
├── VacunaCategoria.cs
├── TipoPotrero.cs
├── RolUsuario.cs
└── EstadoVacuna.cs
```

#### `TipoRes` *(Azul — OCP)*

```csharp
namespace Hacienda.Domain.Enums;

public enum TipoRes : byte
{
    Ternero = 1,
    Novillo = 2,
    Cebon = 3
}
```

#### `VacunaCategoria` *(Azul — OCP)*

```csharp
namespace Hacienda.Domain.Enums;

public enum VacunaCategoria : byte
{
    Bacteriana = 1,
    Viva = 2
}
```

#### `TipoPotrero` *(Negro — conservado)*

```csharp
namespace Hacienda.Domain.Enums;

public enum TipoPotrero : byte
{
    Ternero = 1,
    Novillo = 2,
    Cebon = 3
}
```

#### `RolUsuario` *(Naranja — ISP, reemplaza hardcodeo)*

```csharp
namespace Hacienda.Domain.Enums;

public enum RolUsuario : byte
{
    Admin = 1,
    Empleado = 2,
    Visitante = 3
}
```

#### `EstadoVacuna` *(Verde — SRP, derivado de fechas)*

```csharp
namespace Hacienda.Domain.Enums;

public enum EstadoVacuna : byte
{
    Vigente = 1,
    PorVencer = 2,
    Vencida = 3
}
```

---

### 3.2 Value Objects

> **Decisión:** Conceptos que requieren validación o comportamiento se modelan como Value Objects (inmutables, comparación estructural).

```
Hacienda.Domain/ValueObjects/
├── Credencial.cs
├── Dinero.cs
└── Identificacion.cs
```

#### `Credencial` *(Verde — SRP, reemplaza contraseña en texto plano)*

```csharp
namespace Hacienda.Domain.ValueObjects;

public sealed record Credencial
{
    public string PasswordHash { get; }

    public Credencial(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash no puede ser vacío", nameof(passwordHash));
        PasswordHash = passwordHash;
    }

    public bool Verificar(string passwordPlano, IHasher hasher)
        => hasher.Verificar(passwordPlano, PasswordHash);

    public static Credencial DesdePasswordPlano(string password, IHasher hasher)
        => new Credencial(hasher.Hashear(password));
}
```

> **Dependencia inyectada:** `IHasher` (definida en Domain, implementada en Infrastructure con BCrypt/PBKDF2).

#### `Dinero` *(Verde — SRP, reemplaza uint suelto)*

```csharp
namespace Hacienda.Domain.ValueObjects;

public sealed record Dinero
{
    public decimal Monto { get; }
    public string Moneda { get; }

    public Dinero(decimal monto, string moneda = "COP")
    {
        if (monto < 0)
            throw new ArgumentException("El monto no puede ser negativo", nameof(monto));
        if (string.IsNullOrWhiteSpace(moneda))
            throw new ArgumentException("La moneda es obligatoria", nameof(moneda));
        Monto = monto;
        Moneda = moneda;
    }

    public Dinero Sumar(Dinero otro)
    {
        if (Moneda != otro.Moneda)
            throw new InvalidOperationException($"No se pueden sumar monedas distintas: {Moneda} vs {otro.Moneda}");
        return new Dinero(Monto + otro.Monto, Moneda);
    }
}
```

#### `Identificacion` *(Verde — SRP, reemplaza string suelto)*

```csharp
namespace Hacienda.Domain.ValueObjects;

public sealed record Identificacion
{
    public string Valor { get; }

    public Identificacion(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("La identificación no puede ser vacía", nameof(valor));
        Valor = valor.Trim();
    }

    public override string ToString() => Valor;
}
```

---

### 3.3 Result Types

```
Hacienda.Domain/Results/
├── ValidationResult.cs
├── ResultadoAutenticacion.cs
└── ResultadoAutorizacion.cs
```

#### `ValidationResult` *(Violeta — DIP, reemplaza HttpContext.Items)*

```csharp
namespace Hacienda.Domain.Results;

public sealed record ValidationResult
{
    public bool EsValido { get; }
    public IReadOnlyList<string> Errores { get; }

    private ValidationResult(bool esValido, IReadOnlyList<string> errores)
    {
        EsValido = esValido;
        Errores = errores;
    }

    public static ValidationResult Exito() => new(true, Array.Empty<string>());
    public static ValidationResult Fallo(params string[] errores) => new(false, errores);
}
```

#### `ResultadoAutenticacion` *(Violeta — DIP, reemplaza excepción de auth)*

```csharp
namespace Hacienda.Domain.Results;

public sealed record ResultadoAutenticacion
{
    public bool Exitoso { get; }
    public Usuario? Usuario { get; }
    public string Mensaje { get; }

    private ResultadoAutenticacion(bool exitoso, Usuario? usuario, string mensaje)
    {
        Exitoso = exitoso;
        Usuario = usuario;
        Mensaje = mensaje;
    }

    public static ResultadoAutenticacion Ok(Usuario usuario)
        => new(true, usuario, $"Autenticación exitosa para '{usuario.Nombre}'");
    public static ResultadoAutenticacion Fallido(string motivo)
        => new(false, null, motivo);
}
```

#### `ResultadoAutorizacion` *(Violeta — DIP, reemplaza parseo de ✓/✗)*

```csharp
namespace Hacienda.Domain.Results;

public sealed record ResultadoAutorizacion
{
    public bool Permitido { get; }
    public string Motivo { get; }

    private ResultadoAutorizacion(bool permitido, string motivo)
    {
        Permitido = permitido;
        Motivo = motivo;
    }

    public static ResultadoAutorizacion Concedido(string operacion)
        => new(true, $"Operación '{operacion}' autorizada");
    public static ResultadoAutorizacion Denegado(string motivo)
        => new(false, motivo);
}
```

---

### 3.4 Entities

```
Hacienda.Domain/Entities/
├── Res.cs              (abstract)
├── Ternero.cs
├── Novillo.cs
├── Cebon.cs
├── Vacuna.cs           (abstract)
├── Bacteriana.cs
├── Viva.cs
├── Potrero.cs
├── Venta.cs
└── Usuario.cs
```

---

#### `Res` (abstract) *(Rojo — LSP, Azul — OCP)*

> **Cambio LSP crítico:** El setter de `Edad` es un **assignment puro**. Cero validación en el setter. La validación de rango se mueve al boundary (`FabricaRes.Crear`, `Potrero.AgregarRes`) vía `EsEdadValida()`.
>
> **Cambio OCP crítico:** Los datos específicos de cada subtipo (límites de vacunas, pesos, validación de edad) se mueven a **métodos abstractos polimórficos**. Se eliminan todos los `if (res is Ternero)`.

```csharp
namespace Hacienda.Domain.Entities;

public abstract class Res
{
    public Guid Id { get; }
    public string Nombre { get; }
    public uint Peso { get; set; }            // set público: alimentar cambia el peso
    public ushort Edad { get; set; }           // set PURO: sin validación (LSP corregido)
    public List<Vacuna> VacunasAplicadas { get; }

    protected Res(Guid id, string nombre, uint peso, ushort edad)
    {
        Id = id;
        Nombre = nombre;
        Peso = peso;
        Edad = edad;                            // assignment puro, no lanza
        VacunasAplicadas = new List<Vacuna>();
    }

    // ── OCP: datos específicos del subtipo como abstractos polimórficos ──

    public abstract TipoRes Tipo { get; }
    public abstract byte MaxVacunasBacterianas { get; }
    public abstract byte MaxVacunasVivas { get; }
    public abstract ushort PesoMinimo { get; }
    public abstract ushort PesoRecomendadoVenta { get; }

    // ── LSP: validación de edad como método, no como restricción del setter ──

    public abstract bool EsEdadValida(ushort edad);

    // ── OCP: esquema de vacunación completo evaluado polimórficamente ──

    public virtual bool EsquemaVacunacionCompleto()
    {
        int bac = VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Bacteriana);
        int viv = VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Viva);
        return bac >= MaxVacunasBacterianas && viv >= MaxVacunasVivas;
    }

    // ── OCP: serialización polimórfica (para persistencia) ──

    public abstract string Serializar();
}
```

---

#### `Ternero` *(Rojo — LSP, Azul — OCP)*

```csharp
namespace Hacienda.Domain.Entities;

public class Ternero : Res
{
    public Ternero(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.Ternero;
    public override byte MaxVacunasBacterianas => 3;
    public override byte MaxVacunasVivas => 1;
    public override ushort PesoMinimo => 150;
    public override ushort PesoRecomendadoVenta => 250;

    public override bool EsEdadValida(ushort edad) => edad <= 12;

    public override string Serializar()
        => $"{Id}|{Nombre}|{Peso}|{Edad}|Ternero";
}
```

---

#### `Novillo` *(Rojo — LSP, Azul — OCP)*

```csharp
namespace Hacienda.Domain.Entities;

public class Novillo : Res
{
    public Novillo(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.Novillo;
    public override byte MaxVacunasBacterianas => 2;
    public override byte MaxVacunasVivas => 2;
    public override ushort PesoMinimo => 400;
    public override ushort PesoRecomendadoVenta => 550;

    // Bug corregido: antes decía "El ternero excedió..." (H-08)
    public override bool EsEdadValida(ushort edad) => edad > 48;

    public override string Serializar()
        => $"{Id}|{Nombre}|{Peso}|{Edad}|Novillo";
}
```

---

#### `Cebon` *(Rojo — LSP, Azul — OCP)*

```csharp
namespace Hacienda.Domain.Entities;

public class Cebon : Res
{
    public Cebon(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.Cebon;
    public override byte MaxVacunasBacterianas => 1;
    public override byte MaxVacunasVivas => 4;
    public override ushort PesoMinimo => 290;
    public override ushort PesoRecomendadoVenta => 420;

    public override bool EsEdadValida(ushort edad) => edad > 12 && edad <= 48;

    public override string Serializar()
        => $"{Id}|{Nombre}|{Peso}|{Edad}|Cebon";
}
```

---

#### `Vacuna` (abstract) *(Azul — OCP)*

> **Cambio OCP:** La categoría de vacuna se expone como abstract en la base. Se elimina el `if (vac is Bacteriana)`.
>
> **Cambio LSP:** Se agrega `DetalleVisual()` abstract para que cada subtipo provea su representación sin type-testing en views.

```csharp
namespace Hacienda.Domain.Entities;

public abstract class Vacuna
{
    public Guid Id { get; }
    public string Nombre { get; }
    public string Lote { get; }
    public DateTime FechaVencimiento { get; }
    public DateTime FechaAplicacion { get; }

    protected Vacuna(Guid id, string nombre, string lote, DateTime fechaVencimiento, DateTime fechaAplicacion)
    {
        Id = id;
        Nombre = nombre;
        Lote = lote;
        FechaVencimiento = fechaVencimiento;
        FechaAplicacion = fechaAplicacion;
    }

    public abstract VacunaCategoria Categoria { get; }
    public abstract string Serializar();
    public abstract string DetalleVisual();

    public EstadoVacuna CalcularEstado(TimeProvider reloj)
    {
        var ahora = reloj.GetUtcNow();
        if (FechaVencimiento <= ahora)
            return EstadoVacuna.Vencida;
        if (FechaVencimiento <= ahora.AddMonths(1))
            return EstadoVacuna.PorVencer;
        return EstadoVacuna.Vigente;
    }
}
```

---

#### `Bacteriana` *(Azul — OCP)*

```csharp
namespace Hacienda.Domain.Entities;

public class Bacteriana : Vacuna
{
    public uint PeriodoAplicacion { get; }

    public Bacteriana(Guid id, string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, uint periodoAplicacion)
        : base(id, nombre, lote, fechaVencimiento, fechaAplicacion)
    {
        if (periodoAplicacion < 2 || periodoAplicacion > 4)
            throw new ArgumentException(
                $"Período debe estar entre 2 y 4 semanas. Recibido: {periodoAplicacion}");
        PeriodoAplicacion = periodoAplicacion;
    }

    public override VacunaCategoria Categoria => VacunaCategoria.Bacteriana;

    public override string Serializar()
        => $"{Nombre}|{Lote}|{FechaVencimiento:yyyy-MM-dd}|{FechaAplicacion:yyyy-MM-dd}|Bacteriana|{PeriodoAplicacion}";

    public override string DetalleVisual() => $"{PeriodoAplicacion} sem.";
}
```

---

#### `Viva` *(Azul — OCP)*

```csharp
namespace Hacienda.Domain.Entities;

public class Viva : Vacuna
{
    public enum GradoAtenuacion : byte
    {
        Atenuacion10 = 10,
        Atenuacion20 = 20,
        Atenuacion30 = 30
    }

    public GradoAtenuacion Atenuacion { get; }

    public Viva(Guid id, string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, GradoAtenuacion atenuacion)
        : base(id, nombre, lote, fechaVencimiento, fechaAplicacion)
    {
        Atenuacion = atenuacion;
    }

    public override VacunaCategoria Categoria => VacunaCategoria.Viva;

    public override string Serializar()
        => $"{Nombre}|{Lote}|{FechaVencimiento:yyyy-MM-dd}|{FechaAplicacion:yyyy-MM-dd}|Viva|{(byte)Atenuacion}";

    public override string DetalleVisual() => "Atenuada";
}
```

---

#### `Potrero` *(Verde — SRP)*

> **Cambio SRP:** `Potrero` solo mantiene invariantes de capacidad. Ya NO crea subtipos de `Res` (eso va en `FabricaRes`). Ya NO dispara eventos (eso va en `GestorReses`). Ya NO valida edad (eso va en `FabricaRes`).

```csharp
namespace Hacienda.Domain.Entities;

public class Potrero
{
    public Guid Id { get; }
    public Identificacion Identificacion { get; }
    public TipoPotrero Tipo { get; }
    public List<Res> Reses { get; }

    private const ushort MAX_RESES = 150;

    public Potrero(Guid id, Identificacion identificacion, TipoPotrero tipo)
    {
        Id = id;
        Identificacion = identificacion;
        Tipo = tipo;
        Reses = new List<Res>();
    }

    // Única responsabilidad: agregar res respetando capacidad
    public void AgregarRes(Res res)
    {
        if (Reses.Count >= MAX_RESES)
            throw new InvalidOperationException(
                $"El potrero '{Identificacion}' está lleno ({MAX_RESES} reses máximo)");
        Reses.Add(res);
    }

    public void RemoverRes(Res res)
        => Reses.Remove(res);

    public Res? BuscarRes(string nombre)
        => Reses.FirstOrDefault(r =>
            r.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

    public ushort CantidadReses => (ushort)Reses.Count;
    public bool EstaAlaMitad => CantidadReses == MAX_RESES / 2;
    public bool EstaLleno => CantidadReses >= MAX_RESES;
}
```

---

#### `Venta` *(Negro — conservado con limpieza)*

```csharp
namespace Hacienda.Domain.Entities;

public class Venta
{
    public Guid Id { get; }
    public DateTime Fecha { get; }
    public Res Res { get; }              // snapshot de la res vendida
    public string PotreroOrigen { get; } // identificación del potrero
    public Dinero Monto { get; }

    public Venta(Guid id, DateTime fecha, Res res, string potreroOrigen, Dinero monto)
    {
        Id = id;
        Fecha = fecha;
        Res = res;
        PotreroOrigen = potreroOrigen;
        Monto = monto;
    }
}
```

---

#### `Usuario` *(Naranja — ISP, Verde — SRP)*

> **Cambio:** Ya no tiene `Contrasena` en texto plano. Usa `RolUsuario` enum en vez de comparar `Nombre == "admin"`. La credencial es un Value Object.

```csharp
namespace Hacienda.Domain.Entities;

public class Usuario
{
    public Guid Id { get; }
    public string Nombre { get; }
    public Credencial Credencial { get; }
    public RolUsuario Rol { get; }

    public Usuario(Guid id, string nombre, Credencial credencial, RolUsuario rol)
    {
        Id = id;
        Nombre = nombre;
        Credencial = credencial;
        Rol = rol;
    }
}
```

---

### 3.5 Domain Services — Factories

```
Hacienda.Domain/Factories/
├── IPotreroFactory.cs
├── FabricaPotrero.cs
├── IResFactory.cs
├── FabricaRes.cs
├── IVacunaFactory.cs
├── FabricaVacuna.cs
├── IVentaFactory.cs
└── FabricaVenta.cs
```

> **Decisión arquitectónica:** Las factories de entidades de dominio viven en Domain (tanto interfaz como implementación), porque crean entidades de dominio y contienen reglas de negocio del dominio.

#### `IPotreroFactory` *(Violeta — DIP)*

```csharp
namespace Hacienda.Domain.Factories;

public interface IPotreroFactory
{
    Potrero Crear(string identificacion, TipoPotrero tipo);
}
```

#### `FabricaPotrero`

```csharp
namespace Hacienda.Domain.Factories;

public class FabricaPotrero : IPotreroFactory
{
    private readonly IGuidProvider _guidProvider;

    public FabricaPotrero(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public Potrero Crear(string identificacion, TipoPotrero tipo)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            throw new ArgumentException("La identificación no puede ser vacía", nameof(identificacion));

        return new Potrero(
            _guidProvider.Nuevo(),
            new Identificacion(identificacion),
            tipo);
    }
}
```

#### `IResFactory` *(Violeta — DIP, Azul — OCP)*

```csharp
namespace Hacienda.Domain.Factories;

public interface IResFactory
{
    Res Crear(TipoRes tipo, string nombre, uint peso, ushort edad);
}
```

#### `FabricaRes` — Plugin Registry *(Azul — OCP)*

```csharp
namespace Hacienda.Domain.Factories;

public class FabricaRes : IResFactory
{
    private readonly Dictionary<TipoRes, Func<string, uint, ushort, Res>> _creators;
    private readonly IGuidProvider _guidProvider;

    public FabricaRes(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
        _creators = new()
        {
            [TipoRes.Ternero] = (n, p, e) => new Ternero(_guidProvider.Nuevo(), n, p, e),
            [TipoRes.Novillo] = (n, p, e) => new Novillo(_guidProvider.Nuevo(), n, p, e),
            [TipoRes.Cebon]   = (n, p, e) => new Cebon(_guidProvider.Nuevo(), n, p, e),
        };
    }

    public Res Crear(TipoRes tipo, string nombre, uint peso, ushort edad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede ser vacío", nameof(nombre));

        if (!_creators.TryGetValue(tipo, out var creator))
            throw new ArgumentException($"Tipo de res no soportado: {tipo}");

        Res res = creator(nombre, peso, edad);

        // ── Validación de edad en el BOUNDARY, no en el setter (LSP) ──
        if (!res.EsEdadValida(edad))
            throw new InvalidOperationException(
                $"La edad {edad} no es válida para {tipo}. Rango: {DescribirRango(res)}");

        return res;
    }

    private static string DescribirRango(Res res) => res.Tipo switch
    {
        TipoRes.Ternero => "0-12 meses",
        TipoRes.Cebon => "13-48 meses",
        TipoRes.Novillo => "49+ meses",
        _ => "desconocido"
    };
}
```

> **Extensión OCP:** Agregar `VacaLechera : Res` = crear 1 clase nueva + agregar 1 entrada al diccionario del constructor. **Cero modificaciones en consumers.**

---

#### `IVacunaFactory` *(Violeta — DIP, Azul — OCP)*

```csharp
namespace Hacienda.Domain.Factories;

public interface IVacunaFactory
{
    Vacuna CrearBacteriana(string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, uint periodoAplicacion);

    Vacuna CrearViva(string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, Viva.GradoAtenuacion atenuacion);
}
```

#### `FabricaVacuna`

```csharp
namespace Hacienda.Domain.Factories;

public class FabricaVacuna : IVacunaFactory
{
    private readonly IGuidProvider _guidProvider;

    public FabricaVacuna(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public Vacuna CrearBacteriana(string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, uint periodoAplicacion)
    {
        ValidarParametrosComunes(nombre, lote, fechaVencimiento, fechaAplicacion);
        return new Bacteriana(_guidProvider.Nuevo(), nombre, lote,
            fechaVencimiento, fechaAplicacion, periodoAplicacion);
    }

    public Vacuna CrearViva(string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, Viva.GradoAtenuacion atenuacion)
    {
        ValidarParametrosComunes(nombre, lote, fechaVencimiento, fechaAplicacion);
        return new Viva(_guidProvider.Nuevo(), nombre, lote,
            fechaVencimiento, fechaAplicacion, atenuacion);
    }

    private static void ValidarParametrosComunes(
        string nombre, string lote, DateTime fechaVenc, DateTime fechaAplic)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("Nombre vacío", nameof(nombre));
        if (string.IsNullOrWhiteSpace(lote))
            throw new ArgumentException("Lote vacío", nameof(lote));
        if (fechaVenc <= fechaAplic)
            throw new ArgumentException("El vencimiento debe ser posterior a la aplicación");
    }
}
```

---

#### `IVentaFactory` *(Violeta — DIP)*

```csharp
namespace Hacienda.Domain.Factories;

public interface IVentaFactory
{
    Venta Crear(Res res, string potreroOrigen, decimal monto, TimeProvider reloj);
}
```

#### `FabricaVenta`

```csharp
namespace Hacienda.Domain.Factories;

public class FabricaVenta : IVentaFactory
{
    private readonly IGuidProvider _guidProvider;

    public FabricaVenta(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public Venta Crear(Res res, string potreroOrigen, decimal monto, TimeProvider reloj)
    {
        if (res == null) throw new ArgumentNullException(nameof(res));
        if (monto < 0) throw new ArgumentException("Monto negativo", nameof(monto));

        return new Venta(
            _guidProvider.Nuevo(),
            reloj.GetUtcNow().DateTime,
            res,
            potreroOrigen,
            new Dinero(monto));
    }
}
```

---

### 3.6 Domain Interfaces — Abstracciones

```
Hacienda.Domain/Interfaces/
├── IGuidProvider.cs
├── IHasher.cs
├── IRepositorioPotrero.cs
├── IRepositorioRes.cs
├── IRepositorioVacuna.cs
├── IRepositorioVenta.cs
├── IRepositorioUsuario.cs
├── IDomainEventPublisher.cs
├── IDomainEvent.cs
└── IDomainEventHandler.cs
```

#### `IGuidProvider` *(Violeta — DIP)*

```csharp
namespace Hacienda.Domain.Interfaces;

public interface IGuidProvider
{
    Guid Nuevo();
}

// Implementation en Infrastructure:
// public class GuidProviderSistema : IGuidProvider { public Guid Nuevo() => Guid.NewGuid(); }
```

#### `IHasher` *(Violeta — DIP)*

```csharp
namespace Hacienda.Domain.Interfaces;

public interface IHasher
{
    string Hashear(string passwordPlano);
    bool Verificar(string passwordPlano, string passwordHash);
}

// Implementation en Infrastructure:
// public class HasherBcrypt : IHasher { ... usa BCrypt.Net }
```

#### `IDomainEvent` / `IDomainEventPublisher` / `IDomainEventHandler` *(Violeta — DIP, Azul — OCP)*

> **Nota DIP:** Los eventos de dominio reciben `OcurridoEn` como parámetro (inyectado desde servicios), no lo generan internamente con `DateTime.UtcNow`.

```csharp
namespace Hacienda.Domain.Interfaces;

public interface IDomainEvent
{
    DateTime OcurridoEn { get; }
}

public interface IDomainEventPublisher
{
    void Publicar<TEvento>(TEvento evento) where TEvento : IDomainEvent;
}

public interface IDomainEventHandler<in TEvento> where TEvento : IDomainEvent
{
    void Manejar(TEvento evento);
}
```

---

### 3.7 Repository Interfaces *(Naranja — ISP, Violeta — DIP)*

> Cada repositorio conoce **solo** su agregado. Un cliente nunca depende de métodos que no usa.
>
> **Nota ISP:** `IRepositorioChip` no incluye `ObtenerPorResId` porque ningún consumidor lo necesita y su implementación anterior retornaba `null` falso (violación LSP). Si aparece un consumidor, se agrega vía interfaz segregada.

```csharp
// IRepositorioPotrero.cs
namespace Hacienda.Domain.Interfaces;

public interface IRepositorioPotrero
{
    List<Potrero> ObtenerTodos();
    Potrero? ObtenerPorIdentificacion(string identificacion);
    void GuardarTodos(List<Potrero> potreros);
}

// IRepositorioRes.cs
public interface IRepositorioRes
{
    List<Res> ObtenerTodas();
    void GuardarTodas(List<Potrero> potreros); // reses viven dentro de potreros
}

// IRepositorioVacuna.cs
public interface IRepositorioVacuna
{
    List<Vacuna> ObtenerTodas();
    void GuardarTodas(List<Vacuna> vacunas);
    void GuardarAplicadas(List<Potrero> potreros);
}

// IRepositorioVenta.cs
public interface IRepositorioVenta
{
    List<Venta> ObtenerTodas();
    void GuardarTodas(List<Venta> ventas);
}

// IRepositorioUsuario.cs
public interface IRepositorioUsuario
{
    List<Usuario> ObtenerTodos();
    void GuardarTodos(List<Usuario> usuarios);
}
```

---

### 3.8 Domain Events (definiciones concretas)

```
Hacienda.Domain/Events/
├── VacunacionCompletadaEvent.cs
├── VacunaVencidaEvent.cs
├── PesoMinimoEvent.cs
├── PesoVentaEvent.cs
├── PotreroMitadEvent.cs
└── PotreroLlenoEvent.cs
```

```csharp
// Patrón DIP: el timestamp se recibe como parámetro, no se genera internamente.
// Los servicios de aplicación proveen _reloj.GetUtcNow().DateTime.
namespace Hacienda.Domain.Events;

public record VacunacionCompletadaEvent : IDomainEvent
{
    public string NombreRes { get; }
    public DateTime OcurridoEn { get; }

    public VacunacionCompletadaEvent(string nombreRes, DateTime ocurridoEn)
    {
        NombreRes = nombreRes;
        OcurridoEn = ocurridoEn;
    }
}

public record PesoMinimoEvent : IDomainEvent
{
    public string NombreRes { get; }
    public uint PesoActual { get; }
    public DateTime OcurridoEn { get; }

    public PesoMinimoEvent(string nombreRes, uint pesoActual, DateTime ocurridoEn)
    {
        NombreRes = nombreRes;
        PesoActual = pesoActual;
        OcurridoEn = ocurridoEn;
    }
}

public record PesoVentaEvent : IDomainEvent
{
    public string NombreRes { get; }
    public uint PesoActual { get; }
    public DateTime OcurridoEn { get; }

    public PesoVentaEvent(string nombreRes, uint pesoActual, DateTime ocurridoEn)
    {
        NombreRes = nombreRes;
        PesoActual = pesoActual;
        OcurridoEn = ocurridoEn;
    }
}

public record PotreroMitadEvent : IDomainEvent
{
    public string IdentificacionPotrero { get; }
    public DateTime OcurridoEn { get; }

    public PotreroMitadEvent(string identificacionPotrero, DateTime ocurridoEn)
    {
        IdentificacionPotrero = identificacionPotrero;
        OcurridoEn = ocurridoEn;
    }
}

public record PotreroLlenoEvent : IDomainEvent
{
    public string IdentificacionPotrero { get; }
    public DateTime OcurridoEn { get; }

    public PotreroLlenoEvent(string identificacionPotrero, DateTime ocurridoEn)
    {
        IdentificacionPotrero = identificacionPotrero;
        OcurridoEn = ocurridoEn;
    }
}

public record VacunaVencidaEvent : IDomainEvent
{
    public string NombreVacuna { get; }
    public string Lote { get; }
    public DateTime FechaVencimiento { get; }
    public DateTime OcurridoEn { get; }

    public VacunaVencidaEvent(string nombreVacuna, string lote, DateTime fechaVencimiento, DateTime ocurridoEn)
    {
        NombreVacuna = nombreVacuna;
        Lote = lote;
        FechaVencimiento = fechaVencimiento;
        OcurridoEn = ocurridoEn;
    }
}
```

---

## 4. Capa Application — Completa

### 4.1 Application Interfaces

```
Hacienda.Application/Interfaces/
├── IGestorPotreros.cs
├── IGestorReses.cs
├── IServicioVacunacion.cs
├── IInventarioVacunas.cs
├── IServicioVentas.cs
├── IServicioAutenticacion.cs
├── IAutorizador.cs
├── IPoliticaPermisos.cs
├── IValidarRes.cs
├── IValidarPotrero.cs
├── IValidarVacuna.cs
├── IValidarVenta.cs
└── IDataSeeder.cs
```

#### Gestores / Servicios de Aplicación *(Verde — SRP, Violeta — DIP)*

```csharp
// IGestorPotreros.cs — actor: administrador de potreros
namespace Hacienda.Application.Interfaces;

public interface IGestorPotreros
{
    string CrearPotrero(string identificacion, TipoPotrero tipo);
    Potrero? BuscarPotrero(string identificacion);
    List<Potrero> ListarPotreros();
    Dictionary<string, object> ObtenerEstadisticas();
}

// IGestorReses.cs — actor: operador de ganado
public interface IGestorReses
{
    // El TipoRes se deriva automáticamente del TipoPotrero (preserva comportamiento AS-IS)
    string AgregarRes(string potreroId, string nombre, ushort edad, uint peso);
    string AlimentarRes(string potreroId, string nombreRes);
    string AlimentarRes(string potreroId, string nombreRes, uint cantidad);
    List<(Potrero Potrero, Res Res)> ListarReses();
    Dictionary<string, object> ObtenerEstadisticas();
}

// IServicioVacunacion.cs — actor: veterinario
// NOTA: Se fusiona InventarioVacunas + ServicioVacunacion en un solo servicio
// porque ambos operan sobre el mismo agregado (vacunas) y comparten repositorios.
// La separación artificial generaría un service anémico sin lógica propia.
public interface IServicioVacunacion
{
    string AplicarVacuna(string loteVacuna, string potreroId, string nombreRes);
    string CrearVacunaBacteriana(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo);
    string CrearVacunaViva(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion);
    string CrearLoteVacunaBacteriana(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo, uint cantidad);
    string CrearLoteVacunaViva(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion, uint cantidad);
    List<Vacuna> ListarVacunasDisponibles();
    Dictionary<string, object> ObtenerEstadisticas();
}

// IServicioVentas.cs — actor: vendedor / administrador
public interface IServicioVentas
{
    string VenderRes(string potreroId, string nombreRes, decimal monto);
    List<Venta> ListarVentas();
    Dictionary<string, object> ObtenerEstadisticas();
}

// IServicioAutenticacion.cs — actor: sistema de identidad
public interface IServicioAutenticacion
{
    ResultadoAutenticacion Autenticar(string username, string password);
}

// IAutorizador.cs — actor: sistema de seguridad
public interface IAutorizador
{
    ResultadoAutorizacion Autorizar(Usuario usuario, string operacion);
}

// IPoliticaPermisos.cs — Naranja (ISP) + Azul (OCP): plugin por rol
public interface IPoliticaPermisos
{
    RolUsuario Rol { get; }
    ResultadoAutorizacion Evaluar(string operacion);
}

// IDataSeeder.cs — Verde (SRP): extrae carga de datos del composition root
public interface IDataSeeder
{
    Task CargarDatosAsync();
}
```

#### Validation Interfaces *(Naranja — ISP: 1 método cada una)*

```csharp
// IValidarRes.cs
public interface IValidarRes
{
    ValidationResult Validar(Res res);
}

// IValidarPotrero.cs
public interface IValidarPotrero
{
    ValidationResult Validar(Potrero potrero);
}

// IValidarVacuna.cs
public interface IValidarVacuna
{
    ValidationResult Validar(Vacuna vacuna);
}

// IValidarVenta.cs
public interface IValidarVenta
{
    ValidationResult Validar(Venta venta);
}
```

---

### 4.2 Application Services (Implementaciones)

```
Hacienda.Application/Services/
├── GestorPotreros.cs
├── GestorReses.cs
├── ServicioVacunacion.cs
├── ServicioVentas.cs
├── ServicioAutenticacion.cs
└── AutorizadorRbca.cs
```

#### `GestorPotreros` *(Verde — SRP)*

```csharp
namespace Hacienda.Application.Services;

public class GestorPotreros : IGestorPotreros
{
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IValidarPotrero _validador;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IPotreroFactory _fabricaPotrero;

    public GestorPotreros(
        IRepositorioPotrero repoPotrero,
        IValidarPotrero validador,
        IDomainEventPublisher eventPublisher,
        IPotreroFactory fabricaPotrero)
    {
        _repoPotrero = repoPotrero;
        _validador = validador;
        _eventPublisher = eventPublisher;
        _fabricaPotrero = fabricaPotrero;
    }

    public string CrearPotrero(string identificacion, TipoPotrero tipo)
    {
        // Validar unicidad
        if (_repoPotrero.ObtenerPorIdentificacion(identificacion) != null)
            throw new InvalidOperationException($"Ya existe un potrero '{identificacion}'");

        // Crear entidad via factory
        var potrero = _fabricaPotrero.Crear(identificacion, tipo);

        // Validar antes de persistir
        var validacion = _validador.Validar(potrero);
        if (!validacion.EsValido)
            return string.Join("; ", validacion.Errores);

        // Persistir
        var potreros = _repoPotrero.ObtenerTodos();
        potreros.Add(potrero);
        _repoPotrero.GuardarTodos(potreros);

        return $"El potrero '{identificacion}' se añadió con éxito.";
    }

    public Potrero? BuscarPotrero(string identificacion)
        => _repoPotrero.ObtenerPorIdentificacion(identificacion);

    public List<Potrero> ListarPotreros()
        => _repoPotrero.ObtenerTodos().OrderBy(p => p.Identificacion.Valor).ToList();

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var potreros = _repoPotrero.ObtenerTodos();
        return new Dictionary<string, object>
        {
            ["TotalPotreros"] = potreros.Count,
            ["TotalReses"] = potreros.Sum(p => p.CantidadReses),
            ["PotrerosVacios"] = potreros.Count(p => p.CantidadReses == 0),
            ["PotrerosConReses"] = potreros.Count(p => p.CantidadReses > 0)
        };
    }
}
```

#### `GestorReses` *(Verde — SRP, Azul — OCP)*

```csharp
namespace Hacienda.Application.Services;

public class GestorReses : IGestorReses
{
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IResFactory _fabricaRes;
    private readonly IValidarRes _validador;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly TimeProvider _reloj;

    public GestorReses(
        IRepositorioPotrero repoPotrero,
        IResFactory fabricaRes,
        IValidarRes validador,
        IDomainEventPublisher eventPublisher,
        TimeProvider reloj)
    {
        _repoPotrero = repoPotrero;
        _fabricaRes = fabricaRes;
        _validador = validador;
        _eventPublisher = eventPublisher;
        _reloj = reloj;
    }

    public string AgregarRes(string potreroId, string nombre, ushort edad, uint peso)
    {
        // Buscar potrero
        var potrero = _repoPotrero.ObtenerPorIdentificacion(potreroId)
            ?? throw new InvalidOperationException($"Potrero '{potreroId}' no encontrado");

        // Validar que no existe una res con ese nombre
        if (potrero.BuscarRes(nombre) != null)
            throw new InvalidOperationException($"Ya existe una res '{nombre}' en el potrero '{potreroId}'");

        // Derivar TipoRes del TipoPotrero (preserva comportamiento AS-IS)
        TipoRes tipo = MapearTipoRes(potrero.Tipo);

        // Factory crea + valida edad (boundary)
        Res res = _fabricaRes.Crear(tipo, nombre, peso, edad);

        // Validar antes de agregar
        var validacion = _validador.Validar(res);
        if (!validacion.EsValido)
            return string.Join("; ", validacion.Errores);

        // Agregar al potrero (entidad mantiene su invariante de capacidad)
        potrero.AgregarRes(res);

        // Disparar eventos de dominio (OCP: publisher inyectado, no acoplado)
        string mensajeEventos = "";
        if (res.Peso < res.PesoMinimo)
        {
            _eventPublisher.Publicar(new PesoMinimoEvent(res.Nombre, res.Peso));
            mensajeEventos += $"\n[Evento] La res '{res.Nombre}' está en desnutrición ({res.Peso} kg).";
        }
        if (res.Peso >= res.PesoRecomendadoVenta)
        {
            _eventPublisher.Publicar(new PesoVentaEvent(res.Nombre, res.Peso));
            mensajeEventos += $"\n[Evento] La res '{res.Nombre}' está apta para venta ({res.Peso} kg).";
        }
        if (potrero.EstaAlaMitad)
        {
            _eventPublisher.Publicar(new PotreroMitadEvent(potreroId));
            mensajeEventos += $"\n[Evento] El potrero '{potreroId}' alcanzó la mitad de su capacidad.";
        }
        if (potrero.EstaLleno)
        {
            _eventPublisher.Publicar(new PotreroLlenoEvent(potreroId));
            mensajeEventos += $"\n[Evento] El potrero '{potreroId}' está lleno.";
        }

        // Persistir
        _repoPotrero.GuardarTodos(_repoPotrero.ObtenerTodos());

        return $"La res '{nombre}' fue añadida al potrero '{potreroId}'.{mensajeEventos}";
    }

    // Sobrecarga sin cantidad: alimenta +1 (preserva comportamiento AS-IS)
    public string AlimentarRes(string potreroId, string nombreRes)
        => AlimentarRes(potreroId, nombreRes, 1);

    public string AlimentarRes(string potreroId, string nombreRes, uint cantidad)
    {
        var potrero = _repoPotrero.ObtenerPorIdentificacion(potreroId)
            ?? throw new InvalidOperationException($"Potrero '{potreroId}' no encontrado");
        var res = potrero.BuscarRes(nombreRes)
            ?? throw new InvalidOperationException($"Res '{nombreRes}' no encontrada");

        // Incrementar peso (setter puro, no lanza — LSP)
        res.Peso += cantidad;

        string mensajeEventos = "";
        if (res.Peso < res.PesoMinimo)
        {
            _eventPublisher.Publicar(new PesoMinimoEvent(res.Nombre, res.Peso));
            mensajeEventos += $"\n[Evento] La res '{res.Nombre}' sigue en desnutrición ({res.Peso} kg).";
        }
        if (res.Peso >= res.PesoRecomendadoVenta)
        {
            _eventPublisher.Publicar(new PesoVentaEvent(res.Nombre, res.Peso));
            mensajeEventos += $"\n[Evento] La res '{res.Nombre}' está apta para venta ({res.Peso} kg).";
        }

        _repoPotrero.GuardarTodos(_repoPotrero.ObtenerTodos());

        return $"La res '{res.Nombre}' fue alimentada, ahora pesa {res.Peso} kg.{mensajeEventos}";
    }

    public List<(Potrero Potrero, Res Res)> ListarReses()
    {
        var resultado = new List<(Potrero, Res)>();
        foreach (var potrero in _repoPotrero.ObtenerTodos())
            foreach (var res in potrero.Reses)
                resultado.Add((potrero, res));
        return resultado;
    }

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var todas = ListarReses();
        return new Dictionary<string, object>
        {
            ["TotalReses"] = todas.Count,
            ["Terneros"] = todas.Count(r => r.Res.Tipo == TipoRes.Ternero),
            ["Cebones"] = todas.Count(r => r.Res.Tipo == TipoRes.Cebon),
            ["Novillos"] = todas.Count(r => r.Res.Tipo == TipoRes.Novillo),
            ["PesoPromedio"] = todas.Any() ? todas.Average(r => r.Res.Peso) : 0
        };
    }

    // Mapea TipoPotrero → TipoRes (preserva regla AS-IS: potrero ternero solo acepta terneros)
    private static TipoRes MapearTipoRes(TipoPotrero tipoPotrero) => tipoPotrero switch
    {
        TipoPotrero.Ternero => TipoRes.Ternero,
        TipoPotrero.Cebon => TipoRes.Cebon,
        TipoPotrero.Novillo => TipoRes.Novillo,
        _ => throw new ArgumentException($"Tipo de potrero no reconocido: {tipoPotrero}")
    };
}
```

#### `ServicioVacunacion` *(Verde — SRP, Azul — OCP)*

```csharp
namespace Hacienda.Application.Services;

public class ServicioVacunacion : IServicioVacunacion
{
    private readonly IRepositorioVacuna _repoVacuna;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IVacunaFactory _fabricaVacuna;
    private readonly IDomainEventPublisher _eventPublisher;

    public ServicioVacunacion(
        IRepositorioVacuna repoVacuna,
        IRepositorioPotrero repoPotrero,
        IVacunaFactory fabricaVacuna,
        IDomainEventPublisher eventPublisher)
    {
        _repoVacuna = repoVacuna;
        _repoPotrero = repoPotrero;
        _fabricaVacuna = fabricaVacuna;
        _eventPublisher = eventPublisher;
    }

    public string CrearVacunaBacteriana(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo)
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        if (vacunas.Any(v => v.Lote.Equals(lote, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ya existe una vacuna con lote '{lote}'");

        Vacuna vacuna = _fabricaVacuna.CrearBacteriana(nombre, lote, fechaVenc, fechaAplic, periodo);
        vacunas.Add(vacuna);
        _repoVacuna.GuardarTodas(vacunas);

        return $"Vacuna bacteriana '{nombre}' (lote '{lote}') agregada al inventario.";
    }

    public string CrearVacunaViva(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion)
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        if (vacunas.Any(v => v.Lote.Equals(lote, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ya existe una vacuna con lote '{lote}'");

        Vacuna vacuna = _fabricaVacuna.CrearViva(nombre, lote, fechaVenc, fechaAplic, atenuacion);
        vacunas.Add(vacuna);
        _repoVacuna.GuardarTodas(vacunas);

        return $"Vacuna viva '{nombre}' (lote '{lote}') agregada al inventario.";
    }

    public string CrearLoteVacunaBacteriana(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo, uint cantidad)
    {
        if (cantidad == 0 || cantidad > 100)
            throw new ArgumentException("La cantidad debe estar entre 1 y 100");

        var vacunas = _repoVacuna.ObtenerTodas();
        int vacunasCreadas = 0;

        for (int i = 1; i <= cantidad; i++)
        {
            string loteNumerado = $"{loteBase}-{i:D3}";
            if (vacunas.Any(v => v.Lote.Equals(loteNumerado, StringComparison.OrdinalIgnoreCase)))
                continue;

            Vacuna vacuna = _fabricaVacuna.CrearBacteriana(nombre, loteNumerado, fechaVenc, fechaAplic, periodo);
            vacunas.Add(vacuna);
            vacunasCreadas++;
        }

        if (vacunasCreadas == 0)
            throw new InvalidOperationException("No se pudo crear ninguna vacuna. Todos los lotes ya existen");

        _repoVacuna.GuardarTodas(vacunas);

        return $"Lote de vacunas bacterianas creado: {vacunasCreadas} de {cantidad}. " +
               $"Lotes: {loteBase}-001 a {loteBase}-{vacunasCreadas:D3}. Período: {periodo} semanas.";
    }

    public string CrearLoteVacunaViva(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion, uint cantidad)
    {
        if (cantidad == 0 || cantidad > 100)
            throw new ArgumentException("La cantidad debe estar entre 1 y 100");

        var vacunas = _repoVacuna.ObtenerTodas();
        int vacunasCreadas = 0;

        for (int i = 1; i <= cantidad; i++)
        {
            string loteNumerado = $"{loteBase}-{i:D3}";
            if (vacunas.Any(v => v.Lote.Equals(loteNumerado, StringComparison.OrdinalIgnoreCase)))
                continue;

            Vacuna vacuna = _fabricaVacuna.CrearViva(nombre, loteNumerado, fechaVenc, fechaAplic, atenuacion);
            vacunas.Add(vacuna);
            vacunasCreadas++;
        }

        if (vacunasCreadas == 0)
            throw new InvalidOperationException("No se pudo crear ninguna vacuna. Todos los lotes ya existen");

        _repoVacuna.GuardarTodas(vacunas);

        return $"Lote de vacunas vivas creado: {vacunasCreadas} de {cantidad}. " +
               $"Lotes: {loteBase}-001 a {loteBase}-{vacunasCreadas:D3}. Atenuación: {(byte)atenuacion}.";
    }

    public string AplicarVacuna(string loteVacuna, string potreroId, string nombreRes)
    {
        // Buscar vacuna en inventario
        var vacunas = _repoVacuna.ObtenerTodas();
        var vacuna = vacunas.FirstOrDefault(v => v.Lote.Equals(loteVacuna, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Vacuna con lote '{loteVacuna}' no encontrada");

        // Buscar res
        var potrero = _repoPotrero.ObtenerPorIdentificacion(potreroId)
            ?? throw new InvalidOperationException($"Potrero '{potreroId}' no encontrado");
        var res = potrero.BuscarRes(nombreRes)
            ?? throw new InvalidOperationException($"Res '{nombreRes}' no encontrada");

        // Validar que no esté ya aplicada
        if (res.VacunasAplicadas.Any(v => v.Lote.Equals(loteVacuna, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"La vacuna lote '{loteVacuna}' ya fue aplicada a '{nombreRes}'");

        // OCP: límites calculados polimórficamente (sin is-checking)
        int bacActual = res.VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Bacteriana);
        int vivActual = res.VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Viva);

        if (vacuna.Categoria == VacunaCategoria.Bacteriana && bacActual >= res.MaxVacunasBacterianas)
            throw new InvalidOperationException(
                $"No se pueden aplicar más bacterianas a '{nombreRes}' (máximo {res.MaxVacunasBacterianas})");

        if (vacuna.Categoria == VacunaCategoria.Viva && vivActual >= res.MaxVacunasVivas)
            throw new InvalidOperationException(
                $"No se pueden aplicar más vivas a '{nombreRes}' (máximo {res.MaxVacunasVivas})");

        // Aplicar
        res.VacunasAplicadas.Add(vacuna);
        vacunas.Remove(vacuna);

        // Persistir ambos stores
        _repoVacuna.GuardarTodas(vacunas);
        _repoVacuna.GuardarAplicadas(_repoPotrero.ObtenerTodos());
        _repoPotrero.GuardarTodos(_repoPotrero.ObtenerTodos());

        // OCP: esquema completo evaluado polimórficamente
        string mensajeEventos = "";
        if (res.EsquemaVacunacionCompleto())
        {
            _eventPublisher.Publicar(new VacunacionCompletadaEvent(res.Nombre));
            mensajeEventos = $" Esquema completo para '{res.Nombre}'.";
        }

        return $"Vacuna '{vacuna.Nombre}' aplicada a '{nombreRes}'.{mensajeEventos}";
    }

    public List<Vacuna> ListarVacunasDisponibles()
        => _repoVacuna.ObtenerTodas().OrderBy(v => v.Nombre).ToList();

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        return new Dictionary<string, object>
        {
            ["TotalVacunas"] = vacunas.Count,
            ["Bacterianas"] = vacunas.Count(v => v.Categoria == VacunaCategoria.Bacteriana),
            ["Vivas"] = vacunas.Count(v => v.Categoria == VacunaCategoria.Viva)
        };
    }
}
```

#### `ServicioVentas` *(Verde — SRP)*

```csharp
namespace Hacienda.Application.Services;

public class ServicioVentas : IServicioVentas
{
    private readonly IRepositorioVenta _repoVenta;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IVentaFactory _fabricaVenta;
    private readonly IValidarVenta _validador;
    private readonly TimeProvider _reloj;

    public ServicioVentas(
        IRepositorioVenta repoVenta,
        IRepositorioPotrero repoPotrero,
        IVentaFactory fabricaVenta,
        IValidarVenta validador,
        TimeProvider reloj)
    {
        _repoVenta = repoVenta;
        _repoPotrero = repoPotrero;
        _fabricaVenta = fabricaVenta;
        _validador = validador;
        _reloj = reloj;
    }

    public string VenderRes(string potreroId, string nombreRes, decimal monto)
    {
        var potrero = _repoPotrero.ObtenerPorIdentificacion(potreroId)
            ?? throw new InvalidOperationException($"Potrero '{potreroId}' no encontrado");
        var res = potrero.BuscarRes(nombreRes)
            ?? throw new InvalidOperationException($"Res '{nombreRes}' no encontrada");

        // Factory crea la venta (con TimeProvider inyectado para testabilidad)
        Venta venta = _fabricaVenta.Crear(res, potreroId, monto, _reloj);

        // Validar
        var validacion = _validador.Validar(venta);
        if (!validacion.EsValido)
            return string.Join("; ", validacion.Errores);

        // Remover res del potrero
        potrero.RemoverRes(res);

        // Persistir
        var ventas = _repoVenta.ObtenerTodas();
        ventas.Add(venta);
        _repoVenta.GuardarTodas(ventas);
        _repoPotrero.GuardarTodos(_repoPotrero.ObtenerTodos());

        return $"Venta de la res '{nombreRes}' realizada con éxito por {monto:C}.";
    }

    public List<Venta> ListarVentas()
        => _repoVenta.ObtenerTodas().OrderByDescending(v => v.Fecha).ToList();

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var ventas = _repoVenta.ObtenerTodas();
        return new Dictionary<string, object>
        {
            ["TotalVentas"] = ventas.Count,
            ["MontoTotal"] = ventas.Sum(v => v.Monto.Monto),
            ["PromedioVenta"] = ventas.Any() ? ventas.Average(v => v.Monto.Monto) : 0
        };
    }
}
```

#### `ServicioAutenticacion` *(Verde — SRP: solo AuthN)*

```csharp
namespace Hacienda.Application.Services;

public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IRepositorioUsuario _repoUsuario;
    private readonly IHasher _hasher;

    public ServicioAutenticacion(IRepositorioUsuario repoUsuario, IHasher hasher)
    {
        _repoUsuario = repoUsuario;
        _hasher = hasher;
    }

    public ResultadoAutenticacion Autenticar(string username, string password)
    {
        var usuarios = _repoUsuario.ObtenerTodos();
        var usuario = usuarios.FirstOrDefault(u =>
            u.Nombre.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (usuario == null)
            return ResultadoAutenticacion.Fallido($"Usuario '{username}' no encontrado");

        if (!usuario.Credencial.Verificar(password, _hasher))
            return ResultadoAutenticacion.Fallido("Credenciales inválidas");

        return ResultadoAutenticacion.Ok(usuario);
    }
}
```

#### `AutorizadorRbca` — Plugin Registry *(Azul — OCP, Violeta — DIP)*

```csharp
namespace Hacienda.Application.Services;

public class AutorizadorRbca : IAutorizador
{
    private readonly Dictionary<RolUsuario, IPoliticaPermisos> _politicas;

    // DI inyecta todas las implementaciones de IPoliticaPermisos registradas
    public AutorizadorRbca(IEnumerable<IPoliticaPermisos> politicas)
    {
        _politicas = politicas.ToDictionary(p => p.Rol);
    }

    public ResultadoAutorizacion Autorizar(Usuario usuario, string operacion)
    {
        if (usuario == null)
            return ResultadoAutorizacion.Denegado("Usuario no autenticado");

        if (_politicas.TryGetValue(usuario.Rol, out var politica))
            return politica.Evaluar(operacion);

        return ResultadoAutorizacion.Denegado($"Rol no configurado: {usuario.Rol}");
    }
}
```

> **Extensión OCP:** Agregar rol `Supervisor` = crear `PoliticaSupervisor : IPoliticaPermisos` + registrar en DI. **Cero if-else, cero modificación de AutorizadorRbca.**

---

### 4.3 Validadores

```
Hacienda.Application/Validaciones/
├── ValidadorRes.cs
├── ValidadorPotrero.cs
├── ValidadorVacuna.cs
└── ValidadorVenta.cs
```

> **Cambio ISP crítico:** Cada validador implementa **solo su interfaz**. Cero `NotImplementedException`. Cero `throw new NotImplementedException("Use ValidadorX")`.

```csharp
// ValidadorRes.cs — implementa SOLO IValidarRes
namespace Hacienda.Application.Validaciones;

public class ValidadorRes : IValidarRes
{
    public ValidationResult Validar(Res res)
    {
        if (res == null) return ValidationResult.Fallo("Res no puede ser null");

        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(res.Nombre)) errores.Add("Nombre vacío");
        if (res.Peso == 0) errores.Add("Peso debe ser mayor a 0");
        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores.ToArray());
    }
}

// ValidadorPotrero.cs — implementa SOLO IValidarPotrero
public class ValidadorPotrero : IValidarPotrero
{
    public ValidationResult Validar(Potrero potrero)
    {
        if (potrero == null) return ValidationResult.Fallo("Potrero no puede ser null");

        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(potrero.Identificacion.Valor)) errores.Add("Identificación vacía");
        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores.ToArray());
    }
}

// ValidadorVacuna.cs — implementa SOLO IValidarVacuna
public class ValidadorVacuna : IValidarVacuna
{
    public ValidationResult Validar(Vacuna vacuna)
    {
        if (vacuna == null) return ValidationResult.Fallo("Vacuna no puede ser null");

        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(vacuna.Nombre)) errores.Add("Nombre vacío");
        if (string.IsNullOrWhiteSpace(vacuna.Lote)) errores.Add("Lote vacío");
        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores.ToArray());
    }
}

// ValidadorVenta.cs — implementa SOLO IValidarVenta
public class ValidadorVenta : IValidarVenta
{
    public ValidationResult Validar(Venta venta)
    {
        if (venta == null) return ValidationResult.Fallo("Venta no puede ser null");

        var errores = new List<string>();
        if (venta.Res == null) errores.Add("Res de la venta no puede ser null");
        if (venta.Monto.Monto <= 0) errores.Add("Monto debe ser mayor a 0");
        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores.ToArray());
    }
}
```

---

### 4.4 DTOs

```
Hacienda.Application/DTOs/
├── PotreroDto.cs
├── ResDto.cs
├── VacunaDto.cs
└── VentaDto.cs
```

```csharp
namespace Hacienda.Application.DTOs;

public record PotreroDto(string Identificacion, string Tipo, int CantidadReses);
public record ResDto(string Nombre, string Tipo, uint Peso, ushort Edad, string PotreroId);
public record VacunaDto(string Nombre, string Lote, string Categoria, DateTime FechaVencimiento);
public record VentaDto(DateTime Fecha, string NombreRes, string TipoRes, decimal Monto);
```

---

## 5. Capa Infrastructure — Completa

### 5.1 Persistencia (SQLite + Dapper)

```
Hacienda.Infrastructure/Persistence/Sqlite/
├── DatabaseInitializer.cs
├── RepositorioPotreroSqlite.cs
├── RepositorioResSqlite.cs
├── RepositorioVacunaSqlite.cs
├── RepositorioVentaSqlite.cs
├── RepositorioUsuarioSqlite.cs
├── RepositorioChipSqlite.cs
└── RepositorioGeolocalizacionSqlite.cs
```

> **Migración desde CSV:** Los 7 repositorios CSV y los 7 parsers fueron reemplazados por 7 repositorios SQLite con Dapper. SQLite elimina la necesidad de parsers porque el filtrado por tipo se hace con `WHERE tipo = '...'` en SQL. El schema usa TPH (Table Per Hierarchy) para `Res` y `Vacuna` con columnas discriminatorias `tipo TEXT` y `categoria TEXT`. Se agregó `chip_id TEXT` FK en la tabla `reses` para persistir la relación opcional `Res.Chip`. `DatabaseInitializer` crea el schema con `CREATE TABLE IF NOT EXISTS` y migra DBs existentes con `ALTER TABLE`.

```csharp
// RepositorioPotreroSqlite.cs — carga potreros + reses asociadas + chip opcional
namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioPotreroSqlite : IRepositorioPotrero
{
    private readonly string _connectionString;
    private readonly IGuidProvider _guidProvider;

    public RepositorioPotreroSqlite(string connectionString, IGuidProvider guidProvider)
    {
        _connectionString = connectionString;
        _guidProvider = guidProvider;
    }

    public List<Potrero> ObtenerTodos()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var potreros = new List<Potrero>();
        var potreroRows = conn.Query<dynamic>("SELECT * FROM potreros");

        foreach (var row in potreroRows)
        {
            var id = Guid.Parse((string)row.id);
            var identificacion = new Identificacion((string)row.identificacion);
            var tipo = (TipoPotrero)(byte)row.tipo;
            var potrero = new Potrero(id, identificacion, tipo);

            var resRows = conn.Query<dynamic>(
                "SELECT * FROM reses WHERE potrero_id = @PotreroId",
                new { PotreroId = row.id });

            foreach (var resRow in resRows)
            {
                var res = MapearRes(resRow);
                CargarChipSiExiste(res, resRow, conn);
                potrero.AgregarRes(res);
            }

            potreros.Add(potrero);
        }

        return potreros;
    }

    public void GuardarTodos(List<Potrero> potreros)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        conn.Execute("DELETE FROM vacunas_aplicadas", transaction: tx);
        conn.Execute("DELETE FROM reses", transaction: tx);
        conn.Execute("DELETE FROM potreros", transaction: tx);

        foreach (var potrero in potreros)
        {
            conn.Execute(
                "INSERT INTO potreros (id, identificacion, tipo) VALUES (@Id, @Identificacion, @Tipo)",
                new { Id = potrero.Id.ToString(), Identificacion = potrero.Identificacion.Valor, Tipo = (byte)potrero.Tipo },
                transaction: tx);

            foreach (var res in potrero.Reses)
            {
                conn.Execute(
                    "INSERT INTO reses (id, potrero_id, nombre, peso, edad, tipo, chip_id) VALUES (@Id, @PotreroId, @Nombre, @Peso, @Edad, @Tipo, @ChipId)",
                    new { Id = res.Id.ToString(), PotreroId = potrero.Id.ToString(), Nombre = res.Nombre, Peso = (int)res.Peso, Edad = (int)res.Edad, Tipo = res.Tipo.ToString(), ChipId = res.Chip?.Id.ToString() },
                    transaction: tx);
            }
        }

        tx.Commit();
    }

    internal static Res MapearRes(dynamic row)
    {
        var id = Guid.Parse((string)row.id);
        var nombre = (string)row.nombre;
        var peso = (uint)(long)row.peso;
        var edad = (ushort)(long)row.edad;
        var tipo = (string)row.tipo;

        return tipo switch
        {
            "Ternero" => new Ternero(id, nombre, peso, edad),
            "Novillo" => new Novillo(id, nombre, peso, edad),
            "Cebon" => new Cebon(id, nombre, peso, edad),
            _ => throw new InvalidOperationException($"Tipo de res desconocido: {tipo}")
        };
    }

    internal static void CargarChipSiExiste(Res res, dynamic row, SqliteConnection conn)
    {
        var chipIdObj = row.chip_id;
        if (chipIdObj == null) return;

        var chipIdStr = (string)chipIdObj;
        if (string.IsNullOrWhiteSpace(chipIdStr)) return;

        var chipRow = conn.QueryFirstOrDefault<dynamic>(
            "SELECT * FROM chips WHERE id = @Id", new { Id = chipIdStr });
        if (chipRow == null) return;

        var chip = MapearChip(chipRow);
        res.Chip = chip;
    }

    internal static Chip MapearChip(dynamic row)
    {
        var id = Guid.Parse((string)row.id);
        var numeroSerie = new NumeroSerieChip((string)row.numero_serie);
        var fechaInstalacion = DateTime.Parse((string)row.fecha_instalacion);
        var chip = Chip.Crear(id, numeroSerie, fechaInstalacion);
        var estado = (EstadoChip)(byte)row.estado;
        if (estado != EstadoChip.Activo)
            chip.CambiarEstado(estado);
        return chip;
    }
}
```

---

### 5.2 Parseadores (Plugin Registry para deserialización)

> **Nota post-migración:** estos parseadores pertenecen a la era CSV y **ya no existen en el código** — con SQLite + Dapper el filtrado polimórfico se resuelve con `WHERE tipo/categoria = ...` (TPH). Se conserva esta sección como registro de la decisión OCP original (ADR-02, ADR-09).

```
Hacienda.Infrastructure/Parsers/
├── IParseadorRes.cs
├── ParseadorTernero.cs
├── ParseadorNovillo.cs
├── ParseadorCebon.cs
├── IParseadorVacuna.cs
├── ParseadorBacteriana.cs
└── ParseadorViva.cs
```

```csharp
// IParseadorRes.cs — Azul (OCP): un parser por subtipo
namespace Hacienda.Infrastructure.Parsers;

public interface IParseadorRes
{
    string TipoNombre { get; }
    Res Parsear(string[] campos, IGuidProvider guidProvider);
}

// ParseadorTernero.cs
public class ParseadorTernero : IParseadorRes
{
    public string TipoNombre => "Ternero";
    public Res Parsear(string[] campos, IGuidProvider guidProvider)
        => new Ternero(guidProvider.Nuevo(), campos[1], uint.Parse(campos[2]), ushort.Parse(campos[3]));
}

// ParseadorNovillo.cs
public class ParseadorNovillo : IParseadorRes
{
    public string TipoNombre => "Novillo";
    public Res Parsear(string[] campos, IGuidProvider guidProvider)
        => new Novillo(guidProvider.Nuevo(), campos[1], uint.Parse(campos[2]), ushort.Parse(campos[3]));
}

// ParseadorCebon.cs
public class ParseadorCebon : IParseadorRes
{
    public string TipoNombre => "Cebon";
    public Res Parsear(string[] campos, IGuidProvider guidProvider)
        => new Cebon(guidProvider.Nuevo(), campos[1], uint.Parse(campos[2]), ushort.Parse(campos[3]));
}
```

> **Extensión OCP:** Agregar `VacaLechera` = crear `ParseadorVacaLechera : IParseadorRes` + registrar en DI. El `RepositorioResCsv` **no se modifica**.

```csharp
// IParseadorVacuna.cs — Azul (OCP): un parser por subtipo de vacuna
namespace Hacienda.Infrastructure.Parsers;

public interface IParseadorVacuna
{
    string TipoNombre { get; }
    Vacuna Parsear(string[] campos, IGuidProvider guidProvider);
}

// ParseadorBacteriana.cs
public class ParseadorBacteriana : IParseadorVacuna
{
    public string TipoNombre => "Bacteriana";

    public Vacuna Parsear(string[] campos, IGuidProvider guidProvider)
    {
        var nombre = campos[0];
        var lote = campos[1];
        var fechaVenc = DateTime.ParseExact(campos[2].Trim(), "yyyy-MM-dd", null);
        var fechaAplic = DateTime.ParseExact(campos[3].Trim(), "yyyy-MM-dd", null);
        var periodo = uint.Parse(campos[5].Trim());

        return new Bacteriana(guidProvider.Nuevo(), nombre, lote, fechaVenc, fechaAplic, periodo);
    }
}

// ParseadorViva.cs
public class ParseadorViva : IParseadorVacuna
{
    public string TipoNombre => "Viva";

    public Vacuna Parsear(string[] campos, IGuidProvider guidProvider)
    {
        var nombre = campos[0];
        var lote = campos[1];
        var fechaVenc = DateTime.ParseExact(campos[2].Trim(), "yyyy-MM-dd", null);
        var fechaAplic = DateTime.ParseExact(campos[3].Trim(), "yyyy-MM-dd", null);
        var atenuacion = Enum.TryParse<Viva.GradoAtenuacion>(campos[5].Trim(), out var aten)
            ? aten
            : Viva.GradoAtenuacion.Atenuacion10;

        return new Viva(guidProvider.Nuevo(), nombre, lote, fechaVenc, fechaAplic, atenuacion);
    }
}
```

> **Corrección LSP:** El `ParseadorViva` preserva la atenuación persistida (antes se hardcodeaba `Atenuacion10`). Toda `Viva` con `Atenuacion20` o `Atenuacion30` ahora sobrevive al ciclo de persistencia.
>
> **Extensión OCP:** Agregar `Toxoide : Vacuna` = crear `ParseadorToxoide : IParseadorVacuna` + 1 línea en composition root. El `RepositorioVacunaCsv` **no se modifica**.

---

### 5.3 Políticas de Autorización (Plugin Registry)

```
Hacienda.Infrastructure/Policies/
├── PoliticaAdmin.cs
├── PoliticaEmpleado.cs
└── PoliticaVisitante.cs
```

```csharp
// PoliticaAdmin.cs — Admin tiene todos los permisos
namespace Hacienda.Infrastructure.Policies;

public class PoliticaAdmin : IPoliticaPermisos
{
    public RolUsuario Rol => RolUsuario.Admin;
    public ResultadoAutorizacion Evaluar(string operacion)
        => ResultadoAutorizacion.Concedido(operacion);
}

// PoliticaEmpleado.cs — Empleado puede todo excepto eliminar
public class PoliticaEmpleado : IPoliticaPermisos
{
    public RolUsuario Rol => RolUsuario.Empleado;
    public ResultadoAutorizacion Evaluar(string operacion)
        => operacion.Contains("Eliminar")
            ? ResultadoAutorizacion.Denegado("Empleado no puede eliminar")
            : ResultadoAutorizacion.Concedido(operacion);
}

// PoliticaVisitante.cs — Visitante solo consulta
public class PoliticaVisitante : IPoliticaPermisos
{
    public RolUsuario Rol => RolUsuario.Visitante;
    public ResultadoAutorizacion Evaluar(string operacion)
        => (operacion.Contains("Consultar") || operacion.Contains("Listar"))
            ? ResultadoAutorizacion.Concedido(operacion)
            : ResultadoAutorizacion.Denegado("Visitante: solo consulta");
}
```

> **Cero hardcodeo de `if (usuario.Nombre == "admin")`.** Cada rol es una clase que implementa `IPoliticaPermisos`. La autorización se evalúa polimórficamente.

---

### 5.4 Domain Events Publisher

```
Hacienda.Infrastructure/Events/
└── DomainEventPublisherConsola.cs
```

```csharp
namespace Hacienda.Infrastructure.Events;

public class DomainEventPublisherConsola : IDomainEventPublisher
{
    public void Publicar<TEvento>(TEvento evento) where TEvento : IDomainEvent
    {
        // Implementación actual: imprimir a consola
        // Extensible: PublisherEmail, PublisherLog, PublisherWebhook (OCP)
        Console.WriteLine($"[DOMINIO] {evento.GetType().Name}: {evento}");
    }
}
```

---

### 5.5 Cross-cutting Services

```
Hacienda.Infrastructure/CrossCutting/
├── GuidProviderSistema.cs
├── HasherBcrypt.cs
└── DataLoader.cs
```

```csharp
// GuidProviderSistema.cs
namespace Hacienda.Infrastructure.CrossCutting;

public class GuidProviderSistema : IGuidProvider
{
    public Guid Nuevo() => Guid.NewGuid();
}

// HasherBcrypt.cs
namespace Hacienda.Infrastructure.CrossCutting;

public class HasherBcrypt : IHasher
{
    public string Hashear(string passwordPlano)
        => BCrypt.Net.BCrypt.HashPassword(passwordPlano);

    public bool Verificar(string passwordPlano, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(passwordPlano, passwordHash);
}

// DataLoader.cs — Verde (SRP): extrae hidratación del composition root
namespace Hacienda.Infrastructure.CrossCutting;

public class DataLoader : IDataSeeder
{
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IRepositorioRes _repoRes;
    private readonly IRepositorioVacuna _repoVacuna;
    private readonly IRepositorioVenta _repoVenta;

    public DataLoader(
        IRepositorioPotrero repoPotrero,
        IRepositorioRes repoRes,
        IRepositorioVacuna repoVacuna,
        IRepositorioVenta repoVenta)
    {
        _repoPotrero = repoPotrero;
        _repoRes = repoRes;
        _repoVacuna = repoVacuna;
        _repoVenta = repoVenta;
    }

    public async Task CargarDatosAsync()
    {
        // Los repositorios cargan lazy o se hidratan acá
        // Separado del composition root (SRP)
        await Task.CompletedTask;
    }
}
```

---

## 6. Capa Web — Completa

### 6.1 Controllers *(Verde — SRP: delgados, solo delegan)*

```
Hacienda.Web/Controllers/
├── HomeController.cs
├── PotreroController.cs
├── ResController.cs
├── VacunaController.cs
├── VentaController.cs
├── AccountController.cs
└── UsuarioController.cs
```

> **Cambio crítico:** Los controladores **solo dependen de interfaces de aplicación**. Cero `Hacienda` concreto. Cero `PersistenciaService` directo. Cero reglas de negocio en el controlador.

```csharp
// PotreroController.cs — DELGADO
namespace Hacienda.Web.Controllers;

[Authorize]
public class PotreroController : Controller
{
    private readonly IGestorPotreros _gestorPotreros;

    public PotreroController(IGestorPotreros gestorPotreros)
    {
        _gestorPotreros = gestorPotreros;  // 1 sola dependencia (antes eran 3)
    }

    public ActionResult Index()
    {
        var potreros = _gestorPotreros.ListarPotreros();
        ViewBag.Estadisticas = _gestorPotreros.ObtenerEstadisticas();
        return View(potreros);
    }

    [HttpGet]
    public ActionResult Create() => View();

    [HttpPost]
    public ActionResult Create(string identificacion, TipoPotrero tipo)
    {
        try
        {
            string mensaje = _gestorPotreros.CrearPotrero(identificacion, tipo);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewBag.Mensaje = ex.Message;
            ViewBag.TipoMensaje = "danger";
            return View();
        }
    }

    public ActionResult Details(string id)
    {
        var potrero = _gestorPotreros.BuscarPotrero(id);
        if (potrero == null) return RedirectToAction(nameof(Index));
        return View(potrero);
    }
}
```

```csharp
// ResController.cs — DELGADO (antes: 4 dependencias + reglas de negocio)
namespace Hacienda.Web.Controllers;

[Authorize]
public class ResController : Controller
{
    private readonly IGestorReses _gestorReses;
    private readonly IServicioVentas _servicioVentas;
    private readonly IGestorPotreros _gestorPotreros;

    public ResController(
        IGestorReses gestorReses,
        IServicioVentas servicioVentas,
        IGestorPotreros gestorPotreros)
    {
        _gestorReses = gestorReses;
        _servicioVentas = servicioVentas;
        _gestorPotreros = gestorPotreros;
    }

    public ActionResult Index()
    {
        var reses = _gestorReses.ListarReses();
        ViewBag.Estadisticas = _gestorReses.ObtenerEstadisticas();
        return View(reses);
    }

    [HttpPost]
    public ActionResult Create(string potreroId, string nombre, ushort edad, uint peso)
    {
        try
        {
            // TipoRes se deriva automáticamente del TipoPotrero en el servicio
            string mensaje = _gestorReses.AgregarRes(potreroId, nombre, edad, peso);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
        }
        catch (Exception ex)
        {
            ViewBag.Mensaje = ex.Message;
            ViewBag.TipoMensaje = "danger";
        }
        ViewBag.Potreros = _gestorPotreros.ListarPotreros();
        return View();
    }

    [HttpPost]
    public ActionResult Alimentar(string potreroId, string nombreRes, uint cantidadAlimento)
    {
        try
        {
            string mensaje = _gestorReses.AlimentarRes(potreroId, nombreRes, cantidadAlimento);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = ex.Message;
            TempData["TipoMensaje"] = "danger";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public ActionResult Vender(string potreroId, string nombreRes, decimal monto)
    {
        // CERO parsing, CERO validación de overflow, CERO acceso a persistencia
        // Todo está en el servicio de aplicación
        try
        {
            string mensaje = _servicioVentas.VenderRes(potreroId, nombreRes, monto);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = ex.Message;
            TempData["TipoMensaje"] = "danger";
        }
        return RedirectToAction(nameof(Index));
    }
}
```

---

## 7. Herencias Justificadas con Verificación LSP

### 7.1 Jerarquía `Res → Ternero / Novillo / Cebon` — SE CONSERVA con corrección

#### ¿Por qué herencia y no composición?

- `Ternero`, `Novillo` y `Cebon` **SON** tipos de `Res` (IS-A verdadera, no taxonómicamente discutible).
- Comparten estructura completa: `Id`, `Nombre`, `Peso`, `Edad`, `VacunasAplicadas`.
- Comparten comportamiento base: `Peso` set, `Edad` set (assignment puro), `EsquemaVacunacionCompleto()`.
- La diferencia está en **valores de configuración** (límites de vacunas, pesos, rango de edad), que se expresan naturalmente como overrides de abstracts.
- Composición requeriría un `TipoResStrategy` inyectado que duplicaría exactamente lo que la herencia resuelve sin indirección.

#### Verificación LSP explícita

| Contrato | `Res` (base) | `Ternero` | `Novillo` | `Cebon` | ¿Sustituible? |
|----------|-------------|-----------|-----------|---------|---------------|
| `Edad` set | Acepta cualquier `ushort` (assignment puro) | Hereda sin override | Hereda sin override | Hereda sin override | ✅ |
| `Peso` set | Acepta cualquier `uint` | Hereda | Hereda | Hereda | ✅ |
| `Tipo` | Abstract | `Ternero` | `Novillo` | `Cebon` | ✅ |
| `MaxVacunasBacterianas` | Abstract | `3` | `2` | `1` | ✅ |
| `MaxVacunasVivas` | Abstract | `1` | `2` | `4` | ✅ |
| `PesoMinimo` | Abstract | `150` | `400` | `290` | ✅ |
| `PesoRecomendadoVenta` | Abstract | `250` | `550` | `420` | ✅ |
| `EsEdadValida(ushort)` | Abstract | `edad <= 12` | `edad > 48` | `edad > 12 && edad <= 48` | ✅ |
| `EsquemaVacunacionCompleto()` | Virtual (usa abstracts) | Funciona | Funciona | Funciona | ✅ |
| `Serializar()` | Abstract | Implementa | Implementa | Implementa | ✅ |
| **Excepciones en setters** | **Ninguna** | **Ninguna** | **Ninguna** | **Ninguna** | ✅ |

**Conclusión:** `foreach (Res r in reses) { r.Edad = 99; r.Peso += 10; }` **compila y ejecuta sin excepciones** para cualquier subtipo. LSP se cumple.

---

### 7.2 Jerarquía `Vacuna → Bacteriana / Viva` — SE CONSERVA

#### ¿Por qué herencia y no composición?

- `Bacteriana` y `Viva` **SON** tipos de `Vacuna` (IS-A verdadera).
- Comparten estructura: `Id`, `Nombre`, `Lote`, `FechaVencimiento`, `FechaAplicacion`.
- Difieren en atributos específicos (`PeriodoAplicacion` vs `Atenuacion`) y serialización.
- La diferencia de atributos justifica herencia: cada subtipo añade su atributo propio.

#### Verificación LSP explícita

| Contrato | `Vacuna` (base) | `Bacteriana` | `Viva` | ¿Sustituible? |
|----------|-----------------|--------------|--------|---------------|
| `Categoria` | Abstract | `Bacteriana` | `Viva` | ✅ |
| `Serializar()` | Abstract | Implementa | Implementa | ✅ |
| `CalcularEstado()` | Virtual | Hereda | Hereda | ✅ |
| Excepciones en constructor | Ninguna | Valida período (2-4) | Ninguna | ✅ |

> **Nota sobre `Bacteriana`:** El constructor valida `PeriodoAplicacion ∈ [2,4]`. Esto **no viola LSP** porque la validación es de un atributo que solo `Bacteriana` tiene (no fortalece la precondición de la base, que no conoce `PeriodoAplicacion`).

---

### 7.3 Jerarquía `Validacion → ValidadorX` — SE ELIMINA

#### ¿Por qué se elimina la herencia?

- Los validadores **NO** comparten comportamiento común (cada uno valida una entidad distinta).
- La clase base `Validacion` solo servía para forzar 4 métodos, 3 de los cuales lanzaban `NotImplementedException` (12 stubs).
- **No hay IS-A relationship:** `ValidadorRes` NO ES un `ValidadorPotrero`. La herencia era una excusa para compartir una interfaz gorda, no una relación legítima.

#### Reemplazo

Cada validador implementa **solo su interfaz segregada** (`IValidarRes`, `IValidarPotrero`, etc.). Cero `NotImplementedException`. Cero acoplamiento entre validadores.

---

### 7.4 Otras relaciones — NO hay herencias nuevas

| Entidad | ¿Hereda? | Justificación |
|---------|----------|---------------|
| `Potrero` | No | No hay IS-A con ninguna otra entidad. Es un agregado raíz. |
| `Venta` | No | Es un evento de dominio (snapshot inmutable). No hay IS-A. |
| `Usuario` | No | Es una entidad de identidad. No hay IS-A. |
| `Credencial` | No | Es un Value Object (record). No hay IS-A. |
| `Dinero` | No | Es un Value Object (record). No hay IS-A. |
| `Identificacion` | No | Es un Value Object (record). No hay IS-A. |

**Total de jerarquías de herencia en TO-BE: 2** (`Res` y `Vacuna`), ambas conservadas del original con LSP corregido.

---

## 8. Inversiones de Dependencia (DIP)

### 8.1 Tabla Completa de Inversiones

| # | Módulo alto nivel | Dependía de (bajo nivel) | Abstracción que los desacopla | Dónde se cablea |
|---|-------------------|--------------------------|-------------------------------|-----------------|
| D-1 | `GestorPotreros` (Application) | `PersistenciaService` (Infra) + `new Potrero()` | `IRepositorioPotrero` + `IPotreroFactory` | `Program.cs` → `RepositorioPotreroSqlite` + `FabricaPotrero` |
| D-2 | `GestorReses` (Application) | `Hacienda.crear_res` (Domain God Class) | `IResFactory` + `IRepositorioPotrero` | `Program.cs` → `FabricaRes` + `RepositorioPotreroSqlite` |
| D-3 | `ServicioVacunacion` (Application) | `Hacienda.crear_vacuna` + `new Bacteriana/Viva` | `IVacunaFactory` + `IRepositorioVacuna` | `Program.cs` → `FabricaVacuna` + `RepositorioVacunaSqlite` |
| D-4 | `ServicioVentas` (Application) | `Hacienda.vender_res` + `DateTime.Now` | `IVentaFactory` + `IRepositorioVenta` + `TimeProvider` | `Program.cs` → `FabricaVenta` + `RepositorioVentaSqlite` + `TimeProvider.System` |
| D-5 | `GestorReses` (Application) | `new PublisherPesoMin()` + 3 más (campos) | `IDomainEventPublisher` | `Program.cs` → `DomainEventPublisherConsola` |
| D-6 | `ServicioAutenticacion` (Application) | `Autenticacion.cs` + `List<Usuario>` | `IRepositorioUsuario` + `IHasher` | `Program.cs` → `RepositorioUsuarioSqlite` + `HasherBcrypt` |
| D-7 | `AutorizadorRbca` (Application) | `if (usuario.Nombre == "admin")` | `IPoliticaPermisos[]` (plugin registry) | `Program.cs` → `PoliticaAdmin`, `PoliticaEmpleado`, `PoliticaVisitante` |
| D-8 | Todos los servicios | `DateTime.Now` directo | `TimeProvider` (.NET 8) | `Program.cs` → `TimeProvider.System` |
| D-9 | `FabricaRes` (Domain) | `new Guid()` directo | `IGuidProvider` | `Program.cs` → `GuidProviderSistema` |
| D-10 | `PersistenciaService` (AS-IS) | `HttpContext.Items` | `ValidationResult` (return value) | Se elimina el acoplamiento web |
| D-11 | `PersistenciaService` (AS-IS) | `Castle.DynamicProxy` | DI nativa + llamadas explícitas | Se elimina Castle.DynamicProxy |
| D-12 | `PotreroController` (Web) | `Hacienda` + `PersistenciaService` concretos | `IGestorPotreros` (1 sola interfaz) | `Program.cs` → `GestorPotreros` |
| D-13 | `ResController` (Web) | `Hacienda` + `PersistenciaService` + reglas de negocio | `IGestorReses` + `IServicioVentas` | `Program.cs` → `GestorReses` + `ServicioVentas` |
| D-14 | `RepositorioResSqlite` (Infra, era CSV) | `switch (tipoStr) { ... }` | `IParseadorRes[]` (plugin registry) | **Retirado en la migración a SQLite** — el filtrado por tipo hoy es `WHERE tipo = ...` en SQL (ADR-09) |

### 8.2 Reglas de Cableo (Composition Root)

1. **Domain:** No se registra nada (no tiene dependencias).
2. **Application Services:** `Scoped` (viven dentro de un request HTTP).
3. **Repositories:** `Scoped` (acceden a datos por request).
4. **Factories:** `Transient` (no tienen estado, crean y se van).
5. **Validators:** `Transient` (no tienen estado).
6. **Domain Events:** `Scoped` (viven dentro del request, evita captive dependency).
7. **Cross-cutting** (`TimeProvider`, `IGuidProvider`, `IHasher`): `Singleton` (thread-safe, sin estado mutable).
8. **Policies / Parsers:** `Transient` (plugin registry, no tienen estado).

---

## 9. Composition Root Final

```csharp
// Hacienda.Web/Program.cs
using Hacienda.Application.Interfaces;
using Hacienda.Application.Services;
using Hacienda.Application.Validaciones;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Hacienda.Infrastructure.CrossCutting;
using Hacienda.Infrastructure.Events;
using Hacienda.Infrastructure.Persistence;
using Hacienda.Infrastructure.Parsers;
using Hacienda.Infrastructure.Policies;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════
// DOMAIN — Factories (Transient: crean y se van)
// ═══════════════════════════════════════════════════
builder.Services.AddTransient<IResFactory, FabricaRes>();
builder.Services.AddTransient<IVacunaFactory, FabricaVacuna>();
builder.Services.AddTransient<IVentaFactory, FabricaVenta>();
builder.Services.AddTransient<IPotreroFactory, FabricaPotrero>();

// ═══════════════════════════════════════════════════
// APPLICATION — Services (Scoped: por request)
// ═══════════════════════════════════════════════════
builder.Services.AddScoped<IGestorPotreros, GestorPotreros>();
builder.Services.AddScoped<IGestorReses, GestorReses>();
builder.Services.AddScoped<IServicioVacunacion, ServicioVacunacion>();
builder.Services.AddScoped<IServicioVentas, ServicioVentas>();
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
builder.Services.AddScoped<IAutorizador, AutorizadorRbca>();
builder.Services.AddScoped<IServicioChip, ServicioChip>();                          // SC-2
builder.Services.AddScoped<IServicioGeolocalizacion, ServicioGeolocalizacion>();    // SC-2

// ═══════════════════════════════════════════════════
// APPLICATION — Validators (Transient: sin estado)
// ═══════════════════════════════════════════════════
builder.Services.AddTransient<IValidarRes, ValidadorRes>();
builder.Services.AddTransient<IValidarPotrero, ValidadorPotrero>();
builder.Services.AddTransient<IValidarVacuna, ValidadorVacuna>();
builder.Services.AddTransient<IValidarVenta, ValidadorVenta>();

// ═══════════════════════════════════════════════════
// APPLICATION — Data Seeder (Scoped)
// ═══════════════════════════════════════════════════
builder.Services.AddScoped<IDataSeeder>(sp =>
    new DataLoader(
        sp.GetRequiredService<IRepositorioUsuario>(),
        sp.GetRequiredService<IRepositorioPotrero>(),
        sp.GetRequiredService<IGuidProvider>(),
        sp.GetRequiredService<IHasher>(),
        connectionString));

// ═══════════════════════════════════════════════════
// DOMAIN EVENTS (Scoped: evita captive dependency)
// ═══════════════════════════════════════════════════
builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisherConsola>();

// ═══════════════════════════════════════════════════
// INFRASTRUCTURE — Repositories SQLite + Dapper (Scoped: por request)
// ═══════════════════════════════════════════════════
var directorioDatos = Path.Combine(builder.Environment.ContentRootPath, "Datos");
Directory.CreateDirectory(directorioDatos);
var connectionString = $"Data Source={Path.Combine(directorioDatos, "hacienda.db")}";

DatabaseInitializer.Initialize(connectionString);

builder.Services.AddScoped<IRepositorioPotrero>(sp =>
    new RepositorioPotreroSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioRes>(sp =>
    new RepositorioResSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioVacuna>(sp =>
    new RepositorioVacunaSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioVenta>(sp =>
    new RepositorioVentaSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioUsuario>(sp =>
    new RepositorioUsuarioSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioChip>(sp =>
    new RepositorioChipSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioGeolocalizacion>(sp =>
    new RepositorioGeolocalizacionSqlite(connectionString));

// NOTA (migración SQLite): los parseadores CSV (IParseadorRes / IParseadorVacuna)
// fueron retirados. El filtrado polimórfico ahora es `WHERE tipo = ...` /
// `WHERE categoria = ...` en SQL (TPH). Ver ADR-09.

// ═══════════════════════════════════════════════════
// INFRASTRUCTURE — Authorization Policies (Transient: plugin registry)
// ═══════════════════════════════════════════════════
builder.Services.AddTransient<IPoliticaPermisos, PoliticaAdmin>();
builder.Services.AddTransient<IPoliticaPermisos, PoliticaEmpleado>();
builder.Services.AddTransient<IPoliticaPermisos, PoliticaVisitante>();

// ═══════════════════════════════════════════════════
// CROSS-CUTTING (Singleton: thread-safe)
// ═══════════════════════════════════════════════════
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IGuidProvider, GuidProviderSistema>();
builder.Services.AddSingleton<IHasher, HasherBcrypt>();

// ═══════════════════════════════════════════════════
// MVC + Auth
// ═══════════════════════════════════════════════════
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "HaciendaSoft.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });
builder.Services.AddHttpContextAccessor();

// ═══════════════════════════════════════════════════
// BUILD
// ═══════════════════════════════════════════════════
var app = builder.Build();

// Cargar datos iniciales (separado del composition root — SRP)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.CargarDatosAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
```

---

## 10. Resumen de Relaciones

### 10.1 Herencia (Generalization)

| Clase base | Subtipos | Tipo de relación |
|------------|----------|------------------|
| `Res` (abstract) | `Ternero`, `Novillo`, `Cebon` | Generalization (extends) |
| `Vacuna` (abstract) | `Bacteriana`, `Viva` | Generalization (extends) |

### 10.2 Composición / Agregación

| Contenedor | Contenido | Multiplicidad | Tipo |
|------------|-----------|---------------|------|
| `Potrero` | `Res` | 1..* | Agregación (la res puede existir sin el potrero conceptualmente) |
| `Res` | `Vacuna` (aplicadas) | 0..* | Agregación |
| `Venta` | `Res` (snapshot) | 1 | Composición (la venta es dueña del snapshot) |

### 10.3 Dependencia (via interfaz)

| Cliente (alto nivel) | Depende de (abstracción) | Implementación real |
|-----------------------|--------------------------|---------------------|
| `GestorPotreros` | `IRepositorioPotrero`, `IValidarPotrero`, `IDomainEventPublisher`, `IGuidProvider` | `RepositorioPotreroSqlite`, `ValidadorPotrero`, `DomainEventPublisherConsola`, `GuidProviderSistema` |
| `GestorReses` | `IRepositorioPotrero`, `IResFactory`, `IValidarRes`, `IDomainEventPublisher`, `TimeProvider` | `RepositorioPotreroSqlite`, `FabricaRes`, `ValidadorRes`, `DomainEventPublisherConsola`, `TimeProvider.System` |
| `ServicioVacunacion` | `IRepositorioVacuna`, `IRepositorioPotrero`, `IVacunaFactory`, `IDomainEventPublisher` | `RepositorioVacunaSqlite`, `RepositorioPotreroSqlite`, `FabricaVacuna`, `DomainEventPublisherConsola` |
| `ServicioVentas` | `IRepositorioVenta`, `IRepositorioPotrero`, `IVentaFactory`, `IValidarVenta`, `TimeProvider` | `RepositorioVentaSqlite`, `RepositorioPotreroSqlite`, `FabricaVenta`, `ValidadorVenta`, `TimeProvider.System` |
| `ServicioAutenticacion` | `IRepositorioUsuario`, `IHasher` | `RepositorioUsuarioSqlite`, `HasherBcrypt` |
| `AutorizadorRbca` | `IEnumerable<IPoliticaPermisos>` | `PoliticaAdmin`, `PoliticaEmpleado`, `PoliticaVisitante` |
| `RepositorioResSqlite` | `IEnumerable<IParseadorRes>` | `ParseadorTernero`, `ParseadorNovillo`, `ParseadorCebon` |

### 10.4 Implementación de Interfaz (Realization)

| Interfaz | Implementaciones |
|----------|------------------|
| `IGestorPotreros` | `GestorPotreros` |
| `IGestorReses` | `GestorReses` |
| `IServicioVacunacion` | `ServicioVacunacion` |
| `IServicioVentas` | `ServicioVentas` |
| `IServicioAutenticacion` | `ServicioAutenticacion` |
| `IAutorizador` | `AutorizadorRbca` |
| `IPoliticaPermisos` | `PoliticaAdmin`, `PoliticaEmpleado`, `PoliticaVisitante` |
| `IValidarRes` | `ValidadorRes` |
| `IValidarPotrero` | `ValidadorPotrero` |
| `IValidarVacuna` | `ValidadorVacuna` |
| `IValidarVenta` | `ValidadorVenta` |
| `IRepositorioPotrero` | `RepositorioPotreroSqlite` |
| `IRepositorioRes` | `RepositorioResSqlite` |
| `IRepositorioVacuna` | `RepositorioVacunaSqlite` |
| `IRepositorioVenta` | `RepositorioVentaSqlite` |
| `IRepositorioUsuario` | `RepositorioUsuarioSqlite` |
| `IResFactory` | `FabricaRes` |
| `IPotreroFactory` | `FabricaPotrero` |
| `IVacunaFactory` | `FabricaVacuna` |
| `IVentaFactory` | `FabricaVenta` |
| `IDomainEventPublisher` | `DomainEventPublisherConsola` |
| `IParseadorRes` | `ParseadorTernero`, `ParseadorNovillo`, `ParseadorCebon` |
| `IGuidProvider` | `GuidProviderSistema` |
| `IHasher` | `HasherBcrypt` |
| `IDataSeeder` | `DataLoader` |

---

## 11. Cumplimiento SOLID por Principio

### 11.1 SRP — Single Responsibility

| Clase TO-BE | Razón de cambio (única) | Antes (AS-IS) |
|-------------|------------------------|---------------|
| `GestorPotreros` | Gestión de potreros | `Hacienda` (6 razones) |
| `GestorReses` | Operaciones de ganado | `Hacienda` |
| `ServicioVacunacion` | Inventario + aplicación de vacunas | `Hacienda` |
| `ServicioVentas` | Venta de reses | `Hacienda` |
| `ServicioAutenticacion` | Verificar identidad (AuthN) | `Autenticacion` (5 razones) |
| `AutorizadorRbca` | Verificar permisos (AuthZ) | `Autenticacion` |
| `RepositorioPotreroSqlite` | Leer/escribir potreros | `PersistenciaService` (7 razones) |
| `RepositorioResSqlite` | Leer/escribir reses | `PersistenciaService` |
| `ValidadorRes` | Validar entidad Res | `Validacion` + 3 NotImplementedException |
| `Potrero` | Mantener invariante de capacidad | `Potrero` (5 responsabilidades) |

### 11.2 OCP — Open/Closed

| Punto de extensión | Cómo se extiende (sin modificar) | Archivos a crear |
|---------------------|----------------------------------|------------------|
| Nuevo subtipo de `Res` | `class NuevoTipo : Res` + entrada en `FabricaRes` + `IParseadorRes` | 2 archivos nuevos, 0 modificados |
| Nuevo tipo de `Vacuna` | `class NuevaVacuna : Vacuna` + método en `IVacunaFactory` | 1 archivo nuevo |
| Nuevo canal de notificación | `class PublisherEmail : IDomainEventPublisher` | 1 archivo nuevo |
| Nuevo medio de persistencia | `class RepositorioPotreroEfCore : IRepositorioPotrero` | 1 archivo nuevo + cableo DI |
| Nuevo rol de usuario | `class PoliticaSupervisor : IPoliticaPermisos` | 1 archivo nuevo + cableo DI |

### 11.3 LSP — Liskov Substitution

| Jerarquía | Estado LSP | Verificación |
|-----------|-----------|--------------|
| `Res → Ternero/Novillo/Cebon` | ✅ Cumple | Setters puros, sin excepciones. `EsEdadValida()` es método, no restricción del setter |
| `Vacuna → Bacteriana/Viva` | ✅ Cumple | Sin overrides de comportamiento base. Diferencia en atributos propios |
| Validadores (eliminada) | ✅ N/A | Se eliminó la jerarquía, reemplazada por interfaces segregadas |

### 11.4 ISP — Interface Segregation

| Antes (AS-IS) | Después (TO-BE) | Beneficio |
|----------------|-----------------|-----------|
| `IValidarInformacion` (4 métodos) | `IValidarRes` + `IValidarPotrero` + `IValidarVacuna` + `IValidarVenta` (1 método c/u) | 0 NotImplementedException |
| `Hacienda` implementa 3 interfaces | Cada servicio implementa 1 interfaz | Sin métodos que no se usan |
| `PersistenciaService` (12 métodos, sin interfaz) | 5 `IRepositorioX` (3-4 métodos c/u) | Cada gestor depende solo de su repositorio |
| `ICreacionVacuna` (4 sobrecargas) | `IVacunaFactory` (2 métodos) | Sin parameter sniffing |

### 11.5 DIP — Dependency Inversion

| Métrica | AS-IS | TO-BE |
|---------|-------|-------|
| Clases concretas cableadas en DI | 7 (todas) | 0 (todas via interfaz) |
| `new` de dependencias inline | 20+ | 0 (todas via factory/constructor injection) |
| `HttpContext.Items` como canal de datos | 12 sitios | 0 (reemplazado por return values) |
| Castle.DynamicProxy | Requerido | Eliminado |
| `DateTime.Now` directo | 8 sitios | 0 (TimeProvider inyectado) |
| Hardcodeo de roles | 3 `if` encadenados | 0 (plugin registry) |

---

## 12. Registros de Decisión Arquitectónica (ADR)

Cada ADR documenta una decisión estructural con: **contexto y evidencia** (hallazgos `H-xx` del inventario, ver `01-diagnostico/Inventario_Hallazgos.md`), **al menos dos alternativas evaluadas** (una decisión sin alternativa descartada no es una decisión), **la decisión tomada**, **el costo o consecuencia negativa aceptada** y **los principios involucrados**. Trazabilidad: los colores del diagrama (§2) y el código de `SolucionSOLID/` reflejan exactamente estas decisiones.

### ADR-01: Descomposición de la God Class `Hacienda` en servicios por actor

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | `Hacienda.cs` (559 líneas) acumula 6 responsabilidades: potreros, ganado, inventario de vacunas, vacunación, orquestación de eventos y reglas de negocio (H-01, H-02, H-03, H-04, H-09, H-10). Cualquier cambio en ventas, vacunación o eventos obliga a recompilar y re-probar el núcleo completo. |
| **Alternativa 1 (descartada)** | Mantener `Hacienda` como facade que delega a servicios internos. Descartada: la facade sigue siendo un punto de cambio múltiple y no elimina los `new` de dependencias concretas en su interior. |
| **Alternativa 2 (elegida)** | Partir `Hacienda` en servicios cohesivos por actor/operación: `GestorPotreros`, `GestorReses`, `ServicioVacunacion`, `ServicioVentas` (más `ServicioAutenticacion`/`AutorizadorRbca` por ADR-05). |
| **Decisión** | La God Class desaparece. Cada servicio de aplicación implementa una única interfaz y recibe sus colaboradores por constructor. |
| **Costo aceptado** | Se pierde la "ventanilla única": un controlador puede depender de 2-3 interfaces (ej. `ResController`). Se acepta porque esas dependencias son abstracciones, no concretos. |
| **Principios** | **SRP** (primario), OCP (cada servicio evoluciona de forma independiente), DIP (servicios detrás de interfaces). |

### ADR-02: Polimorfismo en `Res`/`Vacuna` para eliminar el type-checking

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | 4 sitios despachan con `if (res is Ternero)… else if…`; si ninguno coincide, las variables quedan en 0 y las reglas fallan en silencio (H-04). Mismo patrón con vacunas (H-21, H-24) y un default `_ => new Ternero(...)` que deserializaba tipos desconocidos como Ternero (H-23). |
| **Alternativa 1 (descartada)** | Mantener los `is`-checks agregando un `else` con default seguro. Descartada: sigue siendo shotgun surgery (cada subtipo nuevo toca 4+ archivos) y el default enmascara errores — ya produjo corrupción silenciosa de datos. |
| **Alternativa 2 (elegida)** | Mover los datos específicos del subtipo a miembros abstractos polimórficos en la base: `Res.Tipo`, `MaxVacunasBacterianas`, `MaxVacunasVivas`, `PesoMinimo`, `PesoRecomendadoVenta`, `EsEdadValida()`; `Vacuna.Categoria`, `Serializar()`, `DetalleVisual()`. |
| **Decisión** | Ningún consumer hace type-checking: invoca el miembro polimórfico. Extender = crear la subclase + 1 entrada en `FabricaRes`. |
| **Costo aceptado** | Todo subtipo nuevo está obligado a implementar los abstracts (es el punto: es imposible olvidar los límites y caer en el default 0). |
| **Principios** | **OCP** (primario), LSP (el comportamiento vive en el subtipo, no en el consumer). |

### ADR-03: Corrección LSP en `Res` — setter puro + validación en el boundary

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | `Res.Edad` acepta cualquier `ushort`, pero `Ternero`, `Cebon` y `Novillo` sobrescriben el setter para lanzar excepción fuera de su rango (H-07, H-08). Fortalecen la precondición de la base: `foreach (var r in reses) r.Edad += 1;` compila contra `Res` pero explota en runtime. |
| **Alternativa 1 (descartada)** | Conservar los setters lanzadores y documentar la restricción. Descartada: cualquier operación polimórfica (envejecimiento batch, importación) puede crashear; documentar no cambia el contrato. |
| **Alternativa 2 (elegida)** | El setter de `Edad` queda como assignment puro (idéntico contrato en base y subtipos). La validación de rango se mueve al boundary: `FabricaRes.Crear()` y `Potrero.AgregarRes()` invocan `EsEdadValida(edad)`. |
| **Decisión** | LSP por construcción: ningún subtipo lanza donde la base no lanza. La regla de rango existe una sola vez, en el punto de entrada. |
| **Costo aceptado** | Asignar `Edad` ya no valida automáticamente; la validación es explícita en los puntos de entrada (más visible y testeable, pero hay que recordar invocarla). |
| **Principios** | **LSP** (primario), SRP (la entidad no es dueña de la regla de rango: la fábrica sí). |

### ADR-04: Segregación de `IValidarInformacion` en cuatro interfaces de un método

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | `IValidarInformacion` declara 4 métodos (uno por entidad). Cada validador implementa 1 y lanza `NotImplementedException` en los otros 3: 12 stubs explosivos (H-05), forzados por la clase base `Validacion` (H-06). El interceptor atrapaba esas excepciones como flujo normal. |
| **Alternativa 1 (descartada)** | Mantener la interfaz gorda pero retornar `true` en vez de lanzar. Descartada: un validador mal cableado dejaría pasar datos inválidos silenciosamente. |
| **Alternativa 2 (elegida)** | Cuatro interfaces de un método (`IValidarRes`, `IValidarPotrero`, `IValidarVacuna`, `IValidarVenta`); cada validador implementa solo la suya. Se elimina la jerarquía `Validacion`. |
| **Decisión** | Cero `NotImplementedException`. Cada servicio depende únicamente del validador de su agregado. |
| **Costo aceptado** | El composition root registra 4 validadores en vez de 1 (lo absorbe la DI; el costo es una línea por validador). |
| **Principios** | **ISP** (primario), LSP (ya no hay implementaciones que mienten), DIP. |

### ADR-05: Separación AuthN/AuthZ + políticas de permisos por plugin registry

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | `Autenticacion.cs` (149 líneas) mezcla 5 responsabilidades: lista de usuarios, seed, creación, verificación de credenciales (AuthN) y permisos por rol (AuthZ) con `if (usuario.Nombre == "admin")` (H-19). Un rol nuevo o un cambio de política exige editar esa clase y redeployar. |
| **Alternativa 1 (descartada)** | Mantener una sola clase de seguridad con un `switch` de roles. Descartada: viola OCP — cada rol nuevo modifica código de seguridad ya probado. |
| **Alternativa 2 (elegida)** | `ServicioAutenticacion` (solo AuthN, con `IHasher` inyectado) + `AutorizadorRbca` (solo AuthZ) que resuelve por `Dictionary<RolUsuario, IPoliticaPermisos>` construido desde el `IEnumerable<IPoliticaPermisos>` inyectado. |
| **Decisión** | Agregar un rol = crear `PoliticaX : IPoliticaPermisos` + 1 línea de registro en `Program.cs`. Cero modificación del autorizador. |
| **Costo aceptado** | Las políticas deciden por convención sobre el nombre de la operación (`Contains("Eliminar")`); es simple pero sensible si los nombres de operación cambian — deuda técnica consciente. |
| **Principios** | SRP, **OCP** (plugin registry), ISP, DIP. |

### ADR-06: Inversión de dependencias total con composition root único

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | `Program.cs` no registraba ni una sola interfaz (H-16). Controladores inyectaban `Hacienda` y `PersistenciaService` concretos (H-17). `PersistenciaService` no tenía interfaz (H-12). Publishers y entidades se instanciaban inline (H-02, H-03, H-20). |
| **Alternativa 1 (descartada)** | Interfaces solo para persistencia. Descartada: deja el núcleo (eventos, factories, auth, reloj) acoplado e intestable. |
| **Alternativa 2 (elegida)** | DIP completo: ~30 abstracciones; todas las dependencias se inyectan por constructor; `Program.cs` es el único punto que conoce implementaciones (§8, §9). |
| **Decisión** | Domain no depende de nada; Application solo de Domain; Infrastructure implementa las interfaces; Web cablea. |
| **Costo aceptado** | El composition root crece (~40 registros) y toda dependencia nueva exige registro explícito. |
| **Principios** | **DIP** (primario), OCP (nueva implementación = nuevo registro, sin tocar consumers). |

### ADR-07: Eliminación de Castle.DynamicProxy, `HttpContext.Items` y parseo de ✓/✗

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | La validación corría por interceptores Castle y devolvía resultados en `HttpContext.Items` (diccionario mutable compartido) (H-11, H-13). La autorización decidía parseando `ex.Message.Contains("✓")`: cualquier excepción con "✓" autorizaba — agujero de seguridad (H-11). |
| **Alternativa 1 (descartada)** | Mantener Castle reemplazando `HttpContext.Items` por un tipo fuerte. Descartada: sigue acoplando el dominio a HTTP y arrastra una dependencia pesada e innecesaria en .NET 8. |
| **Alternativa 2 (elegida)** | Validación explícita en los servicios de aplicación antes de persistir; resultados como valores de retorno: `ValidationResult`, `ResultadoAutenticacion`, `ResultadoAutorizacion`. |
| **Decisión** | Sin AOP, sin HTTP en la lógica, sin parseo de mensajes. |
| **Costo aceptado** | Más verboso: cada caso de uso llama al validador explícitamente (visibilidad a cambio de "magía"). |
| **Principios** | **DIP** (primario), SRP, Seguridad. |

### ADR-08: Inversión del reloj del sistema con `TimeProvider`

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | `DateTime.Now` directo en reglas de negocio: `PublisherVacunaVencida` (H-27), la venta en `Hacienda.cs:156`, estadísticas de servicios. Tests no deterministas y reglas que varían con la hora de ejecución. |
| **Alternativa 1 (descartada)** | Mantener `DateTime.Now` y "mockearlo" en tests. Descartada: un llamado estático no se puede sustituir sin wrapper. |
| **Alternativa 2 (elegida)** | `TimeProvider` (API nativa de .NET 8) inyectado por constructor; `TimeProvider.System` como singleton; los eventos de dominio reciben `OcurridoEn` como parámetro. |
| **Decisión** | Ninguna clase de Domain/Application llama a `DateTime.Now` directamente. |
| **Costo aceptado** | Un parámetro adicional en servicios y factories que usan reloj. |
| **Principios** | **DIP** (primario), Testabilidad. |

### ADR-09: Migración de persistencia de CSV a SQLite + Dapper

| Campo | Contenido |
|-------|-----------|
| **Contexto y evidencia** | `PersistenciaService` (643 líneas) mezclaba validación, serialización pipe-delimited, I/O y lectura de `HttpContext.Items` (H-10); los switches de deserialización tenían el default corruptor `_ => new Ternero(...)` (H-23). Además SC-2 exigía persistir la relación `Res ↔ Chip`, incómoda en archivos planos. |
| **Alternativa 1 (descartada)** | Mantener CSV con plugin-parsers. Descartada: parsing manual frágil, sin integridad referencial ni consultas; la relación Res↔Chip quedaba artificial. |
| **Alternativa 2 (descartada)** | EF Core con TPH automático. Descartada: sobredimensionado para el volumen del dominio (7 tablas); arrastra migraciones y magia de ORM para consultas triviales. |
| **Alternativa 3 (elegida)** | SQLite + Dapper: TPH manual con discriminadores `tipo`/`categoria`, FK `chip_id`, y 7 `RepositorioXSqlite` detrás de las mismas interfaces de Domain. |
| **Decisión** | Solo cambió el adapter: Domain y Application no se modificaron — prueba empírica de que DIP quedó bien aplicado. |
| **Costo aceptado** | `GuardarTodos` hace DELETE + INSERT dentro de transacción (simple; suficiente para el volumen de una hacienda) y el SQL se escribe a mano. |
| **Principios** | **DIP** (primario), OCP (el medio de persistencia es un punto de extensión). |

> **Remediaciones post-implementación:** las decisiones correctivas detectadas al validar el código contra el diseño (plugin-parser de vacunas, `Vacuna.DetalleVisual`, timestamps en eventos, simplificación de `DataLoader`, eliminación de `IRepositorioChip.ObtenerPorResId`, cache de instancia en repositorios) están registradas como ADR en `Diseno_TOBE.md` §13–14.

---

## 13. Estructura de Carpetas Completa

```
SolucionTrabajo/
├── 00-lectura-en-frio/                    [Fase 0 — entregado]
├── 01-diagnostico/                        [Fase 1 — completado]
│   ├── AnalisisSOLID/
│   ├── Inventario_Hallazgos.md
│   ├── Mapa_Dependencias.md
│   ├── Puntos_Dolor.md
│   ├── REFERENCIA_Hallazgos_Consolidados.md
│   ├── UML_Bib_Hacienda.dia
│   └── UML_Bib_Hacienda.png
├── 02-Fase2/                              [Fase 2 — completado]
│   └── Analisis_Impacto_SC.md
├── 03-diseno/                             [Fase 3]
│   ├── Diseno_TOBE.md                     [Documento de diseño original]
│   ├── TOBE_Arquitectura_Completa.md      [ESTE DOCUMENTO]
│   └── UML_TOBE.drawio                    [Diagrama UML editable]
├── 03-src/                                [Fase 4 — pendiente]
├── 04-evidencia/                          [Fase 4 — pendiente]
└── README.md                              [Pendiente]
```

---

# ANEXO — SC-2 IMPLEMENTADO: Chips de Geolocalización

**Solicitud de cambio implementada:** SC-2 — La hacienda conecta chips a las reses para geolocalización

**Principios SOLID aplicados:** OCP (nuevo tipo sin modificar existentes), DIP (abstracciones ISP (interfaces segregadas para Chip y Geolocalización)

---

## A.1 Domain — Nuevas Entidades SC-2

### `EstadoChip` enum *(Azul — OCP)*

```csharp
namespace Hacienda.Domain.Enums;

public enum EstadoChip : byte
{
    Activo = 1,
    Inactivo = 2,
    Perdido = 3,
    Dañado = 4
}
```

### `NumeroSerieChip` Value Object *(Verde — SRP)*

```csharp
namespace Hacienda.Domain.ValueObjects;

public readonly record struct NumeroSerieChip
{
    public string Valor { get; }

    public NumeroSerieChip(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("El número de serie no puede ser vacío", nameof(valor));
        if (valor.Length > 50)
            throw new ArgumentException("El número de serie no puede exceder 50 caracteres", nameof(valor));
        Valor = valor.Trim().ToUpperInvariant();
    }

    public static implicit operator string(NumeroSerieChip numeroSerie) => numeroSerie.Valor;
    public static explicit operator NumeroSerieChip(string valor) => new NumeroSerieChip(valor);
    
    public override string ToString() => Valor;
}
```

### `IChip` interface *(Violeta — DIP)*

```csharp
namespace Hacienda.Domain.Entities;

public interface IChip
{
    Guid Id { get; }
    NumeroSerieChip NumeroSerie { get; }
    DateTime FechaInstalacion { get; }
    EstadoChip Estado { get; }
    void CambiarEstado(EstadoChip nuevoEstado);
}
```

### `Chip` entity *(Verde — SRP, Azul — OCP)*

```csharp
namespace Hacienda.Domain.Entities;

public class Chip : IChip
{
    public Guid Id { get; }
    public NumeroSerieChip NumeroSerie { get; }
    public DateTime FechaInstalacion { get; }
    public EstadoChip Estado { get; private set; }

    private Chip(Guid id, NumeroSerieChip numeroSerie, DateTime fechaInstalacion)
    {
        Id = id;
        NumeroSerie = numeroSerie;
        FechaInstalacion = fechaInstalacion;
        Estado = EstadoChip.Activo;
    }

    public static Chip Crear(Guid id, NumeroSerieChip numeroSerie, DateTime fechaInstalacion)
    {
        if (fechaInstalacion > DateTime.Now)
            throw new ArgumentException("La fecha de instalación no puede ser futura", nameof(fechaInstalacion));
        if (fechaInstalacion < new DateTime(2000, 1, 1))
            throw new ArgumentException("La fecha de instalación no puede ser anterior al año 2000", nameof(fechaInstalacion));

        return new Chip(id, numeroSerie, fechaInstalacion);
    }

    public void CambiarEstado(EstadoChip nuevoEstado)
    {
        if (!Enum.IsDefined(typeof(EstadoChip), nuevoEstado))
            throw new ArgumentException($"Estado de chip inválido: {nuevoEstado}", nameof(nuevoEstado));
        if (Estado == nuevoEstado) return;
        ValidarTransicionEstado(nuevoEstado);
        Estado = nuevoEstado;
    }

    private void ValidarTransicionEstado(EstadoChip nuevoEstado)
    {
        switch (Estado)
        {
            case EstadoChip.Activo:
                if (nuevoEstado == EstadoChip.Perdido || nuevoEstado == EstadoChip.Dañado || nuevoEstado == EstadoChip.Inactivo)
                    return;
                break;
            case EstadoChip.Inactivo:
                if (nuevoEstado == EstadoChip.Activo || nuevoEstado == EstadoChip.Perdido || nuevoEstado == EstadoChip.Dañado)
                    return;
                break;
            case EstadoChip.Perdido:
            case EstadoChip.Dañado:
                if (nuevoEstado == EstadoChip.Activo)
                    return;
                break;
        }

        throw new InvalidOperationException(
            $"Transición de estado no permitida: de {Estado} a {nuevoEstado}. " +
            "Contacte al administrador para transiciones especiales.");
    }

    public override string ToString() 
        => $"Chip: {NumeroSerie} | Estado: {Estado} | Instalado: {FechaInstalacion:yyyy-MM-dd}";
}
```

### `Geolocalizacion` entity *(Verde — SRP)*

```csharp
namespace Hacienda.Domain.Entities;

public class Geolocalizacion
{
    public Guid Id { get; }
    public Guid ChipId { get; }
    public double Latitud { get; }
    public double Longitud { get; }
    public DateTime FechaHora { get; }
    public double? PrecisionMetros { get; }

    public Geolocalizacion(Guid id, Guid chipId, double latitud, double longitud, DateTime fechaHora, double? precisionMetros = null)
    {
        Id = id;
        ChipId = chipId;
        Latitud = latitud;
        Longitud = longitud;
        FechaHora = fechaHora;
        PrecisionMetros = precisionMetros;
    }
}
```

### Actualización: `Res` entity *(OCP — nueva propiedad Chip)*

```csharp
public abstract class Res
{
    // ... propiedades existentes ...
    public IChip Chip { get; set; }  // ← NUEVA PROPIEDAD SC-2
}
```

---

## A.2 Domain — Nuevas Interfaces SC-2

```csharp
// IRepositorioChip.cs
namespace Hacienda.Domain.Interfaces;

public interface IRepositorioChip
{
    List<IChip> ObtenerTodos();
    IChip? ObtenerPorNumeroSerie(string numeroSerie);
    void Guardar(IChip chip);
    void GuardarTodos(List<IChip> chips);
}

// IRepositorioGeolocalizacion.cs
public interface IRepositorioGeolocalizacion
{
    List<Geolocalizacion> ObtenerPorChipId(Guid chipId);
    List<Geolocalizacion> ObtenerUltimas(int cantidad);
    void Guardar(Geolocalizacion geolocalizacion);
    void GuardarTodas(List<Geolocalizacion> geolocalizaciones);
}
```

---

## A.3 Application — Nuevas Interfaces SC-2

```csharp
// IServicioChip.cs
namespace Hacienda.Application.Interfaces;

public interface IServicioChip
{
    string InstalarChip(Guid resId, string numeroSerie);
    string CambiarEstadoChip(string numeroSerie, EstadoChip estado);
    IChip? ObtenerChipPorNumeroSerie(string numeroSerie);
    IChip? ObtenerChipPorResId(Guid resId);
    List<IChip> ListarChips();
}

// IServicioGeolocalizacion.cs
public interface IServicioGeolocalizacion
{
    string RegistrarUbicacion(string numeroSerieChip, double latitud, double longitud, double? precisionMetros = null);
    List<Geolocalizacion> ObtenerHistorialChip(string numeroSerieChip);
    List<Geolocalizacion> ObtenerUltimasUbicaciones(int cantidad = 10);
    List<Geolocalizacion> ObtenerUbicacionesCercanas(double latitud, double longitud, double radioKm = 1.0);
}
```

---

## A.4 Application — Nuevos Servicios SC-2

### `ServicioChip`

```csharp
namespace Hacienda.Application.Services;

public class ServicioChip : IServicioChip
{
    private readonly IRepositorioChip _repoChip;
    private readonly IRepositorioRes _repoRes;
    private readonly IGuidProvider _guidProvider;

    public ServicioChip(IRepositorioChip repoChip, IRepositorioRes repoRes, IGuidProvider guidProvider)
    {
        _repoChip = repoChip;
        _repoRes = repoRes;
        _guidProvider = guidProvider;
    }

    public string InstalarChip(Guid resId, string numeroSerie)
    {
        var res = _repoRes.ObtenerTodos().FirstOrDefault(r => r.Id == resId)
            ?? throw new InvalidOperationException($"Res {resId} no encontrada");
        if (res.Chip != null)
            throw new InvalidOperationException($"La res ya tiene chip instalado ({res.Chip.NumeroSerie})");
        if (_repoChip.ObtenerPorNumeroSerie(numeroSerie) != null)
            throw new InvalidOperationException($"Ya existe chip con serie {numeroSerie}");
        var chip = Chip.Crear(_guidProvider.Nuevo(), new NumeroSerieChip(numeroSerie), DateTime.Now);
        res.Chip = chip;
        _repoChip.Guardar(chip);
        return $"Chip {numeroSerie} instalado en res {res.Nombre}";
    }

    public string CambiarEstadoChip(string numeroSerie, EstadoChip estado)
    {
        var chip = _repoChip.ObtenerPorNumeroSerie(numeroSerie)
            ?? throw new InvalidOperationException($"Chip {numeroSerie} no encontrado");
        chip.CambiarEstado(estado);
        _repoChip.Guardar(chip);
        return $"Estado de chip {numeroSerie} cambiado a {estado}";
    }

    public IChip? ObtenerChipPorNumeroSerie(string numeroSerie)
        => _repoChip.ObtenerPorNumeroSerie(numeroSerie);

    public IChip? ObtenerChipPorResId(Guid resId)
    {
        var res = _repoRes.ObtenerTodos().FirstOrDefault(r => r.Id == resId);
        return res?.Chip;
    }

    public List<IChip> ListarChips() => _repoChip.ObtenerTodos();
}
```

### `ServicioGeolocalizacion`

```csharp
namespace Hacienda.Application.Services;

public class ServicioGeolocalizacion : IServicioGeolocalizacion
{
    private readonly IRepositorioGeolocalizacion _repoGeo;
    private readonly IRepositorioChip _repoChip;

    public ServicioGeolocalizacion(IRepositorioGeolocalizacion repoGeo, IRepositorioChip repoChip)
    {
        _repoGeo = repoGeo;
        _repoChip = repoChip;
    }

    public string RegistrarUbicacion(string numeroSerieChip, double latitud, double longitud, double? precisionMetros)
    {
        var chip = _repoChip.ObtenerPorNumeroSerie(numeroSerieChip)
            ?? throw new InvalidOperationException($"Chip {numeroSerieChip} no encontrado");
        if (chip.Estado != EstadoChip.Activo)
            throw new InvalidOperationException($"El chip no está activo (estado: {chip.Estado})");
        if (latitud < -90 || latitud > 90) throw new ArgumentException("Latitud inválida");
        if (longitud < -180 || longitud > 180) throw new ArgumentException("Longitud inválida");

        var geo = new Geolocalizacion(Guid.NewGuid(), chip.Id, latitud, longitud, DateTime.Now, precisionMetros);
        _repoGeo.Guardar(geo);
        return $"Ubicación registrada para chip {numeroSerieChip}: [{latitud}, {longitud}]";
    }

    public List<Geolocalizacion> ObtenerHistorialChip(string numeroSerieChip)
    {
        var chip = _repoChip.ObtenerPorNumeroSerie(numeroSerieChip);
        return chip == null ? new List<Geolocalizacion>() : _repoGeo.ObtenerPorChipId(chip.Id);
    }

    public List<Geolocalizacion> ObtenerUltimasUbicaciones(int cantidad = 10)
        => _repoGeo.ObtenerUltimas(cantidad);

    public List<Geolocalizacion> ObtenerUbicacionesCercanas(double latitud, double longitud, double radioKm = 1.0)
    {
        return _repoGeo.ObtenerUltimas(1000)
            .Where(g => CalcularDistancia(latitud, longitud, g.Latitud, g.Longitud) <= radioKm)
            .OrderBy(g => CalcularDistancia(latitud, longitud, g.Latitud, g.Longitud))
            .ToList();
    }

    private static double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
```

---

## A.5 Infrastructure — Nuevos Repositorios CSV SC-2

> **Nota post-migración:** la implementación final de estos repositorios es SQLite (`RepositorioChipSqlite`, `RepositorioGeolocalizacionSqlite`). El código CSV de esta sección queda como registro del diseño intermedio (ADR-09).

### `RepositorioChipCsv`

```csharp
namespace Hacienda.Infrastructure.Persistence;

public class RepositorioChipSqlite : IRepositorioChip
{
    private readonly string _ruta;
    private readonly IGuidProvider _guidProvider;
    private static List<IChip>? _cache;  // Cache estático (compartido entre instancias)

    public RepositorioChipCsv(string directorioDatos, IGuidProvider guidProvider)
    {
        _ruta = Path.Combine(directorioDatos, "Chips.txt");
        _guidProvider = guidProvider;
    }

    public List<IChip> ObtenerTodos()
    {
        if (_cache != null) return _cache;
        if (!File.Exists(_ruta)) return new List<IChip>();

        var chips = new List<IChip>();
        foreach (var linea in File.ReadAllLines(_ruta))
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;
            var partes = linea.Split('|');
            if (partes.Length >= 4)
            {
                var id = Guid.Parse(partes[0]);
                var chip = Chip.Crear(id, new NumeroSerieChip(partes[1]), DateTime.ParseExact(partes[2].Trim(), "yyyy-MM-dd", null));
                if (Enum.Parse<EstadoChip>(partes[3], true) != EstadoChip.Activo)
                    chip.CambiarEstado(Enum.Parse<EstadoChip>(partes[3], true));
                chips.Add(chip);
            }
        }
        _cache = chips;
        return _cache;
    }

    public IChip? ObtenerPorNumeroSerie(string numeroSerie)
        => ObtenerTodos().FirstOrDefault(c => c.NumeroSerie.Valor.Equals(numeroSerie, StringComparison.OrdinalIgnoreCase));

    public IChip? ObtenerPorResId(Guid resId) => null;

    public void Guardar(IChip chip)
    {
        var chips = ObtenerTodos();
        chips.RemoveAll(c => c.Id == chip.Id);
        chips.Add(chip);
        File.WriteAllLines(_ruta, chips.Select(c => $"{c.Id}|{c.NumeroSerie.Valor}|{c.FechaInstalacion:yyyy-MM-dd}|{c.Estado}"));
        _cache = chips;
    }

    public void GuardarTodos(List<IChip> chips)
    {
        File.WriteAllLines(_ruta, chips.Select(c => $"{c.Id}|{c.NumeroSerie.Valor}|{c.FechaInstalacion:yyyy-MM-dd}|{c.Estado}"));
        _cache = chips;
    }
}
```

### `RepositorioGeolocalizacionCsv`

```csharp
namespace Hacienda.Infrastructure.Persistence;

public class RepositorioGeolocalizacionSqlite : IRepositorioGeolocalizacion
{
    private readonly string _ruta;
    private List<Geolocalizacion>? _cache;

    public RepositorioGeolocalizacionCsv(string directorioDatos)
    {
        _ruta = Path.Combine(directorioDatos, "Geolocalizaciones.txt");
    }

    public List<Geolocalizacion> ObtenerPorChipId(Guid chipId)
    {
        return ObtenerTodas().Where(g => g.ChipId == chipId).ToList();
    }

    public List<Geolocalizacion> ObtenerUltimas(int cantidad)
        => ObtenerTodas().OrderByDescending(g => g.FechaHora).Take(cantidad).ToList();

    private List<Geolocalizacion> ObtenerTodas()
    {
        if (_cache != null) return _cache;
        if (!File.Exists(_ruta)) return new List<Geolocalizacion>();

        var geos = new List<Geolocalizacion>();
        foreach (var linea in File.ReadAllLines(_ruta))
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;
            var partes = linea.Split('|');
            if (partes.Length >= 6)
            {
                geos.Add(new Geolocalizacion(
                    Guid.Parse(partes[0]),
                    Guid.Parse(partes[1]),
                    double.Parse(partes[2]),
                    double.Parse(partes[3]),
                    DateTime.ParseExact(partes[4].Trim(), "yyyy-MM-dd HH:mm:ss", null),
                    string.IsNullOrEmpty(partes[5]) ? (double?)null : double.Parse(partes[5])
                ));
            }
        }
        _cache = geos;
        return _cache;
    }

    public void Guardar(Geolocalizacion geolocalizacion)
    {
        var geos = ObtenerTodas();
        geos.Add(geolocalizacion);
        GuardarTodas(geos);
    }

    public void GuardarTodas(List<Geolocalizacion> geolocalizaciones)
    {
        var lineas = geolocalizaciones.Select(g => $"{g.Id}|{g.ChipId}|{g.Latitud}|{g.Longitud}|{g.FechaHora:yyyy-MM-dd HH:mm:ss}|{g.PrecisionMetros}");
        File.WriteAllLines(_ruta, lineas);
        _cache = geolocalizaciones;
    }
}
```

---

## A.6 Composition Root — Actualizado con SC-2

```csharp
// Hacienda.Web/Program.cs
// ── SC-2: Nuevos servicios registrados ──
builder.Services.AddScoped<IServicioChip, ServicioChip>();
builder.Services.AddScoped<IServicioGeolocalizacion, ServicioGeolocalizacion>();

// ── SC-2: Nuevos repositorios registrados ──
builder.Services.AddScoped<IRepositorioChip>(sp =>
    new RepositorioChipSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioGeolocalizacion>(sp =>
    new RepositorioGeolocalizacionSqlite(connectionString));
```

---

## A.7 DataLoader — Actualizado para SC-2

```csharp
// En DataLoader.CargarDatosAsync(), agregar después de cargar vacunas:
// 5. Crear CSVs vacíos para Chips y Geolocalizaciones si no existen
var rutaChips = Path.Combine(directorioDatos, "Chips.txt");
var rutaGeo = Path.Combine(directorioDatos, "Geolocalizaciones.txt");

if (!File.Exists(rutaChips)) File.WriteAllText(rutaChips, "");
if (!File.Exists(rutaGeo)) File.WriteAllText(rutaGeo, "");
```

---

## A.8 Resumen de Cumplimiento SOLID — SC-2

| Principio | Aplicación en SC-2 |
|-----------|-------------------|
| **SRP** | `Chip` gestiona solo su estado y serie; `Geolocalizacion` solo coordenadas; `ServicioChip` orquesta operaciones de chip; `ServicioGeolocalizacion` orquesta geolocalización |
| **OCP** | Agregar nuevo tipo de chip = crear nueva clase sin modificar existentes; `EstadoChip` permite nuevos estados sin cambiar código |
| **LSP** | `Chip` acepta cualquier transición de estado válida; subtipos no fortalecen precondiciones |
| **ISP** | `IChip` es interfaz mínima (4 propiedades + 1 método); `IServicioChip` tiene solo 5 métodos necesarios; `IServicioGeolocalizacion` tiene solo 4 métodos |
| **DIP** | Servicios dependen de interfaces (`IRepositorioChip`, `IRepositorioGeolocalizacion`); repositorios implementan interfaces en Infrastructure |

---

## A.9 UML TO-BE — Actualización

### Nuevos elementos en UML (colores según convención):

| Elemento | Color | Principio |
|----------|-------|-----------|
| `EstadoChip` enum | Azul | OCP |
| `NumeroSerieChip` VO | Verde | SRP |
| `IChip` interface | Violeta | DIP |
| `Chip` entity | Verde | SRP |
| `Geolocalizacion` entity | Verde | SRP |
| `IRepositorioChip` interface | Violeta | DIP |
| `IRepositorioGeolocalizacion` interface | Violeta | DIP |
| `IServicioChip` interface | Naranja | ISP |
| `IServicioGeolocalizacion` interface | Naranja | ISP |
| `ServicioChip` service | Verde | SRP |
| `ServicioGeolocalizacion` service | Verde | SRP |
| `RepositorioChipSqlite` repo | Violeta | DIP |
| `RepositorioGeolocalizacionSqlite` repo | Violeta | DIP |

### Relaciones TO-BE SC-2:

```
Res ──→ IChip (asociación, 0..1)
Chip ──→ Geolocalizacion (1..*, composición)
ServicioChip → IRepositorioChip + IRepositorioRes
ServicioGeolocalizacion → IRepositorioGeolocalizacion + IRepositorioChip
RepositorioChipSqlite : IRepositorioChip (realization)
RepositorioGeolocalizacionSqlite : IRepositorioGeolocalizacion (realization)
```

---

*Fin del documento.*
