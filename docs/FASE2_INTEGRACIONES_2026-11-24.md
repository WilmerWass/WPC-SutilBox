# Fase 2 — Integraciones y funcionalidad

**Fecha:** 2026-11-24  
**Commit anterior:** `8a60526` (`docs: record initial navigation audit`)

## Cambios aplicados

- `SmartScannerView` recibe el `SmartScannerViewModel` real: el ViewModel se inyecta en `MainViewModel` y el host lo enlaza explícitamente. El `d:DataContext` permanece solo como ayuda de diseño.
- `NetworkView` utiliza `RunTestCommand` y `CancelCommand` de `NetworkTestViewModel`, muestra progreso/resultados y aclara que la medición de subida envía hasta 5 MB a Cloudflare.
- Se conservaron las reparaciones DNS y Winsock. Los comandos se despachan mediante una lista permitida; cada uno pide confirmación. Winsock solicita elevación y comunica que puede requerirse reiniciar. Los destinos de herramientas desconocidos se bloquean.
- `PrivacyView` muestra los ajustes obtenidos por `PrivacyViewModel`; eliminó los tres checks fijos que aparentaban representar el estado del equipo. La lectura predeterminada cuando no existe una clave se identifica como estimación. Solo el ID de publicidad se puede modificar: es el único ajuste con escritura DWORD validada en esta revisión; el servicio pide confirmación y vuelve a leer el valor tras aplicar.
- `WindowsDiagnosticEngine` ya no presenta el placeholder de motor no disponible. Implementa una lectura de RAM con `GlobalMemoryStatusEx`, porcentaje de uso de unidades listas y datos básicos de sistema. Es de solo lectura y deliberadamente no reporta SMART, drivers, seguridad, inicio ni temperaturas.
- `README.md`, `CLI.md` y la ayuda CLI describen Windows como diagnóstico parcial, no como escaneo completo.
- La medición de tiempo activo del CLI se corrigió tras detectar un formato inválido en la ejecución de `sbox info`.

## Verificación

- `dotnet build WPC-SutilBox.csproj --no-restore -v:q`: correcto, 0 advertencias, 0 errores.
- `dotnet build SutilBox.CLI/SutilBox.CLI.csproj --no-restore -v:q`: correcto; 1 aviso `NU1900` por falta de acceso a NuGet para auditoría de vulnerabilidades.
- `dotnet build SutilBox.McpServer/SutilBox.McpServer.csproj --no-restore -v:q`: correcto, 0 advertencias, 0 errores.
- `dotnet build WPC-SutilBox.sln --no-restore -v:q`: correcto, 0 errores y el aviso `NU1900`.
- `sbox scan` se ejecutó en Windows e informó una advertencia real de espacio (unidad C: al 88%); no ejecutó cambios.
- `sbox info` se ejecutó y mostró CPU, memoria, unidad y tiempo activo.
- No se encontró un proyecto de pruebas automatizadas .NET en la solución. Las pruebas Python disponibles interactúan por coordenadas con la GUI; no se ejecutaron porque pueden activar tareas del sistema. La revisión visual WPF sigue limitada por la falta de canal de escritorio.

## Límites abiertos

- Las lecturas de privacidad aún muestran estimaciones predeterminadas cuando falta una clave, en vez de modelar cada estado como “no configurado”. Las otras entradas se dejan en solo lectura porque sus tipos y ubicaciones de escritura no están validados.
- La lista de herramientas del host permite abrir utilidades externas seleccionadas; su operación interna queda bajo control del usuario dentro de Windows.
- La verificación de clics, bindings visuales y cancelación durante una ejecución WPF necesita un entorno de escritorio que esta sesión no tiene.
