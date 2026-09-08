# Proyecto Hacienda - Reto 2: Patrones de Diseño Arquitectónico

Sistema de gestión para una hacienda ganadera. Segunda evolución: del diseño correcto (SOLID) al diseño robusto, aplicando patrones de diseño (Factory Method + Template Method, Builder y Observer) sin cambiar el estilo arquitectónico ni el comportamiento observable.

## Roles del Equipo

| Integrante | Rol | Responsabilidad |
|------------|-----|-----------------|
| Mateo Rojas Hernández | Arquitecto Líder | Detección de los puntos rígidos del diseño, evaluación y descarte de patrones, y diseño del TO-BE (qué sale, qué entra, cómo se relacionan) |
| María Alejandra Vargas Duque | Arquitecta de Verificación | Demostrar que SOLID sigue en pie tras introducir los patrones y que el comportamiento observable no cambió (matriz de verificación + casos antes/después) |
| David Salcedo Higuita | Arquitecto de Comunicación | Las dos vistas (negocio y equipo de desarrollo), la bitácora de decisiones frente a la IA y el armado del documento de sustentación |
| Mateo + Alejandra | Riesgos y despliegue | Análisis de riesgos de incorporar los patrones y plan de cambio por fases |

## Instrucciones de Ejecución

### Prerrequisitos
- .NET 8 SDK
- SQLite (se crea automáticamente al ejecutar)

### Pasos

1. Clonar el repositorio:
```bash
git clone https://github.com/MAD-OFI-Architects/ProyectoHacienda.git
cd ProyectoHacienda
```

2. Navegar a la carpeta del proyecto:
```bash
cd SolucionPatrones
```

3. Compilar el proyecto:
```bash
dotnet build Hacienda.TOBE.sln
```

4. Ejecutar la aplicación:
```bash
cd Hacienda.Web
dotnet run
```

5. Abrir en el navegador (el puerto se muestra en la consola al ejecutar):
```
https://localhost:5001
```

### Credenciales por defecto
- **Admin:** admin / admin123
- **Empleado:** empleado / emp456
- **Visitante:** visitante / visit789

## Video de Presentación
https://www.youtube.com/watch?v=W0Ew_MW05Ug

## Estructura del Proyecto

```
SolucionPatrones/
├── Hacienda.Domain/          # Entidades, reglas, value objects, factories (patrones), builders, eventos
├── Hacienda.Application/     # Servicios (casos de uso), fachadas delgadas
├── Hacienda.Infrastructure/  # Persistencia SQLite, despachador de eventos, handlers, políticas
└── Hacienda.Web/             # Controllers, Views (Razor), Program.cs (composition root)
```
