# Linea Base — Solicitudes de Cambio (Fase 2)

**Proyectos:** `Bib_Hacienda` + `p_mvcHacienda`

---

## Solicitudes de cambio aprobadas para el proximo trimestre

| Codigo | Solicitudes de cambio - Hacienda |
|--------|----------------------------------|
| **SC-1** | La hacienda en el futuro va a comenzar a vender productos derivados del ganado como: lacteos, carne, piel |
| **SC-2** | La hacienda tiene la necesidad de conectar a las reses chips para la geolocalizacion |
| **SC-3** | Ademas de las vacunas, se va a requerir tener la historia clinica de cada res en un futuro |

---

## Medicion sobre el codigo actual (AS-IS)

Para cada SC, antes de redisenar, medimos sobre el codigo actual: cuantas clases y cuantos archivos habria que modificar, y que comportamiento existente correria riesgo de romperse.

---

### SC-1 — Productos derivados del ganado (lacteos, carne, piel)

#### Cuantas clases y cuantos archivos habria que modificar hoy

| # | Archivo | Clase / Metodo | Que habria que cambiar |
|---|---------|----------------|------------------------|
| 1 | `Bib_Hacienda/Clases/Venta.cs` | `Venta` | Hoy solo vende `Res`. Hay que extender para vender `ProductoDerivado` o crear jerarquia `IVendible`. |
| 2 | `Bib_Hacienda/Clases/Hacienda.cs` | `Hacienda.vender_res` | Hardcoded a reses. Agregar `vender_producto` o generalizar el metodo. |
| 3 | `Bib_Hacienda/Interfaces/IVentaRes.cs` | `IVentaRes` | Solo declara `vender_res`. Agregar metodo o nueva interfaz. |
| 4 | `Bib_Hacienda/Clases/Potrero.cs` | `Potrero.L_reses` | Solo reses son vendibles. Relacionar productos con potrero. |
| 5 | `p_mvcHacienda/Controllers/VentaController.cs` | `VentaController` | Hoy solo lista ventas de reses. Agregar vistas de productos. |
| 6 | `p_mvcHacienda/Servicios/VentaService.cs` | `VentaService` | `ObtenerTodasLasVentas` accede `_hacienda.L_ventas`. Agregar logica de productos. |
| 7 | `p_mvcHacienda/Servicios/PersistenciaService.cs` | `GuardarVentas` | Serializa `venta.Res.Nombre`. Persistir productos tambien. |
| 8 | `p_mvcHacienda/Servicios/PersistenciaService.cs` | `CargarVentas` | Deserializa con switch de tipo de res. Agregar producto. |
| 9 | `p_mvcHacienda/Datos/Ventas.txt` | Formato | Pipe-delimited hardcoded para reses. Agregar columna de tipo de producto. |

**Total: 9 clases en 8 archivos a modificar.**

#### Que comportamiento existente correria riesgo de romperse

- **Venta de reses existente:** Modificar `Venta` o `Hacienda.vender_res` puede romper la logica de remocion de res del potrero (`Hacienda.cs:160`).
- **Persistencia de ventas:** El formato pipe-delimited tiene `tipoRes` en posicion 5. Agregar productos puede corromper el parseo de ventas legadas.
- **Estadisticas de `VentaService.ObtenerEstadisticas`:** Agrupa por potrero. Los productos no tienen potrero → division por cero o resultados vacios.

---

### SC-2 — Chips de geolocalizacion para reses

#### Cuantas clases y cuantos archivos habria que modificar hoy

| # | Archivo | Clase / Metodo | Que habria que cambiar |
|---|---------|----------------|------------------------|
| 1 | `Bib_Hacienda/Clases/Res.cs` | `Res` | Agregar atributo `Chip` / coordenadas o crear decorador. Hoy `Res` no tiene esta capacidad. |
| 2 | `Bib_Hacienda/Clases/Ternero.cs` | `Ternero` constructor | Si se agrega a `Res`, el constructor del subtipo lo hereda. Si decorador, hay que tocar cada subtipo. |
| 3 | `Bib_Hacienda/Clases/Novillo.cs` | `Novillo` constructor | Igual que Ternero. |
| 4 | `Bib_Hacienda/Clases/Cebon.cs` | `Cebon` constructor | Igual que Ternero. |
| 5 | `Bib_Hacienda/Clases/Potrero.cs` | `Potrero.anadir_res` | El switch factory (linea 88) crea subtipos. Hay que pasar chip al constructor. |
| 6 | `p_mvcHacienda/Servicios/PersistenciaService.cs` | `GuardarReses` / `CargarReses` | Serializar/deserializar chip en `Reses.txt` (agregar columna al pipe-delimited). |
| 7 | `p_mvcHacienda/Controllers/ResController.cs` | `ResController` | Agregar accion para asignar/consultar chip. |

**Total: 7 clases en 6 archivos a modificar.**

#### Que comportamiento existente correria riesgo de romperse

- **Constructores de subtipos de `Res`:** Los setters de `Edad` ya lanzan excepcion (H-07, H-08, H-09 de inventario). Agregar mas validacion al constructor aumenta la fragilidad.
- **`PersistenciaService.CargarVentas`:** El switch `_ => new Ternero(...)` (H-23) — si una fila legada no tiene columna de chip, el parseo falla o se corrompe.
- **`Potrero.anadir_res`:** El switch factory (H-09) ya es un punto de dolor. Agregar chip al constructor exige tocar el switch de 124 lineas.
- **LSP:** Si el chip es opcional (algunas reses tienen, otras no), un subtipo `ResConChip` puede romper las cadenas `is Ternero` en `Hacienda.aplicar_vacuna` (H-04) — el `is` no匹配 y el limite queda en 0.

---

### SC-3 — Historia clinica de cada res

#### Cuantas clases y cuantos archivos habria que modificar hoy

| # | Archivo | Clase / Metodo | Que habria que cambiar |
|---|---------|----------------|------------------------|
| 1 | `Bib_Hacienda/Clases/Res.cs` | `Res` | Agregar `L_historia_clinica` o relacion con `HistoriaClinica`. |
| 2 | `Bib_Hacienda/Clases/Hacienda.cs` | `Hacienda` | Agregar metodos `registrar_evento_clinico`, `consultar_historia`. La God Class crece. |
| 3 | `Bib_Hacienda/Interfaces/IVacunacion.cs` | `IVacunacion` | La vacunacion hoy solo registra vacunas; historia clinica es mas amplia. |
| 4 | `Bib_Hacienda/Eventos/PublisherVacunacionCompletada.cs` | publisher | Hoy solo dispara al completar esquema; historia requiere registrar cada evento. |
| 5 | `Bib_Hacienda/Clases/Validaciones/Validacion.cs` | `Validacion` (abstracta) | Agregar `ValidarHistoriaClinica` → obliga a agregar stub en los 4 validadores (H-05). |
| 6 | `Bib_Hacienda/Interfaces/IValidarInformacion.cs` | interfaz gorda | Agregar metodo a la interfaz → 6 archivos mas por tocar (H-05). |
| 7 | `p_mvcHacienda/Servicios/PersistenciaService.cs` | nuevos metodos | Nuevo archivo `HistoriasClinicas.txt` + serializacion + reconstruccion. |
| 8 | `p_mvcHacienda/Controllers/ResController.cs` | `ResController` | Agregar acciones de historia clinica. |
| 9 | `p_mvcHacienda/Servicios/ResService.cs` | `ResService` | Agregar metodos de consulta de historia. |
| 10 | `p_mvcHacienda/Datos/` | nuevo archivo | Nuevo archivo de datos para historias clinicas. |

**Total: 10 clases en 9 archivos a modificar.**

#### Que comportamiento existente correria riesgo de romperse

- **`IValidarInformacion` gorda (H-05):** Agregar `ValidarHistoriaClinica` a la interfaz obliga a modificar la interfaz + `Validacion` base + los 4 validadores concretos = **6 archivos** solo para validacion. Es el costo mas alto de las 3 SC.
- **`Hacienda` God Class (H-01):** Agregar metodos de historia clinica la hace aun mas grande. Impacto en tiempo de compilacion y riesgo de regresion.
- **Persistencia:** El formato pipe-delimited no soporta colecciones anidadas. La historia clinica tiene multiples entradas (eventos), por lo que el formato actual no sirve.

---

## Tabla comparativa de linea base

| SC | Descripcion | Clases a modificar (AS-IS) | Archivos a modificar (AS-IS) | Comportamiento en riesgo |
|----|-------------|---------------------------|------------------------------|--------------------------|
| SC-1 | Productos derivados | 9 | 8 | Remocion de res del potrero, parseo de ventas legadas, estadisticas por potrero |
| SC-2 | Chips geolocalizacion | 7 | 6 | Constructores fragiles de subtipos, parseo de reses legadas, LSP con `is`-checks |
| SC-3 | Historia clinica | 10 | 9 | Interfaz gorda (6 archivos solo para validacion), God Class, formato pipe sin colecciones |

### SC mas costosa hoy: SC-3 (historia clinica)

Toca **10 clases en 9 archivos**, incluyendo la jerarquia de validadores (H-05) que exige modificar 6 archivos por la interfaz gorda, y la God Class `Hacienda` (H-01) que ya tiene 559 lineas.

---

## SC seleccionada para implementar

### SC-2 — Chips de geolocalizacion

**Justificacion de la eleccion:**

1. **Presiona OCP y LSP simultaneamente:** agrega capacidad a la jerarquia `Res`, donde estan los hallazgos mas criticos (H-04, H-07, H-08, H-09).
2. **Presiona DIP:** la persistencia debe cambiar para serializar el chip, lo que prueba si los repositorios son extensibles.
3. **Es la mas representativa:** si la arquitectura nueva hace SC-2 barata (1 modificacion vs 7), demuestra empiricamente que OCP quedo aplicado.
4. **Toca el nucleo del dominio:** SC-1 es conceptualmente nueva (productos); SC-3 principalmente agrega un agregado. SC-2 toca `Res`, que es donde el rediseno tiene que brillar.
