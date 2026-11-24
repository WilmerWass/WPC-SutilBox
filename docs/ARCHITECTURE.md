# Arquitectura Técnica — WPC-SutilBox

**Versión alineada:** Beta 1.1.5 (`1.1.5-beta`). El flujo de perfiles documentado en [BETA_1.1.4.md](BETA_1.1.4.md) vive en `Core/PerformanceProfiles.cs` y `ViewModels/MainViewModel.cs`.

## Estado comprobado del repositorio (24-11-2026)

- La aplicación WPF y sus servicios Core están en un solo proyecto .NET 8; CLI es un proyecto aparte. El servidor MCP también compila como proyecto aparte, pero no está incluido en `WPC-SutilBox.sln`.
- El flujo real sigue siendo `App.xaml.cs → MainWindow.xaml → Views → ViewModels → Core`. La navegación mantiene las vistas en la ventana y alterna su visibilidad por `AppSection`.
- La solución usa MVVM y DI, pero aún no tiene ensamblados separados por dominio/aplicación/infraestructura; por eso se describe como arquitectura por capas en un único ensamblado, no como Clean Architecture estricta.
- Zona Gamer detecta instalaciones locales verificables de Steam y Epic mediante manifiestos y existencia de carpeta. No hay monitor de FPS, GPU/VRAM o temperatura, ni integración con otros lanzadores en este cambio.
- El CLI Windows implementa diagnósticos básicos disponibles; no representa compatibilidad completa con un motor Windows de diagnóstico.
- El servidor MCP requiere `confirm=true` para cerrar procesos, bloquea procesos esenciales y redacta valores de variables sensibles por heurística de nombre/patrón.
- La lista de vistas desconectadas y el detalle de estado por fase están en [la auditoría inicial](AUDITORIA_INICIAL_2026-11-24.md) y en los informes FASE2–FASE7.

## 1. Stack Tecnológico Base
* **Lenguaje:** C# (.NET Core / .NET 8+)
* **Framework UI:** WPF (Windows Presentation Foundation)
* **Patrón de Diseño:** MVVM (Model-View-ViewModel)
* **APIs de Sistema:** Windows API (Win32), WMI (Windows Management Instrumentation), Registro de Windows, PowerShell API.

## 2. Estructura de Capas
┌────────────────────────────────────────────────────────┐
│                      VISTAS (WPF)                      │
│                  (XAML / Data Binding)                 │
└───────────────────────────┬────────────────────────────┘
│ Commands / Data Context
┌───────────────────────────▼────────────────────────────┐
│                    VIEW MODELS (MVVM)                  │
│             (State Management, INotifyPC)              │
└───────────────────────────┬────────────────────────────┘
│ Service Injection
┌───────────────────────────▼────────────────────────────┐
│                     SERVICIOS (Core)                   │
│   (WMIService, RegistryService, PowerShellRunner, etc) │
└───────────────────────────┬────────────────────────────┘
│ Native Interop / System Calls
┌───────────────────────────▼────────────────────────────┐
│               SISTEMA OPERATIVO WINDOWS                │
└────────────────────────────────────────────────────────┘


## 3. Patrones y Reglas de Arquitectura
* **MVVM Estricto:** Las Vistas no deben contener lógica de negocio en el Code-Behind salvo manipulación estrictamente visual UI.
* **Inyección de Dependencias:** Servicios registrados mediante contenedor de IoC/DI para facilitar pruebas y desacoplamiento.
* **Comunicaciones Asíncronas:**
  * Toda llamada a WMI, manipulación de Registro masiva o ejecución de PowerShell **debe ser asíncrona (`async/await`)**.
  * Queda estrictamente prohibido bloquear el hilo principal de renderizado (`DispatcherUIThread`).

## 4. Subsystem Interop
* **RegistryService:** Acceso seguro a `HKLM` y `HKCU`. Toda escritura requiere respaldo previo para opción de reversión.
* **PowerShellRunner:** Ejecución encapsulada mediante `System.Management.Automation` o ejecución de procesos desacoplados con privilegio UAC validado.
* **WMI / CMI:** Consultas optimizadas limitando campos proyectados (`SELECT ...`) para reducir el overhead de inicialización.

## 5. Elementos Planificados (No Implementados Aún)
* [PLANIFICADO] Abstracción de capa de métricas para trazabilidad en tiempo real de bajo nivel (DirectX / Frame Rendering).
* [PLANIFICADO] Módulo IPC de comunicación para Overlay/Monitor independiente.
