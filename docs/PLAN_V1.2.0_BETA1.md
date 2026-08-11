# Cierre funcional Beta 1

## Objetivo

Entregar una suite Windows funcional, con acciones reales detrás de cada control visible y una navegación coherente.

## Alcance entregado

- Inicio con `Optimizar`: temporales, RAM y perfil activo.
- `Revisar mi PC`: salud, almacenamiento, hardware/temperaturas y procesos.
- `Liberar espacio`: limpieza básica, Descargas y archivos grandes.
- `Aplicaciones`: inicio, bloatware y Winget.
- `Herramientas avanzadas`: reparación, servicios, procesos y perfiles.
- `Historial y seguridad`: logs de sesión y restauración.
- `Ajustes`: idioma, temas, color, inicio, bandeja y optimización en reposo.

## Criterios de aceptación

- `dotnet build --no-restore` termina con cero advertencias y cero errores.
- El botón `Optimizar` ejecuta una rutina asíncrona real.
- CPU, RAM, temperatura, batería, red y disco actualizan sus bindings.
- Los perfiles no modifican servicios o energía sin punto de restauración.
- Las listas de aplicaciones cargan al refrescar o al abrir la sección.
- Los logs recientes aparecen dentro de la interfaz.
- Tema claro y oscuro definen el mismo conjunto de recursos.
- No existen bindings `TwoWay` sobre propiedades de solo lectura.

## Instalación de validación

Ejecutar `dotnet restore`, después `dotnet build --no-restore` y finalmente `dotnet run` desde la raíz del repositorio.

Validar en Windows 10/11 con una cuenta estándar y repetir las acciones administrativas aceptando o cancelando UAC.

## Riesgos conocidos

- La temperatura depende de que el firmware exponga `MSAcpi_ThermalZoneTemperature`.
- Winget depende de que el cliente esté instalado y disponible en PATH.
- Algunas operaciones requieren permisos elevados o Protección del sistema activa.
