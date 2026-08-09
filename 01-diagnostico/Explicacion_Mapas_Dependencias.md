# Análisis de Dependencias del Proyecto Hacienda

Este documento describe las relaciones de dependencia identificadas en los mapas de arquitectura del proyecto, tanto a nivel de paquetes (vista general) como a nivel de clases y servicios (vista detallada). El proyecto está compuesto por dos librerías: `Bib_Hacienda`, que contiene el modelo de dominio y la lógica de negocio, y `p_mvcHacienda`, que contiene la aplicación web que consume dicho dominio bajo el patrón MVC.

## 1. Bib_Hacienda — Vista general de paquetes

A nivel de paquetes, la librería `Bib_Hacienda` se organiza en cinco módulos: `Reglas`, `Eventos`, `Interfaces`, `Clases` y `Aspectos`.

El paquete `Aspectos` depende del paquete `Clases`, es decir, la funcionalidad transversal del sistema (validación e interceptores, como se verá en el detalle) necesita conocer las clases de dominio para poder envolver su comportamiento. A su vez, `Clases` depende tanto de `Eventos` como de `Interfaces`: las entidades del dominio publican eventos definidos en el paquete `Eventos` y cumplen contratos definidos en el paquete `Interfaces`. Adicionalmente, `Clases` depende directamente de `Reglas`, sin intermediarios, porque las entidades consultan reglas de negocio de forma inmediata. Finalmente, tanto `Eventos` como `Interfaces` dependen también de `Reglas`, lo que convierte a este paquete en el núcleo más estable del sistema: prácticamente todos los demás módulos dependen de él, mientras que él no depende de ninguno.

En resumen, la dirección de las dependencias fluye desde la periferia (`Aspectos`) hacia el centro (`Reglas`), pasando por `Clases` y por la capa intermedia de `Eventos` e `Interfaces`. Esto refleja un diseño donde la lógica de negocio (`Reglas`) permanece aislada y protegida, mientras que las capas más externas dependen de ella y no al revés.

## 2. Bib_Hacienda — Vista detallada de clases

En el mapa detallado se observa que la clase `Hacienda` actúa como agregado raíz del sistema: depende de `Rex`, de `Potrero`, de `Venta` y de `Vacuna`, concentrando referencias hacia casi todas las entidades del dominio ganadero. Por eso es el nodo con mayor número de conexiones entrantes.

Dentro del dominio del ganado existe una jerarquía de herencia clara: `Ternero` depende de (hereda de) `Rex`, `Novillo` depende de `Ternero`, y `Ceba` depende de `Novillo`. Esto representa las etapas sucesivas de crecimiento de un animal. Todas estas clases, junto con `Rex`, dependen además de la clase `Vivo`, que probablemente define el estado de vida del animal, y dependen de la regla `ReglaRes`, que centraliza las validaciones de negocio propias del ganado. `Rex` también depende de los publicadores `PublisherPesoMin` y `PublisherPesoVenta` (y de sus respectivos delegados `dele_peso_min` y `dele_peso_venta`), a través de los cuales notifica cuándo un animal alcanza el peso mínimo o el peso de venta.

La clase `Potrero` depende de la regla `ReglaPotrero` y del enumerado `L_tipos_potreros`, que define los tipos de potrero existentes. Para notificar su estado, `Potrero` depende de los publicadores `PublisherPotreroLleno` y `PublisherPotreroMitad`, apoyados en sus delegados `delegado_potrero_lleno` y `delegado_potrero_mitad`.

Las clases `Vacuna` y `Vacunacion` dependen de la regla `ReglaVacuna`, la cual a su vez depende de `Bacteriana`, una regla más específica orientada al control sanitario. `Vacuna` depende también del enumerado `enum_I_atenuaciones` (niveles de atenuación de la vacuna) y de la interfaz `ICreacionVacuna`, que define el contrato para crear nuevas vacunas. Para notificar eventos, `Vacuna` depende de los publicadores `PublisherVacunacionCompletada` y `PublisherVacunaVencida`.

La clase `Venta` depende de la interfaz `IVentaRes`, que define el contrato de venta de ganado.

En cuanto a la validación, existe una clase base `Validador` de la cual dependen (heredan) `ValidadorVenta`, `ValidadorPotrero` y `ValidadorVacuna`; las tres implementan la interfaz `IValidarInformacion`. Cada validador depende de su entidad correspondiente —`ValidadorVenta` de `Venta`, `ValidadorPotrero` de `Potrero` y `ValidadorVacuna` de `Vacuna`— para verificar la información antes de que se apliquen las reglas o se disparen los eventos.

Por último, el bloque de autenticación queda relativamente aislado del resto del dominio: `Usuario` depende de la interfaz `IAutenticacion`, de la cual depende `InterceptorAutenticacion`; y existe además un `interceptorValidarInformacion` que intercepta las validaciones de forma transversal. Este bloque tiene pocas conexiones hacia las clases de dominio, lo cual confirma que su función es transversal (aspecto) y no forma parte del núcleo del negocio.

## 3. p_mvcHacienda — Vista general de paquetes

La aplicación web `p_mvcHacienda` se organiza en cuatro módulos: `Controllers`, `Models`, `Servicios` y `Program`.

El paquete `Controllers` depende de `Models`, ya que los controladores utilizan modelos de datos y de vista para recibir y devolver información. `Controllers` depende también de `Servicios`, pues cada controlador delega en un servicio la lógica de negocio correspondiente. Por su parte, `Program`, que es el punto de arranque de la aplicación, depende directamente de `Servicios` para inicializarlos o registrarlos, sin pasar por los controladores.

`Servicios` es, al igual que `Reglas` en la librería anterior, el paquete del cual dependen los demás sin depender él de ninguno, lo que lo convierte en el núcleo estable de esta capa.

## 4. p_mvcHacienda — Vista detallada de clases y servicios

Cada entidad del dominio tiene su propio trío controlador–servicio. `UsuarioController` depende de `UsuarioService`; `VentaController` depende de `VentaService`; `VacunaController` depende de `VacunaService`; `ResController` depende de `ResService`; y `PotreroController` depende de `PotreroService`. `AccountController` depende de `UsuarioService` y, además, del `LoginViewModel` para manejar el inicio de sesión. `HomeController` depende del `ErrorViewModel` para mostrar la vista de error, y presenta una dependencia interna sobre sí mismo (una relación cíclica reflejada en el diagrama), lo que sugiere que reutiliza su propia lógica entre distintas acciones del controlador.

Todos los servicios de negocio —`UsuarioService`, `VentaService`, `VacunaService`, `ResService` y `PotreroService`— dependen, sin excepción, de `PersistenciaService`, que actúa como capa única de acceso a datos para toda la aplicación. Esto significa que ningún servicio accede a la base de datos directamente: todos delegan esa responsabilidad en `PersistenciaService`.

Adicionalmente, el diagrama muestra conexiones directas entre `VacunaService`, `ResService` y `PotreroService`, lo que indica que estos tres servicios no solo dependen de la persistencia, sino que también se comunican entre sí —probablemente porque una vacunación o una venta de ganado necesita conocer en qué potrero se encuentra el animal, o porque el servicio de reses (`ResService`) necesita datos tanto de vacunación como de potreros para resolver sus propias reglas—.

Finalmente, `Program` depende directamente de varios servicios (`UsuarioService`, `VentaService`, `VacunaService`, `ResService` y `PotreroService`) para configurarlos al iniciar la aplicación, sin pasar por los controladores, lo cual es consistente con lo mostrado en el mapa general.

## 5. Conclusión

En ambos proyectos se repite el mismo principio arquitectónico: las capas externas (interfaz de usuario, controladores, aspectos transversales) dependen de las capas internas (servicios, reglas de negocio), pero nunca al revés. En `Bib_Hacienda`, todo el sistema termina dependiendo de las `Reglas` de negocio; en `p_mvcHacienda`, todo el sistema termina dependiendo de `PersistenciaService`. Esta dirección única de las dependencias favorece el bajo acoplamiento entre módulos y facilita el mantenimiento y las pruebas del sistema.
