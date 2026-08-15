# 🖥️ WPC-SutilBox

### Suite de mantenimiento, diagnóstico, optimización y control para Windows

<p align="center">
  <strong>Transparente · Ligero · Reversible · Sin placebo</strong>
</p>

<p align="center">

[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![WPF](https://img.shields.io/badge/UI-WPF-68217A?style=for-the-badge)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![Windows](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Status](https://img.shields.io/badge/Status-Beta%201-F59E0B?style=for-the-badge)](https://github.com/WilmerWass/WPC-SutilBox/releases)

</p>

<p align="center">

**[📥 Releases](https://github.com/WilmerWass/WPC-SutilBox/releases)** ·
**[🐛 Issues](https://github.com/WilmerWass/WPC-SutilBox/issues)** ·
**[📚 Documentación](#-documentación)** ·
**[🗺️ Plan de Betas](docs/PLAN_BETAS.md)**

</p>

---

## 📌 Estado del proyecto

> 🟠 **WPC-SutilBox se encuentra actualmente en Beta 1.**

Esta versión representa la nueva generación de la suite y se encuentra en una fase activa de **estabilización, verificación y corrección sobre hardware real**.

El objetivo inmediato no es añadir una gran cantidad de funciones nuevas, sino conseguir una base sólida:

- UI estable.
- Operaciones asíncronas.
- Diagnóstico confiable.
- Manejo correcto de UAC.
- Cero errores de compilación.
- Cero advertencias relevantes.
- Resultados verificables.
- Acciones transparentes y controladas.

La documentación de desarrollo y planificación debe reflejar siempre el **estado real del código**, no únicamente las funciones previstas.

---

## 🧭 Filosofía Sutil

WPC-SutilBox no busca aplicar cientos de modificaciones simplemente por aplicar modificaciones.

El proyecto sigue cuatro principios:

| Principio | Descripción |
|---|---|
| 🔎 **Transparente** | El usuario debe saber qué se va a modificar y por qué. |
| 🛡️ **Seguro** | Las operaciones sensibles deben manejar correctamente permisos, errores y elevación UAC. |
| ↩️ **Reversible** | Siempre que técnicamente sea posible, una modificación debe poder revertirse o restaurarse. |
| ⚡ **Ligero** | La herramienta no debe convertirse en otra carga permanente para Windows. |

### 🚫 Lo que WPC-SutilBox no pretende ser

- ❌ Un antivirus.
- ❌ Un limpiador milagroso.
- ❌ Un "FPS booster" basado en placebo.
- ❌ Un modificador irreversible del kernel.
- ❌ Un ejecutor de scripts opacos.
- ❌ Una aplicación que cambie configuraciones sin informar al usuario.

---

# 🚀 Beta 1

Beta 1 está enfocada principalmente en **estabilización y consolidación de la base existente**.

## 🟢 Áreas funcionales existentes

La aplicación cuenta actualmente con infraestructura para:

- 🖥️ Monitorización del sistema.
- 💾 Información de almacenamiento.
- ⚙️ Herramientas de mantenimiento.
- 🧹 Limpieza de archivos temporales.
- 🛠️ Reparación mediante herramientas nativas de Windows.
- 🔄 Creación de puntos de restauración.
- 📦 Gestión básica de aplicaciones.
- 🚀 Gestión de aplicaciones de inicio.
- ⚙️ Gestión de procesos.
- 🔧 Gestión de servicios.
- 📜 Registro e historial de operaciones.
- 🎨 Temas claro y oscuro.
- ⚙️ Configuración de la aplicación.

> ⚠️ **Importante:** que un módulo exista en la aplicación no significa necesariamente que todas sus funciones estén completamente estabilizadas. Beta 1 se encarga precisamente de verificar y corregir esas diferencias.

---

# 🔍 Diagnóstico del sistema

WPC-SutilBox centraliza información del equipo para facilitar el diagnóstico.

Entre las áreas contempladas se encuentran:

- CPU.
- RAM.
- GPU.
- Almacenamiento.
- Red.
- Batería.
- Temperaturas.
- Procesos y consumo de recursos.

La información procede de las APIs disponibles de Windows, WMI y otros mecanismos del sistema según el módulo.

### 🧪 Verificación en hardware real

Una parte importante del trabajo de Beta 1 procede de pruebas realizadas directamente sobre equipos físicos.

Los resultados de esas pruebas se conservan como evidencia técnica dentro de:

```text
ANOSUBIR/
└── BETA_1/
    └── 1.2_PC_REVIEW/
```

Estos registros tienen prioridad sobre documentación antigua cuando existe una discrepancia entre lo documentado y el comportamiento real.

---

# 🧹 Mantenimiento

La aplicación proporciona herramientas para tareas habituales de mantenimiento de Windows.

### Limpieza

Entre las operaciones disponibles se encuentran:

- Archivos temporales.
- Carpetas de mantenimiento.
- Papelera de reciclaje.
- Otras tareas de limpieza controlada.

### Reparación

Se contemplan herramientas nativas como:

```text
SFC /scannow
DISM
```

Las operaciones potencialmente sensibles deben ejecutarse respetando los permisos requeridos por Windows.

---

# 🛠️ Herramientas del sistema

WPC-SutilBox integra diferentes utilidades para facilitar tareas técnicas.

### Procesos

Permite consultar procesos activos y trabajar con su estado y consumo de recursos.

### Servicios

Permite consultar y administrar servicios de Windows cuando la operación y los permisos lo permiten.

### Restauración

La aplicación dispone de integración con los mecanismos de restauración de Windows.

> ⚠️ La creación de un punto de restauración depende del estado de los servicios de restauración de Windows y de los permisos disponibles en el sistema.

---

# 🎨 Interfaz

WPC-SutilBox utiliza una interfaz **WPF/XAML** organizada alrededor de una arquitectura MVVM.

La interfaz contempla:

- 🏠 Dashboard / Inicio.
- 📊 Información del sistema.
- 🧹 Limpieza.
- 📦 Aplicaciones.
- ⚙️ Herramientas.
- 🔧 Procesos y servicios.
- 🎨 Configuración.
- 📜 Historial.

## 🌗 Tema visual

La aplicación dispone de:

- 🌑 Modo oscuro.
- ☀️ Modo claro.

Los recursos visuales se gestionan mediante recursos XAML y un sistema de temas centralizado.

El objetivo de Beta 1 es consolidar estos recursos y eliminar inconsistencias entre los distintos temas.

---

# 🏗️ Arquitectura

WPC-SutilBox está construido sobre:

| Componente | Tecnología |
|---|---|
| Lenguaje | C# 12 |
| Framework | .NET 8 |
| Interfaz | WPF / XAML |
| Arquitectura | MVVM |
| Configuración | JSON |
| Diagnóstico | Windows APIs / WMI |
| Automatización | PowerShell / herramientas nativas |
| Gestión de paquetes | Winget |
| Logging | Logs locales |

## 📐 Estructura conceptual

```text
WPC-SutilBox
│
├── Views
│   └── WPF / XAML
│
├── ViewModels
│   └── MVVM
│
├── Models
│
├── Core
│   └── Services
│       ├── System Monitor
│       ├── Processes
│       ├── Windows Services
│       ├── Cleanup
│       ├── System Restore
│       ├── Winget
│       ├── Theme Manager
│       └── Logger
│
└── Windows
    ├── WMI
    ├── Registry
    ├── PowerShell
    └── Windows APIs
```

### 🔄 Flujo de una operación

```text
Usuario
   │
   ▼
View
   │
   ▼
ViewModel
   │
   ▼
Core / Service
   │
   ▼
Windows API / WMI / Registry / PowerShell
   │
   ▼
Resultado
   │
   ▼
Log + UI
```

---

# ⚡ Asincronía y estabilidad

Una prioridad fundamental de Beta 1 es evitar bloqueos de la interfaz.

Las operaciones potencialmente pesadas deben utilizar mecanismos asíncronos apropiados:

```text
UI
 │
 ├── operación rápida ───────────────► ejecución directa
 │
 └── operación pesada
        │
        ▼
     async/await
        │
        ▼
    Service / Worker
        │
        ▼
    Resultado
        │
        ▼
     UI Thread
```

Especial atención requiere el uso de:

- WMI.
- PowerShell.
- Acceso a disco.
- Procesos externos.
- Operaciones administrativas.
- Consultas de hardware.

---

# 🛡️ Seguridad operativa

Las operaciones administrativas deben respetar el modelo de seguridad de Windows.

## 🔐 UAC

Cuando una operación requiere privilegios elevados, WPC-SutilBox debe:

1. Detectar la necesidad de elevación.
2. Solicitar permisos mediante UAC.
3. Ejecutar la operación elevada.
4. Capturar errores.
5. Informar del resultado.

No se deben ocultar errores de permisos detrás de resultados aparentemente exitosos.

---

## ↩️ Restauración y reversibilidad

Cuando una operación pueda modificar configuraciones importantes del sistema, se debe considerar:

- Punto de restauración.
- Estado previo.
- Rollback.
- Confirmación explícita.
- Registro de la operación.

La reversibilidad exacta depende de la naturaleza de cada modificación.

---

# 📜 Logs

Las operaciones relevantes se registran para facilitar:

- Diagnóstico.
- Auditoría.
- Reproducción de errores.
- Soporte.
- Verificación de resultados.

La configuración y los logs se almacenan en el perfil local del usuario.

```text
%LOCALAPPDATA%\Wpc_SutilBox\
```

Ejemplo:

```text
%LOCALAPPDATA%\Wpc_SutilBox\
├── settings.json
└── logs\
    └── session_*.log
```

---

# 📦 Requisitos

| Requisito | Valor |
|---|---|
| Sistema operativo | Windows 10 / Windows 11 |
| Arquitectura | x64 |
| Framework | .NET 8 |
| Interfaz | WPF |
| Privilegios | Administrador para determinadas operaciones |
| Winget | Requerido únicamente por las funciones que lo utilizan |

---

# 🧑‍💻 Compilar desde código fuente

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

### Publicar para Windows x64

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

La salida se genera dentro del directorio correspondiente de:

```text
bin\Release\
```

---

# 📚 Documentación

La documentación del proyecto se está reorganizando bajo el principio de **Fuente Única de Verdad**.

La estructura objetivo es:

```text
docs/
├── PRODUCT.md
├── ARCHITECTURE.md
├── PLAN_BETAS.md
├── DEVELOPMENT.md
└── CHANGELOG.md
```

## 📘 Documentos principales

| Documento | Contenido |
|---|---|
| [`PRODUCT.md`](docs/PRODUCT.md) | Identidad, propósito, filosofía y límites del producto. |
| [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Arquitectura, MVVM, servicios y reglas técnicas. |
| [`PLAN_BETAS.md`](docs/PLAN_BETAS.md) | Plan maestro de Beta 1, Beta 2, Beta 3 y etapas posteriores. |
| [`DEVELOPMENT.md`](docs/DEVELOPMENT.md) | Normas de desarrollo, compilación, asincronía y verificación. |
| [`CHANGELOG.md`](docs/CHANGELOG.md) | Historial de cambios confirmados. |

> 📌 Los documentos históricos de pruebas de hardware no sustituyen la documentación principal. Se mantienen como **evidencia técnica** y referencia de campo.

---

# 🧪 Evidencia de Beta 1

Los análisis y pruebas realizadas durante la estabilización se conservan en:

```text
ANOSUBIR/
└── BETA_1/
    └── 1.2_PC_REVIEW/
```

Entre los materiales de referencia se encuentran:

```text
ANALISIS_AUDITORIA_INTEGRAL_WPC.md
ANALISIS_PREVIEW_HTML_PROTOTYPE.md
LOG_CORRECCIONES_PC2.md
NOTES_TESTING_PC1.txt
WPC-SutilBox_Dashboard_EJEMPLO.html
```

Estos archivos documentan pruebas, errores, correcciones y prototipos específicos de la fase Beta 1.

---

# 🗺️ Evolución del proyecto

La evolución de WPC-SutilBox se organizará por Betas.

```text
WassControlSys
      │
      ▼
WPC-SutilBox
      │
      ▼
   ┌─────────┐
   │ Beta 1  │
   │Estabil. │
   └────┬────┘
        │
        ▼
   ┌─────────┐
   │ Beta 2  │
   │ Evoluc. │
   └────┬────┘
        │
        ▼
   ┌─────────┐
   │ Beta 3  │
   │ Avance  │
   └────┬────┘
        │
        ▼
    V1.0.0
```

El detalle de cada etapa se mantiene exclusivamente en:

**[`docs/PLAN_BETAS.md`](docs/PLAN_BETAS.md)**

Las nuevas funcionalidades no deben añadirse directamente al README si pertenecen al futuro. Primero deben clasificarse en el plan correspondiente y posteriormente reflejarse aquí cuando estén realmente implementadas.

---

# 🤝 Contribuir

Las contribuciones son bienvenidas.

Flujo recomendado:

```powershell
git checkout -b feature/nueva-funcionalidad

git add .

git commit -m "feat: añadir nueva funcionalidad"

git push origin feature/nueva-funcionalidad
```

Después puede abrirse un Pull Request desde GitHub.

Antes de enviar cambios importantes se recomienda comprobar:

```powershell
dotnet restore
dotnet build -c Release
```

Y verificar que no se introduzcan regresiones en las operaciones del sistema.

---

# 🐛 Reportar un error

Si encuentras un problema, abre un Issue:

**[🐛 Reportar un error](https://github.com/WilmerWass/WPC-SutilBox/issues)**

Incluye, cuando sea posible:

- Versión de WPC-SutilBox.
- Versión de Windows.
- Hardware relevante.
- Descripción del problema.
- Pasos para reproducirlo.
- Capturas de pantalla.
- Logs relacionados.

Los errores reproducidos sobre hardware real tienen especial valor durante Beta 1.

---

# 💡 Proponer una funcionalidad

Las nuevas funcionalidades deben evaluarse primero dentro del plan de Betas.

Antes de implementar una idea se recomienda determinar:

```text
¿Está implementada?
       │
       ├── Sí ──► Documentar estado real
       │
       └── No
           │
           ▼
       ¿Es necesaria para Beta actual?
           │
           ├── Sí ──► PLAN_BETAS
           │
           └── No ──► Backlog / Beta futura
```

Esto evita volver a mezclar funcionalidades futuras con tareas de estabilización.

---

# 📊 Proyecto

<p align="center">

⭐ **[Star en GitHub](https://github.com/WilmerWass/WPC-SutilBox)** ·
📦 **[Releases](https://github.com/WilmerWass/WPC-SutilBox/releases)** ·
🐛 **[Issues](https://github.com/WilmerWass/WPC-SutilBox/issues)** ·
🔀 **[Pull Requests](https://github.com/WilmerWass/WPC-SutilBox/pulls)**

</p>

---

# 📄 Licencia

La licencia definitiva del proyecto se documentará en el archivo:

```text
LICENSE
```

Hasta que la licencia sea definida formalmente, no debe asumirse una licencia específica únicamente por referencias presentes en documentación histórica.

---

<p align="center">

### 🖥️ WPC-SutilBox

**Controla. Comprende. Optimiza. Mantén.**

Desarrollado por **WilmerWass**

</p>