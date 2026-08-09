# Puntos de Dolor Priorizados

**Criterio de priorizacion:** impacto en costo de cambio × frecuencia de cambio esperada × riesgo de regresion. El #1 esta antes que el #2 porque cuesta mas cambiarlo y se rompe con mayor frecuencia.

---

## PD-1 — `Hacienda` God Class (Cluster A)

**Hallazgos:** H-01, H-02, H-03, H-04, H-09, H-29

### Por que es el #1

| Dimension | Medicion |
|-----------|----------|
| Lineas de codigo | 559 |
| Razones de cambio | 6 (potreros, ganado, inventario vacunas, vacunacion clinica, eventos, reglas) |
| Interfaces que implementa | 3 (`IVacunacion`, `IVentaRes`, `ICreacionVacuna`) — 3 actores distintos |
| `new` de concretos inline | 10+ (4 publishers, Potrero, Venta, 4× Bacteriana/Viva) |
| Cadenas `is Type` | 3 bloques (`aplicar_vacuna` lineas 474-501) |
| Testabilidad | Nula — no se puede aislar ninguna responsabilidad |

**Argumento:** Es el nucleo del sistema. Toda solicitud de cambio (SC-1, SC-2, SC-3) cae directa o indirectamente en esta clase. Su tamano + la cantidad de `new` inline + las cadenas `is Type` la convierten en el punto donde TODO cambio es costoso y TODO cambio arriesga regresion en funcionalidad no relacionada. Si no se parte esta clase, el resto del redisenio es cosmestica.

---

## PD-2 — `IValidarInformacion` interfaz gorda + 12 `NotImplementedException` (Cluster B)

**Hallazgos:** H-05, H-06

### Por que es el #2

| Dimension | Medicion |
|-----------|----------|
| `NotImplementedException` stubs | 12 (3 por validador × 4 validadores) |
| Violaciones de principios | ISP + LSP + OCP simultaneamente |
| Mecanismo de "solucion" | `InterceptorValidarInformacion` atrapa `NotImplementedException` como flujo normal |
| Riesgo de datos invalidos | Alto — el catch puede dejar pasar datos invalidos |

**Argumento:** Es el #2 porque ademas de ser un defecto de diseno (ISP/LSP/OCP), es un **defecto de seguridad de datos en runtime**. El interceptor que "resuelve" los `NotImplementedException` puede enmascarar fallos de validacion reales. Agregar validacion para una nueva entidad exige modificar 6 archivos. Es la violacion que mas rapidamente puede causar un incidente de datos corruptos en produccion.

---

## PD-3 — Jerarquia `Res` con LSP roto + dispatch por type-checking (Cluster C)

**Hallazgos:** H-07, H-08, H-09

### Por que es el #3

| Dimension | Medicion |
|-----------|----------|
| Subtipos que fortalecen precondiciones | 3 (`Ternero`, `Novillo`, `Cebon`) |
| Sitios con `is Type` sobre `Res` | 4 (`Hacienda` + 3 publishers) |
| Sitios con `is Type` sobre `Vacuna` | 5 |
| Bug de copy-paste | 1 (mensaje de `Novillo` dice "ternero") |
| Comportamiento al agregar subtipo nuevo | Silenciosamente roto (0 vacunas, sin alertas, sin readiness) |

**Argumento:** Es el #3 porque la extension del dominio de ganado es una solicitud de cambio **ya aprobada** (SC-1 productos derivados, SC-2 chips de geolocalizacion que implican nuevos tipos). Con la jerarquia actual, agregar un subtipo rompe vacunacion, alertas de peso y readiness de venta sin error de compilacion — es un riesgo latente que se materializa en el proximo trimestre.

---

## Justificacion del orden

```
PD-1 (Hacienda God Class)
 └── Es el bloqueador: sin partir Hacienda, los demas redisenios no se pueden cablear via DI
     porque Hacienda crea todo inline. Ademas, las 3 SC pasan por aqui.

PD-2 (IValidarInformacion gorda)
 └── Es el riesgo activo de datos: el catch de NotImplementedException puede dejar pasar
     datos invalidos HOY. Ademas, es el patron que la otra jerarquia (Res/Vacuna) imita
     con sus type-checks — arreglarlo sienta el precedente.

PD-3 (Jerarquia Res / LSP)
 └── Es el riesgo futuro: las SC aprobadas exigen extender la taxonomia de ganado.
     Con LSP roto + type-checking, la extension rompe silenciosamente.
```

**Si solo pudieramos arreglar una cosa, seria PD-1. Si pudieramos dos, PD-1 + PD-2. Las tres son prerequisito para que las SC sean baratas.**
