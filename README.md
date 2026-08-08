# WPC SUTILBOX

### Sistema Avanzado de Control, Optimización y Mantenimiento para Windows

<div align="center">

[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows_10%2F11-blue.svg)](https://github.com/WilmerWass/WPC-SutilBox)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Status](https://img.shields.io/badge/status-v1.2.0--beta.1-orange.svg)](CHANGELOG.md)

**WPC SUTILBOX** es una suite de utilidades de nueva generación diseñada para devolverle el control total de su sistema operativo. Optimice el rendimiento, limpie archivos residuales, gestione procesos y mantenga su Windows en perfectas condiciones.

[Descargar Última Versión](#-descarga-e-instalación) • [Características](#-características-principales) • [Documentación](#-documentación-técnica)

</div>

---

## ⚠️ Aviso de Reestructuración (v1.2.0-beta.1)

Este proyecto se encuentra en un **proceso de transición técnica y cambio de identidad**. Hemos migrado oficialmente de **WassControlSys** a la nueva marca **WPC SUTILBOX** para reflejar una evolución en nuestra visión y metodología. Durante este período, la versión actual en desarrollo (**v1.2.0-beta.1**) contiene cambios estructurales importantes y puede presentar intermitencias en la interfaz. 

Se recomienda utilizar la versión estable anterior si buscas producción diaria, y abstenerse de usar la beta en entornos críticos hasta su estabilización.

---

## 🎯 Visión de WPC SUTILBOX

**WPC SUTILBOX** nace de la filosofía de lograr un equilibrio perfecto entre **potencia técnica extrema y sutileza operacional**. Creemos que una herramienta de optimización verdaderamente excepcional debe ser tan eficiente, tan optimizada, que el usuario final sienta que el sistema es ligero—casi como si no hubiera nada ejecutándose en segundo plano. No es suficiente ser poderoso; debe ser transparente. Cada línea de código, cada operación, está diseñada para maximizar el rendimiento del sistema operativo mientras minimiza el impacto visual y computacional. El resultado: una experiencia donde la potencia y la sutileza conviven en armonía perfecta.

---

## ✨ Características Principales

### 🚀 Optimización del Sistema

- **Gestión de Memoria RAM**: Libere memoria de procesos inactivos con un solo clic o de forma automática.
- **Mantenimiento de Red**: Limpieza de caché DNS y restablecimiento de pila TCP/IP para solucionar problemas de conexión.
- **Salud del Disco**: Análisis de fragmentación y herramientas de diagnóstico de almacenamiento.

### 🧹 Limpieza Profunda

- **Archivos Temporales**: Eliminación segura de temporales de sistema, usuario y caché de navegadores.
- **Prefetch**: Mantenimiento de la carpeta prefetch para resolver problemas de arranque de aplicaciones.
- **Desinstalador de Bloatware**: Escaneo inteligente (HKCU/HKLM) para detectar y eliminar software preinstalado no deseado.

### 🛡️ Seguridad y Privacidad

- **Monitor de Seguridad**: Estado en tiempo real de Antivirus, Firewall y UAC.
- **Configuración de Privacidad**: Ajustes rápidos para telemetría y recolección de datos (en desarrollo).

### 🔧 Herramientas Avanzadas

- **Gestor de Servicios**: Visualice, inicie o detenga servicios de Windows con información detallada.
- **Gestor de Procesos**: Controle qué se ejecuta en su PC, cambie prioridades o finalice tareas.
- **Información de Hardware**: Detalles completos sobre CPU, RAM, GPU, BIOS, Red y Uptime.
- **Reparación de Sistema**: Accesos directos a herramientas críticas como SFC, DISM y CHKDSK.

### 🌑 Ejecución en Segundo Plano

- **System Tray**: Minimice la aplicación al área de notificación para mantenerla ejecutándose sin molestar en la barra de tareas.

---

## 📸 Interfaz de Usuario

La interfaz ha sido diseñada siguiendo principios modernos de UI/UX, utilizando **WPF** y **XAML** para ofrecer:

- **Modo Oscuro** nativo y elegante.
- Tipografía **Roboto** para máxima legibilidad.
- Navegación fluida y animaciones sutiles.
- Feedback visual inmediato para todas las operaciones.

_(Capturas de pantalla próximamente en la carpeta `docs/images`)_

---

## 📥 Descarga e Instalación

### Requisitos Previos

- Windows 10 (versión 1809 o superior) o Windows 11.
- Permisos de Administrador (para funciones de limpieza y optimización).

### Versiones Disponibles

Elija la versión que mejor se adapte a sus necesidades: 
- **WPC SUTILBOX (Autocontenida)**
  - **Descripción:** Ideal para la mayoría de los usuarios. Incluye el .NET 8.0 Runtime integrado, por lo que **no necesita instalar .NET por separado**. Simplemente descargue, descomprima y ejecute.
  - **Descarga Directa:** [Versión Portable Autocontenida](https://github.com/WilmerWass/WPC-SutilBox/releases/download/v1.1.8/WassControlSys_v1.1.8_SelfContained.zip)

- **WPC SUTILBOX (Requiere .NET)**
  - **Descripción:** Esta es la versión más ligera en tamaño de descarga. **Requiere que el .NET 8.0 Desktop Runtime esté instalado** previamente en su sistema.
  - **Descargar .NET 8.0 Desktop Runtime:** [Aquí](https://dotnet.microsoft.com/download/dotnet/8.0)
  - **Descarga Directa (Versión Normal):** [Mejor Rendimiento](https://github.com/WilmerWass/WPC-SutilBox/releases/download/v1.1.8/WassControlSys_v1.1.8_Normal.zip)

- Entre a [Lanzamientos Oficiales](https://github.com/WilmerWass/WPC-SutilBox/releases) y obtenga más detalles de cada versión.

### Instalación

1. Descargue el archivo `.zip` de la versión elegida.
2. Descomprima el archivo en una carpeta de su elección.
3. Ejecute `Wpc_SutilBox.exe` (se recomienda "Ejecutar como administrador" para acceso completo a las funciones).

---

## 🛠️ Desarrollo

Si desea compilar el proyecto desde el código fuente:

```bash
# 1. Clonar el repositorio
git clone [https://github.com/WilmerWass/WPC-SutilBox.git](https://github.com/WilmerWass/WPC-SutilBox.git)
cd WPC-SutilBox

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build

# 4. Ejecutar
dotnet run

```

## 📖 Documentación Técnica

Para desarrolladores interesados en la estructura interna:

- **Arquitectura**: Visión general de MVVM, Inyección de Dependencias y organización del código.
- **Changelog**: Historial de versiones y cambios.

---

## ⚠️ Aviso Legal

Este software realiza modificaciones en el sistema operativo. Aunque ha sido probado exhaustivamente, el uso es **bajo su propia responsabilidad**. Se recomienda encarecidamente crear un **Punto de Restauración del Sistema** antes de ejecutar operaciones críticas de limpieza y optimización.

---

## 📄 Licencia

Este proyecto está licenciado bajo la Licencia **MIT**. Consulte el archivo [LICENSE](LICENSE) para más detalles.  
Copyright © 2026 **WilmerWass**.

---

## 🤝 ¡Colabora con el Proyecto!

Este es un proyecto de código abierto y toda ayuda es valiosa, especialmente durante esta etapa de transición y depuración de la versión beta.

Puedes colaborar de las siguientes maneras:

* **Reportando errores (Issues):** Si encuentras fallos en la interfaz o comportamientos extraños, abre un issue detallando el problema.
* **Enviando mejoras (Pull Requests):** Si sabes C# / WPF y quieres proponer correcciones o nuevas características, haz un fork del repositorio y envía tu PR.
