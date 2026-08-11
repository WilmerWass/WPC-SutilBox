# 🖥️ WPC SUTILBOX

### Sistema avanzado de control, optimización y mantenimiento para Windows

**WPC SUTILBOX** es una utilidad de escritorio para Windows diseñada para centralizar **mantenimiento, diagnóstico, rendimiento, configuración y administración del sistema** en una interfaz WPF moderna, organizada y fácil de usar.

> 🚧 **Proyecto en desarrollo — Beta 1**
>
> La nueva generación de WPC SUTILBOX está evolucionando hacia el estándar principal de la suite.

<p align="center">

[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge\&logo=dotnet\&logoColor=white)](https://dotnet.microsoft.com/)
[![Windows 10/11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?style=for-the-badge\&logo=windows\&logoColor=white)](https://www.microsoft.com/windows)
[![WPF](https://img.shields.io/badge/UI-WPF-68217A?style=for-the-badge)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge\&logo=csharp\&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Status](https://img.shields.io/badge/Status-Beta%201-orange?style=for-the-badge)](https://github.com/WilmerWass/WPC-SutilBox/releases)

</p>

## Nombre de la entrega

Esta entrega se publica como **WPC-SutilBox Beta-1**, un nuevo comienzo para la línea de producto. La futura versión estable se identificará como **WPC-SutilBox V1.0.0**.

<p align="center">

**[📥 Descargar](#-descarga-e-instalación)** •
**[🚀 Características](#-características-principales)** •
**[🏗️ Arquitectura](#️-arquitectura-técnica)** •
**[📚 Documentación](#-documentación)** •
**[🐛 Reportar error](https://github.com/WilmerWass/WPC-SutilBox/issues/new?labels=bug&template=bug_report.md)** •
**[💡 Sugerir idea](https://github.com/WilmerWass/WPC-SutilBox/issues/new?labels=enhancement&template=feature_request.md)**

</p>

---

## 📋 Tabla de contenidos

* [⚠️ Aviso de versiones y estabilidad](#️-aviso-de-versiones-y-estabilidad)
* [🚀 Estado de Beta 1](#-estado-de-beta-1)
* [✨ Características principales](#-características-principales)
* [🧭 Navegación](#-navegación)
* [📥 Descarga e instalación](#-descarga-e-instalación)
* [🌑 Modo oscuro y modo claro](#-modo-oscuro-y-modo-claro)
* [🏗️ Arquitectura técnica](#️-arquitectura-técnica)
* [🛡️ Seguridad operativa](#️-seguridad-operativa)
* [📚 Documentación](#-documentación)
* [🗺️ Roadmap](#️-roadmap)
* [🤝 Contribuir](#-contribuir)
* [📄 Licencia](#-licencia)

---

## ⚠️ Aviso de versiones y estabilidad

| Versión                 | Estado     | Uso recomendado         |
| ----------------------- | ---------- | ----------------------- |
| **WassControlSys 1.8**  | 🟢 Estable | Producción y uso diario |
| **WPC SUTILBOX Beta 1** | 🟠 Beta    | Pruebas y evaluación    |

**WassControlSys 1.8** continúa siendo la versión recomendada para entornos de producción diarios.

La nueva **WPC SUTILBOX Beta 1** representa la evolución de la suite y está destinada a convertirse en el nuevo estándar oficial.

> ⚠️ Las versiones Beta pueden contener errores o comportamientos que todavía estén siendo refinados.

---

# 🚀 Estado de Beta 1

La **Beta 1** cierra el flujo funcional principal de la nueva generación de WPC SUTILBOX.

### ✅ Funcionalidades implementadas

* [x] Monitorización de CPU
* [x] Monitorización de RAM
* [x] Monitorización de temperatura
* [x] Monitorización de batería
* [x] Monitorización de red
* [x] Monitorización de almacenamiento
* [x] Botón central **Optimizar**
* [x] Limpieza de archivos temporales
* [x] Liberación de memoria
* [x] Perfiles de rendimiento
* [x] Puntos de restauración
* [x] Gestión de aplicaciones de inicio
* [x] Gestión de bloatware
* [x] Actualizaciones mediante Winget
* [x] Historial de logs de sesión
* [x] Configuración persistente
* [x] Modo oscuro
* [x] Modo claro
* [x] Color de acento
* [x] Inicio con Windows
* [x] Minimización a bandeja
* [x] Optimización durante periodos de inactividad

---

# ✨ Características principales

## 📊 Monitorización avanzada

Consulta en tiempo real el estado del equipo:

* CPU
* Memoria RAM
* Temperaturas
* Disco
* Red
* Batería
* Procesos de alto consumo

La información se presenta de forma centralizada para facilitar el diagnóstico del sistema.

---

## ⚡ Perfiles de rendimiento

WPC SUTILBOX incluye perfiles preparados para diferentes escenarios:

| Perfil               | Objetivo                    |
| -------------------- | --------------------------- |
| ⚖️ **Equilibrado**   | Uso diario                  |
| 🎮 **Gaming**        | Priorizar rendimiento       |
| 💼 **Productividad** | Trabajo y multitarea        |
| 💻 **Desarrollo**    | Entornos de programación    |
| ⚙️ **A tu medida**   | Configuración personalizada |

Antes de realizar modificaciones importantes, el sistema puede crear automáticamente un **punto de restauración**.

---

## 🧹 Optimización rápida

El botón central **Optimizar** permite ejecutar acciones de mantenimiento desde un único lugar.

Puede incluir:

* Limpieza de temporales
* Liberación de memoria
* Aplicación del perfil seleccionado
* Tareas de mantenimiento configuradas

---

## 📦 Gestión de software

La aplicación integra herramientas para administrar el software instalado:

* Aplicaciones de inicio
* Bloatware
* Desinstalación
* Actualizaciones mediante `winget`
* Consulta de paquetes disponibles

---

# 🧭 Navegación

## 🏠 Inicio

Panel principal con:

* Botón **Optimizar**
* Perfil de rendimiento activo
* Estado general del equipo
* Resumen de utilización de recursos

---

## 🔍 Revisar mi PC

Área dedicada al diagnóstico del equipo.

### Resumen de salud

Estado general de los principales componentes.

### 💾 Espacio y almacenamiento

Información sobre discos y espacio disponible.

### 🖥️ Hardware y temperaturas

Información del hardware y temperaturas detectadas.

### 📈 Procesos de alto consumo

Identificación de procesos que utilizan una cantidad elevada de recursos.

---

## 🧹 Liberar espacio

Herramientas para recuperar espacio:

* Limpieza básica
* Análisis de archivos grandes
* Gestión de Descargas
* Herramientas nativas de Windows

---

## 📦 Aplicaciones

Centro de administración de software:

* Aplicaciones de inicio
* Bloatware
* Desinstalación
* Winget

---

## 🛠️ Herramientas avanzadas

Incluye herramientas orientadas a usuarios que necesitan mayor control:

* Reparación de Windows
* Procesos
* Servicios
* Perfiles
* Utilidades nativas

---

## 📜 Historial y seguridad

Permite consultar los eventos registrados durante la sesión.

También permite acceder a la carpeta completa de logs para facilitar:

* Auditoría
* Diagnóstico
* Investigación de errores
* Revisión de operaciones

---

## ⚙️ Ajustes

La configuración permite controlar:

* 🌐 Idioma
* 🎨 Apariencia
* 🖌️ Color de acento
* 🚀 Inicio con Windows
* 📥 Minimización a bandeja
* ⚡ Optimización automática en periodos de inactividad

---

# 📥 Descarga e instalación

## 💻 Requisitos

| Requisito         | Información                                 |
| ----------------- | ------------------------------------------- |
| Sistema operativo | Windows 10 / Windows 11                     |
| Arquitectura      | x64                                         |
| Runtime           | .NET 8 Desktop Runtime                      |
| Privilegios       | Administrador para determinadas operaciones |
| Winget            | Recomendado para gestión de aplicaciones    |

### 📦 Descargar versión publicada

Consulta las versiones disponibles en:

**[➡️ Releases de WPC SUTILBOX](https://github.com/WilmerWass/WPC-SutilBox/releases)**

> 💡 Se recomienda utilizar la versión marcada como **Latest** para obtener la versión estable más reciente.

---

## 🧑‍💻 Instalación desde código fuente

Clonar el repositorio:

```powershell
git clone https://github.com/WilmerWass/WPC-SutilBox.git
cd WPC-SutilBox
```

Restaurar dependencias:

```powershell
dotnet restore
```

Compilar:

```powershell
dotnet build -c Release
```

Ejecutar:

```powershell
dotnet run -c Release
```

---

## 📦 Publicar para Windows x64

Para generar una publicación Windows x64:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

Los archivos generados estarán disponibles dentro de:

```text
bin\Release\net8.0-windows\win-x64\publish\
```

---

# 🌑 Modo oscuro y modo claro

WPC SUTILBOX utiliza un **Design System basado en tokens** para mantener consistencia visual entre los diferentes modos.

Los tokens incluyen:

* Background
* Surface
* Elevated Surface
* Hover
* Primary Text
* Secondary Text
* Borders
* Estados semánticos
* Color de acento

El usuario puede cambiar la apariencia desde:

**Ajustes → Apariencia y Estilo**

El cambio se aplica dinámicamente sin necesidad de reiniciar la aplicación.

---

# 🏗️ Arquitectura técnica

## Stack

| Componente          | Tecnología                              |
| ------------------- | --------------------------------------- |
| Lenguaje            | **C#**                                  |
| Framework           | **.NET 8**                              |
| UI                  | **WPF / XAML**                          |
| Arquitectura        | **MVVM**                                |
| Configuración       | **JSON**                                |
| Monitorización      | PerformanceCounter / WMI / APIs Windows |
| Gestión de paquetes | Winget                                  |
| Logging             | Logs locales de sesión                  |

---

## 📐 Arquitectura

```text
WPC SUTILBOX
│
├── UI
│   └── WPF / XAML
│
├── ViewModels
│   └── MVVM
│
├── Services
│   ├── Performance
│   ├── Battery
│   ├── Restore Point
│   ├── Profiles
│   ├── Processes
│   ├── Startup
│   ├── Bloatware
│   ├── Winget
│   ├── Settings
│   └── Logging
│
└── Windows APIs
    ├── PerformanceCounter
    ├── GlobalMemoryStatusEx
    ├── WMI
    └── PowerStatus
```

### ViewModel principal

```text
MainViewModel
```

### Configuración

```text
%LOCALAPPDATA%\Wpc_SutilBox\settings.json
```

### Logs

```text
%LOCALAPPDATA%\Wpc_SutilBox\logs\session_*.log
```

---

# 🛡️ Seguridad operativa

WPC SUTILBOX está diseñado para que las operaciones administrativas o potencialmente destructivas sean **explícitas y controladas**.

### Restauración

Los perfiles pueden crear un **punto de restauración** antes de modificar:

* Planes de energía
* Servicios
* Configuración del sistema

### 🔐 Privilegios elevados

Las operaciones que requieren permisos administrativos utilizan el mecanismo **UAC de Windows**.

El usuario mantiene el control sobre la elevación de privilegios.

> ⚠️ Algunas herramientas pueden modificar configuraciones importantes de Windows. Se recomienda revisar la acción antes de ejecutarla.

---

# 📚 Documentación

La documentación técnica del proyecto se encuentra dentro de `/docs`.

### 📐 Arquitectura

[**ARCHITECTURE.md →**](https://github.com/WilmerWass/WPC-SutilBox/blob/main/docs/ARCHITECTURE.md)

Descripción de la arquitectura interna y organización técnica.

### 🧪 Plan Beta 1

[**PLAN_V1.2.0_BETA1.md →**](https://github.com/WilmerWass/WPC-SutilBox/blob/main/docs/PLAN_V1.2.0_BETA1.md)

Plan de implementación y evolución de la Beta 1.

### 📘 Blueprint del producto

[**PRODUCT_BLUEPRINT_ES.md →**](https://github.com/WilmerWass/WPC-SutilBox/blob/main/docs/PRODUCT_BLUEPRINT_ES.md)

Definición funcional y conceptual del producto.

### 🗺️ Roadmap

[**ROADMAP_V1.1.8_Y_SIGUIENTES.md →**](https://github.com/WilmerWass/WPC-SutilBox/blob/main/docs/ROADMAP_V1.1.8_Y_SIGUIENTES.md)

Historial y planificación de futuras versiones.

---

# 🗺️ Roadmap

El desarrollo de WPC SUTILBOX se organiza progresivamente por versiones.

```text
WassControlSys 1.8
       │
       ▼
WPC SUTILBOX
       │
       ▼
   Beta 1
       │
       ▼
   Estabilización
       │
       ▼
   Release
       │
       ▼
 Nuevas versiones
```

Consulta el **[Roadmap completo](https://github.com/WilmerWass/WPC-SutilBox/blob/main/docs/ROADMAP_V1.1.8_Y_SIGUIENTES.md)** para conocer las próximas etapas.

---

# 🐛 Reportar errores

¿Encontraste un problema?

Utiliza el sistema de Issues de GitHub:

**[🐛 Reportar un error](https://github.com/WilmerWass/WPC-SutilBox/issues/new?labels=bug&template=bug_report.md)**

Al reportarlo, intenta incluir:

* Versión de WPC SUTILBOX
* Versión de Windows
* Descripción del problema
* Pasos para reproducirlo
* Capturas de pantalla, si son relevantes
* Logs relacionados

---

# 💡 Sugerir una característica

¿Tienes una idea para mejorar WPC SUTILBOX?

**[💡 Crear una propuesta](https://github.com/WilmerWass/WPC-SutilBox/issues/new?labels=enhancement&template=feature_request.md)**

---

# 🤝 Contribuir

Las contribuciones son bienvenidas.

1. Haz un fork del repositorio.
2. Crea una rama para tu cambio.
3. Implementa y prueba la modificación.
4. Realiza un commit descriptivo.
5. Abre un Pull Request.

```bash
git checkout -b feature/nueva-funcionalidad
git add .
git commit -m "feat: añadir nueva funcionalidad"
git push origin feature/nueva-funcionalidad
```

Después puedes abrir un **Pull Request** desde GitHub.

---

# 📊 Proyecto

<p align="center">

**[⭐ Dar una estrella](https://github.com/WilmerWass/WPC-SutilBox)** •
**[📦 Releases](https://github.com/WilmerWass/WPC-SutilBox/releases)** •
**[🐛 Issues](https://github.com/WilmerWass/WPC-SutilBox/issues)** •
**[🔀 Pull Requests](https://github.com/WilmerWass/WPC-SutilBox/pulls)**

</p>

---

# 📄 Licencia

Consulta el archivo [`LICENSE`](https://github.com/WilmerWass/WPC-SutilBox/blob/main/LICENSE) para conocer los términos de distribución y uso del proyecto.

---

<p align="center">

### 🖥️ WPC SUTILBOX

**Controla. Optimiza. Mantén.**

Desarrollado por **WilmerWass**

</p>
