vm: template-vm

## Running inside the VM

Este es un **demo de código** de solo lectura. WPC-SutilBox es una aplicación de escritorio Windows (WPF / .NET 8.0) que no se puede ejecutar en un contenedor Linux.

El demo permite al agente navegar el código fuente, leer la documentación y explorar la estructura del proyecto.

## Descripción del proyecto

**WPC-SutilBox** — Sistema Integral de Control, Diagnóstico y Optimización para Windows.

- **Framework:** .NET 8.0 (net8.0-windows)
- **UI:** WPF + Windows Forms
- **Arquitectura:** MVVM (ViewModels, Views, Models, Core)
- **Plataforma:** Windows-only (no ejecutable en Linux)

## Estructura del proyecto

- `Core/` — Lógica central del sistema
- `Models/` — Modelos de datos
- `ViewModels/` — ViewModels (patrón MVVM)
- `Views/` — Vistas adicionales
- `Resources/` — Recursos (imágenes, iconos)
- `MainWindow.xaml` / `MainWindow.xaml.cs` — Ventana principal
- `App.xaml` / `App.xaml.cs` — Entrada de la aplicación
- `README.md` — Documentación principal
- `CONTRIBUTING.md` — Guía de contribución

## Notas

- No hay comando de lanzamiento, puertos, servicios ni autenticación.
- El agente puede explorar el código, leer archivos de documentación y analizar la arquitectura.
