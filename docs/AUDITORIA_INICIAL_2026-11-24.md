# Auditoría inicial de WPC-SutilBox

**Fecha:** 2026-11-24  
**Base:** `8656686` (`chore: pre-audit checkpoint`), sobre `b591ba1`  
**Alcance de esta fase:** inventario de navegación, vistas, ViewModels y composición.

## Integridad del árbol al iniciar

Antes de la auditoría, el árbol ya contenía cambios locales en servicios de limpieza/escaneo, navegación, XAML, opciones de limpieza, ViewModels, `docs/ROADMAP.md` y varios scripts/capturas Python. Se conservaron fuera del checkpoint inicial. Ninguno se atribuye a esta intervención.

La compilación WPF hecha antes de esta intervención terminó con 0 errores y 0 advertencias; la compilación del servidor MCP también terminó con 0 errores y 0 advertencias. En la verificación de la fase 1, la primera compilación de solución sin restauración falló porque faltaba `SutilBox.CLI/obj/project.assets.json`. `dotnet restore WPC-SutilBox.sln` terminó, y `dotnet build WPC-SutilBox.sln --no-restore -v:q` compiló con 0 errores y 1 advertencia `NU1900`: NuGet no pudo acceder a `https://api.nuget.org/v3/index.json` para la auditoría de vulnerabilidades. El intento de mostrar una ventana WPF no fue verificable en esta sesión porque el canal de escritorio no expone ventanas nativas.

## Estructura real observada

- Aplicación WPF .NET 8 en un proyecto principal; `App.xaml.cs` registra servicios y ViewModels en Microsoft DI.
- `MainWindow.xaml` mantiene una instancia persistente de las vistas y las muestra/oculta con `AppSection` y `EnumToVisibilityConverter`.
- `MainViewModel` es un orquestador grande: contiene navegación, operaciones y estado compartido, además de enlazar ViewModels dedicados.
- `Core/` contiene servicios e interfaces, pero el repositorio no está separado en proyectos de dominio/aplicación/infraestructura. La descripción “Clean Architecture” no coincide con los límites de ensamblado actuales.
- Inventario de archivos: 40 vistas XAML, 23 ViewModels y 76 archivos C# en `Core/`.
- `docs/PLAN_V1.2.0_BETA1.md` no existe con ese nombre; sí existen `docs/PRODUCT_BLUEPRINT_ES.md`, `docs/ARCHITECTURE.md`, `docs/ROADMAP.md` y documentos de beta.

## Navegación declarada y visible

`Models/AppSection.cs` contiene secciones para Dashboard, revisión de PC, batería, benchmark, drivers, salud de discos, USB, periféricos, red, reparación, compatibilidad, duplicados, limpieza, mapa, almacenamiento, aplicaciones, bloatware, instalación/actualización de programas, inicio, optimizaciones, privacidad, rendimiento, historial/deshacer, puntos de restauración, backup/post-formateo, herramientas, historial, ajustes, hardware, escaneo, acerca de, escala y editor de perfiles.

El menú lateral visible ofrece Inicio/Mi PC, mantenimiento y aplicaciones, diagnóstico, optimizaciones/privacidad/rendimiento, restauración/backup/herramientas e historial/ajustes. Mantiene muchas acciones como accesos individuales; aún no sigue los diez dominios principales pedidos.

La navegación convierte alias antiguos antes de cambiar sección:

| Alias | Destino efectivo |
| --- | --- |
| `ProgramasInstalar` | `ProgramasActualizar` |
| `Bloatware` | `Desinstalador` |
| `Red` | `TestRed` |
| `Hardware` | `RevisarPC` |
| `Escala` | `Configuracion` |

Los alias conservan compatibilidad interna, pero algunas opciones visibles/documentadas pueden acabar en una pantalla diferente de la que su nombre sugiere.

## Vistas conectadas y estado de verificación

El host contiene 32 instancias de vista para 31 vistas distintas (Configuración se instancia también para Escala). Las instancias visibles asignan ViewModels explícitamente en Dashboard, PC, batería, benchmark, drivers, disco/almacenamiento, USB, periféricos, red, duplicados, limpieza, mapa, aplicaciones, actualizaciones, inicio, privacidad, historial/deshacer y perfiles. Otras vistas usan estado del `MainViewModel` por herencia de `DataContext` o propiedades directas del host.

Una instancia y un enlace de `DataContext` solo acreditan conexión estructural. El funcionamiento real de bindings, comandos y operaciones queda pendiente de la fase 2; la validación visual de escritorio no está disponible en este entorno.

## Nueve vistas existentes sin acceso desde el host

| Vista | Estado encontrado | Clasificación inicial / tratamiento |
| --- | --- | --- |
| `BloatwareView` | XAML con acción de desinstalación; no está instanciada. `MainViewModel` ya mantiene colección, servicio y comando relacionados. | Desconectada; lógica reutilizable y probable solapamiento con Aplicaciones. No eliminar antes de consolidar. |
| `GamesView` | Existe XAML, pero no hay instancia en `MainWindow`, `GamesViewModel` ni servicio de biblioteca identificados. | Prototipo/desconectada; Zona Gamer pendiente de integración verificable. |
| `HardwareView` | Vista extensa con controles de equipo, discos, drivers y utilidades; no está instanciada. | Desconectada y duplicada parcialmente con PC, almacenamiento, drivers y herramientas. Revisar comandos antes de migrar. |
| `MantenimientoView` | Vista con limpieza, RAM, red y reparación; no está instanciada. | Desconectada y solapada con Limpieza, Optimizaciones y Herramientas. No reutilizar acciones sensibles sin revisión. |
| `ProcessManagerView` | Controles para listar, priorizar y finalizar procesos; no está instanciada ni tiene VM dedicado. | Desconectada; conceptos similares están en `MainViewModel`/servicio. Requiere revisión de seguridad antes de integrar. |
| `SecurityView` | Pantalla de estado y accesos a Windows; no está instanciada. | Desconectada; solapa con Privacidad, Compatibilidad y Herramientas. |
| `ServiceOptimizerView` | Controles para iniciar/detener servicios; no está instanciada. | Desconectada; hay lógica relacionada en `MainViewModel` y `IServiceOptimizerService`. Alto riesgo operativo. |
| `SystemInfoView` | Vista de información y exportación; no está instanciada. | Desconectada y solapada con PC y Drivers. |
| `ThermalMonitorView` | XAML con actualización de temperaturas; no está instanciada. | Desconectada; existe `TemperatureMonitorService`, pero no un ViewModel dedicado detectado. |

## Clasificación inicial global

- **Conectado estructuralmente, falta verificación funcional:** las vistas declaradas en el host y sus ViewModels enlazados.
- **Parcial o no verificable:** comandos que dependen de `MainViewModel`; enlaces declarados sin cobertura de ejecución; navegación con alias; estados de carga/error y cancelación aún por auditar.
- **Desconectado:** las nueve vistas de la tabla anterior.
- **Duplicación probable:** Información/Hardware/PC; Bloatware/Aplicaciones; Mantenimiento/Limpieza/Optimizaciones; ProcessManager/Rendimiento; ServiceOptimizer/Rendimiento; Security/Privacidad; SystemInfo/PC; restauración desde más de una ruta.
- **Pendiente de implementación:** biblioteca Gamer verificable y su monitor de sesión; el estado de cada capacidad se precisará en las fases correspondientes.
- **Obsoleto:** no se elimina ni clasifica definitivamente ninguna vista en esta fase; hace falta revisar las referencias y confirmar si aporta lógica exclusiva.

## Hallazgos para las fases siguientes

1. La navegación usa conversiones de alias y una gran vista raíz con muchas vistas persistentes; el cambio de sección no reconstruye la ventana, pero toda la organización se concentra en `MainWindow.xaml` y `MainViewModel`.
2. Hay responsabilidades duplicadas entre el host estático, el enum de secciones, el menú y las rutas de notificación `OnNavigatedTo/From`.
3. La regla de alias puede cambiar destinos esperados; confirmar cada enlace del menú contra la pantalla real.
4. Se debe mantener como base cualquier trabajo preexistente en progreso. Las siguientes fases modificarán esos archivos con cuidado y los commits incluirán solo cambios atribuibles a esta intervención.
