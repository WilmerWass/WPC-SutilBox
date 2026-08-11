# WPC-SutilBox Beta 1

WPC-SutilBox es una utilidad de escritorio para Windows que centraliza mantenimiento, diagnóstico, rendimiento y configuración del equipo en una interfaz WPF moderna.

## Estado de Beta 1

La Beta 1 cierra el flujo funcional principal:

- Monitorización en tiempo real de CPU, RAM, temperatura, batería, red y disco.
- Botón central **Optimizar**: limpia temporales, libera RAM y aplica el perfil activo.
- Creación de puntos de restauración antes de aplicar perfiles.
- Perfiles Equilibrado, Gaming, Productividad, Desarrollo y A tu medida.
- Gestión de aplicaciones de inicio, bloatware y actualizaciones Winget.
- Historial de logs de la sesión dentro de la aplicación.
- Configuración persistente de apariencia, idioma, inicio con Windows, bandeja y optimización en reposo.

## Requisitos

- Windows 10/11 de 64 bits.
- .NET 8 Desktop Runtime para ejecutar la versión publicada.
- Permisos de administrador para operaciones sobre servicios, planes de energía, bloatware o puntos de restauración.
- `winget` instalado para consultar y actualizar aplicaciones.

## Instalación desde código

```powershell
git clone <URL_DEL_REPOSITORIO>
cd WPC-SutilBox
dotnet restore
dotnet build -c Release
dotnet run -c Release
```

Para generar una publicación Windows x64:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

## Navegación

### Inicio

Incluye el botón **Optimizar**, los perfiles de rendimiento y el resumen de uso del sistema.

### Revisar mi PC

Se organiza en cuatro pestañas: Resumen de salud; Espacio y almacenamiento; Hardware y temperaturas; y Procesos de alto consumo.

### Liberar espacio

Incluye limpieza básica, carpeta de Descargas, análisis de archivos grandes y herramientas de Windows.

### Aplicaciones

Incluye gestor de inicio, desinstalador de bloatware y actualizaciones mediante Winget.

### Herramientas avanzadas

Contiene reparación de Windows, procesos, servicios, perfiles y utilidades nativas del sistema.

### Historial y seguridad

Muestra los logs recientes de la sesión y permite abrir la carpeta completa de registros. Las operaciones sensibles se registran para facilitar reversión y diagnóstico.

### Ajustes

Permite cambiar idioma, apariencia, color de acento, inicio con Windows, minimización a bandeja y optimización automática durante periodos de inactividad.

## Modo oscuro y modo claro

Ambos modos usan los mismos tokens de diseño: fondo, superficie, superficie elevada, texto primario/secundario, bordes y estados semánticos. El cambio se realiza desde **Ajustes → Apariencia y Estilo → Modo Oscuro** y se aplica sin reiniciar.

## Arquitectura técnica

- **UI:** WPF/XAML.
- **Patrón:** MVVM.
- **ViewModel principal:** `MainViewModel`.
- **Servicios:** monitorización, batería, restauración, perfiles, procesos, inicio, bloatware, Winget, configuración y logs.
- **Monitorización:** `PerformanceCounter`, `GlobalMemoryStatusEx`, WMI y `PowerStatus`.
- **Configuración:** `%LOCALAPPDATA%\Wpc_SutilBox\settings.json`.
- **Logs:** `%LOCALAPPDATA%\Wpc_SutilBox\logs\session_*.log`.

## Seguridad operativa

Las operaciones destructivas o administrativas se ejecutan de forma explícita. Los perfiles crean un punto de restauración antes de modificar energía, servicios o ajustes del sistema. Las acciones que requieren elevación muestran el diálogo UAC de Windows.

## Compilación y verificación

```powershell
dotnet build --no-restore
```

La Beta 1 debe compilar sin advertencias ni errores antes de distribuirse.

## Documentación adicional

- [Arquitectura](docs/ARCHITECTURE.md)
- [Plan de Beta 1](docs/PLAN_V1.2.0_BETA1.md)
- [Blueprint de producto](docs/PRODUCT_BLUEPRINT_ES.md)
