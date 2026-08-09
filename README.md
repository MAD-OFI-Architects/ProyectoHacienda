# Proyecto Hacienda - Solución SOLID

Sistema de gestión para una hacienda ganadera, aplicando los principios SOLID con arquitectura limpia (Clean Architecture).

## Roles del Equipo

| Integrante | Rol | Responsabilidad |
|------------|-----|-----------------|
| Mateo Rojas Hernández | Arquitecto de dominio | Identificación de responsabilidades y límites de cada clase (SRP), modelo del dominio, jerarquías de herencia y su validez frente a LSP |
| María Alejandra Vargas Duque | Arquitecto de dependencias | Mapa de dependencias, abstracciones (interfaces), inversión e inyección de dependencias, composition root (DIP, ISP) |
| David Salcedo Higuita | Ingeniero de comportamiento | Pruebas de caracterización, evidencia de que la conducta observable se preservó, escenarios de ejecución del programa principal |
| Los tres | Integrador y evidencia | Consistencia diagrama–código, estructura del entregable, bitácora de uso de IA, métricas antes/después |

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
cd 03-src/SolucionSOLID
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
https://www.youtube.com/watch?v=6mL6s_rgIz4

## Estructura del Proyecto

```
SolucionSOLID/
├── Hacienda.Domain/          # Entidades, enums, interfaces, value objects
├── Hacienda.Application/     # Servicios, validaciones, interfaces de aplicación
├── Hacienda.Infrastructure/  # Persistencia SQLite, eventos, políticas
└── Hacienda.Web/             # Controllers, Views (Razor), Program.cs
```
