# Arquitectura Técnica — WPC-SutilBox

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