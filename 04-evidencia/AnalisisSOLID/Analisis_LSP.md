# Analisis LSP — Liskov Substitution Principle

**Proyectos:** `Bib_Hacienda` + `p_mvcHacienda`
**Hallazgos:** 4 CRITICAL, 2 WARNING, 1 SUGGESTION = 7 hallazgos

---

## Violaciones CRITICAL

### LSP-1 — `Ternero` fortalece la precondicion del setter `Res.Edad`

**Jerarquia:** `Res` (base, abstracta) → `Ternero`
**Tipo de violacion:** Precondicion fortalecida (el subtipo lanza para entradas que el base acepta)
**Ubicacion:** `Bib_Hacienda/Clases/Res.cs:31-35`; `Bib_Hacienda/Clases/Ternero.cs:19-24`

**Evidencia — contrato base (`Res.cs:31`):**
```csharp
public virtual ushort Edad
{
    get => edad;
    set => edad = value;   // acepta CUALQUIER ushort (0–65535), nunca lanza
}
```

**Evidencia — subtipo (`Ternero.cs:19`):**
```csharp
public override ushort Edad
{
    get => base.Edad;
    set => base.Edad = value <= ReglaRes.edad_max_ternero ? value :
        throw new Exception("El ternero excedio la edad maxima");
}
```

**Prueba de no-sustituibilidad:**
```csharp
void CumplirAnios(List<Res> reses) {
    foreach (var r in reses) r.Edad += 1;  // compila contra Res, lanza en runtime para Ternero
}
```
Si `r` es un `Ternero` cuya edad llega a 13, la linea lanza `Exception`. El base garantiza que esto nunca lanza; el subtipo rompe la garantia. El constructor tambien esta envenenado: `new Ternero("x", 200, 50)` lanza porque `: base(...)` ejecuta `this.Edad = edad`.

**Camino que rompe en produccion:** `PersistenciaService.CargarVentas:439` tiene fallback `_ => new Ternero(...)` — si una fila legada tiene `edad > 12`, la carga explota.

---

### LSP-2 — `Cebon` fortalece la precondicion del setter `Res.Edad`

**Jerarquia:** `Res` → `Cebon`
**Tipo de violacion:** Precondicion fortalecida
**Ubicacion:** `Bib_Hacienda/Clases/Cebon.cs:19-24`

```csharp
public override ushort Edad
{
    get => base.Edad;
    set => base.Edad = (value > ReglaRes.edad_max_ternero && value <= ReglaRes.edad_max_cebon) ? value :
        throw new Exception("El cebon excedio la edad maxima");
}
```

El base acepta cualquier `ushort`; `Cebon` solo acepta la ventana `(12, 48]`.

**Prueba de no-sustituibilidad:** `foreach (var r in reses) r.Edad = 10;` funciona para `Res`, lanza para `Cebon`.

---

### LSP-3 — `Novillo` fortalece precondicion + bug de copy-paste

**Jerarquia:** `Res` → `Novillo`
**Tipo de violacion:** Precondicion fortalecida + divergencia behavioral (bug de copy-paste)
**Ubicacion:** `Bib_Hacienda/Clases/Novillo.cs:19-24`

```csharp
public override ushort Edad
{
    get => base.Edad;
    set => base.Edad = value > ReglaRes.edad_max_cebon ? value :       // solo acepta edad > 48
        throw new Exception("El ternero excedio la edad maxima");       // ← mensaje dice "ternero" (copy-paste)
}
```

Dos defectos:
1. **Precondicion fortalecida** — el base acepta cualquier `ushort`; `Novillo` rechaza `edad <= 48`.
2. **Mensaje incorrecto** — el texto de la excepcion dice *"ternero"* cuando deberia decir *"novillo"*. Un lector no puede saber si el limite es intencional o un bug.

**Prueba de no-sustituibilidad:** `res.Edad = 5;` funciona en `Res`, lanza `Exception("El ternero excedio la edad maxima")` en `Novillo`.

---

### LSP-4 — Subtipos de `Validacion` lanzan `NotImplementedException` en 3 de 4 metodos del contrato

**Jerarquia:** `Validacion` (abstracta, implementa `IValidarInformacion`) → `ValidadorPotrero`, `ValidadorRes`, `ValidadorVacuna`, `ValidadorVenta`
**Tipo de violacion:** `NotImplementedException` en metodo heredado del contrato (subtipo no puede sustituir al base)
**Ubicacion:** `Validaciones/Validacion.cs:14-17`; los 4 archivos de subtipos

**Evidencia — contrato base (`Validacion.cs:11-18`):**
```csharp
public abstract class Validacion : IValidarInformacion
{
    public abstract bool ValidarRes(Res res);
    public abstract bool ValidarPotrero(Potrero potrero);
    public abstract bool ValidarVacuna(Vacuna vacuna);
    public abstract bool ValidarVenta(Venta venta);
}
```

La interfaz **promete que los 4 metodos retornan `bool`**. Sin embargo, cada subtipo honra exactamente uno y lanza en los otros 3:

```csharp
// ValidarRes.cs:21
public override bool ValidarPotrero(Potrero potrero)
{
    throw new NotImplementedException("Use ValidadorPotrero");
}
```

Patron identico en los 4 archivos — **12 overrides que lanzan en total**.

**Prueba de no-sustituibilidad:**
```csharp
IValidarInformacion validador = GetValidatorFromConfig();   // devuelve ValidadorPotrero
bool ok = validador.ValidarRes(miRes);                       // ← lanza NotImplementedException
```

Un cliente programado contra la interfaz **no puede llamar ningun metodo con seguridad**. El codigo mismo admite la derrota: `InterceptorValidarInformacion.cs:58` tiene un `catch (NotImplementedException)` que trata el lanzamiento como *flujo normal*.

---

## Violaciones WARNING

### LSP-5 — Cadenas `is`-type-check sobre la jerarquia `Res` asumen un conjunto cerrado de subtipos

**Jerarquia:** `Res` → {Ternero, Novillo, Cebon} (consumidos via type-test)
**Tipo de violacion:** Divergencia behavioral para cualquier subtipo nuevo; sin dispatch polimorfico
**Ubicacion:** `Hacienda.cs:487-501`; `PublisherPesoMin.cs:25-27`; `PublisherPesoVenta.cs:27-29`; `PublisherVacunacionCompletada.cs:30-41`

```csharp
if (res is Ternero)       { max_bac = ReglaVacuna.max_bac_ternero; ... }
else if (res is Novillo)  { max_bac = ReglaVacuna.max_bac_novillo; ... }
else if (res is Cebon)    { max_bac = ReglaVacuna.max_bac_cebon;   ... }
// → si ninguno coincide, max_bac y max_viv quedan en 0
```

**Prueba de no-sustituibilidad:** Agregar `VacaLechera : Res`. Ahora `hacienda.aplicar_vacuna(vac, "Bessie", "potrero-1")` → ningun `is` coincide → `max_bac = max_viv = 0` → `0 >= 0` → lanza inmediatamente. El subtipo **nunca puede vacunarse** a traves de la interfaz base.

---

### LSP-6 — Cadenas `is`-type-check / downcast sobre la jerarquia `Vacuna`

**Jerarquia:** `Vacuna` → {Bacteriana, Viva} (consumidos via type-test/downcast)
**Tipo de violacion:** Sin dispatch polimorfico; subtipo nuevo se maneja mal silenciosamente
**Ubicacion:** `Hacienda.cs:476-484, 504, 507, 531, 535`; `PersistenciaService.cs:201, 256`; `VacunaService.cs:142-143`

```csharp
foreach (Vacuna vac in res.L_vacunas_aplicadas) {
    if (vac is Bacteriana) contador_bacterianas++;
    else if (vac is Viva) contador_vivas++;
}  // un tercer subtipo no lo cuenta ningun contador

uint periodo = vacuna is Bacteriana bacteriana ? bacteriana.Periodo_aplicacion : 0;  // downcast
```

**Prueba de no-sustituibilidad:** Agregar `Toxoide : Vacuna`. En el conteo, ningun contador lo incrementa → los limites nunca se validan para ese tipo → se puede aplicar **sin limite**.

---

## SUGGESTION

### LSP-7 — `PublisherPesoMin` tiene operador de conversion implicita que siempre lanza

**Ubicacion:** `Bib_Hacienda/Eventos/PublisherPesoMin.cs:50-53`

```csharp
public static implicit operator PublisherPesoMin(PublisherPesoVenta v)
{
    throw new NotImplementedException();
}
```

No es un problema de herencia (no comparten base), pero es un contrato publico que compila y detona en runtime. Ningun caller actual lo activa, pero es superficie peligrosa.

---

## Jerarquias auditadas SIN violacion LSP

- **`Vacuna` → {`Bacteriana`, `Viva`}** (la jerarquia en si): ambos subtipos solo *extienden* el base — agregan propiedades y nunca sobrescriben un miembro. Una `Bacteriana`/`Viva` *puede* sustituir a `Vacuna`. El smell es del lado del consumidor (LSP-6).
- **`Hacienda : IVacunacion, IVentaRes, ICreacionVacuna`**: implementa los metodos declarados con firmas correctas.
- **`Autenticacion : IAutenticacion`**: unico implementador, metodo presente.

---

## Patron de causa raiz

LSP-1/2/3 y LSP-4 comparten un mismo ADN: el codigo usa herencia para **reusar un nombre** mientras silenciosamente **rechaza parte del contrato base** — setters que lanzan y overrides que lanzan `NotImplementedException`. LSP-5/6 son el sintoma colateral: como la jerarquia no tiene comportamiento polimorfico, cada consumidor reimplementa el dispatch con `is`-checks, que se rompen cuando la jerarquia crece.

**Fix unificador:** empujar comportamiento real (limites, umbrales de peso, validacion) a miembros polimorficos del base, y reemplazar el patron throw-to-enforce con validacion en el boundary del factory.
